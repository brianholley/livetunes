using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using System.Data.Linq;
using System.Linq;

namespace LiveTunes
{
    public class ConcertListings
    {
        const string lastfmAppKey = "";
        const string lastfmApiUrl = "http://ws.audioscrobbler.com/2.0/?api_key={0}&method={1}&{2}&limit=0";
        const string lastfmEventsMethod = "geo.getevents";
        const string lastfmLocationFmt = "location={0}&distance={1}";
        const string lastfmLatLongFmt = "lat={0}&long={1}&distance={2}";

        public static readonly TimeSpan MinRequeryTime = new TimeSpan(20, 0, 0);
        public static readonly TimeSpan MinRetryTime = new TimeSpan(0, 0, 30);

        public delegate void ListingsCallback(List<ConcertItem> concerts);
        public delegate void ErrorCallback(int error, string message);

        public static void GetListings(Location loc, ListingsCallback listingsCallback, ErrorCallback errorCallback)
        {
            string locParam = "";
            if (loc.Type == Location.LocationType.NamedPlace)
                locParam = string.Format(lastfmLocationFmt, loc.Place, Settings.Distance);
            else if (loc.Type == Location.LocationType.LatLong)
                locParam = string.Format(lastfmLatLongFmt, loc.Lat, loc.Long, Settings.Distance);

            System.Diagnostics.Debug.WriteLine("{0}: Sending request to last.fm", DateTime.Now);

            string url = string.Format(lastfmApiUrl, lastfmAppKey, lastfmEventsMethod, locParam);
            System.Diagnostics.Debug.WriteLine("{0}: Server url: {1}", DateTime.Now, url);
            var request = WebRequest.CreateHttp(url);
            var responseData = new ResponseData() 
            { 
                ListingsCallback = listingsCallback, 
                ErrorCallback = errorCallback, 
                HttpRequest = request
            };
            request.BeginGetResponse(new AsyncCallback(LastfmCallback), responseData);
        }

        class ResponseData
        {
            public ListingsCallback ListingsCallback { get; set; }
            public ErrorCallback ErrorCallback { get; set; }
            public HttpWebRequest HttpRequest { get; set; }
        }

        private static void LastfmCallback(IAsyncResult asynchronousResult)
        {
            System.Diagnostics.Debug.WriteLine("{0}: Got response", DateTime.Now);

            var responseData = asynchronousResult.AsyncState as ResponseData;
            try
            {
                var response = responseData.HttpRequest.EndGetResponse(asynchronousResult) as HttpWebResponse;

                List<ArtistItem> artistsInDB;
                List<VenueItem> venuesInDB;
                List<TagItem> tagsInDB;

                {
                    App.ViewModel.ConcertDBMutex.WaitOne();

                    var allArtists = from ArtistItem artist in App.ViewModel.ConcertDB.Artists select artist;
                    var allVenues = from VenueItem venue in App.ViewModel.ConcertDB.Venues select venue;
                    var allTags = from TagItem tag in App.ViewModel.ConcertDB.Tags select tag;

                    artistsInDB = new List<ArtistItem>(allArtists);
                    venuesInDB = new List<VenueItem>(allVenues);
                    tagsInDB = new List<TagItem>(allTags);

                    App.ViewModel.ConcertDBMutex.ReleaseMutex();
                }

                using (var responseStream = response.GetResponseStream())
                {
                    var concerts = ConcertListingsParser.ParseConcertResponse(responseStream, artistsInDB, venuesInDB, tagsInDB);

                    App.ViewModel.ConcertDBMutex.WaitOne();

                    var concertIdsInDB = from ConcertItem c in App.ViewModel.ConcertDB.Concerts select c.ConcertId;
                    foreach (var concert in concerts)
                    {
                        if (!concertIdsInDB.Contains(concert.ConcertId))
                            App.ViewModel.ConcertDB.Concerts.InsertOnSubmit(concert);
                    }
                    App.ViewModel.ConcertDB.SubmitChanges();

                    App.ViewModel.ConcertDBMutex.ReleaseMutex();
                    System.Diagnostics.Debug.WriteLine("{0}: Wrote concerts to DB", DateTime.Now);

                    Microsoft.Phone.Reactive.Scheduler.Dispatcher.Dispatcher.BeginInvoke(() =>
                    {
                        responseData.ListingsCallback(concerts);
                    });
                }
            }
            catch (ConcertListingsException exc)
            {
                Microsoft.Phone.Reactive.Scheduler.Dispatcher.Dispatcher.BeginInvoke(() =>
                {
                    responseData.ErrorCallback(exc.Error, exc.Detail);
                });
            }
            catch (WebException exc)
            {
                Microsoft.Phone.Reactive.Scheduler.Dispatcher.Dispatcher.BeginInvoke(() =>
                {
                    responseData.ErrorCallback((int)(exc.Response as HttpWebResponse).StatusCode, "");
                });
            }
            catch (Exception exc)
            {
                Microsoft.Phone.Reactive.Scheduler.Dispatcher.Dispatcher.BeginInvoke(() =>
                {
                    responseData.ErrorCallback(0, exc.Message);
                });
            }
        }
    }

    public class ConcertListingsException : Exception
    {
        public ConcertListingsException(int error, string detail)
        {
            Error = error;
            Detail = detail;
        }
        public int Error { get; private set; }
        public string Detail { get; private set; }
    }

    // TODO: The DB lookups during concert parsing could be more efficient
    public class ConcertListingsParser
    {
        public static List<ConcertItem> ParseConcertResponse(Stream stream, List<ArtistItem> artists, List<VenueItem> venues, List<TagItem> tags)
        {
            List<ConcertItem> concerts = new List<ConcertItem>();
            XmlReader reader = XmlReader.Create(stream);

            int i = 0;
            while (reader.Read())
            {
                System.Diagnostics.Debug.WriteLine("{0}: Parsing concert {1}", DateTime.Now, ++i);

                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "events")
                    {
                        // TODO: Without doing incremental list update in the UI there's no reason to do incremental syncs to the server
                        //string total = reader.GetAttribute("total");
                        //string totalPages = reader.GetAttribute("totalPages");
                        //string perPage = reader.GetAttribute("perPage");
                        //string page = reader.GetAttribute("page");
                        //AppCache.LastConcertSyncPlace = reader.GetAttribute("location");
                    }

                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "event")
                    {
                        concerts.Add(ParseConcertEvent(reader, artists, venues, tags));
                    }

                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "error")
                    {
                        int code = 0;
                        if (reader.MoveToAttribute("code"))
                            code = reader.ReadContentAsInt();
                        reader.MoveToElement();
                        string message = reader.ReadElementContentAsString();
                        throw new ConcertListingsException(code, message);
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine("{0}: Finished parsing concerts", DateTime.Now);

            return concerts;
        }

        private static ConcertItem ParseConcertEvent(XmlReader reader, List<ArtistItem> artists, List<VenueItem> venues, List<TagItem> tags)
        {
            ConcertItem concert = null;
            if (reader.NodeType == XmlNodeType.Element && reader.Name == "event")
            {
                concert = new ConcertItem();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "id":
                                concert.ConcertId = reader.ReadElementContentAsInt();
                                break;
                            case "title":
                                concert.Title = reader.ReadElementContentAsString();
                                break;
                            case "headliner":
                            {
                                var artistName = reader.ReadElementContentAsString();
                                concert.Headliner = artists.FirstOrDefault(a => a.ArtistName == artistName);
                                if (concert.Headliner == null)
                                {
                                    concert.Headliner = new ArtistItem() { ArtistName = artistName };
                                    artists.Add(concert.Headliner);
                                }
                                concert.Ignore = concert.Ignore || concert.Headliner.Ignore;
                                break;
                            }
                            case "artist":
                            {
                                var artistName = reader.ReadElementContentAsString();
                                var artistItem = artists.FirstOrDefault(a => a.ArtistName == artistName);
                                if (artistItem == null)
                                {
                                    artistItem = new ArtistItem() { ArtistName = artistName };
                                    artists.Add(artistItem);
                                }
                                concert.Artists.Add(new ConcertArtist() { ConcertId = concert.ConcertId, Artist = artistItem, ArtistId = artistItem.ArtistId });
                                concert.Ignore = concert.Ignore || concert.Artists.Last().Artist.Ignore;
                                break;
                            }
                            case "venue":
                            {
                                var parsedVenue = ParseVenue(reader);
                                concert.Venue = venues.FirstOrDefault(v => v.VenueId == parsedVenue.VenueId);
                                if (concert.Venue == null)
                                {
                                    concert.Venue = parsedVenue;
                                    venues.Add(parsedVenue);
                                }
                                break;
                            }
                            case "startDate":
                                concert.StartTime = DateTime.Parse(reader.ReadElementContentAsString());
                                break;
                            case "description":
                                concert.Description = reader.ReadElementContentAsString();
                                break;
                            case "url":
                                concert.Url = reader.ReadElementContentAsString();
                                break;
                            case "website":
                                concert.Website = reader.ReadElementContentAsString();
                                break;
                            case "cancelled":
                                concert.Cancelled = reader.ReadElementContentAsBoolean();
                                break;
                            case "tags":
                                ParseTags(reader, concert, tags);
                                break;
                            default:
                                break;
                        }
                    }

                    if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "event")
                        break;
                }
            }
            return concert;
        }

        private static VenueItem ParseVenue(XmlReader reader)
        {
            VenueItem venue = null;
            if (reader.NodeType == XmlNodeType.Element && reader.Name == "venue")
            {
                venue = new VenueItem();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "id":
                                venue.VenueId = reader.ReadElementContentAsInt();
                                break;
                            case "name":
                                venue.VenueName = reader.ReadElementContentAsString();
                                break;
                            case "city":
                                venue.City = reader.ReadElementContentAsString();
                                break;
                            case "street":
                                venue.Street = reader.ReadElementContentAsString();
                                break;
                            case "postalcode":
                                venue.PostalCode = reader.ReadElementContentAsString();
                                break;
                            case "geo:lat":
                                venue.Latitude = reader.ReadElementContentAsDouble();
                                break;
                            case "geo:long":
                                venue.Longitude = reader.ReadElementContentAsDouble();
                                break;
                            case "url":
                                venue.Url = reader.ReadElementContentAsString();
                                break;
                            case "website":
                                venue.Website = reader.ReadElementContentAsString();
                                break;
                            case "phonenumber":
                                venue.PhoneNumber = reader.ReadElementContentAsString();
                                break;
                            default:
                                break;
                        }
                    }

                    if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "venue")
                        break;
                }
            }

            return venue;
        }

        private static void ParseTags(XmlReader reader, ConcertItem concert, List<TagItem> tags)
        {
            if (reader.NodeType == XmlNodeType.Element && reader.Name == "tags")
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "tag":
                            {
                                string tagName = reader.ReadElementContentAsString();
                                TagItem tagItem = tags.FirstOrDefault(tag => tag.Tag == tagName);
                                if (tagItem == null)
                                {
                                    tagItem = new TagItem() { Tag = tagName };
                                    tags.Add(tagItem);
                                }
                                concert.Tags.Add(new ConcertTag() { ConcertId = concert.ConcertId, Tag = tagItem, TagId = tagItem.TagId });
                                break;
                            }
                            default:
                                break;
                        }
                    }

                    if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "tags")
                        break;
                }
            }
        }
    }
}

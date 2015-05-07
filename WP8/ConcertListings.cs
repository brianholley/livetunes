using System;
using System.Collections.Generic;
using System.Net;
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

				List<int> concertIdsInDb;
                Dictionary<string, ArtistItem> artistsInDb;
                Dictionary<int, VenueItem> venuesInDb;
                Dictionary<string, TagItem> tagsInDb;

				{
                    App.ViewModel.ConcertDBMutex.WaitOne();

					concertIdsInDb = App.ViewModel.ConcertDB.Concerts.Select(c => c.ConcertId).ToList();
					artistsInDb = App.ViewModel.ConcertDB.Artists.ToDictionary(a => a.ArtistName);
					venuesInDb = App.ViewModel.ConcertDB.Venues.ToDictionary(v => v.VenueId);
					tagsInDb = App.ViewModel.ConcertDB.Tags.ToDictionary(t => t.Tag);

                    App.ViewModel.ConcertDBMutex.ReleaseMutex();
                }

                using (var responseStream = response.GetResponseStream())
                {
                    var concerts = ConcertListingsParser.ParseConcertResponse(responseStream, artistsInDb, venuesInDb, tagsInDb);

                    App.ViewModel.ConcertDBMutex.WaitOne();
                    foreach (var concert in concerts)
                    {
	                    if (!concertIdsInDb.Contains(concert.ConcertId))
	                    {
		                    App.ViewModel.ConcertDB.Concerts.InsertOnSubmit(concert);
							concertIdsInDb.Add(concert.ConcertId);
	                    }
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
}

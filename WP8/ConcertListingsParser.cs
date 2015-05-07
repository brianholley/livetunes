using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace LiveTunes
{
	// TODO: The DB lookups during concert parsing could be more efficient
	public static class ConcertListingsParser
	{
		public static List<ConcertItem> ParseConcertResponse(Stream stream, Dictionary<string, ArtistItem> artists, Dictionary<int, VenueItem> venues, Dictionary<string, TagItem> tags)
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

		private static ConcertItem ParseConcertEvent(XmlReader reader, Dictionary<string, ArtistItem> artists, Dictionary<int, VenueItem> venues, Dictionary<string, TagItem> tags)
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
								if (artists.ContainsKey(artistName))
								{
									concert.Headliner = artists[artistName];
								}
								else
								{
									concert.Headliner = new ArtistItem() { ArtistName = artistName };
									artists.Add(artistName, concert.Headliner);
								}
								concert.Ignore = concert.Ignore || concert.Headliner.Ignore;
								break;
							}
							case "artist":
							{
								var artistName = reader.ReadElementContentAsString();
								ArtistItem artist;
								if (artists.ContainsKey(artistName))
								{
									artist = artists[artistName];
								}
								else
								{
									artist = new ArtistItem() { ArtistName = artistName };
									artists.Add(artistName, artist);
								}
								concert.Artists.Add(new ConcertArtist() { ConcertId = concert.ConcertId, Artist = artist, ArtistId = artist.ArtistId });
								concert.Ignore = concert.Ignore || concert.Artists.Last().Artist.Ignore;
								break;
							}
							case "venue":
							{
								var parsedVenue = ParseVenue(reader);
								if (venues.ContainsKey(parsedVenue.VenueId))
								{
									concert.Venue = venues[parsedVenue.VenueId];
								}
								else
								{
									concert.Venue = parsedVenue;
									venues.Add(parsedVenue.VenueId, parsedVenue);
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

		private static void ParseTags(XmlReader reader, ConcertItem concert, Dictionary<string, TagItem> tags)
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
								TagItem tag;
								if (tags.ContainsKey(tagName))
								{
									tag = tags[tagName];
								}
								else
								{
									tag = new TagItem() { Tag = tagName };
									tags.Add(tagName, tag);
								}
								concert.Tags.Add(new ConcertTag() { ConcertId = concert.ConcertId, Tag = tag, TagId = tag.TagId });
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
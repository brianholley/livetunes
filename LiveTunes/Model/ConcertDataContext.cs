using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Collections.Generic;

namespace LiveTunes
{
    public class ConcertDataContext : DataContext
    {
        public ConcertDataContext(string connectionString) : base(connectionString)
        {
            DataLoadOptions dlo = new DataLoadOptions();
            dlo.LoadWith<ConcertItem>(c => c.Headliner);
            dlo.LoadWith<ConcertItem>(c => c.Artists);
            dlo.LoadWith<ConcertItem>(c => c.Venue);
            dlo.LoadWith<ConcertItem>(c => c.Tags);
            LoadOptions = dlo;
        }

        public Table<ConcertItem> Concerts;
        public Table<ArtistItem> Artists;
        public Table<VenueItem> Venues;
        public Table<TagItem> Tags;

        public Table<ConcertArtist> ConcertArtists;
        public Table<ConcertTag> ConcertTags;

        public void DumpAllConcerts()
        {
        }
        public void DumpAllArtists()
        {
            System.Diagnostics.Debug.WriteLine("=== Artists ===");
            var artists = from ArtistItem artist in Artists select artist;
            foreach (var artist in artists)
                System.Diagnostics.Debug.WriteLine(artist.ArtistId + " = " + artist.ArtistName);
        }
        public void DumpAllVenues()
        {
            System.Diagnostics.Debug.WriteLine("=== Venues ===");
            var venues = from VenueItem venue in Venues select venue;
            foreach (var venue in venues)
                System.Diagnostics.Debug.WriteLine(venue.VenueId + " = " + venue.VenueName + "(" + venue.Street + ", " + venue.City + ")");
        }
        public void DumpAllTags()
        {
            System.Diagnostics.Debug.WriteLine("=== Tags ===");
            var tags = from TagItem tag in Tags select tag;
            foreach (var tag in tags)
                System.Diagnostics.Debug.WriteLine(tag.TagId + " = " + tag.Tag);
        }
    }

#region Linking Tables

    [Table]
    public class ConcertArtist
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = false, DbType = "INT NOT NULL", CanBeNull = false)]
        public int ConcertId { get; set; }

        [Column(IsPrimaryKey = true, IsDbGenerated = false, DbType = "INT NOT NULL", CanBeNull = false)]
        public int ArtistId { get; set; }

        private EntityRef<ArtistItem> _artist;
        [Association(Storage = "_artist", ThisKey = "ArtistId", OtherKey = "ArtistId", IsForeignKey = true)]
        public ArtistItem Artist { get { return _artist.Entity; } set { _artist.Entity = value; } }
    }

    [Table]
    public class ConcertTag
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = false, DbType = "INT NOT NULL", CanBeNull = false)]
        public int ConcertId { get; set; }

        [Column(IsPrimaryKey = true, IsDbGenerated = false, DbType = "INT NOT NULL", CanBeNull = false)]
        public int TagId { get; set; }

        private EntityRef<TagItem> _tag;
        [Association(Storage = "_tag", ThisKey = "TagId", OtherKey = "TagId", IsForeignKey = true)]
        public TagItem Tag { get { return _tag.Entity; } set { _tag.Entity = value; } }
    }

#endregion
}

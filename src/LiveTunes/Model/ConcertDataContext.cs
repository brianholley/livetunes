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

    [Table]
    public class ConcertItem : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private int _concertId;
        [Column(IsPrimaryKey = true, IsDbGenerated = false, DbType = "INT NOT NULL", CanBeNull = false)]
        public int ConcertId
        {
            get { return _concertId; }
            set
            {
                if (_concertId != value)
                {
                    NotifyPropertyChanging("ConcertId");
                    _concertId = value;
                    NotifyPropertyChanged("ConcertId");
                }
            }
        }

        private string _title;
        [Column]
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    NotifyPropertyChanging("Title");
                    _title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }

        private EntitySet<ConcertArtist> _artists = new EntitySet<ConcertArtist>();
        [Association(Storage = "_artists", ThisKey = "ConcertId", OtherKey = "ConcertId")]
        public EntitySet<ConcertArtist> Artists
        {
            get { return _artists; }
            set
            {
                if (_artists != value)
                {
                    NotifyPropertyChanging("Artists");
                    _artists = value;
                    NotifyPropertyChanged("Artists");
                }
            }
        }

        [Column]
        private int HeadlinerId { get; set; }

        private EntityRef<ArtistItem> _headliner;
        [Association(Storage = "_headliner", ThisKey = "HeadlinerId", OtherKey = "ArtistId", IsForeignKey = true)]
        public ArtistItem Headliner
        {
            get { return _headliner.Entity; }
            set
            {
                if (value != _headliner.Entity)
                {
                    NotifyPropertyChanging("Headliner");
                    _headliner.Entity = value;
                    NotifyPropertyChanged("Headliner");
                }
            }
        }

        [Column]
        private int VenueId { get; set; }

        private EntityRef<VenueItem> _venue;
        [Association(Storage = "_venue", ThisKey = "VenueId", OtherKey = "VenueId", IsForeignKey = true)]
        public VenueItem Venue
        {
            get { return _venue.Entity; }
            set
            {
                if (value != _venue.Entity)
                {
                    NotifyPropertyChanging("Venue");
                    _venue.Entity = value;
                    NotifyPropertyChanged("Venue");
                }
            }
        }

        private DateTime _startTime;
        [Column]
        public DateTime StartTime
        {
            get { return _startTime; }
            set
            {
                NotifyPropertyChanging("StartTime");
                _startTime = value;
                NotifyPropertyChanged("StartTime");
            }
        }

        private string _description;
        [Column(DbType = "NTEXT", UpdateCheck=UpdateCheck.Never)]
        public string Description
        {
            get { return _description; }
            set
            {
                NotifyPropertyChanging("Description");
                _description = value;
                NotifyPropertyChanged("Description");
            }
        }

        // Image
        // Attendance (?)
        // Reviews
        // Last.fm Tag
        
        private string _url;
        [Column]
        public string Url
        {
            get { return _url; }
            set
            {
                NotifyPropertyChanging("Url");
                _url = value;
                NotifyPropertyChanged("Url");
            }
        }

        private string _website;
        [Column]
        public string Website
        {
            get { return _website; }
            set
            {
                NotifyPropertyChanging("Website");
                _website = value;
                NotifyPropertyChanged("Website");
            }
        }

        // Tickets (?)

        private bool _cancelled;
        [Column]
        public bool Cancelled
        {
            get { return _cancelled; }
            set
            {
                NotifyPropertyChanging("Cancelled");
                _cancelled = value;
                NotifyPropertyChanged("Cancelled");
            }
        }

        private EntitySet<ConcertTag> _tags = new EntitySet<ConcertTag>();
        [Association(Name = "ConcertTags", Storage = "_tags", ThisKey = "ConcertId", OtherKey = "TagId")]
        public EntitySet<ConcertTag> Tags
        {
            get { return _tags; }
            set
            {
                if (_tags != value)
                {
                    NotifyPropertyChanging("Tags");
                    _tags = value;
                    NotifyPropertyChanged("Tags");
                }
            }
        }

        // User-set properties
        private bool _ignore = false;
        [Column]
        public bool Ignore
        {
            get { return _ignore; }
            set
            {
                NotifyPropertyChanging("Ignore");
                _ignore = value;
                NotifyPropertyChanged("Ignore");
            }
        }

        private bool _favorite = false;
        [Column]
        public bool Favorite
        {
            get { return _favorite; }
            set
            {
                NotifyPropertyChanging("Favorite");
                _favorite = value;
                NotifyPropertyChanged("Favorite");
            }
        }

        // Helpers
        // TODO: This sort of stuff should really go on the VM, not on the model
        public string ArtistNames
        {
            get
            {
                List<string> artistNames = new List<string>();
                foreach (var concertArtist in Artists)
                {
                    if (concertArtist.ArtistId != Headliner.ArtistId)
                        artistNames.Add(concertArtist.Artist.ArtistName);
                }
                return string.Join(", ", artistNames);
            }
        }

        public string DateAndPlace
        {
            get
            {
                return string.Format("{0}, {1} at {2}", StartTime.ToString("dddd"), StartTime.ToString("M"), Venue.VenueName);
            }
        }

        public string TimeAndPlace
        {
            get
            {
                return string.Format("{0} at {1}", StartTime.ToString("t"), Venue.VenueName);
            }
        }

        public string DescriptionAsText
        {
            get
            {
                return Utilities.StripHTML(Description);
            }
        }

        public bool TitleDifferentFromHeadliner
        {
            get
            {
                return Title != Headliner.ArtistName;
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify that a property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify that a property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }

    [Table]
    public class ArtistItem : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private int _artistId;
        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int ArtistId
        {
            get { return _artistId; }
            set
            {
                if (_artistId != value)
                {
                    NotifyPropertyChanging("ArtistId");
                    _artistId = value;
                    NotifyPropertyChanged("ArtistId");
                }
            }
        }

        private string _artistName;
        [Column]
        public string ArtistName
        {
            get { return _artistName; }
            set
            {
                if (_artistName != value)
                {
                    NotifyPropertyChanging("ArtistName");
                    _artistName = value;
                    NotifyPropertyChanged("ArtistName");
                }
            }
        }

        // User-set properties
        private bool _inLibrary = false;
        [Column]
        public bool InLibrary
        {
            get { return _inLibrary; }
            set
            {
                NotifyPropertyChanging("InLibrary");
                _inLibrary = value;
                NotifyPropertyChanged("InLibrary");
            }
        }

        private bool _favorite = false;
        [Column]
        public bool Favorite
        {
            get { return _favorite; }
            set
            {
                NotifyPropertyChanging("Favorite");
                _favorite = value;
                NotifyPropertyChanged("Favorite");
            }
        }

        private bool _ignore = false;
        [Column]
        public bool Ignore
        {
            get { return _ignore; }
            set
            {
                NotifyPropertyChanging("Ignore");
                _ignore = value;
                NotifyPropertyChanged("Ignore");
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify that a property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify that a property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }

    [Table]
    public class VenueItem : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private int _venueId;
        [Column(IsPrimaryKey = true, IsDbGenerated = false, DbType = "INT NOT NULL", CanBeNull = false)]
        public int VenueId
        {
            get { return _venueId; }
            set
            {
                if (_venueId != value)
                {
                    NotifyPropertyChanging("VenueId");
                    _venueId = value;
                    NotifyPropertyChanged("VenueId");
                }
            }
        }

        private string _venueName;
        [Column]
        public string VenueName
        {
            get { return _venueName; }
            set
            {
                if (_venueName != value)
                {
                    NotifyPropertyChanging("VenueName");
                    _venueName = value;
                    NotifyPropertyChanged("VenueName");
                }
            }
        }

        private string _street;
        [Column]
        public string Street
        {
            get { return _street; }
            set
            {
                if (_street != value)
                {
                    NotifyPropertyChanging("Street");
                    _street = value;
                    NotifyPropertyChanged("Street");
                }
            }
        }

        private string _city;
        [Column]
        public string City
        {
            get { return _city; }
            set
            {
                if (_city != value)
                {
                    NotifyPropertyChanging("City");
                    _city = value;
                    NotifyPropertyChanged("City");
                }
            }
        }

        private string _postalCode;
        [Column]
        public string PostalCode
        {
            get { return _postalCode; }
            set
            {
                if (_postalCode != value)
                {
                    NotifyPropertyChanging("PostalCode");
                    _postalCode = value;
                    NotifyPropertyChanged("PostalCode");
                }
            }
        }

        private double _latitude;
        [Column]
        public double Latitude
        {
            get { return _latitude; }
            set
            {
                if (_latitude != value)
                {
                    NotifyPropertyChanging("Latitude");
                    _latitude = value;
                    NotifyPropertyChanged("Latitude");
                }
            }
        }

        private double _longitude;
        [Column]
        public double Longitude
        {
            get { return _longitude; }
            set
            {
                if (_longitude != value)
                {
                    NotifyPropertyChanging("Longitude");
                    _longitude = value;
                    NotifyPropertyChanged("Longitude");
                }
            }
        }

        private string _url;
        [Column]
        public string Url
        {
            get { return _url; }
            set
            {
                if (_url != value)
                {
                    NotifyPropertyChanging("Url");
                    _url = value;
                    NotifyPropertyChanged("Url");
                }
            }
        }

        private string _website;
        [Column]
        public string Website
        {
            get { return _website; }
            set
            {
                if (_website != value)
                {
                    NotifyPropertyChanging("Website");
                    _website = value;
                    NotifyPropertyChanged("Website");
                }
            }
        }

        private string _phoneNumber;
        [Column]
        public string PhoneNumber
        {
            get { return _phoneNumber; }
            set
            {
                if (_phoneNumber != value)
                {
                    NotifyPropertyChanging("PhoneNumber");
                    _phoneNumber = value;
                    NotifyPropertyChanged("PhoneNumber");
                }
            }
        }

        // Image

        // User-set properties
        private bool _ignore = false;
        [Column]
        public bool Ignore
        {
            get { return _ignore; }
            set
            {
                NotifyPropertyChanging("Ignore");
                _ignore = value;
                NotifyPropertyChanged("Ignore");
            }
        }

        // Helpers
        public string Address { get { return string.Join(", ", Street, City, PostalCode); } }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify that a property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify that a property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }

    [Table]
    public class TagItem : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private int _tagId;
        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int TagId
        {
            get { return _tagId; }
            set
            {
                if (_tagId != value)
                {
                    NotifyPropertyChanging("TagId");
                    _tagId = value;
                    NotifyPropertyChanged("TagId");
                }
            }
        }
        private string _tag;
        [Column]
        public string Tag
        {
            get { return _tag; }
            set
            {
                if (_tag != value)
                {
                    NotifyPropertyChanging("Tag");
                    _tag = value;
                    NotifyPropertyChanged("Tag");
                }
            }
        }


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify that a property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify that a property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
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

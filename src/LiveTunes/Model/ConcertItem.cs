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
        [Column(DbType = "NTEXT", UpdateCheck = UpdateCheck.Never)]
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
}
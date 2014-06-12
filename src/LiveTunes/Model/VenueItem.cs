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
}
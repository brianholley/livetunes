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
}
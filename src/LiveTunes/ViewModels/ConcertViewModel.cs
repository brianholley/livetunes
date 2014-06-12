using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;

namespace LiveTunes
{
    public class ConcertViewModel : INotifyPropertyChanged
    {
        private string _title;
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (value != _title)
                {
                    _title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }

        private string _artists;
        public string Artists
        {
            get
            {
                return _artists;
            }
            set
            {
                if (value != _artists)
                {
                    _artists = value;
                    NotifyPropertyChanged("Artists");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
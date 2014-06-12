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
}
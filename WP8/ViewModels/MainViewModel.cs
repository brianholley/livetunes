using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using Microsoft.Xna.Framework.Media;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Data.Linq;


namespace LiveTunes
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel(string dbConnectionString)
        {
            ConcertDBMutex = new Mutex(false /*initiallyOwned*/, "ConcertDB");
            _connectionString = dbConnectionString;

            this.MyConcerts = new ObservableCollection<ConcertItem>();
            this.AllConcerts = new ObservableCollection<ConcertItem>();
            this.Tonight = new ObservableCollection<ConcertItem>();
            this.Artists = new ObservableCollection<ArtistItem>();
        }

        #region Database
        
        private string _connectionString;
        public Mutex ConcertDBMutex { get; private set; }
        private ConcertDataContext _concertDB;
        public ConcertDataContext ConcertDB
        {
            get
            {
#if DEBUG
                ConcertDBMutex.ReleaseMutex();
                ConcertDBMutex.WaitOne();
#endif
                if (_concertDB == null)
                {
                    _concertDB = new ConcertDataContext(_connectionString);
                    if (_concertDB.DatabaseExists() == false)
                    {
                        _concertDB.CreateDatabase();
                        _concertDB.SubmitChanges();
                    }
                }
                
                return _concertDB;
            }
        }

        #endregion

        public ObservableCollection<ConcertItem> MyConcerts { get; private set; }
        public ObservableCollection<ConcertItem> AllConcerts { get; private set; }
        public ObservableCollection<ConcertItem> Tonight { get; private set; }

        public ObservableCollection<ArtistItem> Artists { get; private set; }

        #region Now playing tab data binding properties
        
        private Song _nowPlaying;
        public Song NowPlaying
        {
            get
            {
                return _nowPlaying;
            }
            set
            {
                if (value != _nowPlaying)
                {
                    _nowPlaying = value;
                    NotifyPropertyChanged("NowPlaying");
                }
            }
        }

        private ImageSource _nowPlayingArt;
        public ImageSource NowPlayingArt
        {
            get
            {
                return _nowPlayingArt;
            }
            set
            {
                if (value != _nowPlayingArt)
                {
                    _nowPlayingArt = value;
                    NotifyPropertyChanged("NowPlayingArt");
                }
            }
        }

        #endregion

        #region Loading data binding properties

        public bool IsLoadingAny { get { return IsLoadingMy || IsLoadingTonight || IsLoadingAll; } }

        private bool _isLoadingMy;
        public bool IsLoadingMy
        {
            get
            {
                return _isLoadingMy;
            }
            set
            {
                if (value != _isLoadingMy)
                {
                    _isLoadingMy = value;
                    NotifyPropertyChanged("IsLoadingMy");
                    NotifyPropertyChanged("IsLoadingAny");
                }
            }
        }

        private bool _isLoadingTonight;
        public bool IsLoadingTonight
        {
            get
            {
                return _isLoadingTonight;
            }
            set
            {
                if (value != _isLoadingTonight)
                {
                    _isLoadingTonight = value;
                    NotifyPropertyChanged("IsLoadingTonight");
                    NotifyPropertyChanged("IsLoadingAny");
                }
            }
        }

        private bool _isLoadingAll;
        public bool IsLoadingAll
        {
            get
            {
                return _isLoadingAll;
            }
            set
            {
                if (value != _isLoadingAll)
                {
                    _isLoadingAll = value;
                    NotifyPropertyChanged("IsLoadingAll");
                    NotifyPropertyChanged("IsLoadingAny");
                }
            }
        }
        
        private string _loadingString;
        public string LoadingString
        {
            get
            {
                return _loadingString;
            }
            set
            {
                if (value != _loadingString)
                {
                    _loadingString = value;
                    NotifyPropertyChanged("LoadingString");
                }
            }
        }

        #endregion

        #region My concerts tab data binding properties

        private string _myConcertsError;
        public string MyConcertsError
        {
            get
            {
                return _myConcertsError;
            }
            set
            {
                if (value != _myConcertsError)
                {
                    _myConcertsError = value;
                    NotifyPropertyChanged("MyConcertsError");
                }
            }
        }

        #endregion

        #region Tonight concerts tab data binding properties

        private string _tonightError;
        public string TonightError
        {
            get
            {
                return _tonightError;
            }
            set
            {
                if (value != _tonightError)
                {
                    _tonightError = value;
                    NotifyPropertyChanged("TonightError");
                }
            }
        }

        #endregion

        #region All concerts tab data binding properties

        private string _allConcertsError;
        public string AllConcertsError
        {
            get
            {
                return _allConcertsError;
            }
            set
            {
                if (value != _allConcertsError)
                {
                    _allConcertsError = value;
                    NotifyPropertyChanged("AllConcertsError");
                }
            }
        }

        #endregion

        #region Settings tag data binding properties

        public string LastSyncTime
        {
            get
            {
                if (AppCache.LastConcertListingSyncTime != default(DateTime))
                    return AppCache.LastConcertListingSyncTime.ToString();
                else
                    return "not synced yet";
            }
        }
        public string LastSyncPlace
        {
            get { return AppCache.LastConcertSyncPlace; }
            set
            {
                AppCache.LastConcertSyncPlace = value;
                NotifyPropertyChanged("LastSyncPlace");
            }
        }

        public int Distance
        {
            get { return Settings.Distance; }
            set
            {
                Settings.Distance = value;
                NotifyPropertyChanged("Distance");
            }
        }

        public DistanceUnits DistanceUnits
        {
            get { return Settings.Units; }
            set
            {
                if (value != Settings.Units)
                {
                    Settings.Units = value;
                    NotifyPropertyChanged("DistanceUnits");
                    NotifyPropertyChanged("DistanceUnitsString");
                }
            }
        }
        public string DistanceUnitsString
        {
            get { return (DistanceUnits == DistanceUnits.Miles ? "mi" : "km"); }
        }

        #endregion

        #region Concert detail page data binding properties

        // Currently viewing concert
        // TODO: This should move somewhere else long-term
        private ConcertItem _currentConcert;
        public ConcertItem CurrentConcert
        {
            get
            {
                return _currentConcert;
            }
            set
            {
                if (value != _currentConcert)
                {
                    _currentConcert = value;
                    NotifyPropertyChanged("CurrentConcert");
                }
            }
        }

        public ImageSource FavoriteButtonImage
        {
            get
            {
                if (CurrentConcert.Favorite)
                    return new BitmapImage(new Uri("Images/appbar.favs.remove.rest.png", UriKind.Relative));
                else
                    return new BitmapImage(new Uri("Images/appbar.favs.addto.rest.png", UriKind.Relative));
            }
        }

        public string FavoriteButtonText { get { return (CurrentConcert.Favorite ? "unfavorite" : "favorite"); } }

        public void NotifyFavoriteStatusChanged()
        {
            NotifyPropertyChanged("FavoriteButtonImage");
            NotifyPropertyChanged("FavoriteButtonText");
        }

        #endregion

        public bool IsDataLoaded { get; private set; }

        /// <summary>
        /// </summary>
        public void LoadData()
        {
            System.Diagnostics.Debug.WriteLine("{0} Starting main view load", DateTime.Now);

            // Get active song
            NowPlaying = MediaPlayer.Queue.ActiveSong;
            if (NowPlaying != null)
            {
                BitmapImage imageSource = new BitmapImage();
                imageSource.SetSource(NowPlaying.Album.GetAlbumArt());
                this.NowPlayingArt = imageSource;
            }

            // Start loading sequence
            IsLoadingMy = IsLoadingTonight = IsLoadingAll = true;
            LoadingString = Strings.Loading_Location;

            LoadConcertsFromDB();
            LocationService.GetLocation(OnLocation, OnLocationError);

            // TODO: Does this actually work?
            new Timer(GCCallback, null, 10000, Timeout.Infinite);
            
            this.IsDataLoaded = true;
            System.Diagnostics.Debug.WriteLine("{0} Finished with main view load", DateTime.Now);
        }

        private void LoadConcertsFromDB()
        {
            new Thread(LoadConcertsFromDBAsync).Start();
        }

        public void RefreshTonightList()
        {
            new Thread(RefreshTonightListAsync).Start();
        }

        public void RefreshMyConcertList()
        {
            new Thread(RefreshMyConcertListAsync).Start();
        }

        private void LoadConcertsFromDBAsync()
        {
            int allConcertCount = 0;
            {
                System.Diagnostics.Debug.WriteLine("Started all concerts count query at {0}", DateTime.Now);
                ConcertDBMutex.WaitOne();

	            allConcertCount = ConcertDB.Concerts.Count(c => !c.Cancelled && c.StartTime.Date >= DateTime.Today);

                ConcertDBMutex.ReleaseMutex();
                System.Diagnostics.Debug.WriteLine("Finished all concerts count query at {0}", DateTime.Now);
            }

            if (allConcertCount > 0)
            {
                RefreshMyConcertListAsync();
                RefreshTonightListAsync();

                // Load all the still-valid concerts from the DB into our views (old concerts may not have been GCed yet)
                List<ConcertItem> allConcerts;
                {
                    System.Diagnostics.Debug.WriteLine("Started all concerts query at {0}", DateTime.Now);
                    ConcertDBMutex.WaitOne();

					allConcerts = ConcertDB.Concerts.Where(c => !c.Cancelled && c.StartTime.Date >= DateTime.Today).OrderBy(c => c.Headliner.ArtistName).ToList();

                    ConcertDBMutex.ReleaseMutex();
                    System.Diagnostics.Debug.WriteLine("Finished all concerts query at {0}", DateTime.Now);
                }

                Microsoft.Phone.Reactive.Scheduler.Dispatcher.Dispatcher.BeginInvoke(() =>
                {
                    IsLoadingMy = IsLoadingTonight = IsLoadingAll = false;
                    AllConcerts.Clear();
                    foreach (var concert in allConcerts)
                        AllConcerts.Add(concert);
                });
            }
        }

        private void RefreshTonightListAsync()
        {
            List<ConcertItem> tonight;
            {
                System.Diagnostics.Debug.WriteLine("Started tonight query at {0}", DateTime.Now);
                ConcertDBMutex.WaitOne();
                
				tonight = ConcertDB.Concerts.Where(c => !c.Ignore && !c.Cancelled && c.StartTime.Date == DateTime.Today).OrderBy(c => c.StartTime).ToList();

                ConcertDBMutex.ReleaseMutex();
                System.Diagnostics.Debug.WriteLine("Finished tonight query at {0}", DateTime.Now);
            }

            Microsoft.Phone.Reactive.Scheduler.Dispatcher.Dispatcher.BeginInvoke(() =>
            {
                IsLoadingTonight = false;
                Tonight.Clear();
                foreach (var concert in tonight)
                    Tonight.Add(concert);

                TonightError = (Tonight.Count == 0 ? Strings.Notify_NoTonightConcerts : "");
            });
        }

        private void RefreshMyConcertListAsync()
        {
            List<ConcertItem> myConcerts;
            {
                System.Diagnostics.Debug.WriteLine("Started my concerts query at {0}", DateTime.Now);
                ConcertDBMutex.WaitOne();

                var results = (from concert in ConcertDB.Concerts
                               from concertartist in concert.Artists
                               where concert.Ignore == false && concert.Cancelled == false && concert.StartTime.Date >= DateTime.Now.Date &&
                               ((concert.Headliner.InLibrary || concert.Headliner.Favorite) || (concertartist.Artist.InLibrary || concertartist.Artist.Favorite) || concert.Favorite)
                               select concert).OrderBy(concert => concert.Headliner.ArtistName);
                myConcerts = new List<ConcertItem>(results);

                ConcertDBMutex.ReleaseMutex();
                System.Diagnostics.Debug.WriteLine("Finished my concerts query at {0}", DateTime.Now);
            }

            // For some reason the .Distinct() command isn't working when running the SQL query, so I'm going to have to do the uniquification separately
            myConcerts = new List<ConcertItem>(myConcerts.Distinct());

            Microsoft.Phone.Reactive.Scheduler.Dispatcher.Dispatcher.BeginInvoke(() =>
            {
                IsLoadingMy = false;
                MyConcerts.Clear();
                foreach (var concert in myConcerts)
                    MyConcerts.Add(concert);

                MyConcertsError = (MyConcerts.Count == 0 ? Strings.Notify_NoMyConcerts : "");
            });
        }

        private void GCCallback(object state)
        {
            System.Diagnostics.Debug.WriteLine("Running database GC at {0}", DateTime.Now);

            ConcertDBMutex.WaitOne();

            // Keep any that were from today (we don't want to GC them if they're in progress or the time was "doors open")
	        var oldConcerts = ConcertDB.Concerts.Where(c => c.StartTime.Date < DateTime.Today);
            if (oldConcerts.Any())
            {
                foreach (var concert in oldConcerts)
                {
                    System.Diagnostics.Debug.WriteLine("Deleting {0} with start time {1}", concert.ConcertId, concert.StartTime);
                    ConcertDB.Concerts.DeleteOnSubmit(concert);
                }
                ConcertDB.SubmitChanges();
            }

            ConcertDBMutex.ReleaseMutex();

            System.Diagnostics.Debug.WriteLine("Finished with database GC at {0}", DateTime.Now);
        }

        private void FillArtistDBFromLibrary()
        {
            System.Diagnostics.Debug.WriteLine("{0}: Started parsing artists", DateTime.Now);

            // Take the mutex so we can get the list of all artists to compare to
            List<ArtistItem> artistsInDB;
            {
                ConcertDBMutex.WaitOne();

                artistsInDB = ConcertDB.Artists.ToList();

                ConcertDBMutex.ReleaseMutex();
            }

            // Then walk the device media library without the mutex (can be slow)
            List<ArtistItem> localArtists = new List<ArtistItem>();
            var library = new MediaLibrary();
            if (library.Artists.Count > 0)
            {
                foreach (var artist in library.Artists)
                {
                    var artistInDB = artistsInDB.FirstOrDefault(a => a.ArtistName == artist.Name);

                    bool favorite = false;
                    if (artistInDB != null && !artistInDB.InLibrary)
                    {
                        foreach (var song in artist.Songs)
                        {
                            if (song.Rating >= 8)   // "liked" songs = 8, "disliked" = 2 or 3 (?), and not set = 0 (per MSDN Song.Rating)
                            {
                                favorite = true;
                                break;
                            }
                        }
                    }
                    if (artistInDB == null)
                    {
                        localArtists.Add(new ArtistItem() { ArtistName = artist.Name, InLibrary = true, Favorite = favorite });
                    }
                    else
                    {
                        artistInDB.InLibrary = true;
                        artistInDB.Favorite = artistInDB.Favorite || favorite;
                    }
                }

                // Then re-take the mutex to re-compare (in case we finished another sync or something) and do the DB commit
                {
                    ConcertDBMutex.WaitOne();

                    var allArtists = ConcertDB.Artists.ToDictionary(a => a.ArtistName);
					foreach (var localArtist in localArtists)
                    {
                        if (!allArtists.ContainsKey(localArtist.ArtistName))
                            ConcertDB.Artists.InsertOnSubmit(localArtist);
                    }
                    ConcertDB.SubmitChanges();

                    ConcertDBMutex.ReleaseMutex();
                }
            }

            System.Diagnostics.Debug.WriteLine("{0}: Finished parsing artists", DateTime.Now);

            RefreshMyConcertListAsync();
        }

        public bool SyncConcertListings(bool forceRecheck)
        {
            if ((DateTime.Now > AppCache.LastConcertListingSyncTime.Add(ConcertListings.MinRequeryTime) || forceRecheck) &&
                (DateTime.Now > AppCache.LastAttemptedSyncTime.Add(ConcertListings.MinRetryTime)))
            {
                IsLoadingMy = IsLoadingTonight = IsLoadingAll = true;
                LoadingString = Strings.Loading_Concerts;

                AppCache.LastAttemptedSyncTime = DateTime.Now;
                ConcertListings.GetListings(Settings.Location, OnConcertListings, OnConcertListingsError);
                return true;
            }
            else
            {
                LoadingString = Strings.Loading_Concerts;
            }
            return false;
        }

        private void OnLocation(Location loc)
        {
            bool forceResync = !AppCache.CompletedFirstRun;
            if (SyncConcertListings(forceResync))
            {
                IsLoadingMy = IsLoadingTonight = IsLoadingAll = true;
            }
            else
            {
                new Thread(FillArtistDBFromLibrary).Start();
            }
        }

        public void OnLocationError(int error, string message)
        {
            // If we already have a location, then we can (temporarily) ignore this error
            if (!Settings.Location.Empty)
            {
                OnLocation(Settings.Location);
            }
            else
            {
                IsLoadingMy = IsLoadingTonight = IsLoadingAll = false;

                string errorMsg = string.Format(Strings.Error_Location, message);
                MyConcertsError = errorMsg;
                TonightError = errorMsg;
                AllConcertsError = errorMsg;
            }
        }

        public void OnConcertListings(List<ConcertItem> concerts)
        {
            AppCache.LastConcertListingSyncTime = DateTime.Now;
            AppCache.CompletedFirstRun = true;
            NotifyPropertyChanged("LastSyncTime");

            LoadConcertsFromDB();

            new Thread(FillArtistDBFromLibrary).Start();
        }

        public void OnConcertListingsError(int error, string message)
        {
            IsLoadingMy = IsLoadingTonight = IsLoadingAll = false;

            if (AllConcerts.Count == 0)
            {
                string errorMsg = string.Format(Strings.Error_ConcertService, error, message);
                MyConcertsError = errorMsg;
                TonightError = errorMsg;
                AllConcertsError = errorMsg;
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
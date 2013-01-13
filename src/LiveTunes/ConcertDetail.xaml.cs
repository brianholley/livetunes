using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System.Device.Location;

namespace LiveTunes
{
    public partial class ConcertDetail : PhoneApplicationPage
    {
        public ConcertDetail()
        {
            InitializeComponent();

            DataContext = App.ViewModel;

            if (App.ViewModel.CurrentConcert == null)
            {
                App.ViewModel.ConcertDBMutex.WaitOne();

                var concerts = from ConcertItem concert in App.ViewModel.ConcertDB.Concerts 
                               where concert.ConcertId == AppCache.CurrentConcertId 
                               select concert;
                App.ViewModel.CurrentConcert = concerts.First();

                App.ViewModel.ConcertDBMutex.ReleaseMutex();
            }
        }

        private void PhoneApplicationPage_Unloaded(object sender, RoutedEventArgs e)
        {
            App.ViewModel.CurrentConcert = null;
            AppCache.CurrentConcertId = 0;
        }

        private void NavigateToVenueInMaps()
        {
            try
            {
                ConcertItem concert = App.ViewModel.CurrentConcert;

                BingMapsTask task = new BingMapsTask();
                if (concert.Venue.Latitude != 0 && concert.Venue.Longitude != 0)
                    task.Center = new GeoCoordinate(concert.Venue.Latitude, concert.Venue.Longitude);
                else
                    task.Center = new GeoCoordinate(Settings.Location.Lat, Settings.Location.Long);
                task.SearchTerm = concert.Venue.VenueName;
                task.Show();
            }
            catch (InvalidOperationException)
            {
                // User clicked more than once.  That's cool...
            }
        }

        private void LocationLink_Click(object sender, EventArgs e)
        {
            NavigateToVenueInMaps();
        }

        private void PhoneLink_Click(object sender, EventArgs e)
        {
            try
            {
                PhoneCallTask task = new PhoneCallTask()
                {
                    PhoneNumber = App.ViewModel.CurrentConcert.Venue.PhoneNumber, 
                    DisplayName = App.ViewModel.CurrentConcert.Venue.VenueName
                };
                task.Show();
            }
            catch (InvalidOperationException)
            {
                // User clicked more than once.  That's cool...
            }
        }
        
        private void MapButton_Tap(object sender, GestureEventArgs e)
        {
            NavigateToVenueInMaps();
        }
        
        private void FavoriteButton_Tap(object sender, GestureEventArgs e)
        {
            App.ViewModel.ConcertDBMutex.WaitOne();

            ConcertItem concert = App.ViewModel.CurrentConcert;
            concert.Favorite = !concert.Favorite;
            App.ViewModel.ConcertDB.SubmitChanges();

            App.ViewModel.ConcertDBMutex.ReleaseMutex();

            App.ViewModel.RefreshMyConcertList();
            App.ViewModel.NotifyFavoriteStatusChanged();
        }
        
        private void IgnoreButton_Tap(object sender, GestureEventArgs e)
        {
            App.ViewModel.ConcertDBMutex.WaitOne();

            ConcertItem concert = App.ViewModel.CurrentConcert;
            concert.Ignore = true;
            App.ViewModel.ConcertDB.SubmitChanges();

            App.ViewModel.ConcertDBMutex.ReleaseMutex();

            App.ViewModel.RefreshMyConcertList();
            App.ViewModel.RefreshTonightList();
            NavigationService.GoBack();
        }
    }
}
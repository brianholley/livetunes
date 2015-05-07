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
using System.Data.Linq;

namespace LiveTunes
{
    public partial class ArtistList : PhoneApplicationPage
    {
        public ArtistList()
        {
            InitializeComponent();

            DataContext = App.ViewModel;

            if (App.ViewModel.Artists.Count == 0)
            {
                App.ViewModel.ConcertDBMutex.WaitOne();

	            var artists = App.ViewModel.ConcertDB.Artists.OrderBy(a => a.ArtistName);
                foreach (var artist in artists)
                    App.ViewModel.Artists.Add(artist);

                App.ViewModel.ConcertDBMutex.ReleaseMutex();
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            ((VisualTreeHelper.GetChild(ArtistListBox, 0) as FrameworkElement).FindName("ScrollViewer") as ScrollViewer).ScrollToVerticalOffset(AppCache.ArtistListScrollPos);
            AppCache.ArtistListScrollPos = 0;
            TombstoningService.PushListBox(ArtistListBox, (double pos) => { AppCache.ArtistListScrollPos = pos; });
        }

        private void PhoneApplicationPage_Unloaded(object sender, RoutedEventArgs e)
        {
            // Remove the list box from the tombstoning service - this won't be called if we're shutting down
            TombstoningService.PopListBox(ArtistListBox);

            // Update the list of concerts on the parent
            App.ViewModel.RefreshMyConcertList();

            // Don't need the artist list loaded unless we're on this page
            App.ViewModel.Artists.Clear();
        }

        private void FavoriteButton_Tap(object sender, GestureEventArgs e)
        {
            App.ViewModel.ConcertDBMutex.WaitOne();

            ArtistItem selectedArtist = this.ArtistListBox.SelectedItem as ArtistItem;
            selectedArtist.Favorite = !selectedArtist.Favorite;
            App.ViewModel.ConcertDB.SubmitChanges();
            
            App.ViewModel.ConcertDBMutex.ReleaseMutex();
        }
    }
}
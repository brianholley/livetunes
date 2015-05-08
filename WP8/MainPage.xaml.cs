﻿#define USE_TEST_ADS

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace LiveTunes
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
			
            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
			this.Loaded += new RoutedEventHandler(MainPage_Loaded);

            Settings.Load();

#if DEBUG || USE_TEST_ADS
            this.MainAdControl.ApplicationId = "test_client";
            this.MainAdControl.AdUnitId = "Image480_80";
#else
            this.MainAdControl.ApplicationId = "";
            this.MainAdControl.AdUnitId="";
#endif

#if DEBUG
            this.DbgSyncButton.Visibility = Visibility.Visible;
#endif
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
	        NavigationService.RemoveBackEntry();

            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }

            // Add the main panorama to the tombstoning service
            if (AppCache.PanoramaPageIndex != 0)
                MainPivot.SelectedIndex = AppCache.PanoramaPageIndex;
            AppCache.PanoramaPageIndex = 0;
            TombstoningService.PushPivot(MainPivot, (int index) => { AppCache.PanoramaPageIndex = index; });

            // Add the all concerts list box to the tombstoning service - we never need to pop this (we'll always serialize that scroll pos)
            if (AppCache.AllConcertsListScrollPos != 0)
                ((VisualTreeHelper.GetChild(AllConcertsListSelector, 0) as FrameworkElement).FindName("ScrollViewer") as ScrollViewer).ScrollToVerticalOffset(AppCache.AllConcertsListScrollPos);
            AppCache.AllConcertsListScrollPos = 0;
			//TombstoningService.PushListBox(AllConcertsListSelector, (double pos) => { AppCache.AllConcertsListScrollPos = pos; });
        }

        private void ConcertListBox_Tap(object sender, GestureEventArgs e)
        {
			NavigateToConcert((sender as ListBox).SelectedItem as ConcertItem);
        }

		private void AllConcertsListSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
			{
				var concert = e.AddedItems[0] as ConcertItem;
				if (concert != null)
				{
					NavigateToConcert(concert);
					(sender as LongListSelector).SelectedItem = null;
				}
			}
		}

	    private void NavigateToConcert(ConcertItem concert)
	    {
		    App.ViewModel.CurrentConcert = concert;
			AppCache.CurrentConcertId = concert.ConcertId;

			NavigationService.Navigate(new Uri("/ConcertDetail.xaml", UriKind.Relative));
	    }

        private void LastFmLogo_Tap(object sender, GestureEventArgs e)
        {
            new WebBrowserTask() { Uri = new Uri("http://www.last.fm") }.Show();
        }

        private void RateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new MarketplaceDetailTask().Show();
            }
            catch (InvalidOperationException)
            {
                // User clicked more than once.  That's cool...
            }
        }

        private void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.SyncConcertListings(true);
            App.ViewModel.MyConcertsError = "";
            App.ViewModel.TonightError = "";
            App.ViewModel.AllConcertsError = "";
        }

        private void DbgSyncButton_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.SyncConcertListings(true);
        }

        private void ArtistButton_Click(object sender, RoutedEventArgs e)
        {
            AppCache.ArtistListScrollPos = 0;
            NavigationService.Navigate(new Uri("/ArtistList.xaml", UriKind.Relative));
        }

        private void UnitsButton_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.DistanceUnits = (Settings.Units == DistanceUnits.Miles ? DistanceUnits.Kilometers : DistanceUnits.Miles);
        }

        private void DistanceTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox unitsBox = sender as TextBox;
            try
            {
                string v = unitsBox.Text;
                foreach (char c in " -.+*#".ToArray())
                    v = v.Replace(c.ToString(), "");

                if (v == "")
                    App.ViewModel.Distance = Settings.DefaultDistance;
                else
                    App.ViewModel.Distance = int.Parse(v);
            }
            catch (FormatException)
            {
                // Entered numbers didn't parse after we tried to clear it up - can't do much about it, keep the old distance value
                App.ViewModel.Distance = Settings.Distance;
            }
            catch (OverflowException)
            {
                // User entered too large a number.  Trying to break the app = grrrr.
                App.ViewModel.Distance = Settings.Distance;
            }
        }
    }
}
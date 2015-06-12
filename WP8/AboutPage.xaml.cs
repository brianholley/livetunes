using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace LiveTunes
{
	public partial class AboutPage : PhoneApplicationPage
	{
		public AboutPage()
		{
			InitializeComponent();
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
	}
}
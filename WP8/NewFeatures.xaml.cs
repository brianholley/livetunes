using System;
using System.Windows;
using Microsoft.Phone.Controls;

namespace LiveTunes
{
	public partial class NewFeatures : PhoneApplicationPage
	{
		public NewFeatures()
		{
			InitializeComponent();
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			AppCache.CompletedFirstRun = true;
		}
	}
}
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace LiveTunes
{
	public partial class SettingsPage : PhoneApplicationPage
	{
		public SettingsPage()
		{
			InitializeComponent();
			DataContext = App.ViewModel;
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
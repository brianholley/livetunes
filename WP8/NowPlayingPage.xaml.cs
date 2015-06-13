using Microsoft.Phone.Controls;

namespace LiveTunes
{
	public partial class NowPlayingPage : PhoneApplicationPage
	{
		public NowPlayingPage()
		{
			InitializeComponent();
			this.DataContext = App.ViewModel;
		}
	}
}
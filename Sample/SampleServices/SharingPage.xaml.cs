using System;
using Xamarin.Forms;
using Xamarin.Services.Sharing;

namespace SampleServices
{
	public partial class SharingPage : TabbedPage
	{
		public SharingPage()
		{
			InitializeComponent();
		}

		private void Share_Clicked(object sender, EventArgs e)
		{
			var sharing = new Xamarin.Services.Sharing.SharingService();
			sharing.Share(new ShareMessage
			{
				Title = titleText.Text,
				Text = bodyText.Text,
				Url = urlText.Text
			});
		}

		private void OpenBrowser_Clicked(object sender, EventArgs e)
		{
			var sharing = new Xamarin.Services.Sharing.SharingService();
			if (sharing.CanOpenUrl(browserText.Text))
			{
				sharing.OpenBrowser(browserText.Text);
			}
		}

		private void Copy_Clicked(object sender, EventArgs e)
		{
			var sharing = new Xamarin.Services.Sharing.SharingService();
			if (sharing.SupportsClipboard)
			{
				sharing.SetClipboardText(clipboardText.Text);
			}
		}
	}
}

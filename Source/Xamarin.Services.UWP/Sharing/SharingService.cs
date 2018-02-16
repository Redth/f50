using Xamarin.Services.Sharing;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;

namespace Xamarin.Services.Sharing
{
	public partial class SharingService
	{
		string title, text, url;
		DataTransferManager dataTransferManager;

		public async Task<bool> OpenBrowser(string url, BrowserOptions options = null)
		{
			try
			{
				await Windows.System.Launcher.LaunchUriAsync(new Uri(url));
				return true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Unable to open browser: " + ex.Message);
				return false;
			}
		}

		public Task<bool> Share(ShareMessage message, ShareOptions options = null)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			try
			{
				title = message.Title;
				text = message.Text;
				url = message.Url;
				if (dataTransferManager == null)
				{
					dataTransferManager = DataTransferManager.GetForCurrentView();
					dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(this.ShareTextHandler);
				}
				DataTransferManager.ShowShareUI();

				return Task.FromResult(true);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Unable to share: " + ex.Message);
				return Task.FromResult(false);
			}
		}

		private void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e)
		{
			try
			{
				DataRequest request = e.Request;

				// The Title is mandatory
				request.Data.Properties.Title = title ?? Windows.ApplicationModel.Package.Current.DisplayName;

				if (text != null)
					request.Data.SetText(text);
				if (url != null)
					request.Data.SetWebLink(new Uri(url));
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Unable to share: " + ex.Message);
			}
		}

		public Task<bool> SetClipboardText(string text, string label = null)
		{
			try
			{
				var dataPackage = new DataPackage();
				dataPackage.RequestedOperation = DataPackageOperation.Copy;
				dataPackage.SetText(text);

				Clipboard.SetContent(dataPackage);

				return Task.FromResult(true);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Unable to copy to clipboard: " + ex.Message);
				return Task.FromResult(false);
			}
		}

		public bool CanOpenUrl(string url) => true;

		public bool SupportsClipboard => true;
	}
}

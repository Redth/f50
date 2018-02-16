using System.Threading.Tasks;

namespace Xamarin.Services.Sharing
{
	public partial class SharingService
	{
		public bool SupportsClipboard => throw new System.NotImplementedException();

		public bool CanOpenUrl(string url)
		{
			throw new System.NotImplementedException();
		}

		public Task<bool> OpenBrowser(string url, BrowserOptions options = null)
		{
			throw new System.NotImplementedException();
		}

		public Task<bool> SetClipboardText(string text, string label = null)
		{
			throw new System.NotImplementedException();
		}

		public Task<bool> Share(ShareMessage message, ShareOptions options = null)
		{
			throw new System.NotImplementedException();
		}
	}
}

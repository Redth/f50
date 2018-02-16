using System.Threading.Tasks;

namespace Xamarin.Services.Sharing
{
	public interface ISharingService
	{
		Task<bool> OpenBrowser(string url, BrowserOptions options = null);

		bool CanOpenUrl(string url);

		Task<bool> Share(ShareMessage message, ShareOptions options = null);

		Task<bool> SetClipboardText(string text, string label = null);

		bool SupportsClipboard { get; }
	}
}

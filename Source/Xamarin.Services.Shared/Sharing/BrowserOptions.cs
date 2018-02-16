namespace Xamarin.Services.Sharing
{
	public class BrowserOptions
	{
		public bool UseSafariWebViewController { get; set; } = true;

		public bool UseSafariReaderMode { get; set; } = false;

		public ShareColor SafariBarTintColor { get; set; } = null;

		public ShareColor SafariControlTintColor { get; set; } = null;

		public bool ChromeShowTitle { get; set; } = true;

		public ShareColor ChromeToolbarColor { get; set; } = null;
	}
}

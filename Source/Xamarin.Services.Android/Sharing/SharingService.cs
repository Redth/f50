using Android.Content;
using Android.Support.CustomTabs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.Services.Sharing
{
	public partial class SharingService
	{
		public Task<bool> OpenBrowser(string url, BrowserOptions options = null)
		{
			try
			{
				if (options == null)
					options = new BrowserOptions();

				if (Platform.CurrentActivity == null)
				{
					var intent = new Intent(Intent.ActionView);
					intent.SetData(global::Android.Net.Uri.Parse(url));

					intent.SetFlags(ActivityFlags.ClearTop);
					intent.SetFlags(ActivityFlags.NewTask);
					Platform.CurrentContext.StartActivity(intent);
				}
				else
				{
					var tabsBuilder = new CustomTabsIntent.Builder();
					tabsBuilder.SetShowTitle(options?.ChromeShowTitle ?? false);

					var toolbarColor = options?.ChromeToolbarColor;
					if (toolbarColor != null)
						tabsBuilder.SetToolbarColor(toolbarColor.ToNativeColor());

					var intent = tabsBuilder.Build();
					intent.LaunchUrl(Platform.CurrentActivity, global::Android.Net.Uri.Parse(url));
				}

				return Task.FromResult(true);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unable to open browser: " + ex.Message);
				return Task.FromResult(false);
			}
		}

		public Task<bool> Share(ShareMessage message, ShareOptions options = null)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			try
			{
				var items = new List<string>();
				if (message.Text != null)
					items.Add(message.Text);
				if (message.Url != null)
					items.Add(message.Url);

				var intent = new Intent(Intent.ActionSend);
				intent.SetType("text/plain");
				intent.PutExtra(Intent.ExtraText, string.Join(Environment.NewLine, items));
				if (message.Title != null)
					intent.PutExtra(Intent.ExtraSubject, message.Title);

				var chooserIntent = Intent.CreateChooser(intent, options?.ChooserTitle);
				chooserIntent.SetFlags(ActivityFlags.ClearTop);
				chooserIntent.SetFlags(ActivityFlags.NewTask);
				Platform.CurrentContext.StartActivity(chooserIntent);

				return Task.FromResult(true);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unable to share: " + ex.Message);
				return Task.FromResult(false);
			}
		}

		public Task<bool> SetClipboardText(string text, string label = null)
		{
			try
			{
				var sdk = (int)global::Android.OS.Build.VERSION.SdkInt;
				if (sdk < (int)global::Android.OS.BuildVersionCodes.Honeycomb)
				{
					var clipboard = (global::Android.Text.ClipboardManager)Platform.CurrentContext.GetSystemService(Context.ClipboardService);
					clipboard.Text = text;
				}
				else
				{
					var clipboard = (ClipboardManager)Platform.CurrentContext.GetSystemService(Context.ClipboardService);
					clipboard.PrimaryClip = ClipData.NewPlainText(label ?? string.Empty, text);
				}

				return Task.FromResult(true);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unable to copy to clipboard: " + ex.Message);
				return Task.FromResult(false);
			}
		}

		public bool CanOpenUrl(string url)
		{
			try
			{
				var context = Platform.CurrentContext;
				var intent = new Intent(Intent.ActionView);
				intent.SetData(global::Android.Net.Uri.Parse(url));

				intent.SetFlags(ActivityFlags.ClearTop);
				intent.SetFlags(ActivityFlags.NewTask);
				return intent.ResolveActivity(context.PackageManager) != null;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		public bool SupportsClipboard => true;
	}
}

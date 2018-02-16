using CoreGraphics;
using Foundation;
using Xamarin.Services.Sharing;
using SafariServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UIKit;

namespace Xamarin.Services.Sharing
{
	public partial class SharingService
	{
		public static List<NSString> ExcludedUIActivityTypes { get; set; } = new List<NSString> { UIActivityType.PostToFacebook };

		public async Task<bool> OpenBrowser(string url, BrowserOptions options = null)
		{
			try
			{
				if (options == null)
					options = new BrowserOptions();

				if ((options?.UseSafariWebViewController ?? false) && UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
				{
					// create safari controller
					var sfViewController = new SFSafariViewController(new NSUrl(url), options?.UseSafariReaderMode ?? false);

					// apply custom tint colors
					if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
					{
						var barTintColor = options?.SafariBarTintColor;
						if (barTintColor != null)
							sfViewController.PreferredBarTintColor = barTintColor.ToUIColor();

						var controlTintColor = options?.SafariControlTintColor;
						if (controlTintColor != null)
							sfViewController.PreferredControlTintColor = controlTintColor.ToUIColor();
					}

					// show safari controller
					var vc = GetVisibleViewController();

					if (sfViewController.PopoverPresentationController != null)
					{
						sfViewController.PopoverPresentationController.SourceView = vc.View;
					}

					await vc.PresentViewControllerAsync(sfViewController, true);
				}
				else
				{
					UIApplication.SharedApplication.OpenUrl(new NSUrl(url));
				}

				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unable to open browser: " + ex.Message);
				return false;
			}
		}

		public Task<bool> Share(ShareMessage message, ShareOptions options = null)
		{
			return Share(message, options, null);
		}

		private async Task<bool> Share(ShareMessage message, ShareOptions options = null, params NSString[] excludedActivityTypes)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			try
			{
				// create activity items
				var items = new List<NSObject>();
				if (message.Text != null)
					items.Add(new ShareActivityItemSource(new NSString(message.Text), message.Title));
				if (message.Url != null)
					items.Add(new ShareActivityItemSource(NSUrl.FromString(message.Url), message.Title));

				// create activity controller
				var activityController = new UIActivityViewController(items.ToArray(), null);

				// set excluded activity types
				if (excludedActivityTypes == null)
					// use ShareOptions.ExcludedUIActivityTypes
					excludedActivityTypes = options?.ExcludedUIActivityTypes?.Select(x => GetUIActivityType(x)).Where(x => x != null).ToArray();

				if (excludedActivityTypes == null)
					// use ShareImplementation.ExcludedUIActivityTypes
					excludedActivityTypes = ExcludedUIActivityTypes?.ToArray();

				if (excludedActivityTypes != null && excludedActivityTypes.Length > 0)
					activityController.ExcludedActivityTypes = excludedActivityTypes;

				// show activity controller
				var vc = GetVisibleViewController();

				if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
				{
					if (activityController.PopoverPresentationController != null)
					{
						activityController.PopoverPresentationController.SourceView = vc.View;

						var rect = options?.PopoverAnchorRectangle;
						if (rect != null)
						{
							activityController.PopoverPresentationController.SourceRect = new CGRect(rect.X, rect.Y, rect.Width, rect.Height);
						}
					}
				}

				await vc.PresentViewControllerAsync(activityController, true);

				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unable to share: " + ex.Message);
				return false;
			}
		}

		UIViewController GetVisibleViewController()
		{
			UIViewController viewController = null;
			var window = UIApplication.SharedApplication.KeyWindow;

			if (window != null && window.WindowLevel == UIWindowLevel.Normal)
				viewController = window.RootViewController;

			if (viewController == null)
			{
				window = UIApplication.SharedApplication.Windows.OrderByDescending(w => w.WindowLevel).FirstOrDefault(w => w.RootViewController != null && w.WindowLevel == UIWindowLevel.Normal);
				if (window == null)
					throw new InvalidOperationException("Could not find current view controller");
				else
					viewController = window.RootViewController;
			}

			while (viewController.PresentedViewController != null)
				viewController = viewController.PresentedViewController;

			return viewController;
		}

		NSString GetUIActivityType(ShareUIActivityType type)
		{
			switch (type)
			{
				case ShareUIActivityType.AssignToContact:
					return UIActivityType.AssignToContact;
				case ShareUIActivityType.CopyToPasteboard:
					return UIActivityType.CopyToPasteboard;
				case ShareUIActivityType.Mail:
					return UIActivityType.Mail;
				case ShareUIActivityType.Message:
					return UIActivityType.Message;
				case ShareUIActivityType.PostToFacebook:
					return UIActivityType.PostToFacebook;
				case ShareUIActivityType.PostToTwitter:
					return UIActivityType.PostToTwitter;
				case ShareUIActivityType.PostToWeibo:
					return UIActivityType.PostToWeibo;
				case ShareUIActivityType.Print:
					return UIActivityType.Print;
				case ShareUIActivityType.SaveToCameraRoll:
					return UIActivityType.SaveToCameraRoll;

				case ShareUIActivityType.AddToReadingList:
					return UIDevice.CurrentDevice.CheckSystemVersion(7, 0) ? UIActivityType.AddToReadingList : null;
				case ShareUIActivityType.AirDrop:
					return UIDevice.CurrentDevice.CheckSystemVersion(7, 0) ? UIActivityType.AirDrop : null;
				case ShareUIActivityType.PostToFlickr:
					return UIDevice.CurrentDevice.CheckSystemVersion(7, 0) ? UIActivityType.PostToFlickr : null;
				case ShareUIActivityType.PostToTencentWeibo:
					return UIDevice.CurrentDevice.CheckSystemVersion(7, 0) ? UIActivityType.PostToTencentWeibo : null;
				case ShareUIActivityType.PostToVimeo:
					return UIDevice.CurrentDevice.CheckSystemVersion(7, 0) ? UIActivityType.PostToVimeo : null;

				case ShareUIActivityType.OpenInIBooks:
					return UIDevice.CurrentDevice.CheckSystemVersion(9, 0) ? UIActivityType.OpenInIBooks : null;

				default:
					return null;
			}
		}

		public Task<bool> SetClipboardText(string text, string label = null)
		{
			try
			{
				UIPasteboard.General.String = text;

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
				return UIApplication.SharedApplication.OpenUrl(NSUrl.FromString(url));
			}
			catch
			{
				return false;
			}
		}

		public bool SupportsClipboard => true;
	}
}

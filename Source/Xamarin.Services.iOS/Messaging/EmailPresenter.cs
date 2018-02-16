using System;
using MessageUI;
using UIKit;

namespace Xamarin.Services.Messaging
{
	public class EmailPresenter : IEmailPresenter
	{
		public void PresentMailComposeViewController(MFMailComposeViewController mailController)
		{
			void handler(object sender, MFComposeResultEventArgs e)
			{
				mailController.Finished -= handler;

				var uiViewController = sender as UIViewController;
				if (uiViewController == null)
				{
					throw new ArgumentException("sender");
				}

				uiViewController.DismissViewController(true, () => { });
			}

			mailController.Finished += handler;
			mailController.PresentUsingRootViewController();
		}
	}
}

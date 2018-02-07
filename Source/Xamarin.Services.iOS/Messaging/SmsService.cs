using System;
using MessageUI;
using UIKit;

namespace Xamarin.Services.Messaging
{
	public partial class SmsService
	{
		private MFMessageComposeViewController smsController;

		public bool CanSendSms => MFMessageComposeViewController.CanSendText;

		public bool CanSendSmsInBackground => false;

		public void SendSms(string recipient = null, string message = null)
		{
			message = message ?? string.Empty;

			if (CanSendSms)
			{
				smsController = new MFMessageComposeViewController();

				if (!string.IsNullOrWhiteSpace(recipient))
				{
					string[] recipients = recipient.Split(';');
					if (recipients.Length > 0)
						smsController.Recipients = recipients;
				}

				smsController.Body = message;

				void handler(object sender, MFMessageComposeResultEventArgs args)
				{
					smsController.Finished -= handler;

					if (!(sender is UIViewController uiViewController))
					{
						throw new ArgumentException("sender");
					}

					uiViewController.DismissViewController(true, () => { });
				}

				smsController.Finished += handler;

				smsController.PresentUsingRootViewController();
			}
		}

		public void SendSmsInBackground(string recipient, string message = null)
		{
			throw new PlatformNotSupportedException("Sending SMS in background not supported on iOS");
		}
	}
}

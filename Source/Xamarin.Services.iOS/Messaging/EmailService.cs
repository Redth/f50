using System;
using System.Linq;
using Foundation;
using MessageUI;

namespace Xamarin.Services.Messaging
{
	public partial class EmailService
	{
		private readonly EmailSettings settings;

		public EmailService()
		{
			settings = new EmailSettings();
		}

		public EmailService(EmailSettings settings)
		{
			this.settings = settings;
		}

		public bool CanSendEmail => MFMailComposeViewController.CanSendMail;

		public bool CanSendEmailAttachments => true;

		public bool CanSendEmailBodyAsHtml => true;

		public void SendEmail(EmailMessage email)
		{
			if (email == null)
				throw new ArgumentNullException(nameof(email));

			if (CanSendEmail)
			{
				var mailController = new MFMailComposeViewController();
				mailController.SetSubject(email.Subject);
				mailController.SetMessageBody(email.Message, ((EmailMessage)email).IsHtml);
				mailController.SetToRecipients(email.Recipients.ToArray());

				if (email.RecipientsCc.Count > 0)
					mailController.SetCcRecipients(email.RecipientsCc.ToArray());

				if (email.RecipientsBcc.Count > 0)
					mailController.SetBccRecipients(email.RecipientsBcc.ToArray());

				foreach (var attachment in email.Attachments.Cast<EmailAttachment>())
				{
					if (attachment.File == null)
						mailController.AddAttachmentData(NSData.FromFile(attachment.FilePath), attachment.ContentType, attachment.FileName);
					else
						mailController.AddAttachmentData(NSData.FromUrl(attachment.File), attachment.ContentType, attachment.FileName);
				}

				settings.EmailPresenter.PresentMailComposeViewController(mailController);
			}
		}
	}
}

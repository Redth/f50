namespace Xamarin.Services.Messaging
{
#if INCLUDE_INTERFACES
	public interface IEmailService
	{
		bool CanSendEmail { get; }

		bool CanSendEmailAttachments { get; }

		bool CanSendEmailBodyAsHtml { get; }

		void SendEmail(EmailMessage email);

		void SendEmail(string to = null, string subject = null, string message = null);
	}
#endif
}

namespace Xamarin.Services.Messaging
{
	public partial class EmailService
#if INCLUDE_INTERFACES
		: IEmailService
#endif
	{
		public void SendEmail(string to, string subject, string message)
		{
			SendEmail(new EmailMessage(to, subject, message));
		}
	}
}

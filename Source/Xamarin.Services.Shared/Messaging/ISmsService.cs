namespace Xamarin.Services.Messaging
{
#if INCLUDE_INTERFACES
	public interface ISmsService
	{
		bool CanSendSms { get; }

		bool CanSendSmsInBackground { get; }

		void SendSms(string recipient = null, string message = null);

		void SendSmsInBackground(string recipient, string message = null);
	}
#endif
}

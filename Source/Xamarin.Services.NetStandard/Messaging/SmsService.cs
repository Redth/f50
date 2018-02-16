namespace Xamarin.Services.Messaging
{
	public partial class SmsService
	{
		public bool CanSendSms => throw new System.NotImplementedException();

		public bool CanSendSmsInBackground => throw new System.NotImplementedException();

		public void SendSms(string recipient = null, string message = null)
		{
			throw new System.NotImplementedException();
		}

		public void SendSmsInBackground(string recipient, string message = null)
		{
			throw new System.NotImplementedException();
		}
	}
}

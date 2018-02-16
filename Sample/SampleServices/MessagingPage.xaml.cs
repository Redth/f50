using System;
using Xamarin.Forms;

namespace SampleServices
{
	public partial class MessagingPage : TabbedPage
	{
		public MessagingPage()
		{
			InitializeComponent();
		}

		private void SendEmail_Clicked(object sender, EventArgs e)
		{
			var emails = new Xamarin.Services.Messaging.EmailService();
			if (emails.CanSendEmail)
			{
				emails.SendEmail(emailTo.Text, emailSubject.Text, emailMessage.Text);
			}
		}

		private void SendSms_Clicked(object sender, EventArgs e)
		{
			var sms = new Xamarin.Services.Messaging.SmsService();
			if (sms.CanSendSms)
			{
				sms.SendSms(smsTo.Text, smsMessage.Text);
			}
		}

		private void CallTel_Clicked(object sender, EventArgs e)
		{
			var phone = new Xamarin.Services.Messaging.PhoneDialerService();
			if (phone.CanMakePhoneCall)
			{
				phone.MakePhoneCall(telNumber.Text);
			}
		}
	}
}

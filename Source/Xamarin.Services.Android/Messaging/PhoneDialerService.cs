using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Telephony;
using Uri = Android.Net.Uri;

namespace Xamarin.Services.Messaging
{
	public partial class PhoneDialerService
	{
		private readonly PhoneDialerSettings settings;

		public PhoneDialerService()
		{
			settings = new PhoneDialerSettings();
		}

		public PhoneDialerService(PhoneDialerSettings settings)
		{
			this.settings = settings;
		}

		public bool CanMakePhoneCall
		{
			get
			{
				var packageManager = Application.Context.PackageManager;
				var dialIntent = ResolveDialIntent("0000000000");

				return null != dialIntent.ResolveActivity(packageManager);
			}
		}

		public void MakePhoneCall(string number, string name = null)
		{
			if (string.IsNullOrWhiteSpace(number))
				throw new ArgumentException(nameof(number));

			if (CanMakePhoneCall)
			{
				string phoneNumber = number;
				if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
				{
#pragma warning disable CS0618
					phoneNumber = PhoneNumberUtils.FormatNumber(number);
#pragma warning restore CS0618
				}
				else
				{
					if (!string.IsNullOrEmpty(settings.DefaultCountryIso))
						phoneNumber = PhoneNumberUtils.FormatNumber(number, settings.DefaultCountryIso);
				}

				var dialIntent = ResolveDialIntent(phoneNumber);
				dialIntent.StartNewTopMostActivity();
			}
		}

		private Intent ResolveDialIntent(string phoneNumber)
		{
			string dialIntent = settings.AutoDial ? Intent.ActionCall : Intent.ActionDial;

			Uri telUri = Uri.Parse("tel:" + phoneNumber);
			return new Intent(dialIntent, telUri);
		}
	}
}

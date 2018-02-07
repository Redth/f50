using System;
using Windows.ApplicationModel.Calls;
using Windows.Foundation.Metadata;

namespace Xamarin.Services.Messaging
{
	public partial class PhoneDialerService
	{
		public bool CanMakePhoneCall => ApiInformation.IsTypePresent("Windows.ApplicationModel.Calls.PhoneCallManager");

		public void MakePhoneCall(string number, string name = null)
		{
			if (string.IsNullOrWhiteSpace(number))
				throw new ArgumentNullException(nameof(number));

			if (CanMakePhoneCall)
				PhoneCallManager.ShowPhoneCallUI(number, name ?? "");
		}
	}
}

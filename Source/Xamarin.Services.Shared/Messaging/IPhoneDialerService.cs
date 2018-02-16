namespace Xamarin.Services.Messaging
{
#if INCLUDE_INTERFACES
	public interface IPhoneDialerService
	{
		bool CanMakePhoneCall { get; }

		void MakePhoneCall(string number, string name = null);
	}
#endif
}

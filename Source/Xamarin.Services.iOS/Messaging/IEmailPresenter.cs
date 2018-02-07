using MessageUI;

namespace Xamarin.Services.Messaging
{
	public interface IEmailPresenter
	{
		void PresentMailComposeViewController(MFMailComposeViewController mailController);
	}
}

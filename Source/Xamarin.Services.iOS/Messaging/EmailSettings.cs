namespace Xamarin.Services.Messaging
{
	public class EmailSettings
	{
		public IEmailPresenter EmailPresenter { get; set; } = new EmailPresenter();
	}
}

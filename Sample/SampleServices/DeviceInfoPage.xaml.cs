using Xamarin.Forms;
using Xamarin.Services.DeviceInfo;

namespace SampleServices
{
	public partial class DeviceInfoPage : ContentPage
	{
		public DeviceInfoPage()
		{
			InitializeComponent();

			BindingContext = new Xamarin.Services.DeviceInfo.DeviceInfoService();
		}
	}
}

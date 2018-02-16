using System;
using Xamarin.Forms;

namespace SampleServices
{
	public partial class GeolocationPage : ContentPage
	{
		public GeolocationPage()
		{
			InitializeComponent();
		}

		private async void Location_Clicked(object sender, EventArgs e)
		{
			var geo = new Xamarin.Services.Geolocation.GeolocationService();
			var pos = await geo.GetPositionAsync(TimeSpan.FromSeconds(60));

			if (pos != null)
			{
				labelLocation.Text = $"{pos.Latitude}, {pos.Longitude} @ {pos.Timestamp}";
			}
		}
	}
}

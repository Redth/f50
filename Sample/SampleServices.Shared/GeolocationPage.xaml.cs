using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace SampleServices
{
    public partial class GeolocationPage : ContentPage
    {
        public GeolocationPage ()
        {
            InitializeComponent ();
        }

        async void Location_Clicked (object sender, System.EventArgs e)
        {
            var geo = new Xamarin.Services.Geolocation.GeolocationService ();
            var pos = await geo.GetPositionAsync (TimeSpan.FromSeconds(60));

            if (pos != null)
                labelLocation.Text = $"{pos.Latitude}, {pos.Longitude} @ {pos.Timestamp}";
        }
    }
}

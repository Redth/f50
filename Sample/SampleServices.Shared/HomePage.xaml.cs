using Xamarin.Forms;

namespace SampleServices
{
    public partial class HomePage : ContentPage
    {
        public HomePage ()
        {
            InitializeComponent ();
        }

        async void TextToSpeech_Clicked (object sender, System.EventArgs e)
        {
            await Navigation.PushAsync (new TextToSpeechPage ());
        }

        async void Geolocation_Clicked (object sender, System.EventArgs e)
        {
            await Navigation.PushAsync (new GeolocationPage ());
        }
    }
}

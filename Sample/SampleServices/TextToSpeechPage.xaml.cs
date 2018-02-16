using System;
using Xamarin.Forms;

namespace SampleServices
{
	public partial class TextToSpeechPage : ContentPage
	{
		public TextToSpeechPage()
		{
			InitializeComponent();
		}

		private async void Speak_Clicked(object sender, EventArgs e)
		{
			var tts = new Xamarin.Services.TextToSpeech.TextToSpeechService();
			await tts.SpeakAsync(textSpeak.Text);
		}
	}
}

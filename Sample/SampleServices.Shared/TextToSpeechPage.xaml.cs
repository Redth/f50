using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace SampleServices
{
    public partial class TextToSpeechPage : ContentPage
    {
        public TextToSpeechPage ()
        {
            InitializeComponent ();
        }

        async void Speak_Clicked (object sender, System.EventArgs e)
        {
            var tts = new Xamarin.Services.TextToSpeech.TextToSpeechService();
            await tts.Speak (textSpeak.Text);
        }
    }
}

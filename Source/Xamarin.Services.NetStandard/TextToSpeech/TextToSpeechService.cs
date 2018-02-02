using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Services.TextToSpeech
{
    public class TextToSpeechService
#if DEBUG
        : ITextToSpeechService
#endif
    {
        /// <summary>
        /// Default contstructor. Creates new AVSpeechSynthesizer
        /// </summary>
        public TextToSpeechService() => throw new NotImplementedException();

        /// <summary>
        /// Speak back text
        /// </summary>
        /// <param name="text">Text to speak</param>
        /// <param name="locale">Locale of voice</param>
        /// <param name="pitch">Pitch of voice</param>
        /// <param name="speakRate">Speak Rate of voice (All) (0.0 - 2.0f)</param>
        /// <param name="volume">Volume of voice (iOS/WP) (0.0-1.0)</param>
        /// <param name="cancelToken">Canelation token to stop speak</param>
        /// <exception cref="ArgumentNullException">Thrown if text is null</exception>
        /// <exception cref="ArgumentException">Thrown if text length is greater than maximum allowed</exception>
        public async Task SpeakAsync(string text, Locale? locale = null, float? pitch = null, float? speakRate = null, float? volume = null, CancellationToken cancelToken = default(CancellationToken)) => throw new NotImplementedException();

        /// <summary>
        /// Get all installed and valid languages
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<Locale>> GetInstalledLanguagesAsync() => throw new NotImplementedException();

        /// <summary>
        /// Gets the max string length of the speech engine
        /// -1 means no limit
        /// </summary>
        public int MaxSpeechInputLength => throw new NotImplementedException();

        /// <summary>
        /// Dispose of TTS
        /// </summary>
        public void Dispose() => throw new NotImplementedException();
    }
}

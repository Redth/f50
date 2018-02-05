using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Services
{
#if !EXCLUDE_INTERFACES
    public interface ITextToSpeechService : IDisposable
    {
        /// <summary>
        /// Speak back text
        /// </summary>
        /// <param name="text">Text to speak</param>
        /// <param name="crossLocale">Locale of voice</param>
        /// <param name="pitch">Pitch of voice</param>
        /// <param name="speakRate">Speak Rate of voice (All) (0.0 - 2.0f)</param>
        /// <param name="volume">Volume of voice (iOS/WP) (0.0-1.0)</param>
        /// <param name="cancelToken">Canelation token to stop speak</param> 
        /// <exception cref="ArgumentNullException">Thrown if text is null</exception>
        /// <exception cref="ArgumentException">Thrown if text length is greater than maximum allowed</exception>
        Task SpeakAsync (string text, Locale? crossLocale = null, float? pitch = null, float? speakRate = null, float? volume = null, CancellationToken cancelToken = default (CancellationToken));

        /// <summary>
        /// Get avalid list of installed languages for TTS
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Locale>> GetInstalledLanguagesAsync ();

        /// <summary>
        /// Gets the max string length of the speech engine
        /// -1 means no limit
        /// </summary>
        int MaxSpeechInputLength { get; }
    }
#endif
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Services.TextToSpeech
{
#if INCLUDE_INTERFACES
	public interface ITextToSpeechService
	{
		Task SpeakAsync(string text, Locale? crossLocale = null, float? pitch = null, float? speakRate = null, float? volume = null, CancellationToken cancelToken = default(CancellationToken));

		Task<IEnumerable<Locale>> GetInstalledLanguagesAsync();

		int MaxSpeechInputLength { get; }
	}
#endif
}

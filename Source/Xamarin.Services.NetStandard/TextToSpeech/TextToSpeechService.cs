using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Services.TextToSpeech
{
	public partial class TextToSpeechService
	{
		public TextToSpeechService() => throw new NotImplementedException();

		public Task SpeakAsync(string text, Locale? locale = null, float? pitch = null, float? speakRate = null, float? volume = null, CancellationToken cancelToken = default(CancellationToken)) => throw new NotImplementedException();

		public Task<IEnumerable<Locale>> GetInstalledLanguagesAsync() => throw new NotImplementedException();

		public int MaxSpeechInputLength => throw new NotImplementedException();

		private void OnDispose(bool disposing) => throw new NotImplementedException();
	}
}

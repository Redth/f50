using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;

namespace Xamarin.Services.TextToSpeech
{
	public partial class TextToSpeechService : IDisposable
#if INCLUDE_INTERFACES
		, ITextToSpeechService
#endif
	{
		private bool disposed;

		public virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				OnDispose(disposing);

				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~TextToSpeechService()
		{
			Dispose(false);
		}
	}
}

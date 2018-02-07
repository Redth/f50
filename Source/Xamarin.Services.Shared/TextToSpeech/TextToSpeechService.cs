using System;

namespace Xamarin.Services.TextToSpeech
{
	public partial class TextToSpeechService : IDisposable
#if INCLUDE_INTERFACES
		, ITextToSpeechService
#endif
	{
		private bool disposed;

		protected virtual void Dispose(bool disposing)
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Speech.Tts;
using Android.App;

namespace Xamarin.Services.TextToSpeech
{
	public partial class TextToSpeechService
	{
		const int DefaultMaxSpeechLength = 4000;
		readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
		TTSInitializer ttsInitializer;
		string text;
		Locale? language;
		float pitch, speakRate;
		int count;

		public TextToSpeechService()
		{
			ttsInitializer = new TTSInitializer();
		}

		public async Task SpeakAsync(string text, Locale? locale = null, float? pitch = null, float? speakRate = null, float? volume = null, CancellationToken cancelToken = default(CancellationToken))
		{
			if (text == null)
				throw new ArgumentNullException(nameof(text), "Text can not be null");

			if (text.Length > MaxSpeechInputLength)
				throw new ArgumentException(nameof(text), "Text length is over the maximum speech input length.");

			try
			{
				await semaphore.WaitAsync(cancelToken);
				this.text = text;
				language = locale;
				this.pitch = pitch == null ? 1.0f : pitch.Value;
				this.speakRate = speakRate == null ? 1.0f : speakRate.Value;

				// TODO: need to wait lock so not to break people using queuing mechanism
				await ttsInitializer.Init();
				await Speak(cancelToken);
			}
			finally
			{
				if (semaphore.CurrentCount == 0)
					semaphore.Release();
			}
		}

		public int MaxSpeechInputLength => (int)global::Android.OS.Build.VERSION.SdkInt < 18 ? DefaultMaxSpeechLength : global::Android.Speech.Tts.TextToSpeech.MaxSpeechInputLength;

		private void SetDefaultLanguage() => SetDefaultLanguageNonLollipop();

		private void SetDefaultLanguageNonLollipop()
		{
			var textToSpeech = ttsInitializer.TextToSpeech;
			if (textToSpeech == null)
				return;

			//disable warning because we are checking ahead of time.
#pragma warning disable 0618
			var sdk = (int)global::Android.OS.Build.VERSION.SdkInt;
			if (sdk >= 18)
			{
				try
				{
#if __ANDROID_18__
					if (textToSpeech.DefaultLanguage == null && textToSpeech.Language != null)
						textToSpeech.SetLanguage(textToSpeech.Language);
					else if (textToSpeech.DefaultLanguage != null)
						textToSpeech.SetLanguage(textToSpeech.DefaultLanguage);
#endif
				}
				catch
				{
					if (textToSpeech.Language != null)
						textToSpeech.SetLanguage(textToSpeech.Language);
				}
			}
			else
			{
				if (textToSpeech.Language != null)
					textToSpeech.SetLanguage(textToSpeech.Language);
			}
#pragma warning restore 0618
		}

		private async Task Speak(CancellationToken cancelToken)
		{
			if (string.IsNullOrWhiteSpace(text))
				return;

			var textToSpeech = ttsInitializer.TextToSpeech;

			if (language.HasValue && !string.IsNullOrWhiteSpace(language.Value.Language))
			{
				Java.Util.Locale locale = null;
				if (!string.IsNullOrWhiteSpace(language.Value.Country))
					locale = new Java.Util.Locale(language.Value.Language, language.Value.Country);
				else
					locale = new Java.Util.Locale(language.Value.Language);

				var result = textToSpeech.IsLanguageAvailable(locale);
				if (result == LanguageAvailableResult.CountryAvailable)
				{
					textToSpeech.SetLanguage(locale);
				}
				else
				{
					Console.WriteLine("Locale: " + locale + " was not valid, setting to default.");
					SetDefaultLanguage();
				}
			}
			else
			{
				SetDefaultLanguage();
			}

			var tcs = new TaskCompletionSource<object>();

			textToSpeech.SetPitch(pitch);
			textToSpeech.SetSpeechRate(speakRate);
			textToSpeech.SetOnUtteranceProgressListener(new TtsProgressListener(tcs));

#pragma warning disable CS0618 // Type or member is obsolete
			count++;
			var map = new Dictionary<string, string>
			{
				[global::Android.Speech.Tts.TextToSpeech.Engine.KeyParamUtteranceId] = count.ToString()
			};
			textToSpeech.Speak(text, QueueMode.Flush, map);
#pragma warning restore CS0618 // Type or member is obsolete

			void OnCancel()
			{
				textToSpeech.Stop();
				tcs.TrySetCanceled();
			}

			using (cancelToken.Register(OnCancel))
			{
				await tcs.Task;
			}
		}

		public async Task<IEnumerable<Locale>> GetInstalledLanguagesAsync()
		{
			await ttsInitializer.Init();

			var textToSpeech = ttsInitializer.TextToSpeech;
			if (textToSpeech != null)
			{
				var version = (int)global::Android.OS.Build.VERSION.SdkInt;
				var isLollipop = version >= 21;
				if (isLollipop)
				{
					try
					{
						//in a different method as it can crash on older target/compile for some reason
						return GetInstalledLanguagesLollipop();
					}
					catch (Exception ex)
					{
						Console.WriteLine("Something went horribly wrong, defaulting to old implementation to get languages: " + ex);
					}
				}

				var languages = new List<Locale>();
				var allLocales = Java.Util.Locale.GetAvailableLocales();
				foreach (var locale in allLocales)
				{

					try
					{
						var result = textToSpeech.IsLanguageAvailable(locale);

						if (result == LanguageAvailableResult.CountryAvailable)
						{
							languages.Add(new Locale { Country = locale.Country, Language = locale.Language, DisplayName = locale.DisplayName });
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine("Error checking language; " + locale + " " + ex);
					}
				}

				return languages.GroupBy(c => c.ToString())
					  .Select(g => g.First());
			}
			else
			{
				return Java.Util.Locale.GetAvailableLocales()
				  .Where(a => !string.IsNullOrWhiteSpace(a.Language) && !string.IsNullOrWhiteSpace(a.Country))
				  .Select(a => new Locale { Country = a.Country, Language = a.Language, DisplayName = a.DisplayName })
				  .GroupBy(c => c.ToString())
				  .Select(g => g.First());
			}
		}

		private IEnumerable<Locale> GetInstalledLanguagesLollipop()
		{
			var sdk = (int)global::Android.OS.Build.VERSION.SdkInt;
			if (sdk < 21)
				return new List<Locale>();

#if __ANDROID_21__
			var textToSpeech = ttsInitializer.TextToSpeech;
			return textToSpeech.AvailableLanguages
				.Select(a => new Locale
				{
					Country = a.Country,
					Language = a.Language,
					DisplayName = a.DisplayName
				});
#endif
		}

		partial void OnDispose(bool disposing)
		{
			if (disposing)
			{
				ttsInitializer?.Dispose();
				ttsInitializer = null;
			}
		}
	}

	internal class TTSInitializer : Java.Lang.Object, global::Android.Speech.Tts.TextToSpeech.IOnInitListener
	{
		private TaskCompletionSource<bool> tcs;

		public bool Initialized { get; private set; }

		public global::Android.Speech.Tts.TextToSpeech TextToSpeech { get; private set; }

		public Task Init()
		{
			if (Initialized)
				return Task.FromResult(true);

			tcs = new TaskCompletionSource<bool>();

			Console.WriteLine("Current version: " + (int)global::Android.OS.Build.VERSION.SdkInt);
			global::Android.Util.Log.Info("CrossTTS", "Current version: " + (int)global::Android.OS.Build.VERSION.SdkInt);
			TextToSpeech = new global::Android.Speech.Tts.TextToSpeech(Application.Context, this);

			return tcs.Task;
		}

		public void OnInit(OperationResult status)
		{
			if (status.Equals(OperationResult.Success))
			{
				tcs.TrySetResult(true);
				Initialized = true;
			}
			else
			{
				tcs.TrySetException(new ArgumentException("Failed to initialize TTS engine"));
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Initialized = false;

				TextToSpeech?.Stop();
				TextToSpeech?.Dispose();
				TextToSpeech = null;
			}

			base.Dispose(disposing);
		}
	}

	internal class TtsProgressListener : UtteranceProgressListener
	{
		readonly TaskCompletionSource<object> completionSource;

		public TtsProgressListener(TaskCompletionSource<object> tcs) => completionSource = tcs;

		public override void OnDone(string utteranceId) =>
			completionSource?.TrySetResult(null);

		public override void OnError(string utteranceId) =>
			completionSource?.TrySetException(new ArgumentException("Error with TTS engine on progress listener"));

		public override void OnStart(string utteranceId)
		{
		}
	}
}

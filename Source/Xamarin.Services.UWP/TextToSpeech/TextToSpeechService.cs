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
	public class TextToSpeechService
#if !EXCLUDE_INTERFACES
		: ITextToSpeechService
#endif
	{
		readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
		SpeechSynthesizer speechSynthesizer;

		public TextToSpeechService() => speechSynthesizer = new SpeechSynthesizer();


		public async Task SpeakAsync(string text, Locale? Locale = null, float? pitch = null, float? speakRate = null, float? volume = null, CancellationToken cancelToken = default(CancellationToken))
		{
			if (text == null)
				throw new ArgumentNullException(nameof(text), "Text can not be null");

			try
			{
				await semaphore.WaitAsync(cancelToken);
				var localCode = string.Empty;

				//nothing fancy needed here
				if (pitch == null && speakRate == null && volume == null)
				{
					if (Locale.HasValue && !string.IsNullOrWhiteSpace(Locale.Value.Language))
					{
						localCode = Locale.Value.Language;

						var voices = from voice in SpeechSynthesizer.AllVoices
									 where (voice.Language == localCode
											&& voice.Gender.Equals(VoiceGender.Female))
									 select voice;
						speechSynthesizer.Voice = (voices.Any() ? voices.ElementAt(0) : SpeechSynthesizer.DefaultVoice);


					}
					else
					{
						speechSynthesizer.Voice = SpeechSynthesizer.DefaultVoice;

					}
				}


				if (Locale.HasValue && !string.IsNullOrWhiteSpace(Locale.Value.Language))
				{
					localCode = Locale.Value.Language;
					var voices = from voice in SpeechSynthesizer.AllVoices
								 where (voice.Language == localCode
										&& voice.Gender.Equals(VoiceGender.Female))
								 select voice;

					if (!voices.Any())
					{
						localCode = SpeechSynthesizer.DefaultVoice.Language;

					}
				}
				else
				{
					localCode = SpeechSynthesizer.DefaultVoice.Language;

				}


				if (!volume.HasValue)
					volume = 100.0f;
				else if (volume.Value > 1.0f)
					volume = 100.0f;
				else if (volume.Value < 0.0f)
					volume = 0.0f;
				else
					volume = volume.Value * 100.0f;

				var pitchProsody = "default";
				//var test = "x-low", "low", "medium", "high", "x-high", or "default";
				if (!pitch.HasValue)
					pitchProsody = "default";
				else if (pitch.Value >= 1.6f)
					pitchProsody = "x-high";
				else if (pitch.Value >= 1.1f)
					pitchProsody = "high";
				else if (pitch.Value >= .9f)
					pitchProsody = "medium";
				else if (pitch.Value >= .4f)
					pitchProsody = "low";
				else
					pitchProsody = "x-low";


				var ssml = @"<speak version='1.0' " +
						$"xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='{localCode}'>" +
						$"<prosody pitch='{pitchProsody}' volume='{volume.Value}' rate='{speakRate ?? 1F}'>{text}</prosody> " +
						"</speak>";

				var tcs = new TaskCompletionSource<object>();
				var handler = new TypedEventHandler<MediaPlayer, object>((sender, args) => tcs.TrySetResult(null));

				try
				{
					var player = BackgroundMediaPlayer.Current;
					var stream = await speechSynthesizer.SynthesizeSsmlToStreamAsync(ssml);

					player.MediaEnded += handler;
					player.SetStreamSource(stream);
					player.Play();

					void OnCancel()
					{
						player.PlaybackRate = 0;
						tcs.TrySetResult(null);
					}

					using (cancelToken.Register(OnCancel))
					{
						await tcs.Task;
					}

					player.MediaEnded -= handler;
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Unable to playback stream: " + ex);
				}

			}
			finally
			{
				if (semaphore.CurrentCount == 0)
					semaphore.Release();
			}
		}

		public Task<IEnumerable<Locale>> GetInstalledLanguagesAsync() =>
			Task.FromResult(SpeechSynthesizer.AllVoices
			  .OrderBy(a => a.Language)
			  .Select(a => new Locale { Language = a.Language, DisplayName = a.DisplayName })
			  .GroupBy(c => c.ToString())
			  .Select(g => g.First()));

		public int MaxSpeechInputLength => -1;

		public void Dispose() =>
			speechSynthesizer?.Dispose();
	}
}

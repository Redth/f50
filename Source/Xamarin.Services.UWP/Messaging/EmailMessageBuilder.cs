using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace Xamarin.Services.Messaging
{
	public partial class EmailMessageBuilder
	{
		public EmailMessageBuilder BodyAsHtml(string htmlBody)
		{
			throw new PlatformNotSupportedException();
		}

		public EmailMessageBuilder WithAttachment(string filePath, string contentType)
		{
			var file = Task.Run(async () =>
			{
				try
				{
					return await StorageFile.GetFileFromPathAsync(filePath).AsTask().ConfigureAwait(false);
				}
				catch (UnauthorizedAccessException)
				{
					throw new PlatformNotSupportedException("Windows apps cannot access files by filePath unless they reside in ApplicationData. Use the platform-specific WithAttachment(IStorageFile) overload instead.");
				}
			}).Result;

			email.Attachments.Add(new EmailAttachment(file));
			return this;
		}

		public EmailMessageBuilder WithAttachment(IStorageFile file)
		{
			email.Attachments.Add(new EmailAttachment(file));
			return this;
		}
	}
}

using System;
using System.IO;

namespace Xamarin.Services.Messaging
{
	public partial class EmailAttachment
	{
		public EmailAttachment(string filePath, string contentType)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentNullException(nameof(filePath));

			if (string.IsNullOrWhiteSpace(contentType))
				throw new ArgumentNullException(nameof(contentType));

			FilePath = filePath;
			FileName = Path.GetFileName(filePath);
			ContentType = contentType;
		}

		public string ContentType { get; }

		public string FileName { get; }

		public string FilePath { get; }
	}
}

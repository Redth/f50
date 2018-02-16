using Android.Webkit;
using System;

namespace Xamarin.Services.Messaging
{
	public partial class EmailAttachment
	{
		public EmailAttachment(Java.IO.File file)
		{
			if (file == null)
				throw new ArgumentNullException(nameof(file));

			var extension = MimeTypeMap.GetFileExtensionFromUrl(file.Path);
			var contentType = MimeTypeMap.Singleton.GetMimeTypeFromExtension(extension);

			File = file;
			FilePath = file.Path;
			FileName = file.Name;
			ContentType = contentType;
		}

		public Java.IO.File File { get; }
	}
}

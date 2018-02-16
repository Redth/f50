using System;
using Windows.Storage;

namespace Xamarin.Services.Messaging
{
	public partial class EmailAttachment
	{
		public EmailAttachment(IStorageFile file)
		{
			File = file ?? throw new ArgumentNullException(nameof(file));
			FilePath = file.Path;
			FileName = file.Name;
			ContentType = file.ContentType;
		}

		public IStorageFile File { get; }
	}
}

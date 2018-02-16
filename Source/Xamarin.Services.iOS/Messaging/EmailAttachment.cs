using System;

namespace Xamarin.Services.Messaging
{
	public partial class EmailAttachment
	{
		public EmailAttachment(Foundation.NSUrl file)
		{
			File = file ?? throw new ArgumentNullException(nameof(file));
			FilePath = file.Path;
			FileName = Foundation.NSFileManager.DefaultManager.DisplayName(file.Path);
			var id = MobileCoreServices.UTType.CreatePreferredIdentifier(MobileCoreServices.UTType.TagClassFilenameExtension, file.PathExtension, null);
			var mimeTypes = MobileCoreServices.UTType.CopyAllTags(id, MobileCoreServices.UTType.TagClassMIMEType);

			if (mimeTypes.Length > 0)
			{
				ContentType = mimeTypes[0];
			}
		}

		public Foundation.NSUrl File { get; }
	}
}

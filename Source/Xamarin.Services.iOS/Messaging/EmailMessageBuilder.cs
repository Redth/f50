namespace Xamarin.Services.Messaging
{
	public partial class EmailMessageBuilder
	{
		public EmailMessageBuilder BodyAsHtml(string htmlBody)
		{
			if (!string.IsNullOrEmpty(htmlBody))
			{
				email.Message = htmlBody;
				email.IsHtml = true;
			}

			return this;
		}

		public EmailMessageBuilder WithAttachment(string filePath, string contentType)
		{
			email.Attachments.Add(new EmailAttachment(filePath, contentType));
			return this;
		}

		public EmailMessageBuilder WithAttachment(Foundation.NSUrl file)
		{
			email.Attachments.Add(new EmailAttachment(file));
			return this;
		}
	}
}

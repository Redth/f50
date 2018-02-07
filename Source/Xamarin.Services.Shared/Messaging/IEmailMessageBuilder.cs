using System.Collections.Generic;

namespace Xamarin.Services.Messaging
{
#if INCLUDE_INTERFACES
	public interface IEmailMessageBuilder
	{
		EmailMessageBuilder Bcc(IEnumerable<string> bcc);
		EmailMessageBuilder Bcc(string bcc);
		EmailMessageBuilder Body(string body);
		EmailMessageBuilder BodyAsHtml(string htmlBody);
		EmailMessage Build();
		EmailMessageBuilder Cc(IEnumerable<string> cc);
		EmailMessageBuilder Cc(string cc);
		EmailMessageBuilder Subject(string subject);
		EmailMessageBuilder To(IEnumerable<string> to);
		EmailMessageBuilder To(string to);
		EmailMessageBuilder WithAttachment(string filePath, string contentType);
	}
#endif
}

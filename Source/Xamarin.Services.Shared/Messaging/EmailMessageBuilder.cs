using System.Collections.Generic;

namespace Xamarin.Services.Messaging
{
	public partial class EmailMessageBuilder
#if INCLUDE_INTERFACES
		: IEmailMessageBuilder
#endif
	{
		private readonly EmailMessage email;

		public EmailMessageBuilder()
		{
			email = new EmailMessage();
		}

		public EmailMessageBuilder Bcc(string bcc)
		{
			if (!string.IsNullOrWhiteSpace(bcc))
				email.RecipientsBcc.Add(bcc);

			return this;
		}

		public EmailMessageBuilder Bcc(IEnumerable<string> bcc)
		{
			email.RecipientsBcc.AddRange(bcc);
			return this;
		}

		public EmailMessageBuilder Body(string body)
		{
			if (!string.IsNullOrEmpty(body))
				email.Message = body;

			return this;
		}

		public EmailMessage Build()
		{
			return email;
		}

		public EmailMessageBuilder Cc(string cc)
		{
			if (!string.IsNullOrWhiteSpace(cc))
				email.RecipientsCc.Add(cc);

			return this;
		}

		public EmailMessageBuilder Cc(IEnumerable<string> cc)
		{
			email.RecipientsCc.AddRange(cc);
			return this;
		}

		public EmailMessageBuilder Subject(string subject)
		{
			if (!string.IsNullOrEmpty(subject))
				email.Subject = subject;

			return this;
		}

		public EmailMessageBuilder To(string to)
		{
			if (!string.IsNullOrWhiteSpace(to))
				email.Recipients.Add(to);

			return this;
		}

		public EmailMessageBuilder To(IEnumerable<string> to)
		{
			email.Recipients.AddRange(to);
			return this;
		}
	}
}

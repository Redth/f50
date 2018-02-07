using System.Collections.Generic;

namespace Xamarin.Services.Messaging
{
	public class EmailMessage
	{
		private List<string> _recipientsBcc;
		private List<string> _recipientsCc;
		private List<string> _recipients;
		private List<EmailAttachment> _attachments;

		public EmailMessage(string to)
			: this()
		{
			if (!string.IsNullOrWhiteSpace(to))
				Recipients.Add(to);
		}

		public EmailMessage(string to = null, string subject = null, string message = null)
			: this(to)
		{
			Subject = subject ?? string.Empty;
			Message = message ?? string.Empty;
		}

		internal EmailMessage()
		{
			Subject = string.Empty;
			Message = string.Empty;
		}

		public string Subject { get; set; }

		public string Message { get; set; }

		public bool IsHtml { get; set; }

		public List<EmailAttachment> Attachments
		{
			get { return _attachments ?? (_attachments = new List<EmailAttachment>()); }
			set { _attachments = value; }
		}

		public List<string> Recipients
		{
			get { return _recipients ?? (_recipients = new List<string>()); }
			set { _recipients = value; }
		}

		public List<string> RecipientsBcc
		{
			get { return _recipientsBcc ?? (_recipientsBcc = new List<string>()); }
			set { _recipientsBcc = value; }
		}

		public List<string> RecipientsCc
		{
			get { return _recipientsCc ?? (_recipientsCc = new List<string>()); }
			set { _recipientsCc = value; }
		}
	}
}

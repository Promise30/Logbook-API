using MimeKit;

namespace LogBook_API.Contracts.Constants
{
    public class EmailMessage
    {
            public List<MailboxAddress> To { get; set; }
            public string Subject { get; set; }
            public BodyBuilder Body { get; set; }
            public EmailMessage(IEnumerable<string> to, string subject, string body)
            {

                To = new List<MailboxAddress>();
                To.AddRange(to.Select(x => new MailboxAddress("email", x)));
                Subject = subject;
                Body = new BodyBuilder();
                Body.HtmlBody = body;
            }
        
    }
}

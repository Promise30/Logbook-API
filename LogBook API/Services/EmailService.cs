using LogBook_API.Contracts.Constants;
using LogBook_API.Services.Abstractions;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace LogBook_API.Services
{
    public class EmailService : IEmailService
    {
       
        private readonly ILogger<EmailService> _logger;
        private readonly EmailConfiguration _emailConfiguration;
        public EmailService(ILogger<EmailService> logger, EmailConfiguration emailConfiguration)
        {
            _logger = logger;
            _emailConfiguration = emailConfiguration;
        }
      
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var emailMessage = CreateEmailMessage(toEmail, subject, body);
            Send(emailMessage);
        }

        #region Private methods

        private MimeMessage CreateEmailMessage(string toEmail, string subject, string body)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Logbook API Service", _emailConfiguration.From));
            emailMessage.To.Add(new MailboxAddress("", toEmail));
            emailMessage.Subject = subject;

            // Convert the body to a format suitable for the MimeMessage
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = body
            };

            return emailMessage;
        }

        private void Send(MimeMessage mailMessage)
        {
            using var client = new SmtpClient();
            try
            {
                client.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.Port, SecureSocketOptions.StartTls);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(_emailConfiguration.UserName, _emailConfiguration.Password);

                client.Send(mailMessage);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Error, $"Error occurred with the send method: {ex.Message}");
                throw;
            }
            finally
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }

        #endregion
        
    }
}

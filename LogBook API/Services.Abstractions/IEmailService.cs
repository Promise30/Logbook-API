using LogBook_API.Contracts.Constants;
using System.Net.Mail;

namespace LogBook_API.Services.Abstractions
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}

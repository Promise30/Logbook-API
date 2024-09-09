using LogBook_API.Services.Abstractions;

namespace LogBook_API.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        public string GenerateEmailChangeConfirmationLink(string userName, string emailChangeConfirmationLink)
        {
            return $"Hello {userName},<br><br>" +
                $"You requested to change your email. Please confirm the new email by clicking on the link below:<br>" +
                $"<a href='{emailChangeConfirmationLink}'>Confirm New Email</a><br><br>" +
                $"If you did not request this, please contact support.";
        }
        public string GeneratePasswordResetEmail(string userName, string passwordResetLink)
        {
            return $"Hello {userName},<br><br>" +
               $"You requested to reset your password. Please reset your password by using the password reset link below:<br>" +
               $"<a href='{passwordResetLink}'>Reset Password</a><br><br>" +
               $"If you did not request this, please ignore this email.";
        }
        public string GenerateRegistrationConfirmationEmail(string userName, string confirmationLink)
        {
            return $"Hello {userName},<br><br>" +
               $"Kindly confirm your email by clicking on the link below:<br>" +
               $"<a href='{confirmationLink}'>Confirm Email</a><br><br>" +
               $"Thank you!";
        }
    }
}



namespace LogBook_API.Services.Abstractions
{
    public interface IEmailTemplateService
    {
        public string GenerateRegistrationConfirmationEmail(string userName, string confirmationLink);
        public string GeneratePasswordResetEmail(string userName, string passwordResetLink);
        public string GenerateEmailChangeConfirmationLink(string userName, string confirmationLink);
    }   
}

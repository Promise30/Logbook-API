using System.ComponentModel.DataAnnotations;

namespace LogBook_API.Contracts.Auth
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "A valid email address is required")]
        public string Email { get; set; }
    }
}

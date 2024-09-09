using System.ComponentModel.DataAnnotations;

namespace LogBook_API.Contracts.Auth
{
    public class PasswordResetDto
    {
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "A valid email address is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password field is required")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Password confirmation field is required")]
        [Compare("Password", ErrorMessage = "The passwords provided do not match")]
        public string ConfirmPassword { get; set; }
        [Required(ErrorMessage = "Token field is required")]
        public string Token { get; set; }
    }
}

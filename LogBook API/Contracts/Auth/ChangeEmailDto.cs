using System.ComponentModel.DataAnnotations;

namespace LogBook_API.Contracts.Auth
{
    public class ChangeEmailDto
    {
        [Required(ErrorMessage = "New user email field is required")]
        [EmailAddress(ErrorMessage ="A valid email address field is required")]
        public string NewEmail { get; set; }
    }
}

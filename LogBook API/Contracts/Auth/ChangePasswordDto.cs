using System.ComponentModel.DataAnnotations;

namespace LogBook_API.Contracts.Auth
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Current password field is required")]
        public string CurrentPassword { get; set; }
        [Required(ErrorMessage = "New password field is required")]
        public string NewPassword { get; set; }
        [Compare("NewPassword", ErrorMessage ="Password fields do not match")]
        [Required(ErrorMessage= "New password field confirmation is required")]
        public string ConfirmNewPassword { get; set; }
    }
}

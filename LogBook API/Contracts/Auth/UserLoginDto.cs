using System.ComponentModel.DataAnnotations;

namespace LogBook_API.Contracts.Auth
{
    public class UserLoginDto
    {
        [Required(ErrorMessage = "User name is required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password field is required")]
        public string Password { get; set; }
    }
}

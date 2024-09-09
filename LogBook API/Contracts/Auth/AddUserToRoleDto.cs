using LogBook_API.Contracts.Validation;
using System.ComponentModel.DataAnnotations;

namespace LogBook_API.Contracts.Auth
{
    public class AddUserToRoleDto
    {
        [Required(ErrorMessage = ("Email field is required"))]
        [EmailAddress(ErrorMessage = "A valid email address is required")]
        public string Email {  get; set; }
        [ValidRoles(ErrorMessage = "Invalid role specified")]
        public ICollection<string> Roles { get; set; }
    }
}

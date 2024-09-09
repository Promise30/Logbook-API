using LogBook_API.Contracts.Validation;
using System.ComponentModel.DataAnnotations;

namespace LogBook_API.Contracts.Auth
{
    public class UserRegistrationDto
    {
        [Required(ErrorMessage = "FirstName is required")]
        public string? FirstName { get; init; }
        [Required(ErrorMessage = "LastName is required")]
        public string? LastName { get; init; }
        [Required(ErrorMessage = "Username is required")]
        public string? UserName { get; init; }
        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; init; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "A valid email address is required")]
        public string? Email { get; init; }
        [Required(ErrorMessage = "Phone country code is required")]
        [RegularExpression(@"^\+\d{1,3}$", ErrorMessage = "Invalid country code format. Use '+' followed by 1-3 digits")]
        public string PhoneCountryCode { get; init; }
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits")]
        public string? PhoneNumber { get; init; }
        [ValidRoles(ErrorMessage = "Invalid role specified. Roles should either be 'Administrator' or' User'")]
        public ICollection<string>? Roles { get; init; }
    }
}

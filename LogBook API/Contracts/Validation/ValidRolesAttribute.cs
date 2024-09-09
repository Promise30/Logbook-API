using System.ComponentModel.DataAnnotations;

namespace LogBook_API.Contracts.Validation
{
    public class ValidRolesAttribute : ValidationAttribute
    {
        private readonly string[] _validRoles = { "Administrator", "User" };

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is ICollection<string> roles)
            {
                foreach (var role in roles)
                {
                    if (!_validRoles.Contains(role))
                    {
                        return new ValidationResult($"Invalid role: {role}. Valid roles are: {string.Join(", ", _validRoles)}");
                    }
                }
            }

            return ValidationResult.Success;
        }
    }
}

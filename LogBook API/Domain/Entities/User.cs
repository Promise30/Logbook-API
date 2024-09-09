using Microsoft.AspNetCore.Identity;

namespace LogBook_API.Domain.Entities
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? PhoneCountryCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryDate { get; set; }
        public virtual ICollection<LogbookEntry> LogbookEntries { get; set; } = new List<LogbookEntry>();
    }
}

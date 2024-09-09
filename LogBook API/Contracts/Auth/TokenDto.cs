using System.ComponentModel.DataAnnotations;

namespace LogBook_API.Contracts.Auth
{
    public class TokenDto
    {
        public string AccessToken { get; init; }
        public string RefreshToken { get; init; }
        public DateTime AccessTokenExpiryDate { get; init; }
        public DateTime? RefreshTokenExpiryDateExpiry { get; init; }
    }
}

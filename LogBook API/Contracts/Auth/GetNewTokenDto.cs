using System.ComponentModel.DataAnnotations;

namespace LogBook_API.Contracts.Auth
{
    public class GetNewTokenDto
    {
        [Required(ErrorMessage = "Access token is required")]
        public string AccessToken { get; init; }
        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; init; }
    }
}

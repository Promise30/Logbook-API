using LogBook_API.Contracts;
using LogBook_API.Contracts.Auth;
using Microsoft.AspNetCore.Identity;
namespace LogBook_API.Services.Abstractions
{
    public interface IAuthService
    {
        Task<ApiResponse<UserResponseDto>> RegisterUser(UserRegistrationDto userRegistrationDto);
        Task<ApiResponse<TokenDto>> ValidateUser(UserLoginDto userLoginDto);
        Task<TokenDto> CreateToken(bool populateExp);
        Task<ApiResponse<TokenDto>> RefreshToken(GetNewTokenDto tokenDto);
        Task<ApiResponse<object>> DeleteUser(string userEmail);
        Task<ApiResponse<IEnumerable<UserResponseDto>>> GetUsers();
        Task<ApiResponse<UserResponseDto>> GetUserById(string userId);
        Task<ApiResponse<ForgotPasswordResponseDto>> ForgotPasswordRequestAsync(ForgotPasswordDto forgotPasswordDto);
        Task<ApiResponse<object>> PasswordResetAsync(PasswordResetDto passwordResetDto);
        Task<ApiResponse<object>> UserEmailConfirmation(string token, string email);
        Task<ApiResponse<object>> AddUserToRoleAsync(AddUserToRoleDto addUserToRoleDto);
        Task<ApiResponse<IEnumerable<string>>> GetUserRolesAsync(string email);
        Task<ApiResponse<object>> ChangeUserEmailAsync(ChangeEmailDto changeEmailDto);
        Task<ApiResponse<object>> ChangeUserPasswordAsync(ChangePasswordDto changePasswordDto);
        Task<ApiResponse<object>> NewUserEmailConfirmation(string token, string oldEmail, string newEmail);
    }
}

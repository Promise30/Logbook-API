using Hangfire;
using LogBook_API.Contracts;
using LogBook_API.Contracts.Auth;
using LogBook_API.Contracts.MappingExtensions;
using LogBook_API.Domain.Entities;
using LogBook_API.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LogBook_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUrlHelper _urlHelper;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _emailTemplateService;
        private User _user;
        public AuthService(UserManager<User> userManager, IConfiguration configuration, ILogger<AuthService> logger, IHttpContextAccessor httpContextAccessor, IUrlHelper urlHelper, IEmailService emailService, IEmailTemplateService emailTemplateService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _urlHelper = urlHelper;
            _emailService = emailService;
            _emailTemplateService = emailTemplateService;
        }
        public async Task<ApiResponse<UserResponseDto>> RegisterUser(UserRegistrationDto userRegistrationDto)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(userRegistrationDto.Email);
                if (existingUser != null)
                {
                    _logger.Log(LogLevel.Information, $"Existing user found when trying to register new user with email: {userRegistrationDto.Email} at {DateTime.Now.ToString("yy-MM-dd:H m s")}");
                    return ApiResponse<UserResponseDto>.Failure(400, "User already exists.");
                }
                var user = new User
                {
                    UserName = userRegistrationDto.UserName,
                    Email = userRegistrationDto.Email,
                    PhoneCountryCode = userRegistrationDto.PhoneCountryCode,
                    PhoneNumber = userRegistrationDto.PhoneNumber,
                    FirstName = userRegistrationDto.FirstName,
                    LastName = userRegistrationDto.LastName,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    RefreshToken = string.Empty,
                };
                var result = await _userManager.CreateAsync(user, userRegistrationDto.Password);
                if (!result.Succeeded)
                {
                    var errorMessages = result.Errors.Select(e => e.Description).ToList();
                    var errorString = string.Join(", ", errorMessages);
                    _logger.Log(LogLevel.Error, $"Error occurred while creating new user {userRegistrationDto.Email}, {errorString}");
                    return ApiResponse<UserResponseDto>.Failure(400, data: null, message:"Request unsuccessful", errors:errorMessages);
                }
                await _userManager.AddToRolesAsync(user, userRegistrationDto.Roles);
                _logger.Log(LogLevel.Information, $"New user created with username-> {userRegistrationDto.UserName} at {DateTime.Now.ToString("yy-MM-dd:H m s")}");

                // Generate email content and setup a background task to handle it
                
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = _urlHelper.Action("ConfirmEmail", "Auth", new { email = user.Email, token }, _httpContextAccessor?.HttpContext?.Request.Scheme);
                var emailContent = _emailTemplateService.GenerateRegistrationConfirmationEmail(userRegistrationDto.UserName, confirmationLink);

                // Enqueue email sending
                BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(user.Email, "Confirm your email", emailContent));
                var userToReturn = user.ToUserResponseDto();
                return ApiResponse<UserResponseDto>.Success(201, userToReturn, "Registration Successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.LogInformation($"Error occurred while adding a registering a new user: {ex.Message}");
                return ApiResponse<UserResponseDto>.Failure(500, "User could not be created");
            }
        }
        public async Task<ApiResponse<TokenDto>> ValidateUser(UserLoginDto userLoginDto)
        {
            try
            {
                _user = await _userManager.FindByNameAsync(userLoginDto.UserName);
                var result = (_user != null && await _userManager.CheckPasswordAsync(_user, userLoginDto.Password));
                if (!result)
                {
                    _logger.Log(LogLevel.Warning, $"{nameof(ValidateUser)}: Authentication failed. Wrong user name or password.");
                    return ApiResponse<TokenDto>.Failure(401, "Authentication failed. Invalid credentials");
                }
                var token = await CreateToken(true);
                _logger.Log(LogLevel.Information, "New token credentials created: {token}", token);
                return ApiResponse<TokenDto>.Success(200, token, "Validation successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.LogInformation($"Error occured while validating user credentials: {ex.Message}");
                return ApiResponse<TokenDto>.Failure(500, "An error occured. User authentication failed.");
            }
        }
        public async Task<ApiResponse<TokenDto>> RefreshToken(GetNewTokenDto tokenDto)
        {
            try
            {
                var principal = GetPrincipalFromExpiredToken(tokenDto.AccessToken);
                var user = await _userManager.FindByNameAsync(principal.Identity.Name);
                if (user == null || user.RefreshToken != tokenDto.RefreshToken ||
                user.RefreshTokenExpiryDate <= DateTime.Now)
                    return ApiResponse<TokenDto>.Failure(400, "Invalid client request. The tokenDto has some invalid values.");
                _user = user;
                var newToken = await CreateToken(populateExp: false);
                _logger.Log(LogLevel.Information, "Newly generated token: {token}",newToken);
                return ApiResponse<TokenDto>.Success(200, newToken, "Request Successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.LogInformation($"Error occured while trying to generate new access and refresh token for user: {ex.Message}");
                return ApiResponse<TokenDto>.Failure(500, "Request unsuccessful.");
            }
        }
        public async Task<ApiResponse<ForgotPasswordResponseDto>> ForgotPasswordRequestAsync(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
                if (user is null)
                    return ApiResponse<ForgotPasswordResponseDto>.Failure(404, null, "Request unsuccessful. Email does not exist");
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
               
                var forgotPasswordLink = _urlHelper.Action("ResetUserPassword", "Auth", new { email = user.Email, token }, _httpContextAccessor?.HttpContext?.Request.Scheme);
                var emailContent = _emailTemplateService.GeneratePasswordResetEmail(user.UserName, forgotPasswordLink);
                // Enqueue email sending
                BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(forgotPasswordDto.Email, "Reset your password", emailContent));
                var response = new ForgotPasswordResponseDto { Token = token };
                _logger.Log(LogLevel.Information, "Password reset token generated for {userEmail} at {time}: {token}", user.Email, DateTime.Now, token);
                return ApiResponse<ForgotPasswordResponseDto>.Success(200, response, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.LogInformation($"Error occured while trying to perform the forgot password operation: {ex.Message}");
                return ApiResponse<ForgotPasswordResponseDto>.Failure(500, "An error occurred. Request unsuccessful");
            }
        }
        public async Task<ApiResponse<object>> PasswordResetAsync(PasswordResetDto passwordResetDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(passwordResetDto.Email);
                if (user is null)
                    return ApiResponse<object>.Failure(404, "User does not exist");
                var resetPasswordResult = await _userManager.ResetPasswordAsync(user, passwordResetDto.Token, passwordResetDto.Password);
                if (!resetPasswordResult.Succeeded)
                {
                    var errorMessages = resetPasswordResult.Errors.Select(e => e.Description).ToList();
                    _logger.Log(LogLevel.Information, "Error occurred while trying to reset password for {userEmail}: {errors}", user.Email, errorMessages);
                    return ApiResponse<object>.Failure(400, null, "Request unsuccessful.", errors: errorMessages);
                }
                // Invalidate refresh token
                user.RefreshToken = null;
                user.RefreshTokenExpiryDate = null;
                await _userManager.UpdateAsync(user);
                _logger.Log(LogLevel.Information, "Password successfully reset at {time} for {userEmail}", DateTime.Now, user.Email);
                return ApiResponse<object>.Success(200, null, "Password successfully changed.");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.LogInformation($"Error occured while trying to reset the password: {ex.Message}");
                return ApiResponse<object>.Failure(500, "Request unsuccessful");
            }
        }
        public async Task<ApiResponse<object>> UserEmailConfirmation(string token, string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user is null)
                {
                    return ApiResponse<object>.Failure(404, "User does not exist");
                }
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    _logger.Log(LogLevel.Information, "Email confirmation successful for {userEmail}", user.Email);
                    return ApiResponse<object>.Success(200, "User email verification successful");
                }
                var errorMessages = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<object>.Failure(400, null, "User email verification failed.", errors:errorMessages);

            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.LogInformation($"Error occured while trying to confirm user email: {ex.Message}");
                return ApiResponse<object>.Failure(500, "Request unsuccessful");
            }
        }

        public async Task<ApiResponse<object>> DeleteUser(string userEmail)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user is null)
                    return ApiResponse<object>.Failure(404, "User with email '{userEmail}' not found", userEmail);
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    var errorMessages = result.Errors.Select(e => e.Description).ToList();
                    _logger.Log(LogLevel.Information, "Error occurred while deleting user '{userEmail}'", user.Email);
                    return ApiResponse<object>.Failure(400, null, "Request unsuccessful", errors: errorMessages);
                }
                _logger.Log(LogLevel.Information, "User with email '{userEmail}' deleted successfully.", user.Email);
                return ApiResponse<object>.Success(204, "Request successful");

            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Information, ex.Message);
                return ApiResponse<object>.Failure(400, "An error occurred. User could not be deleted.");
            }
        }
        public async Task<ApiResponse<IEnumerable<UserResponseDto>>> GetUsers()
        {
            try
            {
                var users = _userManager.Users;
                var usersToReturn = users.Select(u=> u.ToUserResponseDto()).ToList();
                _logger.Log(LogLevel.Information, "Total number of users retrieved from the database: {users}", users.Count());
                return ApiResponse<IEnumerable<UserResponseDto>>.Success(200, usersToReturn, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Information, ex.Message);
                return ApiResponse<IEnumerable<UserResponseDto>>.Failure(500, "Error occured while retrieving users from the database.");
            }
        }
        public async Task<ApiResponse<object>> AddUserToRoleAsync(AddUserToRoleDto addUserToRoleDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(addUserToRoleDto.Email);
                if (user == null)
                    return ApiResponse<object>.Failure(400, "User does not exist");

                // Get the roles the user currently has
                var currentRoles = await _userManager.GetRolesAsync(user);

                // Identify roles that need to be added
                var rolesToAdd = addUserToRoleDto.Roles.Except(currentRoles).ToList();

                if (!rolesToAdd.Any())
                    return ApiResponse<object>.Success(200, "User already has the specified role");
                
                // Add the new roles to the user
                var result = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!result.Succeeded)
                {
                    var errorMessage = result.Errors.Select(e => e.Description).ToList();
                    _logger.Log(LogLevel.Information, "Error occurred while adding roles to user: {errors}", errorMessage.ToList());
                    return ApiResponse<object>.Failure(400, "Request unsuccessful");
                }
                return ApiResponse<object>.Success(200, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Information, $"Error occured while adding roles to user: {ex.Message}");
                return ApiResponse<object>.Failure(400, "Request unsuccesful");
            }
        }
        public async Task<ApiResponse<UserResponseDto>> GetUserById(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user is null)
                    return ApiResponse<UserResponseDto>.Failure(404, "User does not exist");
                var userToReturn = user.ToUserResponseDto();
                return ApiResponse<UserResponseDto>.Success(200, userToReturn, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Information, $"Error occured while trying to retrieve user record: {ex.Message}");
                return ApiResponse<UserResponseDto>.Failure(400, "Request unsuccesful");
            }
        }
        public async Task<ApiResponse<IEnumerable<string>>> GetUserRolesAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return ApiResponse<IEnumerable<string>>.Failure(404, "User does not exist");
                var result = await _userManager.GetRolesAsync(user);

                _logger.Log(LogLevel.Information, "Roles assigned to {userEmail}: {role}", user.Email,result.ToList());
                return ApiResponse<IEnumerable<string>>.Success(200, result, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Information, $"Error occured while retrieving user roles: {ex.Message}");
                return ApiResponse<IEnumerable<string>>.Failure(500, "Request unsuccesful");
            }
        }
        public async Task<ApiResponse<object>> ChangeUserPasswordAsync(ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _userManager.FindByIdAsync(userId);
                if (user is null)
                    return ApiResponse<object>.Failure(404, "User does not exist");
                var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    _logger.Log(LogLevel.Information, "Error occured while changing user password {userEmail}: {errorString}", user.Email, errors);
                    return ApiResponse<object>.Failure(statusCode: StatusCodes.Status400BadRequest, data: null , message: "Request unsuccessful", errors: errors);
                }
                return ApiResponse<object>.Success(200, "Password reset successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Information, $"Error occured while changing user password: {ex.Message}");
                return ApiResponse<object>.Failure(500, "An error occured. Request unsuccesful");

            }
        }
        public async Task<ApiResponse<object>> ChangeUserEmailAsync(ChangeEmailDto changeEmailDto)
        {
            try
            {
                var userId = _httpContextAccessor?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _userManager.FindByIdAsync(userId);
                if (user is null)
                    return ApiResponse<object>.Failure(404, "User does not exist");
                if (user.Email == changeEmailDto.NewEmail)
                    return ApiResponse<object>.Failure(400, "New email is the same as the current email");
                var token = await _userManager.GenerateChangeEmailTokenAsync(user, changeEmailDto.NewEmail);
                _logger.Log(LogLevel.Information, "New email confirmation token for {userEmail}: {token}", changeEmailDto.NewEmail, token);

                var emailChangeConfirmationLink = _urlHelper.Action("ConfirmUserEmailChange", "Auth", new { oldEmail= user.Email, newEmail = changeEmailDto.NewEmail, token }, _httpContextAccessor?.HttpContext?.Request.Scheme);
                var emailContent = _emailTemplateService.GenerateEmailChangeConfirmationLink(user.UserName, emailChangeConfirmationLink);

                // Enqueue email sending
                BackgroundJob.Enqueue(() => _emailService.SendEmailAsync(changeEmailDto.NewEmail, "Confirm your new email", emailContent));
                return ApiResponse<object>.Success(200, token, "Request successful");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.Log(LogLevel.Information, $"Error occured while changing user password: {ex.Message}");
                return ApiResponse<object>.Failure(500, "An error occured. Request unsuccesful");

            }
        }
        public async Task<ApiResponse<object>> NewUserEmailConfirmation(string token, string oldEmail, string newEmail)
        {
            try
            {
                _user = await _userManager.FindByEmailAsync(oldEmail);
                if (_user is null)
                {
                    return ApiResponse<object>.Failure(404, null, "User does not exist");
                }
                var result = await _userManager.ChangeEmailAsync(_user, newEmail, token);
                if (result.Succeeded)
                {
                    _logger.Log(LogLevel.Information, "Email confirmation successful for {userEmail}", _user.UserName);
                    return ApiResponse<object>.Success(200, "New User email verification successful");
                }
                    var errorMessages = result.Errors.Select(e => e.Description).ToList();
                return ApiResponse<object>.Failure(400, null, "User email verification failed.", errors: errorMessages); 

            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, ex.StackTrace);
                _logger.LogInformation($"Error occured while trying to confirm new user email: {ex.Message}");
                return ApiResponse<object>.Failure(500, "Request unsuccessful");
            }
        }
        #region Methods
        public async Task<TokenDto> CreateToken(bool populateExp)
        {
            var signingCredentials = GetSigningCredentials();
            var claims = await GetClaims();
            var tokenOptions = GenerateTokenOptions(signingCredentials, claims);
            var refreshToken = GenerateRefreshToken();
            _user.RefreshToken = refreshToken;
            if (populateExp)
                _user.RefreshTokenExpiryDate = DateTime.Now.AddDays(7);
            await _userManager.UpdateAsync(_user);
            var accessToken = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            return new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiryDate = tokenOptions.ValidTo,
                RefreshTokenExpiryDateExpiry = _user.RefreshTokenExpiryDate
            };
        }
        private SigningCredentials GetSigningCredentials()
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["secretKey"]);
            var secret = new SymmetricSecurityKey(key);
            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }
        private async Task<List<Claim>> GetClaims()
        {
            var claims = new List<Claim>
             {
             new Claim(ClaimTypes.Name, _user.UserName),
             new Claim(ClaimTypes.NameIdentifier, _user.Id),
             new Claim(ClaimTypes.Email, _user.Email),
             };
            var roles = await _userManager.GetRolesAsync(_user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            return claims;
        }
        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var tokenOptions = new JwtSecurityToken
            (
            issuer: jwtSettings["validIssuer"],
            audience: jwtSettings["validAudience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["expires"])),
            signingCredentials: signingCredentials
            );
            return tokenOptions;
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber);
        }
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["secretKey"])),
                ValidateLifetime = true,
                ValidIssuer = jwtSettings["validIssuer"],
                ValidAudience = jwtSettings["validAudience"]
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
            StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }
            return principal;
        }
        #endregion
    }
}

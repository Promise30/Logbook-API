using LogBook_API.Contracts;
using LogBook_API.Contracts.Auth;
using LogBook_API.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace LogBook_API.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
            
        }
        /// <summary>
        /// Registers a new user with the provided registration details.
        /// </summary>
        /// <param name="userRegistrationDto">The DTO containing the user registration information, including username, email, password, and other details.</param>
        /// <returns>A response indicating the success or failure of the registration process, including any validation errors or user information.</returns>
        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status201Created)]  // For successful registration
        [ProducesResponseType(typeof(ApiResponse<ModelStateDictionary>), StatusCodes.Status400BadRequest)]  // For invalid payload or bad request
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]  // For user already exists or creation failure
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]  // For internal server error
        public async Task<IActionResult> Register(UserRegistrationDto userRegistrationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ModelStateDictionary>.Success(400, ModelState, "Invalid payload"));
            }
            var result = await _authService.RegisterUser(userRegistrationDto);
            if(result.StatusCode == 201)
                return CreatedAtAction(nameof(GetUserById), new { userId = result.Data.Id }, result.Data);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Authenticates a user and provides a JWT access and refresh token if the credentials are valid.
        /// </summary>
        /// <param name="userLoginDto">The DTO containing the username and password for authentication.</param>
        /// <returns>A response with a JWT access token if authentication is successful, or an error message if the credentials are invalid.</returns>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<TokenDto>), StatusCodes.Status200OK)]  // For successful login
        [ProducesResponseType(typeof(ApiResponse<ModelStateDictionary>), StatusCodes.Status400BadRequest)]  // For invalid payload or bad request
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]  // For invalid login credentials
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]  // For internal server error
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ModelStateDictionary>.Success(400, ModelState, "Invalid payload"));
            }
            var result = await _authService.ValidateUser(userLoginDto);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Initiates the password reset process by sending the reset link to the user's email address
        /// </summary>
        /// <param name="forgotPasswordDto">The email address associated with the user account</param>
        /// <returns>A password reset link</returns>
        [Authorize]
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ApiResponse<ForgotPasswordResponseDto>), StatusCodes.Status200OK)]  // For successful password reset link generation
        [ProducesResponseType(typeof(ApiResponse<ModelStateDictionary>), StatusCodes.Status400BadRequest)]  // For invalid payload or bad request
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]  // If no user exists for the provided email address
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]  // For internal server error
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ModelStateDictionary>.Success(400, ModelState, "Invalid payload"));
            }
            var result = await _authService.ForgotPasswordRequestAsync(forgotPasswordDto);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Verifies the user's email and password reset token, preparing the user for a password reset.
        /// </summary>
        /// <param name="email">The user's email address associated with the reset request.</param>
        /// <param name="token">The token for validating the password reset request.</param>
        /// <returns>An object containing the email and token if valid.</returns>
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]  // For successful response
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]  // For invalid requests
        [AllowAnonymous]
        [HttpGet("reset-password")]
        public IActionResult ResetUserPassword(string email, string token)
        {
            // Optionally, render a view for password reset or redirect to a frontend URL.
            return Ok(new { email, token });
        }
        /// <summary>
        /// Resets the user's password using the provided new password and token.
        /// </summary>
        /// <param name="passwordResetDto">The DTO containing the user's email, new password and token for validation.</param>
        /// <returns>The result of the password reset operation, including success or failure information.</returns>
        [Authorize]
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]  // For successful password reset
        [ProducesResponseType(typeof(ApiResponse<ModelStateDictionary>), StatusCodes.Status400BadRequest)]  // For invalid payload or bad request
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)] // If the password reset operation failed
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]  // If user's email provided does not return an existing user
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]  // For internal server error
        public async Task<IActionResult> ResetPassword(PasswordResetDto passwordResetDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ModelStateDictionary>.Success(400, ModelState, "Invalid payload"));
            }
            var result = await _authService.PasswordResetAsync(passwordResetDto);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Refreshes the access token using the provided refresh token, if valid.
        /// </summary>
        /// <param name="tokenDto">The DTO containing the access token and refresh token for validation.</param>
        /// <returns>The new access token if the refresh is successful, or an error message if not.</returns>
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<TokenDto>), StatusCodes.Status200OK)]  // For successful token refresh
        [ProducesResponseType(typeof(ApiResponse<ModelStateDictionary>), StatusCodes.Status400BadRequest)]  // For invalid payload or request
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]  // For invalid refresh token or expired token
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]  // For internal server error
        public async Task<IActionResult> RefreshToken(GetNewTokenDto tokenDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ModelStateDictionary>.Success(400, ModelState, "Invalid payload"));
            }
            var result = await _authService.RefreshToken(tokenDto);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Deletes a user specified by their email address. Only accessible to users with the "Administrator" role.
        /// </summary>
        /// <param name="userEmail">The email address of the user to be deleted.</param>
        /// <returns>A response indicating the result of the delete operation, including success or failure details.</returns>
        [Authorize(Roles = "Administrator")]
        [HttpDelete("delete-user")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)]  // For successful user deletion
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]  // For invalid requests or deletion failures
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]   // For user not found
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)] // For internal server error
        public async Task<IActionResult> DeleteUser(string userEmail)
        {
            var result = await _authService.DeleteUser(userEmail);
            if (result.StatusCode == 204)
                return NoContent();
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Returns details of all registered users. Only accessible to users with the 'Administrator' role.
        /// </summary>
        /// <returns>A response containing the list of users if successful, or an error message if there was an issue retrieving users.</returns>
        [Authorize(Roles = "Administrator")]
        [HttpGet("users")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserResponseDto>>), StatusCodes.Status200OK)]  // For successful retrieval of users
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]   // For server error during retrieval
        public async Task<IActionResult> GetAllRegisteredUsers()
        {
            var result = await _authService.GetUsers();
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Adds specified role(s) to a registered user. Only accessible to users with the 'Administrator' role.
        /// </summary>
        /// <param name="addUserToRoleDto">The data transfer object containing user email and roles to add.</param>
        /// <returns>A response indicating the success or failure of the operation.</returns>
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]  // For successful role addition
        [ProducesResponseType(typeof(ApiResponse<ModelStateDictionary>), StatusCodes.Status400BadRequest)]  // For invalid payload
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]   // For server error
        [Authorize(Roles = "Administrator")]
        [HttpPost("addRolesToUsers")]
        public async Task<IActionResult> AddRolesToUsers(AddUserToRoleDto addUserToRoleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ModelStateDictionary>.Success(400, ModelState, "Invalid payload"));
            }
            var result = await _authService.AddUserToRoleAsync(addUserToRoleDto);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Retrieves the role(s) assigned to a user based on the provided email address. Only accessible to users with the 'Administrator' role.
        /// </summary>
        /// <param name="email">The email address of the user whose roles are to be retrieved.</param>
        /// <returns>A response containing the list of roles if successful, or an error message if the user does not exist or another issue occurs.</returns>
        [Authorize(Roles = "Administrator")]
        [HttpGet("userRoles")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<string>>), StatusCodes.Status200OK)]  // For successful retrieval of roles
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]  // For invalid request or user not found
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)] // For internal server error
        public async Task<IActionResult> GetUserRoles(string email)
        {
            var result = await _authService.GetUserRolesAsync(email);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Changes the password of the currently authenticated user.
        /// </summary>
        /// <param name="changePasswordDto">The data transfer object containing current and new passwords.</param>
        /// <returns>Returns a status indicating success or failure of the password change operation.</returns>
        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]  // For successful password change
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]  // For invalid request or password change failure
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)] // For user not found
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)] // For internal server error
        public async Task<IActionResult> ChangeUserPassword(ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ModelStateDictionary>.Success(400, ModelState, "Invalid payload"));
            }
            var result = await _authService.ChangeUserPasswordAsync(changePasswordDto);
            return StatusCode(result.StatusCode, result); 
        }
        /// <summary>
        /// Changes the email address of the currently authenticated user.
        /// </summary>
        /// <param name="changeEmailDto">The data transfer object containing the new email address.</param>
        /// <returns>Returns a status indicating success or failure of the email change operation, along with an email confirmation token if successful.</returns>
        [Authorize]
        [HttpPost("change-email")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]  // For successful email change
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]  // For invalid request or email change failure
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)] // For user not found
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)] // For internal server error
        public async Task<IActionResult> ChangeUserEmail(ChangeEmailDto changeEmailDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ModelStateDictionary>.Success(400, ModelState, "Invalid payload"));
            }
            var result = await _authService.ChangeUserEmailAsync(changeEmailDto);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Confirms the user's email address using a token sent via email.
        /// </summary>
        /// <param name="token">The email confirmation token.</param>
        /// <param name="email">The user's email address.</param>
        /// <returns>Returns a status indicating the success or failure of the email confirmation process.</returns>
        [AllowAnonymous]
        [HttpGet("confirm-email")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]  // For successful email confirmation
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]  // For invalid token or email
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]  // For user not found
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]  // For unexpected errors
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var result = await _authService.UserEmailConfirmation(token, email);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Confirms the user's new email address using a token sent via email.
        /// </summary>
        /// <param name="token">The email confirmation token.</param>
        /// <param name="oldEmail">The user's old email address.</param>
        /// <param name="newEmail">The user's new email address.</param>
        /// <returns>Returns a status indicating the success or failure of the email confirmation process.</returns>
        [AllowAnonymous]
        [HttpGet("new-email-confirmation")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]  // For successful email confirmation
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]  // For invalid token or email
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]  // For user not found
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]  // For unexpected errors
        public async Task<IActionResult> ConfirmUserEmailChange(string token, string oldEmail, string newEmail)
        {
            var result = await _authService.NewUserEmailConfirmation(token, oldEmail, newEmail);
            return StatusCode(result.StatusCode, result);
        }
        /// <summary>
        /// Retrieves user details by their unique identifier.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>Returns user details if found, or an error message if the user does not exist or an error occurs.</returns>
        [Authorize(Roles = "Administrator")]
        [HttpGet("users/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<UserResponseDto>), StatusCodes.Status200OK)]  // For successful retrieval
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]  // For user not found
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]  // For bad request
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]  // For unexpected errors
        public async Task<IActionResult> GetUserById(string userId)
        {
            var result = await _authService.GetUserById(userId);
            return StatusCode(result.StatusCode, result);
        }
    }
}

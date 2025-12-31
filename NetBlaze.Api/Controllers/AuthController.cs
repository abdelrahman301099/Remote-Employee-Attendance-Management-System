using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetBlaze.Application.Interfaces.ServicesInterfaces;
using NetBlaze.SharedKernel.Dtos.RegisterAndLoginDTOs.Request;
using NetBlaze.SharedKernel.Dtos.RegisterAndLoginDTOs.Response;
using NetBlaze.SharedKernel.Dtos.ResetPasswordDTOs.Request;
using NetBlaze.SharedKernel.Dtos.ResetPasswordDTOs.Response;
using NetBlaze.SharedKernel.Dtos.UserDTOs.Request;
using NetBlaze.SharedKernel.Dtos.UserDTOs.Response;
using NetBlaze.SharedKernel.Enums;
using NetBlaze.SharedKernel.HelperUtilities.General;

namespace NetBlaze.Api.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase, IAuthService
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // ================= AUTH =================

        [HttpPost("register")]
        public async Task<ApiResponse<RegisterResponseDTO>> RegisterAsync(
            [FromBody] RegisterRequestDTO request,
            CancellationToken cancellationToken)
        {
            return await _authService.RegisterAsync(request, cancellationToken);
        }

        [HttpPost("login")]
        
        public async Task<ApiResponse<LogInResponseDTO>> LogInAsync(
            [FromBody] LogInRequestDTO request,
            CancellationToken cancellationToken)
        {
            return await _authService.LogInAsync(request, cancellationToken);
        }

        // ================= USERS =================

        [Authorize($"{nameof(AppRoles.Admin)},{nameof(AppRoles.Employee)}")]
        [HttpGet("users")]
        public async Task<ApiResponse<object>> GetAllUsersAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            return await _authService.GetAllUsersAsync(pageNumber, pageSize);
        }

        [Authorize($"{nameof(AppRoles.Admin)},{nameof(AppRoles.Employee)}")]
        [HttpDelete("users/{userId:long}")]
        public async Task<ApiResponse<bool>> DeleteUserAsync(
            long userId,
            CancellationToken cancellationToken)
        {
            return await _authService.DeleteUserAsync(userId, cancellationToken);
        }

        [Authorize]
        [HttpPut("users")]
        public async Task<ApiResponse<UpdateUserResponseDTO>> UpdateUserAsync(
            [FromBody] UpdateUserRequestDTO request,
            CancellationToken cancellationToken)
        {
            return await _authService.UpdateUserAsync(request, cancellationToken);
        }

        // ================= PASSWORD =================

        [HttpPost("password/forgot")]
        public async Task<ApiResponse<bool>> SendPasswordResetEmailAsync(
            [FromQuery] string email,
            CancellationToken cancellationToken)
        {
            return await _authService.SendPasswordResetEmailAsync(email, cancellationToken);
        }

        [HttpPost("password/verify-code")]
        public async Task<ApiResponse<bool>> VerifyResetCodeAsync(
            [FromQuery] string email,
            [FromQuery] string code,
            CancellationToken cancellationToken)
        {
            return await _authService.VerifyResetCodeAsync(email, code, cancellationToken);
        }

        [HttpPost("password/reset")]
        public async Task<ApiResponse<ResetPasswordResponseDTO>> ResetPasswordAsync(
            [FromBody] ResetPasswordRequestDTO request,
            CancellationToken cancellationToken)
        {
            return await _authService.ResetPasswordAsync(request, cancellationToken);
        }

      
       
    }

}

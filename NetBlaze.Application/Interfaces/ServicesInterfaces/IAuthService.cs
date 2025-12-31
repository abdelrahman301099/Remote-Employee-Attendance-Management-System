using NetBlaze.SharedKernel.Dtos.RegisterAndLoginDTOs.Response;
using NetBlaze.SharedKernel.Dtos.ResetPasswordDTOs.Response;
using NetBlaze.SharedKernel.Dtos.ResetPasswordDTOs.Request;
using NetBlaze.SharedKernel.HelperUtilities.General;
using NetBlaze.SharedKernel.Dtos.UserDTOs.Response;
using NetBlaze.SharedKernel.Dtos.UserDTOs.Request;
using NetBlaze.SharedKernel.Dtos.RegisterAndLoginDTOs.Request;

namespace NetBlaze.Application.Interfaces.ServicesInterfaces
{
    public interface IAuthService
    {
        //Register and Login
        Task<ApiResponse<LogInResponseDTO>> LogInAsync(LogInRequestDTO logInRequestDto, CancellationToken cancellationToken = default);

        Task<ApiResponse<RegisterResponseDTO>> RegisterAsync(RegisterRequestDTO registerRequestDto, CancellationToken cancellationToken = default);

        //Password Reset
        Task<ApiResponse<bool>> SendPasswordResetEmailAsync(string email, CancellationToken cancellationToken = default);

        Task<ApiResponse<bool>> VerifyResetCodeAsync(string email, string code, CancellationToken cancellationToken = default);

        Task<ApiResponse<ResetPasswordResponseDTO>> ResetPasswordAsync(ResetPasswordRequestDTO requestDto, CancellationToken cancellationToken = default);

        //Task SendPasswordResetCodeAsync(string email, string code, CancellationToken cancellationToken = default);

        //User Management
        Task<ApiResponse<bool>> DeleteUserAsync(long UserId, CancellationToken cancellationToken);

        Task<ApiResponse<object>> GetAllUsersAsync(int pageNumber, int pageSize);

        Task<ApiResponse<UpdateUserResponseDTO>> UpdateUserAsync(UpdateUserRequestDTO updateUserDTO, CancellationToken cancellationToken);



    }
}

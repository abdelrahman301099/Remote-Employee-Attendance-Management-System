

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using NetBlaze.Application.Interfaces.General;
using NetBlaze.Application.Interfaces.ServicesInterfaces;
using NetBlaze.Application.Mappings;
using NetBlaze.Domain.Entities.Identity;
using NetBlaze.SharedKernel.Dtos.CachingDTOs;
using NetBlaze.SharedKernel.Dtos.General;
using NetBlaze.SharedKernel.Dtos.General;
using NetBlaze.SharedKernel.Dtos.RegisterAndLoginDTOs.Request;
using NetBlaze.SharedKernel.Dtos.RegisterAndLoginDTOs.Response;
using NetBlaze.SharedKernel.Dtos.ResetPasswordDTOs.Request;
using NetBlaze.SharedKernel.Dtos.ResetPasswordDTOs.Response;
using NetBlaze.SharedKernel.Dtos.UserDTOs.Request;
using NetBlaze.SharedKernel.Dtos.UserDTOs.Response;
using NetBlaze.SharedKernel.Enums;
using NetBlaze.SharedKernel.HelperUtilities.General;
using NetBlaze.SharedKernel.SharedResources;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text.Json;
namespace NetBlaze.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtBearerService _jwtBearerService;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private const int MAX_ATTEMPTS = 3;
        private const int CODE_EXPIRATION_MINUTES = 15;
        public AuthService(IUnitOfWork unitOfWork, IJwtBearerService jwtBearerService, UserManager<User> userManager, RoleManager<Role> roleManager, IDistributedCache cache, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _jwtBearerService = jwtBearerService;
            _userManager = userManager;
            _roleManager = roleManager;
            _cache = cache;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        #region Register and Login

       
        // Register and Login
        public async Task<ApiResponse<RegisterResponseDTO>> RegisterAsync(RegisterRequestDTO registerRequestDto, CancellationToken cancellationToken = default)
        {
            var alreadyExist = await _userManager.FindByEmailAsync(registerRequestDto.Email);
            if (alreadyExist != null)
            {
                return ApiResponse<RegisterResponseDTO>.ReturnFailureResponse("TODO:add localization",HttpStatusCode.Conflict);
            }

            if (!Enum.IsDefined(typeof(AppRoles), registerRequestDto.RoleId))
            {
                return ApiResponse<RegisterResponseDTO>.ReturnFailureResponse(
                    "Invalid role", //TODO:Add Localization
                    HttpStatusCode.BadRequest
                );
            }

            var roleName = ((AppRoles)registerRequestDto.RoleId).ToString();

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                return ApiResponse<RegisterResponseDTO>.ReturnFailureResponse(
                    "Role not found",// TODO:Add Localization
                    HttpStatusCode.BadRequest
                );
            }

            var newUser = new User
            {
                UserName = registerRequestDto.UserName,
                Email = registerRequestDto.Email,
                DisplayName = registerRequestDto.FullName,
                PhoneNumber = "0000000000"
            };

            var createResult = await _userManager.CreateAsync(newUser, registerRequestDto.Password);

            if (!createResult.Succeeded)
            {
                return ApiResponse<RegisterResponseDTO>.ReturnFailureResponse(
                    string.Join(", ", createResult.Errors.Select(e => e.Description)),
                    HttpStatusCode.BadRequest
                );
            }

            var roleResult = await _userManager.AddToRoleAsync(newUser, roleName);

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(newUser);

                return ApiResponse<RegisterResponseDTO>.ReturnFailureResponse(
                    "Error assigning role",
                    HttpStatusCode.BadRequest,
                    roleResult.Errors.Select(e => e.Description).ToArray()
                );
            }


            var registerResponseDto = new RegisterResponseDTO
            {
                UserName = newUser.UserName,
                Email = newUser.Email,
                DisplayName = newUser.DisplayName
            };  
            return ApiResponse<RegisterResponseDTO>.ReturnSuccessResponse(registerResponseDto, "User registered successfully", HttpStatusCode.Created);
        }

        public async Task<ApiResponse<LogInResponseDTO>> LogInAsync(LogInRequestDTO logInRequestDto, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(logInRequestDto.Email);

            if (user == null )
            {
                return ApiResponse<LogInResponseDTO>.ReturnFailureResponse("Invalid email or password", HttpStatusCode.Unauthorized);
            }

            var validPassword = await _userManager.CheckPasswordAsync(user, logInRequestDto.Password);

            if (!validPassword)
            {
                return ApiResponse<LogInResponseDTO>.ReturnFailureResponse("TODO:Addd localization", HttpStatusCode.Unauthorized);
            }

            var roles = await _userManager.GetRolesAsync(user);

            var tokenRequest = new GenerateTokenRequestDto(user.Id, user.UserName, user.Email, roles.ToList());

            var token = _jwtBearerService.GenerateToken(tokenRequest);

            var logInResponseDto = new LogInResponseDTO
            {
                AccessToken = token,
                Expiration = DateTime.UtcNow.AddMinutes(30)
            };

            return ApiResponse<LogInResponseDTO>.ReturnSuccessResponse(logInResponseDto, "TODO:Addd localization", HttpStatusCode.OK);
        }
        #endregion

        #region Password Reset

        // Password Reset
        public async Task<ApiResponse<bool>> SendPasswordResetEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return ApiResponse<bool>.ReturnFailureResponse(
                    "User Not found", //TODO: Add Localization
                    HttpStatusCode.NotFound);
            }

            
            var rateLimitKey = $"password_reset_rate:{user.Id}";
            var attemptCount = await _cache.GetStringAsync(rateLimitKey, cancellationToken);

            if (int.TryParse(attemptCount, out int count) && count >= 3) 
            {
                return ApiResponse<bool>.ReturnFailureResponse(
                    "Too many reset requests. Please try again later",
                    HttpStatusCode.TooManyRequests);
            }

            var otp = GenerateSecureOtp(6);
            var cacheKey = $"password_reset:{user.Id}";

            var cacheData = new PasswordResetCacheDTO
            {
                Code = otp,
                UserId = user.Id.ToString(),
                Email = user.Email!,
                CreatedAt = DateTime.UtcNow,
                AttemptCount = 0,
                IsVerified = false 
            };

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CODE_EXPIRATION_MINUTES)
            };

            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(cacheData),
                cacheOptions,
                cancellationToken
            );

            
            var rateLimitOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
            };
            var newCount = (count + 1).ToString();
            await _cache.SetStringAsync(rateLimitKey, newCount, rateLimitOptions, cancellationToken);

            await SendPasswordResetCodeAsync(user.Email!, otp, cancellationToken);

            return ApiResponse<bool>.ReturnSuccessResponse(
                true,
                "Reset code sent successfully",
                HttpStatusCode.OK);
        }

        public async Task<ApiResponse<bool>> VerifyResetCodeAsync(string email, string code, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return ApiResponse<bool>.ReturnFailureResponse(
                    "Invalid request",
                    HttpStatusCode.BadRequest);
            }

            var cacheKey = $"password_reset:{user.Id}";
            var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (string.IsNullOrEmpty(cachedData))
            {
                return ApiResponse<bool>.ReturnFailureResponse(
                    "Reset code expired or not found",
                    HttpStatusCode.BadRequest);
            }

            var resetData = JsonSerializer.Deserialize<PasswordResetCacheDTO>(cachedData);
            if (resetData == null)
            {
                return ApiResponse<bool>.ReturnFailureResponse(
                    "Invalid reset data",
                    HttpStatusCode.BadRequest);
            }

            if (resetData.AttemptCount >= MAX_ATTEMPTS)
            {
                await _cache.RemoveAsync(cacheKey, cancellationToken);
                return ApiResponse<bool>.ReturnFailureResponse(
                    "Too many attempts. Please request a new code",
                    HttpStatusCode.TooManyRequests);
            }

            // Use constant-time comparison to prevent timing attacks
            bool codeMatch = System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(resetData.Code),
                System.Text.Encoding.UTF8.GetBytes(code)
            );

            if (!codeMatch)
            {
                resetData.AttemptCount++;
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CODE_EXPIRATION_MINUTES)
                };
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(resetData),
                    cacheOptions,
                    cancellationToken
                );

                var remainingAttempts = MAX_ATTEMPTS - resetData.AttemptCount;
                return ApiResponse<bool>.ReturnFailureResponse(
                    $"Invalid code. {remainingAttempts} attempts remaining",
                    HttpStatusCode.BadRequest);
            }

            // Mark as verified
            resetData.IsVerified = true;
            resetData.AttemptCount = 0; // Reset attempt count after successful verification
            var verifyOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CODE_EXPIRATION_MINUTES)
            };
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(resetData),
                verifyOptions,
                cancellationToken
            );

            return ApiResponse<bool>.ReturnSuccessResponse(
                true,
                "Code verified successfully",
                HttpStatusCode.OK);
        }

        public async Task<ApiResponse<ResetPasswordResponseDTO>> ResetPasswordAsync(
            ResetPasswordRequestDTO requestDto,
            CancellationToken cancellationToken = default)
        {
           
                var user = await _userManager.FindByEmailAsync(requestDto.Email);
                if (user == null)
                {
                    return ApiResponse<ResetPasswordResponseDTO>.ReturnFailureResponse(
                        "Invalid request",//TODO: Add Localization
                        HttpStatusCode.BadRequest);
                }

                var cacheKey = $"password_reset:{user.Id}";
                var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);

                if (string.IsNullOrEmpty(cachedData))
                {
                    return ApiResponse<ResetPasswordResponseDTO>.ReturnFailureResponse(
                        "Reset code expired or not found",//TODO: Add Localization
                        HttpStatusCode.BadRequest);
                }

                var resetData = JsonSerializer.Deserialize<PasswordResetCacheDTO>(cachedData);
                if (resetData == null)
                {
                    return ApiResponse<ResetPasswordResponseDTO>.ReturnFailureResponse(
                        "Invalid reset data",//TODO: Add Localization
                        HttpStatusCode.BadRequest);
                }

                bool codeMatch = System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
                    System.Text.Encoding.UTF8.GetBytes(resetData.Code),
                    System.Text.Encoding.UTF8.GetBytes(requestDto.Code)
                );

                if (!codeMatch || !resetData.IsVerified)
                {
                    return ApiResponse<ResetPasswordResponseDTO>.ReturnFailureResponse(
                        "Invalid or unverified reset code",//TODO: Add Localization
                        HttpStatusCode.BadRequest);
                }
            

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                var resetResult = await _userManager.ResetPasswordAsync(user, token, requestDto.NewPassword);

                if (!resetResult.Succeeded)
                {
                    var errors = resetResult.Errors.Select(e => e.Description).ToArray();

                    return ApiResponse<ResetPasswordResponseDTO>.ReturnFailureResponse(
                        "Password reset failed",//TODO: Add Localization
                        HttpStatusCode.BadRequest,
                        errors);
                }

                
                await _cache.RemoveAsync(cacheKey, cancellationToken);

                return ApiResponse<ResetPasswordResponseDTO>.ReturnSuccessResponse(
                    new ResetPasswordResponseDTO
                    {
                       IsPasswordResetSuccessful = true
                    },
                    "Password Reset Successfully",//TODO: Add Localization
                    HttpStatusCode.OK);
            
          
        }


      
        #endregion

        #region User Management


        // User Management
        public async Task<ApiResponse<bool>> DeleteUserAsync(long userId, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Repository.GetByIdAsync<User>(true, userId, cancellationToken);

            if (user == null)
            {
                return ApiResponse<bool>.ReturnFailureResponse(
                   " UserNotFound",// TODO: Add Localization
                    HttpStatusCode.NotFound
                );
            }

            if (user.IsDeleted)
            {
                return ApiResponse<bool>.ReturnFailureResponse(
                    "UserAlreadyDeleted",// TODO: Add Localization
                    HttpStatusCode.BadRequest
                );
            }
            var deletedBy = _httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sid)?.Value;

            // Get current user from claims/context (inject IHttpContextAccessor or similar)
           // var deletedBy = GetCurrentUserId(); // Implement based on your auth context

            user.IsDeleted = true;
            user.DeletedAt = DateTimeOffset.UtcNow;
            user.DeletedBy = deletedBy;
            user.IsActive = false;

            

            _unitOfWork.Repository.Update(user);
            await _unitOfWork.Repository.CompleteAsync(cancellationToken);

            return ApiResponse<bool>.ReturnSuccessResponse(
                true,
                "User deleted successfully",// TODO: Add Localization
                HttpStatusCode.OK
            );
        }

        public async Task<ApiResponse<UpdateUserResponseDTO>> UpdateUserAsync(UpdateUserRequestDTO updateUserDTO, CancellationToken cancellationToken)
        {

            if (string.IsNullOrWhiteSpace(updateUserDTO.UserId.ToString()) ||
                string.IsNullOrWhiteSpace(updateUserDTO.UserName) ||
                string.IsNullOrWhiteSpace(updateUserDTO.Email) ||
                string.IsNullOrWhiteSpace(updateUserDTO.DisplayName))
            {
                return ApiResponse<UpdateUserResponseDTO>.ReturnFailureResponse(
                    "InvalidRequest",// TODO: Add Localization
                    HttpStatusCode.BadRequest);
            }

            if (!long.TryParse(updateUserDTO.UserId.ToString(), out var uid))
            {
                return ApiResponse<UpdateUserResponseDTO>.ReturnFailureResponse(
                    "InvalidRequest",// todo: Add Localization
                    HttpStatusCode.BadRequest);
            }

            var currentUserId = _httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sid)?.Value;

            var IsAdmin =  _httpContextAccessor.HttpContext?.User.IsInRole("Admin") == true;
           
            if (currentUserId != updateUserDTO.UserId.ToString() && !IsAdmin) 
            {
                return ApiResponse<UpdateUserResponseDTO>.ReturnFailureResponse(
                    "Unauthorized to update this user",
                    HttpStatusCode.Forbidden);
            }

            var user = await _unitOfWork.Repository.GetByIdAsync<User>(false, uid, cancellationToken).ConfigureAwait(false);

            if (user == null)
            {
                return ApiResponse<UpdateUserResponseDTO>.ReturnFailureResponse(
                   " UserNotFound",// todo: Add Localization
                    HttpStatusCode.NotFound);
            }

            
            if (user.IsDeleted)
            {
                return ApiResponse<UpdateUserResponseDTO>.ReturnFailureResponse(
                    "Cannot update a deleted user",
                    HttpStatusCode.BadRequest);
            }

            var existingUserWithEmail = await _unitOfWork.Repository
                .GetSingleAsync<User>(
                   false,
                    u => u.Email.ToLower() == updateUserDTO.Email.ToLower() && u.Id != uid,
                    cancellationToken);

            if (existingUserWithEmail != null)
            {
                return ApiResponse<UpdateUserResponseDTO>.ReturnFailureResponse(
                    "Email already in use",
                    HttpStatusCode.BadRequest);
            }

            user.UserName = updateUserDTO.UserName.Trim();
            user.DisplayName = updateUserDTO.DisplayName.Trim();
            user.Email = updateUserDTO.Email.Trim();

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                var errors = updateResult.Errors.Select(e => e.Description).ToArray();
                return ApiResponse<UpdateUserResponseDTO>.ReturnFailureResponse(
                    Messages.ErrorOccurredInServer,
                    HttpStatusCode.BadRequest,
                    errors);
            }

            
            if (updateUserDTO.RoleId.HasValue)
            {
                var roleUpdateResult = await UpdateUserRoleAsync(user, updateUserDTO.RoleId.Value, cancellationToken);

                if (!roleUpdateResult.Succeeded)
                {
                    
                    user.UserName = await GetOriginalValueAsync(uid, nameof(User.UserName), cancellationToken);

                    user.DisplayName = await GetOriginalValueAsync(uid, nameof(User.DisplayName), cancellationToken);

                    user.Email = await GetOriginalValueAsync(uid, nameof(User.Email), cancellationToken);

                    await _userManager.UpdateAsync(user);

                    return ApiResponse<UpdateUserResponseDTO>.ReturnFailureResponse(
                        roleUpdateResult.ErrorMessage,
                        HttpStatusCode.BadRequest);
                }
            }

            var responseDto = new UpdateUserResponseDTO
            {
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.DisplayName,
                RoleId = updateUserDTO.RoleId ?? 0
            };

            return ApiResponse<UpdateUserResponseDTO>.ReturnSuccessResponse(
                responseDto,
                "UserUpdated",//TODO: Add Localization
                HttpStatusCode.OK);
        }
       
        private async Task<(bool Succeeded, string ErrorMessage)> UpdateUserRoleAsync(User user, long roleId, CancellationToken cancellationToken)
        {
          

            var role = await _roleManager.FindByIdAsync(roleId.ToString());

            if (role == null)
            {
                return (false, "Invalid Role Id");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    return (false, "ErrorAssigningRole");// todo: Add Localization
                }
            }

            var addResult = await _userManager.AddToRoleAsync(user, role.Name);
            if (!addResult.Succeeded)
            {
                return (false, "ErrorAssigningRole");//todo: Add Localization
            }

            return (true, string.Empty);
        }

       
        public async Task<ApiResponse<object>> GetAllUsersAsync(
            int pageNumber,
            int pageSize)
        {
            
            if (pageNumber < 1 || pageSize < 1)
            {
                return ApiResponse<object>.ReturnFailureResponse(
                    "Invalid page number or page size",
                    HttpStatusCode.BadRequest);
            }

            
            var usersQuery = _unitOfWork.Repository.GetQueryable<User>()
                .Where(u => !u.IsDeleted)
                .Select(u => new UserResponseDTO
                {
                    DisplayName = u.DisplayName,
                    UserName = u.UserName,
                    UserEmail = u.Email
                });

            
            var paginatedUsers = await usersQuery.PaginatedListAsync(pageNumber, pageSize);

            if (paginatedUsers == null || !paginatedUsers.Items.Any())
            {
                return ApiResponse<object>.ReturnSuccessResponse(
                    paginatedUsers,
                    "No users found",
                    HttpStatusCode.OK);
            }

            return ApiResponse<object>.ReturnSuccessResponse(
                paginatedUsers,
                "All Users",
                HttpStatusCode.OK);
        }
        #endregion

        #region Helper
        private static string GenerateSecureOtp(int length)
        {
            const string digits = "0123456789";
            var bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = digits[bytes[i] % digits.Length];
            }
            return new string(chars);
        }


        private string GetEmailBody(string code)
        {
            return $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #333;'>Password Reset Request</h2>
                    <p>You have requested to reset your password. Use the code below to reset your password:</p>
                    <div style='background-color: #f4f4f4; padding: 15px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 5px; margin: 20px 0;'>
                        {code}
                    </div>
                    <p style='color: #666;'>This code will expire in 15 minutes.</p>
                    <p style='color: #666;'>If you didn't request this, please ignore this email.</p>
                </div>
            </body>
            </html>
        ";
        }


        private async Task<string> GetOriginalValueAsync(long userId, string propertyName, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Repository.GetByIdAsync<User>(false, userId, cancellationToken);
            return propertyName switch
            {
                nameof(User.UserName) => user?.UserName ?? string.Empty,
                nameof(User.DisplayName) => user?.DisplayName ?? string.Empty,
                nameof(User.Email) => user?.Email ?? string.Empty,
                _ => string.Empty,
            };
        }



        public async Task SendPasswordResetCodeAsync(string email, string code, CancellationToken cancellationToken = default)
        {

            var section = _configuration.GetSection("EmailSettings");
            var host = section["Host"];
            var portStr = section["Port"];
            var username = section["Username"];
            var password = section["Password"];
            var from = section["From"];
            var enableSslStr = section["EnableSsl"];

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(from))
            {

                throw new InvalidOperationException("Email settings are not properly configured");
            }

            var port = int.TryParse(portStr, out var p) ? p : 587;
            var enableSsl = bool.TryParse(enableSslStr, out var s) ? s : true;

            using var smtp = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(username, password)
            };

            using var message = new MailMessage(from, email)
            {
                Subject = "Password Reset Code",
                Body = GetEmailBody(code),
                IsBodyHtml = true
            };

            await smtp.SendMailAsync(message, cancellationToken);
        }


        #endregion


    }
}

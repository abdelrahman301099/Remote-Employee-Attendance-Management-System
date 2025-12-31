
namespace NetBlaze.SharedKernel.Dtos.CachingDTOs
{
    public class PasswordResetCacheDTO
    {
        public string Code { get; set; } = null!;

        public string UserId { get; set; } = null!;

        public string Email { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public int AttemptCount { get; set; } = 0;

        public bool IsVerified { get; set; }     }
}


namespace NetBlaze.SharedKernel.Dtos.RegisterAndLoginDTOs.Response
{
    public class LogInResponseDTO
    {
        public string AccessToken { get; set; } = null!;

        public string? RefreshToken { get; set; }

        public DateTime Expiration { get; set; }
    }
}



namespace NetBlaze.SharedKernel.Dtos.UserDTOs.Response
{
    public class UpdateUserResponseDTO
    {
        public long Id { get; set; }

        public string UserName { get; set; } 

        public string Email { get; set; } 

        public string FullName { get; set; }

        public long RoleId { get; set; }

        public bool IsActive { get; set; }
    }
}

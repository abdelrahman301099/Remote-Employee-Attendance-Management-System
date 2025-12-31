using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetBlaze.SharedKernel.Dtos.UserDTOs.Response
{
    public class UserResponseDTO
    {
        public string UserName { get; set; }

        public string DisplayName { get; set; }

        public string UserEmail { get; set; }

        public long RoleId { get; set; }
    }
}

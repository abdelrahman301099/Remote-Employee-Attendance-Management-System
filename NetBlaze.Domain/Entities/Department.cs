using NetBlaze.Domain.Common;
using NetBlaze.Domain.Entities.Identity;

namespace NetBlaze.Domain.Entities
{
    public class Department:BaseEntity<int>
    {
        public string DepartmentName { get; set; }

        public string Description { get; set; }

        public long? ManagerId { get; set; }

        //navigational properties
        public virtual User Manager { get; set; }= null!;
        
        public virtual ICollection<User> Employees { get; private set; } = [];
    }
}

using NetBlaze.Domain.Common;
using NetBlaze.Domain.Entities.Identity;

namespace NetBlaze.Domain.Entities
{
    public class UserAttendancePolicy : BaseEntity<long>
    {
        
        
        public DateTime EffectiveFrom { get; private set; }
        
        public DateTime? EffectiveTo { get; private set; }
        
        public bool IsActive { get; private set; } = true;
        
        
        public string? Notes { get; private set; }
        
        // Navigational Properties
         public long UserId { get; private set; }
        
        public virtual User User { get; private set; } = null!;
        
        public int AttendancePolicyId { get; private set; }

        public virtual AttendancePolicy AttendancePolicy { get; private set; } = null!;
        
        public long? AssignedByUserId { get; private set; }

        public virtual User? AssignedByUser { get; private set; }
        
    }
    
}
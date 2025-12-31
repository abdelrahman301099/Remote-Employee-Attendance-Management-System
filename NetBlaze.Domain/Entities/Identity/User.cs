using Microsoft.AspNetCore.Identity;

namespace NetBlaze.Domain.Entities.Identity
{
    public class User : IdentityUser<long>
    {
        // Properties

        public string DisplayName { get;  set; } = null!;

        public DateTimeOffset CreatedAt { get;  set; }

        public string? CreatedBy { get;  set; }

        public DateTimeOffset? LastModifiedAt { get;  set; }

        public string? LastModifiedBy { get;  set; }

        public DateTimeOffset? DeletedAt { get; set; }

        public string? DeletedBy { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; }



        // Navigational Properties

        public ICollection<UserRole> UserRoles { get; set; } = [];

        public int? DepartmentId { get; set; }
        public virtual Department? Department { get; set; }
        public virtual ICollection<AttendanceLog> AttendanceLogs { get; set; } = [];
        public virtual ICollection<FidoCredential> FidoCredentials { get; set; } = [];
        public virtual ICollection<RandomAttendanceCheck> RandomAttendanceChecks { get; set; } = [];
        public virtual ICollection<UserAttendancePolicy> UserAttendancePolicies { get; set; } = [];
    }
}

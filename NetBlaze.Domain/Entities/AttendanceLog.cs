using NetBlaze.Domain.Common;

using NetBlaze.Domain.Entities.Identity;

namespace NetBlaze.Domain.Entities
{
    public class AttendanceLog:BaseEntity<long>
    {
    public DateOnly AttendanceDate { get; set; }

       public TimeOnly CheckInTime { get; set; }

       public TimeOnly? CheckOutTime { get; set; }

       public int TotalMinutsWorked { get; set; }

       public int? LateMinutes { get;  set; }

       public int? EarlyLeaveMinutes { get;  set; }

        //Navigational Properties
        public long EmployeeId { get; set; }

        public virtual User Employee { get; set; } = null!;

        public int AttendancePolicyId { get; set; }

        public virtual AttendancePolicy AttendancePolicy { get; set; } = null!;
    }
}

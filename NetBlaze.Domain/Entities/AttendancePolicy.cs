using NetBlaze.Domain.Common;

namespace NetBlaze.Domain.Entities
{
    public class AttendancePolicy: BaseEntity<int>
    {
        public string PolicyName { get; set; }

        public TimeOnly WorkingHoursStart { get; set; }

        public TimeOnly WorkingHoursEnd { get; set; }

        public int GracePeriodInMinutes { get; set; }

        public int latePenaltyInMinutes { get; set; }

        public int MinRandomCheckInPerDay { get; set; }

        public int MaxRandomCheckInPerDay { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime EffectiveTo { get; set; }

        // Navigational Properties

        public virtual ICollection<PolicyRule> PolicyRules { get; private set; } = [];
        public virtual ICollection<UserAttendancePolicy> UserAttendancePolicies { get; private set; } = [];
    }
}

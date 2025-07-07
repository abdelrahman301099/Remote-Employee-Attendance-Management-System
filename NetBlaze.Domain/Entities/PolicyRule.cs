
using NetBlaze.Domain.Common;
using NetBlaze.SharedKernel.Enums;

namespace NetBlaze.Domain.Entities
{
    public class PolicyRule:BaseEntity<int>
    {
        string RuleTypeString { get; set; }

        public string Condition { get; set; }

        public string Action { get; set; } 

        public double  PenaltyValue { get; set; }

        public string Description { get; set; }

        public RuleType? RuleType { get; set; }

        //navigational properties
        public int AttendancePolicyId { get; set; }
        
        public virtual AttendancePolicy AttendancePolicy { get; set; } = null!;

}
   
}

using NetBlaze.Domain.Common;
using NetBlaze.Domain.Entities.Identity;
using NetBlaze.SharedKernel.Enums;

namespace NetBlaze.Domain.Entities
{
    public class RandomAttendanceCheck : BaseEntity<int>
    {
        public DateTime CheckedTime { get; set; }

        public DateTime ResponseDeadLine { get; set; }

        public DateTime ResponseTime { get; set; }

        public RandomCheckStatus RandomCheckStatus { get; set; }


        //navigational properties
        public int FidoCredentialId { get; set; }

        public virtual FidoCredential FidoCredential { get; set; }

        public long UserId { get; set; }

        public virtual User User { get; set; } = null!;
    }

}

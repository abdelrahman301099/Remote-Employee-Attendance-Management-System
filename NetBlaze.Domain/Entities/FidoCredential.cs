using NetBlaze.Domain.Common;
using NetBlaze.Domain.Entities.Identity;

namespace NetBlaze.Domain.Entities
{
    public class FidoCredential:BaseEntity<int>
    {
        public string CredentialPublicKey { get; set; }

        public string credentialIdBase64 { get; set; }

        public int SignCount { get; set; }

        public string DeviceName { get; set; }

        public DateTime RegisteredAt { get; set; }

        public Boolean IsActive { get; set; }

        //navigational properties
        public long UserId { get; set; }

        public virtual User User { get; set; }



    }
}

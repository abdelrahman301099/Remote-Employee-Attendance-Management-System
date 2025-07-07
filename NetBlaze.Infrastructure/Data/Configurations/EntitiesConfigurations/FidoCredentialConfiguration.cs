using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetBlaze.Domain.Entities;
using NetBlaze.SharedKernel.HelperUtilities.Constants;

namespace NetBlaze.Infrastructure.Data.Configurations.EntitiesConfigurations
{
    internal class FidoCredentialConfiguration : IEntityTypeConfiguration<FidoCredential>
    {
        public void Configure(EntityTypeBuilder<FidoCredential> builder)
        {
            
            builder.ToTable("FidoCredentials");

            builder
                .Property(e => e.CredentialPublicKey)
                .HasMaxLength(CommonStringLength.ExtraLongContentText)
                .IsRequired();

            builder
                .Property(e => e.credentialIdBase64)
                .HasMaxLength(CommonStringLength.ExtraLongContentText)
                .IsRequired();

            builder
                .Property(e => e.SignCount)
                .IsRequired();

            builder
                .Property(e => e.DeviceName)
                .HasMaxLength(CommonStringLength.StandardText)
                .IsRequired();

            builder
                .Property(e => e.RegisteredAt)
                .IsRequired();

            builder
                .Property(e => e.IsActive)
                .IsRequired();

            
            builder
                .HasOne(fc => fc.User)
                .WithMany(u => u.FidoCredentials)
                .HasForeignKey(fc => fc.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder
                .HasIndex(fc => fc.UserId)
                .HasDatabaseName("IX_FidoCredential_UserId");

            builder
                .HasIndex(fc => new { fc.UserId, fc.IsActive })
                .HasDatabaseName("IX_FidoCredential_User_IsActive");

            builder
                .HasIndex(fc => fc.credentialIdBase64)
                .IsUnique()
                .HasDatabaseName("IX_FidoCredential_CredentialId");
        }
    }
}


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetBlaze.Domain.Entities;

namespace NetBlaze.Infrastructure.Data.Configurations.EntitiesConfigurations
{
    internal class RandomAttendanceCheckConfiguration : IEntityTypeConfiguration<RandomAttendanceCheck>
    {
        public void Configure(EntityTypeBuilder<RandomAttendanceCheck> builder)
        {
            
            builder.ToTable("RandomAttendanceChecks");

            builder
                .Property(e => e.CheckedTime)
                .IsRequired();

            builder
                .Property(e => e.ResponseDeadLine)
                .IsRequired();

            builder
                .Property(e => e.ResponseTime)
                .IsRequired();

            builder
                .Property(e => e.RandomCheckStatus)
                .IsRequired();

            builder
                .HasOne(rac => rac.User)
                .WithMany(u => u.RandomAttendanceChecks)
                .HasForeignKey(rac => rac.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder
                .HasOne(rac => rac.FidoCredential)
                .WithMany()
                .HasForeignKey(rac => rac.FidoCredentialId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder
                .HasIndex(rac => new { rac.UserId, rac.CheckedTime })
                .HasDatabaseName("IX_RandomAttendanceCheck_User_CheckedTime");

            builder
                .HasIndex(rac => rac.CheckedTime)
                .HasDatabaseName("IX_RandomAttendanceCheck_CheckedTime");

            builder
                .HasIndex(rac => rac.RandomCheckStatus)
                .HasDatabaseName("IX_RandomAttendanceCheck_Status");

            builder
                .HasIndex(rac => rac.FidoCredentialId)
                .HasDatabaseName("IX_RandomAttendanceCheck_FidoCredentialId");
        }
    }
}


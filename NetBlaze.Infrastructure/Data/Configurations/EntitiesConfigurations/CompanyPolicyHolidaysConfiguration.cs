using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetBlaze.Domain.Entities;
using NetBlaze.SharedKernel.HelperUtilities.Constants;

namespace NetBlaze.Infrastructure.Data.Configurations.EntitiesConfigurations
{
    internal class CompanyPolicyHolidaysConfiguration : IEntityTypeConfiguration<CompanyPolicyHolidays>
    {
        public void Configure(EntityTypeBuilder<CompanyPolicyHolidays> builder)
        {
            
            builder.ToTable("CompanyPolicyHolidays");

            builder
                .Property(e => e.HolidayDate)
                .IsRequired();

            builder
                .Property(e => e.Description)
                .HasMaxLength(CommonStringLength.LongText)
                .IsRequired(false);

            builder
                .Property(e => e.HolidayName)
                .HasMaxLength(CommonStringLength.StandardText)
                .IsRequired(false);

            builder
                .Property(e => e.Mandatory)
                .IsRequired();

            builder
                .Property(e => e.IsRecurring)
                .IsRequired();

            builder
                .HasIndex(cph => cph.HolidayDate)
                .HasDatabaseName("IX_CompanyPolicyHolidays_HolidayDate");

            builder
                .HasIndex(cph => new { cph.HolidayDate, cph.IsRecurring })
                .HasDatabaseName("IX_CompanyPolicyHolidays_Date_Recurring");
        }
    }
}


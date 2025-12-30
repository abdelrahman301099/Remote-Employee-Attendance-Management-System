using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetBlaze.Domain.Entities;
using NetBlaze.Domain.Entities.Identity;
using NetBlaze.Infrastructure.Data.Configurations.MiscConfigurations;
using System.Reflection;

namespace NetBlaze.Infrastructure.Data.DatabaseContext
{
    public class ApplicationDbContext : IdentityDbContext<User,
                                                          Role,
                                                          long,
                                                          IdentityUserClaim<long>,
                                                          UserRole,
                                                          IdentityUserLogin<long>,
                                                          IdentityRoleClaim<long>,
                                                          IdentityUserToken<long>>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }


        public DbSet<SampleEntity> Samples => Set<SampleEntity>();

        public DbSet<AttendancePolicy> AttendancePolicies => Set<AttendancePolicy>();

        public DbSet<CompanyPolicyHolidays> CompanyPolicyHolidays => Set<CompanyPolicyHolidays>();

        public DbSet<Department> Departments => Set<Department>();

        public DbSet<FidoCredential> FidoCredentials => Set<FidoCredential>();

        public DbSet<RandomAttendanceCheck> RandomAttendanceChecks => Set<RandomAttendanceCheck>();

        public DbSet<AttendanceLog> AttendanceLogs => Set<AttendanceLog>();

        public DbSet<PolicyRule> PolicyRules => Set<PolicyRule>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            builder.ConfigureIdentityTablesNames();

            builder.SetGlobalIsDeletedFilterToAllEntities();
        }
    }
}
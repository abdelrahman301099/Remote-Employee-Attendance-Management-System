using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetBlaze.Application.Interfaces.General;
using NetBlaze.Domain.Entities.Identity;
using NetBlaze.SharedKernel.Enums;
using System;


namespace NetBlaze.Infrastructure.Data.DatabaseContext
{
    public static class InitializerExtensions
    {
        public static async Task InitializeDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();

            await initializer.InitializeAsync();

            await initializer.SeedAsync();
        }
    }

    public class ApplicationDbContextInitializer
    {
        private readonly ILogger<ApplicationDbContextInitializer> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public ApplicationDbContextInitializer(ILogger<ApplicationDbContextInitializer> logger,
                                               ApplicationDbContext context,
                                               IUnitOfWork unitOfWork,
                                               UserManager<User> userManager,
                                               RoleManager<Role> roleManager)
        {
            _logger = logger;
            _context = context;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task InitializeAsync()
        {
            await _context.Database.MigrateAsync();
        }

        public async Task SeedAsync()
        {
            await TrySeedAsync();
        }

        public async Task TrySeedAsync()
        {
            // WARNING: Missing with methods order can lead to errors when seeding the database for the first time.

            await TrySeedSystemPredefinedRolesAsync();

            await TrySeedRootAccountAsync();
        }

        private async Task TrySeedSystemPredefinedRolesAsync()
        {
            var admin = await _unitOfWork.Repository.GetSingleAsync<Role>(
               true, x => x.NormalizedName == "ADMIN");
            var hr = await _unitOfWork.Repository.GetSingleAsync<Role>(
                true, x => x.NormalizedName == "HR");
            var employee = await _unitOfWork.Repository.GetSingleAsync<Role>(
                true, x => x.NormalizedName == "EMPLOYEE");

            var toAdd = new List<Role>();

            if (admin is null)
            {
                toAdd.Add(Role.Create("Admin"));
            }
            if (hr is null)
            {
                toAdd.Add(Role.Create("HR"));
            }
            if (employee is null)
            {
                toAdd.Add(Role.Create("Employee"));
            }

            if (toAdd.Count > 0)
            {
                _unitOfWork.Repository.AddRange<Role>(toAdd);
                await _unitOfWork.Repository.CompleteAsync();
                _logger.LogInformation("Seeded predefined roles: {Roles}", string.Join(", ", toAdd.Select(r => r.Name)));
            }
        }

        private async Task TrySeedRootAccountAsync()
        {
            const string email = "superadmin@gmail.com";
            const string defaultPassword = "Super@123";
            const string userName = "superadmin";
            const string displayName = "Super Admin";

            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser is not null)
            {
                var roles = await _userManager.GetRolesAsync(existingUser);
                if (!roles.Any(r => string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase)))
                {
                    var addRoleResult = await _userManager.AddToRoleAsync(existingUser, "Admin");
                    if (!addRoleResult.Succeeded)
                    {
                        _logger.LogError("Failed assigning Admin role to existing root account: {Errors}",
                            string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
                    }
                    else
                    {
                        _logger.LogInformation("Assigned Admin role to existing root account: {Email}", email);
                    }
                }
                return;
            }

            var adminRole = await _roleManager.FindByNameAsync("Admin");
            if (adminRole is null)
            {
                _logger.LogWarning("Admin role not found while seeding root account. Seeding roles now.");
                await TrySeedSystemPredefinedRolesAsync();
                adminRole = await _roleManager.FindByNameAsync("Admin");
                if (adminRole is null)
                {
                    _logger.LogError("Admin role still not found after seeding. Aborting root account creation.");
                    return;
                }
            }

            var newUser = new User
            {
                UserName = userName,
                Email = email,
                DisplayName = displayName,
                PhoneNumber = "0000000000",
                EmailConfirmed = true,
                IsActive = true
            };

            var createResult = await _userManager.CreateAsync(newUser, defaultPassword);

            if (!createResult.Succeeded)
            {
                _logger.LogError("Failed to seed root account: {Errors}",
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
                return;
            }

            var roleAssignResult = await _userManager.AddToRoleAsync(newUser, "Admin");
            if (!roleAssignResult.Succeeded)
            {
                _logger.LogError("Root account created but failed to assign Admin role: {Errors}",
                    string.Join(", ", roleAssignResult.Errors.Select(e => e.Description)));
                return;
            }

            _logger.LogInformation("Seeded Super Admin account: {Email}", email);
        }
    }
}

using AdminPanel.Application.Common.Interfaces;
using AdminPanel.Domain.Constants;
using AdminPanel.Domain.Entities.Identity;
using AdminPanel.Domain.Entities.Tenancy;
using AdminPanel.Infrastructure.Data;
using AdminPanel.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AdminPanel.Infrastructure.Data.Seeders;

public static class DatabaseSeeder
{
    private static readonly AppSettings? _appSettings;
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
       
    await context.Database.MigrateAsync();

        // Seed Permissions
        if (!await context.Permissions.AnyAsync())
        {
            var allPermissions = Permissions.GetAllPermissions();
            int order = 1;
            foreach (var (code, module, action, displayNameAr) in allPermissions)
            {
                context.Permissions.Add(new Permission
                {
                    Code = code,
                    Module = module,
                    Action = action,
                    DisplayNameAr = displayNameAr,
                    DisplayNameEn = $"{module}.{action}",
                    DisplayOrder = order++,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await context.SaveChangesAsync();
        }

        // Seed Tenant
        if (!await context.Tenants.AnyAsync())
        {
            context.Tenants.Add(new Tenant
            {
                Name = "النظام الرئيسي",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        // Seed Roles
        if (!await context.Roles.AnyAsync())
        {
            var adminRole = new Role
            {
                Name = "مدير النظام",
                Description = "صلاحيات كاملة على النظام",
                IsSystemRole = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Roles.Add(adminRole);

            var userRole = new Role
            {
                Name = "مستخدم",
                Description = "صلاحيات محدودة للمستخدم العادي",
                IsSystemRole = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Roles.Add(userRole);
            await context.SaveChangesAsync();

            // Assign all permissions to admin role
            var allPermissions = await context.Permissions.ToListAsync();
            foreach (var permission in allPermissions)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = permission.Id,
                    IsGranted = true
                });
            }

            // Assign view permissions to user role
            var viewPermissions = allPermissions.Where(p => p.Action == "View").ToList();
            foreach (var permission in viewPermissions)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = userRole.Id,
                    PermissionId = permission.Id,
                    IsGranted = true
                });
            }
            await context.SaveChangesAsync();
        }

        // Seed Admin User
        if (!await context.Users.AnyAsync())
        {
            var adminRole = await context.Roles.FirstAsync(r => r.Name == "مدير النظام");
           

    var admin = new User
            {
                Username = "admin",
                Email = "admin@system.com",
                PasswordHash = passwordHasher.HashPassword("Admin@123"),
                FullName = "مدير النظام",
                IsActive = true,
                EmailConfirmed = true,
                TenantId = _appSettings.DefaultTenantId,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(admin);
            await context.SaveChangesAsync();

            context.UserRoles.Add(new UserRole
            {
                UserId = admin.Id,
                RoleId = adminRole.Id,
                AssignedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }
    }
}

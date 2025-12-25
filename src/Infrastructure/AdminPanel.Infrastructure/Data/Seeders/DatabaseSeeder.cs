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

        // Seed Actions (New Permission System)
        if (!await context.Actions.AnyAsync())
        {
            var actions = new List<AdminPanel.Domain.Entities.Identity.Action>
            {
                new() { NameAr = "عرض", NameEn = "View", Code = "view", Icon = "bi-eye", DisplayOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { NameAr = "إضافة", NameEn = "Create", Code = "create", Icon = "bi-plus-circle", DisplayOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { NameAr = "تعديل", NameEn = "Edit", Code = "edit", Icon = "bi-pencil", DisplayOrder = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { NameAr = "حذف", NameEn = "Delete", Code = "delete", Icon = "bi-trash", DisplayOrder = 4, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { NameAr = "تصدير", NameEn = "Export", Code = "export", Icon = "bi-download", DisplayOrder = 5, IsActive = true, CreatedAt = DateTime.UtcNow },
                new() { NameAr = "طباعة", NameEn = "Print", Code = "print", Icon = "bi-printer", DisplayOrder = 6, IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            context.Actions.AddRange(actions);
            await context.SaveChangesAsync();
        }

        // Seed Pages (New Permission System)
        if (!await context.Pages.AnyAsync())
        {
            var pages = new List<Page>
            {
                new() { NameAr = "لوحة التحكم", NameEn = "Dashboard", Url = "/", Icon = "bi-house-door", DisplayOrder = 1, IsActive = true, IsInMenu = true, CreatedAt = DateTime.UtcNow },
                new() { NameAr = "المستخدمين", NameEn = "Users", Url = "/Users", Icon = "bi-people", DisplayOrder = 2, IsActive = true, IsInMenu = true, CreatedAt = DateTime.UtcNow },
                new() { NameAr = "الأدوار", NameEn = "Roles", Url = "/Roles", Icon = "bi-shield-check", DisplayOrder = 3, IsActive = true, IsInMenu = true, CreatedAt = DateTime.UtcNow },
                new() { NameAr = "التقويم", NameEn = "Calendar", Url = "/Calendar", Icon = "bi-calendar3", DisplayOrder = 4, IsActive = true, IsInMenu = true, CreatedAt = DateTime.UtcNow },
                new() { NameAr = "الملف الشخصي", NameEn = "Profile", Url = "/Profile", Icon = "bi-person-circle", DisplayOrder = 5, IsActive = true, IsInMenu = false, CreatedAt = DateTime.UtcNow }
            };
            context.Pages.AddRange(pages);
            await context.SaveChangesAsync();
        }

        // Seed PageActions (Link Actions to Pages)
        if (!await context.PageActions.AnyAsync())
        {
            var actions = await context.Actions.ToListAsync();
            var pages = await context.Pages.ToListAsync();

            var viewAction = actions.First(a => a.Code == "view");
            var createAction = actions.First(a => a.Code == "create");
            var editAction = actions.First(a => a.Code == "edit");
            var deleteAction = actions.First(a => a.Code == "delete");

            var pageActions = new List<PageAction>();

            // Dashboard - View only
            var dashboard = pages.First(p => p.Url == "/");
            pageActions.Add(new PageAction { PageId = dashboard.Id, ActionId = viewAction.Id, IsActive = true });

            // Users - View, Create, Edit, Delete
            var users = pages.First(p => p.Url == "/Users");
            pageActions.Add(new PageAction { PageId = users.Id, ActionId = viewAction.Id, IsActive = true });
            pageActions.Add(new PageAction { PageId = users.Id, ActionId = createAction.Id, IsActive = true });
            pageActions.Add(new PageAction { PageId = users.Id, ActionId = editAction.Id, IsActive = true });
            pageActions.Add(new PageAction { PageId = users.Id, ActionId = deleteAction.Id, IsActive = true });

            // Roles - View, Create, Edit, Delete
            var roles = pages.First(p => p.Url == "/Roles");
            pageActions.Add(new PageAction { PageId = roles.Id, ActionId = viewAction.Id, IsActive = true });
            pageActions.Add(new PageAction { PageId = roles.Id, ActionId = createAction.Id, IsActive = true });
            pageActions.Add(new PageAction { PageId = roles.Id, ActionId = editAction.Id, IsActive = true });
            pageActions.Add(new PageAction { PageId = roles.Id, ActionId = deleteAction.Id, IsActive = true });

            // Calendar - View, Create, Edit, Delete
            var calendar = pages.First(p => p.Url == "/Calendar");
            pageActions.Add(new PageAction { PageId = calendar.Id, ActionId = viewAction.Id, IsActive = true });
            pageActions.Add(new PageAction { PageId = calendar.Id, ActionId = createAction.Id, IsActive = true });
            pageActions.Add(new PageAction { PageId = calendar.Id, ActionId = editAction.Id, IsActive = true });
            pageActions.Add(new PageAction { PageId = calendar.Id, ActionId = deleteAction.Id, IsActive = true });

            // Profile - View, Edit
            var profile = pages.First(p => p.Url == "/Profile");
            pageActions.Add(new PageAction { PageId = profile.Id, ActionId = viewAction.Id, IsActive = true });
            pageActions.Add(new PageAction { PageId = profile.Id, ActionId = editAction.Id, IsActive = true });

            context.PageActions.AddRange(pageActions);
            await context.SaveChangesAsync();
        }

        // Seed RolePageActions (Assign all PageActions to Admin role)
        if (!await context.RolePageActions.AnyAsync())
        {
            var adminRole = await context.Roles.FirstAsync(r => r.Name == "مدير النظام");
            var userRole = await context.Roles.FirstAsync(r => r.Name == "مستخدم");
            var allPageActions = await context.PageActions.ToListAsync();
            var viewPageActions = await context.PageActions
                .Include(pa => pa.Action)
                .Where(pa => pa.Action!.Code == "view")
                .ToListAsync();

            // Admin gets all permissions
            foreach (var pageAction in allPageActions)
            {
                context.RolePageActions.Add(new RolePageAction
                {
                    RoleId = adminRole.Id,
                    PageActionId = pageAction.Id,
                    IsGranted = true
                });
            }

            // User role gets view permissions only
            foreach (var pageAction in viewPageActions)
            {
                context.RolePageActions.Add(new RolePageAction
                {
                    RoleId = userRole.Id,
                    PageActionId = pageAction.Id,
                    IsGranted = true
                });
            }

            await context.SaveChangesAsync();
        }
    }
}

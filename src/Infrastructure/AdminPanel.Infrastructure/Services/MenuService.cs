using AdminPanel.Application.Common.Interfaces;
using AdminPanel.Application.Features.Menu.DTOs;
using AdminPanel.Domain.Entities;
using AdminPanel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdminPanel.Infrastructure.Services;

public class MenuService : IMenuService
{
    private readonly ApplicationDbContext _context;

    public MenuService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MenuItemDto>> GetUserMenuAsync(int userId, CancellationToken cancellationToken = default)
    {
        // 1. Get user's role IDs
        var userRoleIds = await _context.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        if (!userRoleIds.Any())
            return new List<MenuItemDto>();

        // 2. SQL Server 2008 compatible: Load all RolePageActions with includes, then filter in memory
        var allRolePageActions = await _context.RolePageActions
            .AsNoTracking()
            .Include(rpa => rpa.PageAction)
            .ThenInclude(pa => pa!.Action)
            .ToListAsync(cancellationToken);

        // Filter in memory for user's roles with "view" permission
        var grantedPageIds = allRolePageActions
            .Where(rpa => userRoleIds.Contains(rpa.RoleId)
                       && rpa.IsGranted
                       && rpa.PageAction != null
                       && rpa.PageAction.Action != null
                       && rpa.PageAction.Action.Code.Equals("view", StringComparison.OrdinalIgnoreCase)
                       && rpa.PageAction.IsActive)
            .Select(rpa => rpa.PageAction!.PageId)
            .Distinct()
            .ToList();

        // 3. Get all active menu pages
        var allPages = await _context.Pages
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.IsActive && p.IsInMenu)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync(cancellationToken);

        // 4. Filter pages that user has permission to view
        // Include parent pages if any child/grandchild is accessible (3 levels deep)
        var accessiblePageIds = new HashSet<int>(grantedPageIds);

        // Recursively add parent pages of accessible pages (supports 3+ levels)
        bool addedNew;
        do
        {
            addedNew = false;
            foreach (var page in allPages.Where(p => accessiblePageIds.Contains(p.Id) && p.ParentId.HasValue))
            {
                if (!accessiblePageIds.Contains(page.ParentId!.Value))
                {
                    accessiblePageIds.Add(page.ParentId!.Value);
                    addedNew = true;
                }
            }
        } while (addedNew);

        var accessiblePages = allPages
            .Where(p => accessiblePageIds.Contains(p.Id))
            .ToList();

        // 5. Build tree structure recursively (supports 3+ levels)
        var menuItems = BuildMenuTree(accessiblePages, null);

        return menuItems;
    }

    /// <summary>
    /// Recursively builds the menu tree structure from a flat list of pages.
    /// Supports unlimited levels of nesting.
    /// </summary>
    private List<MenuItemDto> BuildMenuTree(List<Page> allPages, int? parentId)
    {
        return allPages
            .Where(p => p.ParentId == parentId)
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new MenuItemDto
            {
                Id = p.Id,
                NameAr = p.NameAr,
                NameEn = p.NameEn,
                Url = p.Url,
                Icon = p.Icon,
                DisplayOrder = p.DisplayOrder,
                Children = BuildMenuTree(allPages, p.Id) // Recursive call for children
            })
            .ToList();
    }
}

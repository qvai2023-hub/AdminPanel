using AdminPanel.Application.Common.Interfaces;
using AdminPanel.Application.Features.Menu.DTOs;
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
        // Include parent pages if any child is accessible
        var accessiblePageIds = new HashSet<int>(grantedPageIds);

        // Add parent pages of accessible pages
        foreach (var page in allPages.Where(p => accessiblePageIds.Contains(p.Id) && p.ParentId.HasValue))
        {
            accessiblePageIds.Add(page.ParentId!.Value);
        }

        var accessiblePages = allPages
            .Where(p => accessiblePageIds.Contains(p.Id))
            .ToList();

        // 5. Build tree structure
        var rootPages = accessiblePages
            .Where(p => !p.ParentId.HasValue)
            .OrderBy(p => p.DisplayOrder)
            .ToList();

        var menuItems = rootPages.Select(p => new MenuItemDto
        {
            Id = p.Id,
            NameAr = p.NameAr,
            NameEn = p.NameEn,
            Url = p.Url,
            Icon = p.Icon,
            DisplayOrder = p.DisplayOrder,
            Children = accessiblePages
                .Where(c => c.ParentId == p.Id)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new MenuItemDto
                {
                    Id = c.Id,
                    NameAr = c.NameAr,
                    NameEn = c.NameEn,
                    Url = c.Url,
                    Icon = c.Icon,
                    DisplayOrder = c.DisplayOrder,
                    Children = new List<MenuItemDto>()
                })
                .ToList()
        }).ToList();

        return menuItems;
    }
}

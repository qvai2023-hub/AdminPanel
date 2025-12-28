using AdminPanel.Application.Common.Interfaces;
using AdminPanel.Application.Common.Models;
using AdminPanel.Application.Features.Actions.DTOs;
using AdminPanel.Application.Features.Pages.DTOs;
using AdminPanel.Domain.Constants;
using AdminPanel.Domain.Entities.Identity;
using AdminPanel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdminPanel.Infrastructure.Services;

public class PageService : IPageService
{
    private readonly ApplicationDbContext _context;

    public PageService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<PageListDto>>> GetPagedAsync(PageFilterDto filter, CancellationToken cancellationToken = default)
    {
        // Ensure valid pagination values
        if (filter.PageNumber < 1) filter.PageNumber = 1;
        if (filter.PageSize < 1) filter.PageSize = 10;

        // SQL Server 2008 compatible: Load all then filter/paginate in memory
        var allPages = await _context.Pages
            .AsNoTracking()
            .Include(p => p.Parent)
            .Include(p => p.Children)
            .Include(p => p.PageActions)
            .ToListAsync(cancellationToken);

        // Apply filters in memory
        var filteredPages = allPages.Where(p => !p.IsDeleted).AsEnumerable();

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            filteredPages = filteredPages.Where(p =>
                p.NameAr.ToLower().Contains(term) ||
                p.NameEn.ToLower().Contains(term) ||
                p.Url.ToLower().Contains(term));
        }

        if (filter.IsActive.HasValue)
            filteredPages = filteredPages.Where(p => p.IsActive == filter.IsActive.Value);

        if (filter.IsInMenu.HasValue)
            filteredPages = filteredPages.Where(p => p.IsInMenu == filter.IsInMenu.Value);

        if (filter.ParentId.HasValue)
            filteredPages = filteredPages.Where(p => p.ParentId == filter.ParentId.Value);

        var pagesList = filteredPages
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Id)
            .ToList();

        var totalCount = pagesList.Count;

        // Paginate in memory
        var pagedItems = pagesList
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => new PageListDto
            {
                Id = p.Id,
                NameAr = p.NameAr,
                NameEn = p.NameEn,
                Url = p.Url,
                Icon = p.Icon,
                ParentId = p.ParentId,
                ParentNameAr = p.Parent?.NameAr,
                DisplayOrder = p.DisplayOrder,
                IsActive = p.IsActive,
                IsInMenu = p.IsInMenu,
                ActionsCount = p.PageActions.Count,
                ChildrenCount = p.Children.Count(c => !c.IsDeleted)
            })
            .ToList();

        return Result<PaginatedList<PageListDto>>.Success(
            new PaginatedList<PageListDto>(pagedItems, totalCount, filter.PageNumber, filter.PageSize));
    }

    public async Task<Result<PageDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var page = await _context.Pages
            .AsNoTracking()
            .Include(p => p.Parent)
            .Include(p => p.Children.Where(c => !c.IsDeleted))
            .Include(p => p.PageActions)
            .ThenInclude(pa => pa.Action)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (page == null)
            return Result<PageDto>.Failure(Messages.Error.PageNotFound);

        return Result<PageDto>.Success(new PageDto
        {
            Id = page.Id,
            NameAr = page.NameAr,
            NameEn = page.NameEn,
            Url = page.Url,
            Icon = page.Icon,
            ParentId = page.ParentId,
            ParentNameAr = page.Parent?.NameAr,
            DisplayOrder = page.DisplayOrder,
            IsActive = page.IsActive,
            IsInMenu = page.IsInMenu,
            CreatedAt = page.CreatedAt,
            AvailableActions = page.PageActions
                .Where(pa => pa.Action != null && pa.IsActive)
                .Select(pa => new ActionListDto
                {
                    Id = pa.Action!.Id,
                    NameAr = pa.Action.NameAr,
                    NameEn = pa.Action.NameEn,
                    Code = pa.Action.Code,
                    Icon = pa.Action.Icon,
                    DisplayOrder = pa.Action.DisplayOrder,
                    IsActive = pa.Action.IsActive
                })
                .ToList()
        });
    }

    public async Task<Result<List<PageDropdownDto>>> GetAllForDropdownAsync(int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var pages = await _context.Pages
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.IsActive)
            .ToListAsync(cancellationToken);

        // Exclude the current page and its children (to avoid circular reference)
        if (excludeId.HasValue)
        {
            var excludeIds = GetChildIds(pages, excludeId.Value);
            excludeIds.Add(excludeId.Value);
            pages = pages.Where(p => !excludeIds.Contains(p.Id)).ToList();
        }

        var result = pages
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new PageDropdownDto
            {
                Id = p.Id,
                NameAr = p.NameAr,
                ParentId = p.ParentId
            })
            .ToList();

        return Result<List<PageDropdownDto>>.Success(result);
    }

    private List<int> GetChildIds(List<Page> allPages, int parentId)
    {
        var childIds = new List<int>();
        var directChildren = allPages.Where(p => p.ParentId == parentId).ToList();

        foreach (var child in directChildren)
        {
            childIds.Add(child.Id);
            childIds.AddRange(GetChildIds(allPages, child.Id));
        }

        return childIds;
    }

    public async Task<Result<List<PageDto>>> GetMenuPagesAsync(CancellationToken cancellationToken = default)
    {
        var pages = await _context.Pages
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.IsActive && p.IsInMenu)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync(cancellationToken);

        // Build tree structure
        var rootPages = pages.Where(p => p.ParentId == null).ToList();
        var result = rootPages.Select(p => BuildPageTree(p, pages)).ToList();

        return Result<List<PageDto>>.Success(result);
    }

    private PageDto BuildPageTree(Page page, List<Page> allPages)
    {
        var children = allPages.Where(p => p.ParentId == page.Id).ToList();

        return new PageDto
        {
            Id = page.Id,
            NameAr = page.NameAr,
            NameEn = page.NameEn,
            Url = page.Url,
            Icon = page.Icon,
            DisplayOrder = page.DisplayOrder,
            IsActive = page.IsActive,
            IsInMenu = page.IsInMenu,
            Children = children.Select(c => BuildPageTree(c, allPages)).ToList()
        };
    }

    public async Task<Result<PageDto>> CreateAsync(CreatePageDto dto, CancellationToken cancellationToken = default)
    {
        // Check if URL is unique
        if (!await IsUrlUniqueAsync(dto.Url, null, cancellationToken))
            return Result<PageDto>.Failure(Messages.Error.PageUrlExists);

        // Validate parent exists if specified
        if (dto.ParentId.HasValue)
        {
            var parentExists = await _context.Pages.AnyAsync(p => p.Id == dto.ParentId.Value && !p.IsDeleted, cancellationToken);
            if (!parentExists)
                return Result<PageDto>.Failure("القسم الرئيسي غير موجود");
        }

        var page = new Page
        {
            NameAr = dto.NameAr,
            NameEn = dto.NameEn,
            Url = dto.Url.StartsWith("/") ? dto.Url : "/" + dto.Url,
            Icon = dto.Icon,
            ParentId = dto.ParentId,
            DisplayOrder = dto.DisplayOrder,
            IsActive = true,
            IsInMenu = dto.IsInMenu,
            CreatedAt = DateTime.UtcNow
        };

        _context.Pages.Add(page);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(page.Id, cancellationToken);
    }

    public async Task<Result<PageDto>> UpdateAsync(int id, UpdatePageDto dto, CancellationToken cancellationToken = default)
    {
        var page = await _context.Pages.FindAsync(new object[] { id }, cancellationToken);
        if (page == null || page.IsDeleted)
            return Result<PageDto>.Failure(Messages.Error.PageNotFound);

        // Check if URL is unique (exclude current)
        if (!await IsUrlUniqueAsync(dto.Url, id, cancellationToken))
            return Result<PageDto>.Failure(Messages.Error.PageUrlExists);

        // Prevent setting parent to self or children
        if (dto.ParentId.HasValue)
        {
            if (dto.ParentId.Value == id)
                return Result<PageDto>.Failure("لا يمكن أن تكون الصفحة قسم رئيسي لنفسها");

            var allPages = await _context.Pages.Where(p => !p.IsDeleted).ToListAsync(cancellationToken);
            var childIds = GetChildIds(allPages, id);
            if (childIds.Contains(dto.ParentId.Value))
                return Result<PageDto>.Failure("لا يمكن اختيار صفحة فرعية كقسم رئيسي");
        }

        page.NameAr = dto.NameAr;
        page.NameEn = dto.NameEn;
        page.Url = dto.Url.StartsWith("/") ? dto.Url : "/" + dto.Url;
        page.Icon = dto.Icon;
        page.ParentId = dto.ParentId;
        page.DisplayOrder = dto.DisplayOrder;
        page.IsActive = dto.IsActive;
        page.IsInMenu = dto.IsInMenu;
        page.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<PageDto>.Success(new PageDto
        {
            Id = page.Id,
            NameAr = page.NameAr,
            NameEn = page.NameEn,
            Url = page.Url,
            Icon = page.Icon,
            ParentId = page.ParentId,
            DisplayOrder = page.DisplayOrder,
            IsActive = page.IsActive,
            IsInMenu = page.IsInMenu
        }, Messages.Success.PageUpdated);
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var page = await _context.Pages
            .Include(p => p.Children)
            .Include(p => p.PageActions)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (page == null || page.IsDeleted)
            return Result<bool>.Failure(Messages.Error.PageNotFound);

        // Check if page has children
        if (page.Children.Any(c => !c.IsDeleted))
            return Result<bool>.Failure(Messages.Error.PageHasChildren);

        // Remove PageActions first
        _context.PageActions.RemoveRange(page.PageActions);

        // Soft delete
        page.IsDeleted = true;
        page.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true, Messages.Success.PageDeleted);
    }

    public async Task<Result<bool>> ToggleStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var page = await _context.Pages.FindAsync(new object[] { id }, cancellationToken);
        if (page == null || page.IsDeleted)
            return Result<bool>.Failure(Messages.Error.PageNotFound);

        page.IsActive = !page.IsActive;
        page.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var message = page.IsActive ? Messages.Success.PageActivated : Messages.Success.PageDeactivated;
        return Result<bool>.Success(true, message);
    }

    public async Task<bool> IsUrlUniqueAsync(string url, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalizedUrl = url.StartsWith("/") ? url : "/" + url;

        var query = _context.Pages.Where(p => p.Url == normalizedUrl && !p.IsDeleted);

        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<Result<List<ActionListDto>>> GetPageActionsAsync(int pageId, CancellationToken cancellationToken = default)
    {
        var page = await _context.Pages
            .AsNoTracking()
            .Include(p => p.PageActions)
            .ThenInclude(pa => pa.Action)
            .FirstOrDefaultAsync(p => p.Id == pageId, cancellationToken);

        if (page == null)
            return Result<List<ActionListDto>>.Failure(Messages.Error.PageNotFound);

        var actions = page.PageActions
            .Where(pa => pa.Action != null && pa.IsActive)
            .Select(pa => new ActionListDto
            {
                Id = pa.Action!.Id,
                NameAr = pa.Action.NameAr,
                NameEn = pa.Action.NameEn,
                Code = pa.Action.Code,
                Icon = pa.Action.Icon,
                DisplayOrder = pa.Action.DisplayOrder,
                IsActive = pa.Action.IsActive
            })
            .OrderBy(a => a.DisplayOrder)
            .ToList();

        return Result<List<ActionListDto>>.Success(actions);
    }

    public async Task<Result<List<ActionListDto>>> GetAllActionsWithAssignmentAsync(int pageId, CancellationToken cancellationToken = default)
    {
        // Get all active actions
        var allActions = await _context.Actions
            .AsNoTracking()
            .Where(a => !a.IsDeleted && a.IsActive)
            .OrderBy(a => a.DisplayOrder)
            .ToListAsync(cancellationToken);

        // Get assigned action IDs for this page
        var assignedActionIds = await _context.PageActions
            .Where(pa => pa.PageId == pageId && pa.IsActive)
            .Select(pa => pa.ActionId)
            .ToListAsync(cancellationToken);

        var result = allActions.Select(a => new ActionListDto
        {
            Id = a.Id,
            NameAr = a.NameAr,
            NameEn = a.NameEn,
            Code = a.Code,
            Icon = a.Icon,
            DisplayOrder = a.DisplayOrder,
            IsActive = assignedActionIds.Contains(a.Id) // Use IsActive to indicate if assigned
        }).ToList();

        return Result<List<ActionListDto>>.Success(result);
    }

    public async Task<Result<bool>> AssignActionsAsync(int pageId, List<int> actionIds, CancellationToken cancellationToken = default)
    {
        var page = await _context.Pages
            .Include(p => p.PageActions)
            .FirstOrDefaultAsync(p => p.Id == pageId, cancellationToken);

        if (page == null || page.IsDeleted)
            return Result<bool>.Failure(Messages.Error.PageNotFound);

        // Remove existing PageActions
        _context.PageActions.RemoveRange(page.PageActions);

        // Add new PageActions
        foreach (var actionId in actionIds)
        {
            _context.PageActions.Add(new PageAction
            {
                PageId = pageId,
                ActionId = actionId,
                IsActive = true
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true, Messages.Success.PageActionsAssigned);
    }
}

using AdminPanel.Application.Common.Interfaces;
using AdminPanel.Application.Common.Models;
using AdminPanel.Application.Features.Actions.DTOs;
using AdminPanel.Domain.Constants;
using AdminPanel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Action = AdminPanel.Domain.Entities.Identity.Action;

namespace AdminPanel.Infrastructure.Services;

public class ActionService : IActionService
{
    private readonly ApplicationDbContext _context;

    public ActionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedList<ActionListDto>>> GetPagedAsync(ActionFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Actions.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.ToLower();
            query = query.Where(a =>
                a.NameAr.ToLower().Contains(searchTerm) ||
                a.NameEn.ToLower().Contains(searchTerm) ||
                a.Code.ToLower().Contains(searchTerm));
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(a => a.IsActive == filter.IsActive.Value);
        }

        // Order by DisplayOrder
        query = query.OrderBy(a => a.DisplayOrder).ThenBy(a => a.Id);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // SQL Server 2008 compatible: Load all then paginate in memory
        var allItems = await query
            .Select(a => new ActionListDto
            {
                Id = a.Id,
                NameAr = a.NameAr,
                NameEn = a.NameEn,
                Code = a.Code,
                Icon = a.Icon,
                DisplayOrder = a.DisplayOrder,
                IsActive = a.IsActive
            })
            .ToListAsync(cancellationToken);

        // Paginate in memory (no OFFSET/FETCH for SQL 2008)
        var items = allItems
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        var result = new PaginatedList<ActionListDto>(items, totalCount, filter.PageNumber, filter.PageSize);
        return Result<PaginatedList<ActionListDto>>.Success(result);
    }

    public async Task<Result<ActionDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var action = await _context.Actions
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (action == null)
            return Result<ActionDto>.Failure(Messages.Error.ActionNotFound);

        return Result<ActionDto>.Success(new ActionDto
        {
            Id = action.Id,
            NameAr = action.NameAr,
            NameEn = action.NameEn,
            Code = action.Code,
            Icon = action.Icon,
            DisplayOrder = action.DisplayOrder,
            IsActive = action.IsActive,
            CreatedAt = action.CreatedAt
        });
    }

    public async Task<Result<List<ActionListDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var actions = await _context.Actions
            .OrderBy(a => a.DisplayOrder)
            .Select(a => new ActionListDto
            {
                Id = a.Id,
                NameAr = a.NameAr,
                NameEn = a.NameEn,
                Code = a.Code,
                Icon = a.Icon,
                DisplayOrder = a.DisplayOrder,
                IsActive = a.IsActive
            })
            .ToListAsync(cancellationToken);

        return Result<List<ActionListDto>>.Success(actions);
    }

    public async Task<Result<ActionDto>> CreateAsync(CreateActionDto dto, CancellationToken cancellationToken = default)
    {
        // Check if code is unique
        var codeExists = await _context.Actions.AnyAsync(a => a.Code == dto.Code, cancellationToken);
        if (codeExists)
            return Result<ActionDto>.Failure(Messages.Error.ActionCodeExists);

        var action = new Action
        {
            NameAr = dto.NameAr,
            NameEn = dto.NameEn,
            Code = dto.Code,
            Icon = dto.Icon,
            DisplayOrder = dto.DisplayOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Actions.Add(action);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<ActionDto>.Success(new ActionDto
        {
            Id = action.Id,
            NameAr = action.NameAr,
            NameEn = action.NameEn,
            Code = action.Code,
            Icon = action.Icon,
            DisplayOrder = action.DisplayOrder,
            IsActive = action.IsActive,
            CreatedAt = action.CreatedAt
        }, Messages.Success.ActionCreated);
    }

    public async Task<Result<ActionDto>> UpdateAsync(int id, UpdateActionDto dto, CancellationToken cancellationToken = default)
    {
        var action = await _context.Actions.FindAsync(new object[] { id }, cancellationToken);
        if (action == null)
            return Result<ActionDto>.Failure(Messages.Error.ActionNotFound);

        // Check if code is unique (exclude current action)
        var codeExists = await _context.Actions.AnyAsync(a => a.Code == dto.Code && a.Id != id, cancellationToken);
        if (codeExists)
            return Result<ActionDto>.Failure(Messages.Error.ActionCodeExists);

        action.NameAr = dto.NameAr;
        action.NameEn = dto.NameEn;
        action.Code = dto.Code;
        action.Icon = dto.Icon;
        action.DisplayOrder = dto.DisplayOrder;
        action.IsActive = dto.IsActive;
        action.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<ActionDto>.Success(new ActionDto
        {
            Id = action.Id,
            NameAr = action.NameAr,
            NameEn = action.NameEn,
            Code = action.Code,
            Icon = action.Icon,
            DisplayOrder = action.DisplayOrder,
            IsActive = action.IsActive,
            CreatedAt = action.CreatedAt
        }, Messages.Success.ActionUpdated);
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var action = await _context.Actions
            .Include(a => a.PageActions)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (action == null)
            return Result<bool>.Failure(Messages.Error.ActionNotFound);

        // Check if action is used in any PageActions
        if (action.PageActions.Any())
            return Result<bool>.Failure(Messages.Error.ActionHasPageActions);

        // Soft delete
        action.IsDeleted = true;
        action.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true, Messages.Success.ActionDeleted);
    }

    public async Task<Result<bool>> ToggleStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var action = await _context.Actions.FindAsync(new object[] { id }, cancellationToken);
        if (action == null)
            return Result<bool>.Failure(Messages.Error.ActionNotFound);

        action.IsActive = !action.IsActive;
        action.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var message = action.IsActive ? Messages.Success.ActionActivated : Messages.Success.ActionDeactivated;
        return Result<bool>.Success(true, message);
    }

    public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Actions.Where(a => a.Code == code);

        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);

        return !await query.AnyAsync(cancellationToken);
    }
}

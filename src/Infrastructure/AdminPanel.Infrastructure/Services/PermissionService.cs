using AdminPanel.Application.Common.Interfaces;
using AdminPanel.Application.Common.Models;
using AdminPanel.Application.Features.Permissions.DTOs;
using AdminPanel.Domain.Constants;
using AdminPanel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdminPanel.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;

    public PermissionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<PermissionDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await _context.Permissions
            .Where(p => p.IsActive)
            .OrderBy(p => p.Module).ThenBy(p => p.DisplayOrder)
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Module = p.Module,
                Action = p.Action,
                Code = p.Code,
                DisplayNameAr = p.DisplayNameAr,
                DisplayNameEn = p.DisplayNameEn,
                DisplayOrder = p.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return Result<List<PermissionDto>>.Success(permissions);
    }

    public async Task<Result<List<PermissionGroupDto>>> GetGroupedAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await _context.Permissions
            .Where(p => p.IsActive)
            .OrderBy(p => p.Module).ThenBy(p => p.DisplayOrder)
            .ToListAsync(cancellationToken);

        var moduleNames = new Dictionary<string, string>
        {
            { "Users", "المستخدمين" },
            { "Roles", "الأدوار" },
            { "AuditLogs", "سجل التدقيق" },
            { "Settings", "الإعدادات" },
            { "Tenants", "المستأجرين" }
        };

        var grouped = permissions
            .GroupBy(p => p.Module)
            .Select(g => new PermissionGroupDto
            {
                Module = g.Key,
                ModuleDisplayName = moduleNames.GetValueOrDefault(g.Key, g.Key),
                Permissions = g.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Module = p.Module,
                    Action = p.Action,
                    Code = p.Code,
                    DisplayNameAr = p.DisplayNameAr,
                    DisplayNameEn = p.DisplayNameEn,
                    DisplayOrder = p.DisplayOrder
                }).ToList()
            }).ToList();

        return Result<List<PermissionGroupDto>>.Success(grouped);
    }

    public async Task<Result<PermissionDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var permission = await _context.Permissions.FindAsync(new object[] { id }, cancellationToken);
        if (permission == null)
            return Result<PermissionDto>.Failure(Messages.Error.PermissionNotFound);

        return Result<PermissionDto>.Success(new PermissionDto
        {
            Id = permission.Id,
            Module = permission.Module,
            Action = permission.Action,
            Code = permission.Code,
            DisplayNameAr = permission.DisplayNameAr,
            DisplayNameEn = permission.DisplayNameEn,
            DisplayOrder = permission.DisplayOrder
        });
    }

    public async Task<Result<PermissionDto>> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Code == code, cancellationToken);
        if (permission == null)
            return Result<PermissionDto>.Failure(Messages.Error.PermissionNotFound);

        return Result<PermissionDto>.Success(new PermissionDto
        {
            Id = permission.Id,
            Module = permission.Module,
            Action = permission.Action,
            Code = permission.Code,
            DisplayNameAr = permission.DisplayNameAr,
            DisplayNameEn = permission.DisplayNameEn,
            DisplayOrder = permission.DisplayOrder
        });
    }

    public async Task<Result<bool>> UpdateAsync(int id, UpdatePermissionDto dto, CancellationToken cancellationToken = default)
    {
        var permission = await _context.Permissions.FindAsync(new object[] { id }, cancellationToken);
        if (permission == null)
            return Result<bool>.Failure(Messages.Error.PermissionNotFound);

        if (dto.DisplayNameAr != null)
            permission.DisplayNameAr = dto.DisplayNameAr;
        if (dto.DisplayNameEn != null)
            permission.DisplayNameEn = dto.DisplayNameEn;
        if (dto.DisplayOrder.HasValue)
            permission.DisplayOrder = dto.DisplayOrder.Value;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true, Messages.Success.Updated);
    }

    public async Task<Result<List<PermissionDto>>> GetPermissionsForRoleAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var allPermissions = await _context.Permissions
            .Where(p => p.IsActive)
            .OrderBy(p => p.Module).ThenBy(p => p.DisplayOrder)
            .ToListAsync(cancellationToken);

        var rolePermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);

        var result = allPermissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            Module = p.Module,
            Action = p.Action,
            Code = p.Code,
            DisplayNameAr = p.DisplayNameAr,
            DisplayNameEn = p.DisplayNameEn,
            DisplayOrder = p.DisplayOrder,
            IsGranted = rolePermissions.Any(rp => rp.PermissionId == p.Id && rp.IsGranted)
        }).ToList();

        return Result<List<PermissionDto>>.Success(result);
    }

    public async Task<bool> HasPermissionAsync(int userId, string permissionCode, CancellationToken cancellationToken = default)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role!.RolePermissions)
            .AnyAsync(rp => rp.IsGranted && rp.Permission!.Code == permissionCode, cancellationToken);
    }

    public async Task<Result<List<string>>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var permissions = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role!.RolePermissions)
            .Where(rp => rp.IsGranted)
            .Select(rp => rp.Permission!.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        return Result<List<string>>.Success(permissions);
    }
}

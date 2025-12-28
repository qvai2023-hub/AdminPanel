using AdminPanel.Application.Common.Interfaces;
using AdminPanel.Application.Common.Models;
using AdminPanel.Application.Features.Permissions.DTOs;
using AdminPanel.Application.Features.Roles.DTOs;
using AdminPanel.Domain.Constants;
using AdminPanel.Domain.Entities.Identity;
using AdminPanel.Infrastructure.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AdminPanel.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public RoleService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<RoleDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles
            .Include(r => r.UserRoles)
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);

        if (role == null)
            return Result<RoleDto>.Failure(Messages.Error.RoleNotFound);

        var dto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            IsActive = role.IsActive,
            UsersCount = role.UserRoles.Count,
            PermissionsCount = role.RolePermissions.Count,
            CreatedAt = role.CreatedAt
        };

        return Result<RoleDto>.Success(dto);
    }

    public async Task<Result<List<RoleListDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // SQL Server 2008 compatible: Load all then process in memory
        var roles = await _context.Roles
            .AsNoTracking()
            .Include(r => r.UserRoles)
            .Include(r => r.RolePermissions)
            .Where(r => !r.IsDeleted)
            .ToListAsync(cancellationToken);

        var dtos = roles
            .OrderBy(r => r.Name)
            .Select(r => new RoleListDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IsSystemRole = r.IsSystemRole,
                IsActive = r.IsActive,
                UsersCount = r.UserRoles.Count,
                PermissionsCount = r.RolePermissions.Count
            })
            .ToList();

        return Result<List<RoleListDto>>.Success(dtos);
    }

    public async Task<Result<PaginatedList<RoleListDto>>> GetPagedAsync(RoleFilterDto filter, CancellationToken cancellationToken = default)
    {
        // SQL Server 2008 compatible: Load all with includes, then filter/paginate in memory
        var allRoles = await _context.Roles
            .AsNoTracking()
            .Include(r => r.UserRoles)
            .Include(r => r.RolePermissions)
            .Where(r => !r.IsDeleted)
            .ToListAsync(cancellationToken);

        // Apply filters in memory
        var filteredRoles = allRoles.AsEnumerable();

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.ToLower();
            filteredRoles = filteredRoles.Where(r =>
                r.Name.ToLower().Contains(searchTerm) ||
                (r.Description != null && r.Description.ToLower().Contains(searchTerm)));
        }

        if (filter.IsActive.HasValue)
        {
            filteredRoles = filteredRoles.Where(r => r.IsActive == filter.IsActive.Value);
        }

        if (filter.IsSystemRole.HasValue)
        {
            filteredRoles = filteredRoles.Where(r => r.IsSystemRole == filter.IsSystemRole.Value);
        }

        // Order and paginate
        var orderedRoles = filteredRoles.OrderBy(r => r.Name).ToList();
        var totalCount = orderedRoles.Count;

        var pagedRoles = orderedRoles
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(r => new RoleListDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IsSystemRole = r.IsSystemRole,
                IsActive = r.IsActive,
                UsersCount = r.UserRoles.Count,
                PermissionsCount = r.RolePermissions.Count
            })
            .ToList();

        var result = new PaginatedList<RoleListDto>(pagedRoles, totalCount, filter.PageNumber, filter.PageSize);
        return Result<PaginatedList<RoleListDto>>.Success(result);
    }

    public async Task<Result<RoleDto>> CreateAsync(CreateRoleDto dto, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Roles.AnyAsync(r => r.Name == dto.Name && !r.IsDeleted, cancellationToken);
        if (exists)
            return Result<RoleDto>.Failure(Messages.Error.RoleNameExists);

        var role = new Role
        {
            Name = dto.Name,
            Description = dto.Description,
            IsSystemRole = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);

        if (dto.PermissionIds != null && dto.PermissionIds.Any())
        {
            foreach (var permissionId in dto.PermissionIds)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permissionId,
                    IsGranted = true
                });
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        var resultDto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            IsActive = role.IsActive,
            UsersCount = 0,
            PermissionsCount = dto.PermissionIds?.Count ?? 0,
            CreatedAt = role.CreatedAt
        };

        return Result<RoleDto>.Success(resultDto, Messages.Success.RoleCreated);
    }

    public async Task<Result<RoleDto>> UpdateAsync(int id, UpdateRoleDto dto, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles
            .Include(r => r.UserRoles)
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);

        if (role == null)
            return Result<RoleDto>.Failure(Messages.Error.RoleNotFound);

        if (role.IsSystemRole)
            return Result<RoleDto>.Failure(Messages.Error.SystemRoleCannotBeModified);

        if (!string.IsNullOrEmpty(dto.Name))
        {
            var exists = await _context.Roles.AnyAsync(r => r.Name == dto.Name && r.Id != id && !r.IsDeleted, cancellationToken);
            if (exists)
                return Result<RoleDto>.Failure(Messages.Error.RoleNameExists);
            role.Name = dto.Name;
        }

        if (dto.Description != null)
            role.Description = dto.Description;

        role.IsActive = dto.IsActive;
        role.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var resultDto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            IsActive = role.IsActive,
            UsersCount = role.UserRoles.Count,
            PermissionsCount = role.RolePermissions.Count,
            CreatedAt = role.CreatedAt
        };

        return Result<RoleDto>.Success(resultDto, Messages.Success.RoleUpdated);
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);

        if (role == null)
            return Result<bool>.Failure(Messages.Error.RoleNotFound);

        if (role.IsSystemRole)
            return Result<bool>.Failure(Messages.Error.SystemRoleCannotBeDeleted);

        if (role.UserRoles.Any())
            return Result<bool>.Failure(Messages.Error.RoleHasUsers);

        role.IsDeleted = true;
        role.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true, Messages.Success.RoleDeleted);
    }

    public async Task<Result<bool>> ToggleStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);
        if (role == null)
            return Result<bool>.Failure(Messages.Error.RoleNotFound);

        if (role.IsSystemRole)
            return Result<bool>.Failure(Messages.Error.SystemRoleCannotBeModified);

        role.IsActive = !role.IsActive;
        role.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var message = role.IsActive ? Messages.Success.RoleActivated : Messages.Success.RoleDeactivated;
        return Result<bool>.Success(true, message);
    }

    public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Roles.Where(r => r.Name == name && !r.IsDeleted);
        if (excludeId.HasValue)
            query = query.Where(r => r.Id != excludeId.Value);
        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<Result<bool>> AssignPermissionsAsync(int roleId, List<PermissionAssignmentDto> permissions, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted, cancellationToken);
        if (role == null)
            return Result<bool>.Failure(Messages.Error.RoleNotFound);

        var existingPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);

        _context.RolePermissions.RemoveRange(existingPermissions);

        foreach (var permission in permissions)
        {
            _context.RolePermissions.Add(new RolePermission
            {
                RoleId = roleId,
                PermissionId = permission.PermissionId,
                IsGranted = permission.IsGranted
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true, Messages.Success.PermissionsAssigned);
    }

    public async Task<Result<List<PermissionDto>>> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted, cancellationToken);
        if (role == null)
            return Result<List<PermissionDto>>.Failure(Messages.Error.RoleNotFound);

        var permissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .Select(rp => new PermissionDto
            {
                Id = rp.Permission!.Id,
                Module = rp.Permission.Module,
                Action = rp.Permission.Action,
                Code = rp.Permission.Code,
                DisplayNameAr = rp.Permission.DisplayNameAr,
                DisplayNameEn = rp.Permission.DisplayNameEn,
                DisplayOrder = rp.Permission.DisplayOrder,
                IsGranted = rp.IsGranted
            })
            .ToListAsync(cancellationToken);

        return Result<List<PermissionDto>>.Success(permissions);
    }
}

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
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (role == null)
            return Result<RoleDto>.Failure(Messages.Error.RoleNotFound);

        return Result<RoleDto>.Success(_mapper.Map<RoleDto>(role));
    }

    public async Task<Result<List<RoleListDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _context.Roles
            .Include(r => r.UserRoles)
            .Include(r => r.RolePermissions)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return Result<List<RoleListDto>>.Success(_mapper.Map<List<RoleListDto>>(roles));
    }

    public async Task<Result<PaginatedList<RoleListDto>>> GetPagedAsync(PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var query = _context.Roles
            .Include(r => r.UserRoles)
            .Include(r => r.RolePermissions)
            .AsQueryable();

        if (!string.IsNullOrEmpty(pagination.SearchTerm))
        {
            query = query.Where(r => r.Name.Contains(pagination.SearchTerm) ||
                                     (r.Description != null && r.Description.Contains(pagination.SearchTerm)));
        }

        query = pagination.SortDescending
            ? query.OrderByDescending(r => r.Name)
            : query.OrderBy(r => r.Name);

        var totalCount = await query.CountAsync(cancellationToken);

        // SQL Server 2008 compatible: Load all then paginate in memory
        var allItems = await query.ToListAsync(cancellationToken);
        var items = allItems
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<RoleListDto>>(items);
        var result = new PaginatedList<RoleListDto>(dtos, totalCount, pagination.PageNumber, pagination.PageSize);

        return Result<PaginatedList<RoleListDto>>.Success(result);
    }

    public async Task<Result<RoleDto>> CreateAsync(CreateRoleDto dto, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Roles.AnyAsync(r => r.Name == dto.Name, cancellationToken);
        if (exists)
            return Result<RoleDto>.Failure(Messages.Error.RoleNameExists);

        var role = new Role
        {
            Name = dto.Name,
            Description = dto.Description,
            IsSystemRole = false,
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

        return Result<RoleDto>.Success(_mapper.Map<RoleDto>(role), Messages.Success.RoleCreated);
    }

    public async Task<Result<RoleDto>> UpdateAsync(int id, UpdateRoleDto dto, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles.FindAsync(new object[] { id }, cancellationToken);
        if (role == null)
            return Result<RoleDto>.Failure(Messages.Error.RoleNotFound);

        if (role.IsSystemRole)
            return Result<RoleDto>.Failure(Messages.Error.SystemRoleCannotBeModified);

        if (!string.IsNullOrEmpty(dto.Name))
        {
            var exists = await _context.Roles.AnyAsync(r => r.Name == dto.Name && r.Id != id, cancellationToken);
            if (exists)
                return Result<RoleDto>.Failure(Messages.Error.RoleNameExists);
            role.Name = dto.Name;
        }

        if (dto.Description != null)
            role.Description = dto.Description;

        role.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<RoleDto>.Success(_mapper.Map<RoleDto>(role), Messages.Success.RoleUpdated);
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

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

    public async Task<Result<bool>> AssignPermissionsAsync(int roleId, List<PermissionAssignmentDto> permissions, CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles.FindAsync(new object[] { roleId }, cancellationToken);
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
        var role = await _context.Roles.FindAsync(new object[] { roleId }, cancellationToken);
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

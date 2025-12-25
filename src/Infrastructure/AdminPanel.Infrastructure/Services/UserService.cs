using AdminPanel.Application.Common.Interfaces;
using AdminPanel.Application.Common.Models;
using AdminPanel.Application.Features.Users.DTOs;
using AdminPanel.Domain.Constants;
using AdminPanel.Domain.Entities.Identity;
using AdminPanel.Infrastructure.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AdminPanel.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;
    private readonly AppSettings _appSettings;

    public UserService(
        ApplicationDbContext context,
        IMapper mapper,
        IPasswordHasher passwordHasher,
        IOptions<AppSettings> appSettings)
    {
        _context = context;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
        _appSettings = appSettings.Value;
    }

    public async Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
            return Result<UserDto>.Failure(Messages.Error.UserNotFound);

        var dto = _mapper.Map<UserDto>(user);
        dto.Roles = user.UserRoles.Select(ur => ur.Role!.Name).ToList();

        return Result<UserDto>.Success(dto);
    }

    public async Task<Result<UserDto>> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

        if (user == null)
            return Result<UserDto>.Failure(Messages.Error.UserNotFound);

        var dto = _mapper.Map<UserDto>(user);
        dto.Roles = user.UserRoles.Select(ur => ur.Role!.Name).ToList();

        return Result<UserDto>.Success(dto);
    }

    public async Task<Result<PaginatedList<UserListDto>>> GetPagedAsync(UserFilterDto filter, CancellationToken cancellationToken = default)
    {
        // Ensure valid pagination values
        if (filter.PageNumber < 1) filter.PageNumber = 1;
        if (filter.PageSize < 1) filter.PageSize = 10;

        // SQL Server 2008 compatible: Load all users with roles first (no complex SQL)
        var allUsers = await _context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ToListAsync(cancellationToken);

        // Apply all filters in memory
        var filteredUsers = allUsers.AsEnumerable();

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            filteredUsers = filteredUsers.Where(u =>
                u.Username.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term) ||
                u.FullName.ToLower().Contains(term));
        }

        if (filter.IsActive.HasValue)
            filteredUsers = filteredUsers.Where(u => u.IsActive == filter.IsActive.Value);

        if (filter.FromDate.HasValue)
            filteredUsers = filteredUsers.Where(u => u.CreatedAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            filteredUsers = filteredUsers.Where(u => u.CreatedAt <= filter.ToDate.Value);

        if (filter.RoleId.HasValue)
            filteredUsers = filteredUsers.Where(u => u.UserRoles.Any(ur => ur.RoleId == filter.RoleId.Value));

        // Apply soft delete filter
        filteredUsers = filteredUsers.Where(u => !u.IsDeleted);

        var usersList = filteredUsers
            .OrderByDescending(u => u.Id)
            .ToList();

        var totalCount = usersList.Count;

        // Paginate in memory
        var pagedUsers = usersList
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(u => new UserListDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FullName = u.FullName,
                IsActive = u.IsActive,
                LastLoginAt = u.LastLoginAt,
                Roles = u.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role!.Name).ToList()
            })
            .ToList();

        return Result<PaginatedList<UserListDto>>.Success(
            new PaginatedList<UserListDto>(pagedUsers, totalCount, filter.PageNumber, filter.PageSize));
    }
    public async Task<bool> HasPermissionAsync(int userId, string permissionCode, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.UserRoles)
            .SelectMany(ur => ur.Role!.RolePermissions)
            .AnyAsync(rp => rp.IsGranted && rp.Permission!.Code == permissionCode, cancellationToken);
    }
    public async Task<Result<UserDto>> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username, cancellationToken))
            return Result<UserDto>.Failure(Messages.Error.UsernameExists);

        if (await _context.Users.AnyAsync(u => u.Email == dto.Email, cancellationToken))
            return Result<UserDto>.Failure(Messages.Error.EmailExists);

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = _passwordHasher.HashPassword(dto.Password),
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            IsActive = dto.IsActive,
            TenantId = _appSettings.DefaultTenantId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Assign roles
        var roleIds = dto.RoleIds?.Any() == true
            ? dto.RoleIds
            : new List<int> { _appSettings.DefaultRoleId };

        foreach (var roleId in roleIds)
        {
            _context.Set<UserRole>().Add(new UserRole
            {
                UserId = user.Id,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync(cancellationToken);

        var createdUser = await GetByIdAsync(user.Id, cancellationToken);
        return Result<UserDto>.Success(createdUser.Data!, Messages.Success.UserCreated);
    }

    public async Task<Result<UserDto>> UpdateAsync(int id, UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
            return Result<UserDto>.Failure(Messages.Error.UserNotFound);

        if (!string.IsNullOrEmpty(dto.Email) && dto.Email != user.Email)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id, cancellationToken))
                return Result<UserDto>.Failure(Messages.Error.EmailExists);
            user.Email = dto.Email;
        }

        if (!string.IsNullOrEmpty(dto.FullName))
            user.FullName = dto.FullName;

        if (dto.PhoneNumber != null)
            user.PhoneNumber = dto.PhoneNumber;

        if (dto.IsActive.HasValue)
            user.IsActive = dto.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;

        // Update roles
        if (dto.RoleIds != null)
        {
            _context.Set<UserRole>().RemoveRange(user.UserRoles);

            foreach (var roleId in dto.RoleIds)
            {
                _context.Set<UserRole>().Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = roleId,
                    AssignedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var updatedUser = await GetByIdAsync(user.Id, cancellationToken);
        return Result<UserDto>.Success(updatedUser.Data!, Messages.Success.UserUpdated);
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);

        if (user == null)
            return Result<bool>.Failure(Messages.Error.UserNotFound);

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true, Messages.Success.UserDeleted);
    }

    public async Task<Result<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);

        if (user == null)
            return Result<bool>.Failure(Messages.Error.UserNotFound);

        if (!_passwordHasher.VerifyPassword(dto.CurrentPassword, user.PasswordHash))
            return Result<bool>.Failure(Messages.Error.CurrentPasswordWrong);

        if (dto.NewPassword != dto.ConfirmPassword)
            return Result<bool>.Failure(Messages.Error.PasswordMismatch);

        user.PasswordHash = _passwordHasher.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true, Messages.Success.PasswordChanged);
    }

    public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { dto.UserId }, cancellationToken);

        if (user == null)
            return Result<bool>.Failure(Messages.Error.UserNotFound);

        user.PasswordHash = _passwordHasher.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true, Messages.Success.PasswordReset);
    }

    public async Task<Result<bool>> ToggleStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { id }, cancellationToken);

        if (user == null)
            return Result<bool>.Failure(Messages.Error.UserNotFound);

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var message = user.IsActive ? Messages.Success.UserActivated : Messages.Success.UserDeactivated;
        return Result<bool>.Success(true, message);
    }

    public async Task<Result<bool>> AssignRolesAsync(int userId, List<int> roleIds, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            return Result<bool>.Failure(Messages.Error.UserNotFound);

        _context.Set<UserRole>().RemoveRange(user.UserRoles);

        foreach (var roleId in roleIds)
        {
            _context.Set<UserRole>().Add(new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true, Messages.Success.RolesAssigned);
    }

    public async Task<Result<List<string>>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var permissions = await _context.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.UserRoles)
            .SelectMany(ur => ur.Role!.RolePermissions)
            .Where(rp => rp.IsGranted)
            .Select(rp => rp.Permission!.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        return Result<List<string>>.Success(permissions);
    }

  
  
}
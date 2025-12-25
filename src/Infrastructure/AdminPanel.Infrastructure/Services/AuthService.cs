using AdminPanel.Application.Common.Interfaces;
using AdminPanel.Application.Common.Models;
using AdminPanel.Application.Features.Auth.DTOs;
using AdminPanel.Domain.Constants;
using AdminPanel.Domain.Entities.Identity;
using AdminPanel.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AdminPanel.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly JwtSettings _jwtSettings;
    private readonly AppSettings _appSettings;
    private readonly IEmailService _emailService;
    private readonly IAuditService _auditService;

    public AuthService(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IOptions<JwtSettings> jwtSettings,
        IOptions<AppSettings> appSettings,
        IEmailService emailService,
        IAuditService auditService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtSettings = jwtSettings.Value;
        _appSettings = appSettings.Value;
        _emailService = emailService;
        _auditService = auditService;
    }

    public async Task<Result<LoginResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r!.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Username == dto.Username, cancellationToken);

        if (user == null)
            return Result<LoginResponseDto>.Failure(Messages.Error.InvalidCredentials);

        if (!user.IsActive)
            return Result<LoginResponseDto>.Failure(Messages.Error.AccountDisabled);

        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            return Result<LoginResponseDto>.Failure(Messages.Error.AccountLocked);

        if (!_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= _appSettings.MaxLoginAttempts)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(_appSettings.LockoutDurationMinutes);
                user.FailedLoginAttempts = 0;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result<LoginResponseDto>.Failure(Messages.Error.InvalidCredentials);
        }

        // Reset failed attempts
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTime.UtcNow;

        // Generate tokens
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        await _context.SaveChangesAsync(cancellationToken);
        await _auditService.LogLoginAsync(user.Id, user.Username);

        var response = new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            RefreshTokenExpiry = user.RefreshTokenExpiry.Value,
            User = new UserInfoDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                ProfileImageUrl = user.ProfileImageUrl,
                Roles = user.UserRoles.Select(ur => ur.Role!.Name).ToList(),
                Permissions = user.UserRoles
                    .SelectMany(ur => ur.Role!.RolePermissions)
                    .Where(rp => rp.IsGranted)
                    .Select(rp => rp.Permission!.Code)
                    .Distinct()
                    .ToList()
            }
        };

        return Result<LoginResponseDto>.Success(response, Messages.Success.LoginSuccess);
    }

    public async Task<Result<bool>> LogoutAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);

        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _context.SaveChangesAsync(cancellationToken);
            await _auditService.LogLogoutAsync(userId, user.Username);
        }

        return Result<bool>.Success(true, Messages.Success.LogoutSuccess);
    }

    public async Task<Result<LoginResponseDto>> RefreshTokenAsync(RefreshTokenDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r!.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.RefreshToken == dto.RefreshToken, cancellationToken);

        if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            return Result<LoginResponseDto>.Failure(Messages.Error.InvalidToken);

        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

        await _context.SaveChangesAsync(cancellationToken);

        var response = new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            RefreshTokenExpiry = user.RefreshTokenExpiry.Value,
            User = new UserInfoDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                ProfileImageUrl = user.ProfileImageUrl,
                Roles = user.UserRoles.Select(ur => ur.Role!.Name).ToList(),
                Permissions = user.UserRoles
                    .SelectMany(ur => ur.Role!.RolePermissions)
                    .Where(rp => rp.IsGranted)
                    .Select(rp => rp.Permission!.Code)
                    .Distinct()
                    .ToList()
            }
        };

        return Result<LoginResponseDto>.Success(response);
    }

    public async Task<Result<bool>> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken);

        if (user != null)
        {
            var token = GeneratePasswordResetToken();
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(_appSettings.PasswordResetTokenExpiryHours);

            await _context.SaveChangesAsync(cancellationToken);
            await _emailService.SendPasswordResetEmailAsync(user.Email, token);
        }

        return Result<bool>.Success(true, Messages.Success.PasswordResetEmailSent);
    }

    public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordWithTokenDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(
            u => u.Email == dto.Email && u.PasswordResetToken == dto.Token,
            cancellationToken);

        if (user == null)
            return Result<bool>.Failure(Messages.Error.InvalidToken);

        if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
            return Result<bool>.Failure(Messages.Error.TokenExpired);

        if (dto.NewPassword != dto.ConfirmPassword)
            return Result<bool>.Failure(Messages.Error.PasswordMismatch);

        user.PasswordHash = _passwordHasher.HashPassword(dto.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true, Messages.Success.PasswordReset);
    }

    public async Task<Result<bool>> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username, cancellationToken))
            return Result<bool>.Failure(Messages.Error.UsernameExists);

        if (await _context.Users.AnyAsync(u => u.Email == dto.Email, cancellationToken))
            return Result<bool>.Failure(Messages.Error.EmailExists);

        if (dto.Password != dto.ConfirmPassword)
            return Result<bool>.Failure(Messages.Error.PasswordMismatch);

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = _passwordHasher.HashPassword(dto.Password),
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            IsActive = true,
            TenantId = _appSettings.DefaultTenantId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Assign default role
        _context.Set<UserRole>().Add(new UserRole
        {
            UserId = user.Id,
            RoleId = _appSettings.DefaultRoleId,
            AssignedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);

        return Result<bool>.Success(true, Messages.Success.UserCreated);
    }
    public async Task<Result<bool>> ConfirmEmailAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(
            u => u.Email == email && u.EmailConfirmationToken == token,
            cancellationToken);

        if (user == null)
            return Result<bool>.Failure(Messages.Error.InvalidToken);

        user.EmailConfirmed = true;
        user.EmailConfirmationToken = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true, Messages.Success.EmailConfirmed);
    }

    private string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new("FullName", user.FullName),
            new("TenantId", user.TenantId.ToString())
        };

        foreach (var userRole in user.UserRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole.Role!.Name));
        }

        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role!.RolePermissions)
            .Where(rp => rp.IsGranted)
            .Select(rp => rp.Permission!.Code)
            .Distinct();

        claims.Add(new Claim("Permissions", string.Join(",", permissions)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private static string GeneratePasswordResetToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
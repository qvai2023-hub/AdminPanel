using AdminPanel.Application.Common.Interfaces;
using AdminPanel.Domain.Constants;
using AdminPanel.Domain.Entities.Logging;
using AdminPanel.Domain.Enums;
using AdminPanel.Infrastructure.Data;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;

namespace AdminPanel.Infrastructure.Services;

// PasswordHasher
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;

    public string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

        byte[] hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        byte[] hashBytes = Convert.FromBase64String(hashedPassword);
        byte[] salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);

        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

        for (int i = 0; i < HashSize; i++)
        {
            if (hashBytes[i + SaltSize] != hash[i])
                return false;
        }
        return true;
    }
}

// CurrentUserService
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId => int.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
    public int? TenantId => int.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirstValue("TenantId"), out var id) ? id : null;
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    public List<string> Permissions => _httpContextAccessor.HttpContext?.User?.FindFirstValue("Permissions")?.Split(',').ToList() ?? new List<string>();
}

// CurrentTenantService
public class CurrentTenantService : ICurrentTenantService
{
    public int? TenantId { get; private set; }
    public void SetTenant(int tenantId) => TenantId = tenantId;
}

// AuditService
public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AuditService(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task LogAsync(string entityName, string? entityId, AuditAction action, object? oldValues = null, object? newValues = null, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            UserId = _currentUserService.UserId,
            UserName = _currentUserService.Username,
            TenantId = _currentUserService.TenantId,
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task LogLoginAsync(int userId, string username, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            UserName = username,
            EntityName = "User",
            EntityId = userId.ToString(),
            Action = AuditAction.Login,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task LogLogoutAsync(int userId, string username, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            UserName = username,
            EntityName = "User",
            EntityId = userId.ToString(),
            Action = AuditAction.Logout,
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
// AppSettings
public class AppSettings
{
    public int DefaultTenantId { get; set; } = 1;
    public int DefaultRoleId { get; set; } = 2;
    public int SystemAdminRoleId { get; set; } = 1;
    public int MaxLoginAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 15;
    public int PasswordResetTokenExpiryHours { get; set; } = 24;
}
// EmailSettings
public class EmailSettings
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseSsl { get; set; } = true;
    public string BaseUrl { get; set; } = "https://localhost:5001";
}
// JwtSettings
public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
// EmailService
public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        await SendEmailAsync(new[] { to }, subject, body, isHtml);
    }

    public async Task SendEmailAsync(IEnumerable<string> to, string subject, string body, bool isHtml = true)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        foreach (var address in to)
            message.To.Add(MailboxAddress.Parse(address));
        message.Subject = subject;

        var builder = new BodyBuilder();
        if (isHtml) builder.HtmlBody = body;
        else builder.TextBody = body;
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, _settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
        await client.AuthenticateAsync(_settings.Username, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendPasswordResetEmailAsync(string to, string resetLink)
    {
        var body = EmailTemplates.PasswordReset("المستخدم", resetLink);
        await SendEmailAsync(to, Messages.Titles.ResetPassword, body);
    }

    public async Task SendWelcomeEmailAsync(string to, string userName)
    {
        var loginUrl = $"{_settings.BaseUrl}/Auth/Login";
        var body = EmailTemplates.Welcome(userName, loginUrl);
        await SendEmailAsync(to, "مرحباً بك في النظام", body);
    }

    public async Task SendEmailConfirmationAsync(string to, string confirmationLink)
    {
        var body = EmailTemplates.EmailConfirmation("المستخدم", confirmationLink);
        await SendEmailAsync(to, "تأكيد البريد الإلكتروني", body);
    }
}

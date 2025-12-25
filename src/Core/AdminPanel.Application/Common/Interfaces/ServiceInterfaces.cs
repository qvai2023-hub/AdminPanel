using AdminPanel.Application.Common.Models;
using AdminPanel.Application.Features.Auth.DTOs;
using AdminPanel.Application.Features.Calendar.DTOs;
using AdminPanel.Application.Features.Permissions.DTOs;
using AdminPanel.Application.Features.Roles.DTOs;
using AdminPanel.Application.Features.Users.DTOs;
using AdminPanel.Domain.Enums;

namespace AdminPanel.Application.Common.Interfaces;


public interface IUserService
{
    Task<Result<UserDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<Result<PaginatedList<UserListDto>>> GetPagedAsync(UserFilterDto filter, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> UpdateAsync(int id, UpdateUserDto dto, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto, CancellationToken cancellationToken = default);
    Task<Result<bool>> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default);
    Task<Result<bool>> ToggleStatusAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<bool>> AssignRolesAsync(int userId, List<int> roleIds, CancellationToken cancellationToken = default);
    Task<Result<List<string>>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> HasPermissionAsync(int userId, string permissionCode, CancellationToken cancellationToken = default);  // ✅ بدون Result
}

public interface IAuthService 
{
    Task<Result<LoginResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<Result<bool>> LogoutAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result<LoginResponseDto>> RefreshTokenAsync(RefreshTokenDto dto, CancellationToken cancellationToken = default);
    Task<Result<bool>> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken cancellationToken = default);
    Task<Result<bool>> ResetPasswordAsync(ResetPasswordWithTokenDto dto, CancellationToken cancellationToken = default);
    Task<Result<bool>> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);  // ✅ bool
    Task<Result<bool>> ConfirmEmailAsync(string email, string token, CancellationToken cancellationToken = default);
}

public interface IRoleService
{
    Task<Result<RoleDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<List<RoleListDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<PaginatedList<RoleListDto>>> GetPagedAsync(PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<Result<RoleDto>> CreateAsync(CreateRoleDto dto, CancellationToken cancellationToken = default);
    Task<Result<RoleDto>> UpdateAsync(int id, UpdateRoleDto dto, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<bool>> AssignPermissionsAsync(int roleId, List<PermissionAssignmentDto> permissions, CancellationToken cancellationToken = default);
    Task<Result<List<PermissionDto>>> GetRolePermissionsAsync(int roleId, CancellationToken cancellationToken = default);
}

public interface IPermissionService
{
    Task<Result<List<PermissionDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<List<PermissionGroupDto>>> GetGroupedAsync(CancellationToken cancellationToken = default);
    Task<Result<PermissionDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<PermissionDto>> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Result<bool>> UpdateAsync(int id, UpdatePermissionDto dto, CancellationToken cancellationToken = default);
    Task<Result<List<PermissionDto>>> GetPermissionsForRoleAsync(int roleId, CancellationToken cancellationToken = default);
    Task<bool> HasPermissionAsync(int userId, string permissionCode, CancellationToken cancellationToken = default);
    Task<Result<List<string>>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken = default);
}

public interface IAuditService
{
    Task LogAsync(string entityName, string? entityId, AuditAction action, object? oldValues = null, object? newValues = null, CancellationToken cancellationToken = default);
    Task LogLoginAsync(int userId, string username, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task LogLogoutAsync(int userId, string username, CancellationToken cancellationToken = default);
}

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendEmailAsync(IEnumerable<string> to, string subject, string body, bool isHtml = true);
    Task SendPasswordResetEmailAsync(string to, string resetLink);
    Task SendWelcomeEmailAsync(string to, string userName);
    Task SendEmailConfirmationAsync(string to, string confirmationLink);
}

public interface IFileService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string? folder = null, CancellationToken cancellationToken = default);
    Task<Stream?> DownloadAsync(string filePath, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string filePath, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string filePath, CancellationToken cancellationToken = default);
}

public interface INotificationService
{
    Task SendAsync(int userId, string title, string message, NotificationType type = NotificationType.Info, string? link = null, CancellationToken cancellationToken = default);
    Task SendToAllAsync(string title, string message, NotificationType type = NotificationType.Info, CancellationToken cancellationToken = default);
    Task SendToRoleAsync(int roleId, string title, string message, NotificationType type = NotificationType.Info, CancellationToken cancellationToken = default);
    Task<bool> MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default);
    Task<bool> MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default);
}

public interface IExportService
{
    byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName = "Sheet1");
    byte[] ExportToCsv<T>(IEnumerable<T> data);
    Task<List<T>> ImportFromExcelAsync<T>(Stream fileStream) where T : new();
    Task<List<T>> ImportFromCsvAsync<T>(Stream fileStream) where T : new();
}

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public interface ICalendarService
{
    // Hijri Date Conversion
    HijriDateDto ConvertToHijri(DateTime gregorianDate);
    DateTime ConvertToGregorian(int hijriYear, int hijriMonth, int hijriDay);
    DualDateDto GetDualDate(DateTime gregorianDate);

    // Calendar Month Data
    CalendarMonthDto GetMonthData(int year, int month);
    CalendarMonthDto GetHijriMonthData(int hijriYear, int hijriMonth);

    // Hijri Month Names
    string GetHijriMonthName(int month, bool arabic = true);
    string GetHijriDayName(DayOfWeek dayOfWeek, bool arabic = true);

    // Date Range
    List<DualDateDto> GetDateRange(DateTime startDate, DateTime endDate);

    // Today's Date
    DualDateDto GetToday();

    // Validation
    bool IsValidHijriDate(int year, int month, int day);
    int GetDaysInHijriMonth(int year, int month);
}

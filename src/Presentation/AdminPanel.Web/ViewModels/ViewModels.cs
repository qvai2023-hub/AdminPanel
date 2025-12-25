using AdminPanel.Application.Common.Models;
using AdminPanel.Application.Features.Actions.DTOs;
using AdminPanel.Application.Features.Calendar.DTOs;
using AdminPanel.Application.Features.Permissions.DTOs;
using AdminPanel.Application.Features.Roles.DTOs;
using AdminPanel.Application.Features.Users.DTOs;
using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Web.ViewModels;

// Dashboard
public class DashboardViewModel
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers => TotalUsers - ActiveUsers;
    public int TotalRoles { get; set; }
    public int TotalPermissions { get; set; }
    public List<UserListDto> RecentUsers { get; set; } = new();
    public List<RoleListDto> Roles { get; set; } = new();
}

// Auth
public class LoginViewModel
{
    [Required(ErrorMessage = "اسم المستخدم مطلوب")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

// Users
public class UsersViewModel
{
    public PaginatedList<UserListDto> Users { get; set; } = null!;
    public UserFilterDto Filter { get; set; } = new();
    public List<RoleListDto> AvailableRoles { get; set; } = new();
}

public class UserDetailsViewModel
{
    public UserDto User { get; set; } = null!;
}

public class CreateUserViewModel
{
    [Required(ErrorMessage = "اسم المستخدم مطلوب")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    public string FullName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public List<int> SelectedRoleIds { get; set; } = new();
    public List<RoleListDto> AvailableRoles { get; set; } = new();
}

public class EditUserViewModel
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public List<int> SelectedRoleIds { get; set; } = new();
    public List<RoleListDto> AvailableRoles { get; set; } = new();
}

public class ChangePasswordViewModel
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ResetUserPasswordViewModel
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

// Roles
public class RolesViewModel
{
    public List<RoleListDto> Roles { get; set; } = new();
}

public class RoleDetailsViewModel
{
    public RoleDto Role { get; set; } = null!;
    public List<PermissionDto> Permissions { get; set; } = new();
}

public class CreateRoleViewModel
{
    [Required(ErrorMessage = "اسم الدور مطلوب")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public List<int> SelectedPermissionIds { get; set; } = new();
    public List<PermissionGroupDto> PermissionGroups { get; set; } = new();
}

public class EditRoleViewModel
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
}

public class RolePermissionsViewModel
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsSystemRole { get; set; }
    public List<PermissionGroupDto> PermissionGroups { get; set; } = new();
    public List<int> GrantedPermissionIds { get; set; } = new();
}

// Profile
public class ProfileViewModel
{
    public UserDto User { get; set; } = null!;
}

public class EditProfileViewModel
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
}

// Error
public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public string? Message { get; set; }
}

// Calendar
public class CalendarViewModel
{
    public DualDateDto Today { get; set; } = new();
    public CalendarMonthDto CurrentMonth { get; set; } = new();
    public CalendarSettingsDto Settings { get; set; } = new();
    public int Year { get; set; }
    public int Month { get; set; }
    public int? HijriYear { get; set; }
    public int? HijriMonth { get; set; }
    public bool UseHijriNavigation { get; set; }
}

// Actions
public class ActionsViewModel
{
    public PaginatedList<ActionListDto> Actions { get; set; } = null!;
    public ActionFilterDto Filter { get; set; } = new();
}

public class CreateActionViewModel
{
    [Required(ErrorMessage = "الاسم بالعربي مطلوب")]
    [Display(Name = "الاسم بالعربي")]
    public string NameAr { get; set; } = string.Empty;

    [Required(ErrorMessage = "الاسم بالإنجليزي مطلوب")]
    [Display(Name = "الاسم بالإنجليزي")]
    public string NameEn { get; set; } = string.Empty;

    [Required(ErrorMessage = "الكود مطلوب")]
    [Display(Name = "الكود")]
    public string Code { get; set; } = string.Empty;

    [Display(Name = "الأيقونة")]
    public string? Icon { get; set; }

    [Display(Name = "الترتيب")]
    public int DisplayOrder { get; set; }
}

public class EditActionViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "الاسم بالعربي مطلوب")]
    [Display(Name = "الاسم بالعربي")]
    public string NameAr { get; set; } = string.Empty;

    [Required(ErrorMessage = "الاسم بالإنجليزي مطلوب")]
    [Display(Name = "الاسم بالإنجليزي")]
    public string NameEn { get; set; } = string.Empty;

    [Required(ErrorMessage = "الكود مطلوب")]
    [Display(Name = "الكود")]
    public string Code { get; set; } = string.Empty;

    [Display(Name = "الأيقونة")]
    public string? Icon { get; set; }

    [Display(Name = "الترتيب")]
    public int DisplayOrder { get; set; }

    [Display(Name = "نشط")]
    public bool IsActive { get; set; }
}
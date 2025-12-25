using AdminPanel.Application.Common.Interfaces;
using AdminPanel.Application.Common.Models;
using AdminPanel.Application.Features.Auth.DTOs;
using AdminPanel.Application.Features.Permissions.DTOs;
using AdminPanel.Application.Features.Roles.DTOs;
using AdminPanel.Application.Features.Users.DTOs;
using AdminPanel.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdminPanel.Web.Controllers;

#region Base Controller
public abstract class BaseController : Controller
{
    protected void SetSuccessMessage(string message) => TempData["Success"] = message;
    protected void SetErrorMessage(string message) => TempData["Error"] = message;
    protected void SetWarningMessage(string message) => TempData["Warning"] = message;
    protected void SetInfoMessage(string message) => TempData["Info"] = message;

    protected int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out var id) ? id : null;
    }

    protected string? GetCurrentUsername() => User.FindFirstValue(ClaimTypes.Name);
}
#endregion

#region Home Controller
[Authorize]
public class HomeController : BaseController
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly IPermissionService _permissionService;

    public HomeController(IUserService userService, IRoleService roleService, IPermissionService permissionService)
    {
        _userService = userService;
        _roleService = roleService;
        _permissionService = permissionService;
    }

    public async Task<IActionResult> Index()
    {
        var usersResult = await _userService.GetPagedAsync(new UserFilterDto { PageNumber = 1, PageSize = 5 });
        var rolesResult = await _roleService.GetAllAsync();
        var permissionsResult = await _permissionService.GetAllAsync();

        var viewModel = new DashboardViewModel
        {
            TotalUsers = usersResult.IsSuccess ? usersResult.Data!.TotalCount : 0,
            ActiveUsers = usersResult.IsSuccess ? usersResult.Data!.Items.Count(u => u.IsActive) : 0,
            TotalRoles = rolesResult.IsSuccess ? rolesResult.Data!.Count : 0,
            TotalPermissions = permissionsResult.IsSuccess ? permissionsResult.Data!.Count : 0,
            RecentUsers = usersResult.IsSuccess ? usersResult.Data!.Items : new List<UserListDto>(),
            Roles = rolesResult.IsSuccess ? rolesResult.Data! : new List<RoleListDto>()
        };

        return View(viewModel);
    }

    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
    }
}
#endregion

#region Auth Controller
public class AuthController : BaseController
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var result = await _authService.LoginAsync(new LoginDto
        {
            Username = model.Username,
            Password = model.Password,
            RememberMe = model.RememberMe
        });

        if (result.IsFailure)
        {
            ModelState.AddModelError("", result.Errors.FirstOrDefault() ?? "خطأ في تسجيل الدخول");
            return View(model);
        }

        // Create claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.Data!.User.Id.ToString()),
            new(ClaimTypes.Name, result.Data.User.Username),
            new(ClaimTypes.Email, result.Data.User.Email),
            new("FullName", result.Data.User.FullName),
            new("AccessToken", result.Data.AccessToken)
        };

        foreach (var role in result.Data.User.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var permission in result.Data.User.Permissions)
        {
            claims.Add(new Claim("Permission", permission));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = result.Data.AccessTokenExpiry
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

        SetSuccessMessage("تم تسجيل الدخول بنجاح");

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = GetCurrentUserId();
        if (userId.HasValue)
        {
            await _authService.LogoutAsync(userId.Value);
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        SetSuccessMessage("تم تسجيل الخروج بنجاح");
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await _authService.ForgotPasswordAsync(new ForgotPasswordDto { Email = model.Email });

        SetSuccessMessage("إذا كان البريد الإلكتروني مسجلاً، سيتم إرسال رابط إعادة التعيين");
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ResetPassword(string email, string token)
    {
        return View(new ResetPasswordViewModel { Email = email, Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _authService.ResetPasswordAsync(new ResetPasswordWithTokenDto
        {
            Email = model.Email,
            Token = model.Token,
            NewPassword = model.NewPassword,
            ConfirmPassword = model.ConfirmPassword
        });

        if (result.IsFailure)
        {
            ModelState.AddModelError("", result.Errors.FirstOrDefault() ?? "خطأ في إعادة تعيين كلمة المرور");
            return View(model);
        }

        SetSuccessMessage("تم إعادة تعيين كلمة المرور بنجاح");
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
#endregion

#region Users Controller
[Authorize]
public class UsersController : BaseController
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;

    public UsersController(IUserService userService, IRoleService roleService)
    {
        _userService = userService;
        _roleService = roleService;
    }

    public async Task<IActionResult> Index(UserFilterDto filter)
    {
        filter.PageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        filter.PageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

        var usersResult = await _userService.GetPagedAsync(filter);
        var rolesResult = await _roleService.GetAllAsync();

        var viewModel = new UsersViewModel
        {
            Users = usersResult.IsSuccess ? usersResult.Data! : new PaginatedList<UserListDto>(new List<UserListDto>(), 0, 1, 10),
            Filter = filter,
            AvailableRoles = rolesResult.IsSuccess ? rolesResult.Data! : new List<RoleListDto>()
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        if (result.IsFailure)
        {
            SetErrorMessage("المستخدم غير موجود");
            return RedirectToAction(nameof(Index));
        }

        return View(new UserDetailsViewModel { User = result.Data! });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var rolesResult = await _roleService.GetAllAsync();
        return View(new CreateUserViewModel
        {
            AvailableRoles = rolesResult.IsSuccess ? rolesResult.Data! : new List<RoleListDto>()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var rolesResult = await _roleService.GetAllAsync();
            model.AvailableRoles = rolesResult.IsSuccess ? rolesResult.Data! : new List<RoleListDto>();
            return View(model);
        }

        var result = await _userService.CreateAsync(new CreateUserDto
        {
            Username = model.Username,
            Email = model.Email,
            Password = model.Password,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
            IsActive = model.IsActive,
            RoleIds = model.SelectedRoleIds
        });

        if (result.IsFailure)
        {
            var rolesResult = await _roleService.GetAllAsync();
            model.AvailableRoles = rolesResult.IsSuccess ? rolesResult.Data! : new List<RoleListDto>();
            ModelState.AddModelError("", result.Errors.FirstOrDefault() ?? "خطأ في إنشاء المستخدم");
            return View(model);
        }

        SetSuccessMessage("تم إنشاء المستخدم بنجاح");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userResult = await _userService.GetByIdAsync(id);
        if (userResult.IsFailure)
        {
            SetErrorMessage("المستخدم غير موجود");
            return RedirectToAction(nameof(Index));
        }

        var rolesResult = await _roleService.GetAllAsync();
        var user = userResult.Data!;

        return View(new EditUserViewModel
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            AvailableRoles = rolesResult.IsSuccess ? rolesResult.Data! : new List<RoleListDto>(),
            SelectedRoleIds = rolesResult.Data?.Where(r => user.Roles.Contains(r.Name)).Select(r => r.Id).ToList() ?? new List<int>()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var rolesResult = await _roleService.GetAllAsync();
            model.AvailableRoles = rolesResult.IsSuccess ? rolesResult.Data! : new List<RoleListDto>();
            return View(model);
        }

        var result = await _userService.UpdateAsync(id, new UpdateUserDto
        {
            Email = model.Email,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
            IsActive = model.IsActive,
            RoleIds = model.SelectedRoleIds
        });

        if (result.IsFailure)
        {
            var rolesResult = await _roleService.GetAllAsync();
            model.AvailableRoles = rolesResult.IsSuccess ? rolesResult.Data! : new List<RoleListDto>();
            ModelState.AddModelError("", result.Errors.FirstOrDefault() ?? "خطأ في تحديث المستخدم");
            return View(model);
        }

        SetSuccessMessage("تم تحديث المستخدم بنجاح");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _userService.DeleteAsync(id);

        if (result.IsFailure)
            SetErrorMessage(result.Errors.FirstOrDefault() ?? "خطأ في حذف المستخدم");
        else
            SetSuccessMessage("تم حذف المستخدم بنجاح");

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var result = await _userService.ToggleStatusAsync(id);

        if (result.IsFailure)
            SetErrorMessage(result.Errors.FirstOrDefault() ?? "خطأ");
        else
            SetSuccessMessage(result.Message ?? "تم تغيير الحالة");

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(int id)
    {
        var userResult = await _userService.GetByIdAsync(id);
        if (userResult.IsFailure)
        {
            SetErrorMessage("المستخدم غير موجود");
            return RedirectToAction(nameof(Index));
        }

        return View(new ResetUserPasswordViewModel
        {
            UserId = id,
            Username = userResult.Data!.Username
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetUserPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _userService.ResetPasswordAsync(new ResetPasswordDto
        {
            UserId = model.UserId,
            NewPassword = model.NewPassword
        });

        if (result.IsFailure)
        {
            ModelState.AddModelError("", result.Errors.FirstOrDefault() ?? "خطأ");
            return View(model);
        }

        SetSuccessMessage("تم إعادة تعيين كلمة المرور بنجاح");
        return RedirectToAction(nameof(Index));
    }
}
#endregion

#region Roles Controller
[Authorize]
public class RolesController : BaseController
{
    private readonly IRoleService _roleService;
    private readonly IPermissionService _permissionService;

    public RolesController(IRoleService roleService, IPermissionService permissionService)
    {
        _roleService = roleService;
        _permissionService = permissionService;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _roleService.GetAllAsync();
        return View(new RolesViewModel
        {
            Roles = result.IsSuccess ? result.Data! : new List<RoleListDto>()
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var roleResult = await _roleService.GetByIdAsync(id);
        if (roleResult.IsFailure)
        {
            SetErrorMessage("الدور غير موجود");
            return RedirectToAction(nameof(Index));
        }

        var permissionsResult = await _roleService.GetRolePermissionsAsync(id);

        return View(new RoleDetailsViewModel
        {
            Role = roleResult.Data!,
            Permissions = permissionsResult.IsSuccess ? permissionsResult.Data! : new List<PermissionDto>()
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var permissionsResult = await _permissionService.GetGroupedAsync();
        return View(new CreateRoleViewModel
        {
            PermissionGroups = permissionsResult.IsSuccess ? permissionsResult.Data! : new List<PermissionGroupDto>()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var permissionsResult = await _permissionService.GetGroupedAsync();
            model.PermissionGroups = permissionsResult.IsSuccess ? permissionsResult.Data! : new List<PermissionGroupDto>();
            return View(model);
        }

        var result = await _roleService.CreateAsync(new CreateRoleDto
        {
            Name = model.Name,
            Description = model.Description,
            PermissionIds = model.SelectedPermissionIds
        });

        if (result.IsFailure)
        {
            var permissionsResult = await _permissionService.GetGroupedAsync();
            model.PermissionGroups = permissionsResult.IsSuccess ? permissionsResult.Data! : new List<PermissionGroupDto>();
            ModelState.AddModelError("", result.Errors.FirstOrDefault() ?? "خطأ في إنشاء الدور");
            return View(model);
        }

        SetSuccessMessage("تم إنشاء الدور بنجاح");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var roleResult = await _roleService.GetByIdAsync(id);
        if (roleResult.IsFailure)
        {
            SetErrorMessage("الدور غير موجود");
            return RedirectToAction(nameof(Index));
        }

        var role = roleResult.Data!;
        return View(new EditRoleViewModel
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditRoleViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (model.IsSystemRole)
        {
            SetErrorMessage("لا يمكن تعديل أدوار النظام");
            return RedirectToAction(nameof(Index));
        }

        var result = await _roleService.UpdateAsync(id, new UpdateRoleDto
        {
            Name = model.Name,
            Description = model.Description
        });

        if (result.IsFailure)
        {
            ModelState.AddModelError("", result.Errors.FirstOrDefault() ?? "خطأ في تحديث الدور");
            return View(model);
        }

        SetSuccessMessage("تم تحديث الدور بنجاح");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _roleService.DeleteAsync(id);

        if (result.IsFailure)
            SetErrorMessage(result.Errors.FirstOrDefault() ?? "خطأ في حذف الدور");
        else
            SetSuccessMessage("تم حذف الدور بنجاح");

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Permissions(int id)
    {
        var roleResult = await _roleService.GetByIdAsync(id);
        if (roleResult.IsFailure)
        {
            SetErrorMessage("الدور غير موجود");
            return RedirectToAction(nameof(Index));
        }

        var permissionsResult = await _permissionService.GetPermissionsForRoleAsync(id);
        var groupedResult = await _permissionService.GetGroupedAsync();

        return View(new RolePermissionsViewModel
        {
            RoleId = id,
            RoleName = roleResult.Data!.Name,
            IsSystemRole = roleResult.Data.IsSystemRole,
            PermissionGroups = groupedResult.IsSuccess ? groupedResult.Data! : new List<PermissionGroupDto>(),
            GrantedPermissionIds = permissionsResult.IsSuccess ? permissionsResult.Data!.Where(p => p.IsGranted).Select(p => p.Id).ToList() : new List<int>()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Permissions(int id, List<int> permissionIds)
    {
        var permissions = permissionIds.Select(pid => new PermissionAssignmentDto
        {
            PermissionId = pid,
            IsGranted = true
        }).ToList();

        var result = await _roleService.AssignPermissionsAsync(id, permissions);

        if (result.IsFailure)
            SetErrorMessage(result.Errors.FirstOrDefault() ?? "خطأ في تحديث الصلاحيات");
        else
            SetSuccessMessage("تم تحديث الصلاحيات بنجاح");

        return RedirectToAction(nameof(Permissions), new { id });
    }
}
#endregion

#region Profile Controller
[Authorize]
public class ProfileController : BaseController
{
    private readonly IUserService _userService;

    public ProfileController(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Auth");

        var result = await _userService.GetByIdAsync(userId.Value);
        if (result.IsFailure)
            return RedirectToAction("Login", "Auth");

        return View(new ProfileViewModel { User = result.Data! });
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Auth");

        var result = await _userService.GetByIdAsync(userId.Value);
        if (result.IsFailure)
            return RedirectToAction("Login", "Auth");

        var user = result.Data!;
        return View(new EditProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Auth");

        var result = await _userService.UpdateAsync(userId.Value, new UpdateUserDto
        {
            FullName = model.FullName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber
        });

        if (result.IsFailure)
        {
            ModelState.AddModelError("", result.Errors.FirstOrDefault() ?? "خطأ");
            return View(model);
        }

        SetSuccessMessage("تم تحديث الملف الشخصي بنجاح");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return RedirectToAction("Login", "Auth");

        var result = await _userService.ChangePasswordAsync(userId.Value, new ChangePasswordDto
        {
            CurrentPassword = model.CurrentPassword,
            NewPassword = model.NewPassword,
            ConfirmPassword = model.ConfirmPassword
        });

        if (result.IsFailure)
        {
            ModelState.AddModelError("", result.Errors.FirstOrDefault() ?? "خطأ");
            return View(model);
        }

        SetSuccessMessage("تم تغيير كلمة المرور بنجاح");
        return RedirectToAction(nameof(Index));
    }
}
#endregion

using AdminPanel.Application.Common.Interfaces;
using AdminPanel.Application.Common.Models;
using AdminPanel.Application.Features.Auth.DTOs;
using AdminPanel.Application.Features.Permissions.DTOs;
using AdminPanel.Application.Features.Roles.DTOs;
using AdminPanel.Application.Features.Users.DTOs;
using AdminPanel.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdminPanel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected int? GetUserId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    protected ApiResponse<T> HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return ApiResponse<T>.Ok(result.Data!, result.Message);
        return ApiResponse<T>.Fail(result.Errors);
    }
}

[Route("api/[controller]")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Ok(HandleResult(result));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<bool>>> Logout()
    {
        var userId = GetUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var result = await _authService.LogoutAsync(userId.Value);
        return Ok(HandleResult(result));
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        var result = await _authService.RefreshTokenAsync(dto);
        return Ok(HandleResult(result));
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var result = await _authService.ForgotPasswordAsync(dto);
        return Ok(HandleResult(result));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ResetPassword([FromBody] ResetPasswordWithTokenDto dto)
    {
        var result = await _authService.ResetPasswordAsync(dto);
        return Ok(HandleResult(result));
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return Ok(HandleResult(result));
    }
}

[Authorize]
[Route("api/[controller]")]
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedList<UserListDto>>>> GetAll([FromQuery] UserFilterDto filter)
    {
        var result = await _userService.GetPagedAsync(filter);
        return Ok(HandleResult(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        return Ok(HandleResult(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create([FromBody] CreateUserDto dto)
    {
        var result = await _userService.CreateAsync(dto);
        return Ok(HandleResult(result));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Update(int id, [FromBody] UpdateUserDto dto)
    {
        var result = await _userService.UpdateAsync(id, dto);
        return Ok(HandleResult(result));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _userService.DeleteAsync(id);
        return Ok(HandleResult(result));
    }

    [HttpPatch("{id}/toggle-status")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleStatus(int id)
    {
        var result = await _userService.ToggleStatusAsync(id);
        return Ok(HandleResult(result));
    }

    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var result = await _userService.ChangePasswordAsync(userId.Value, dto);
        return Ok(HandleResult(result));
    }

    [HttpPost("{id}/reset-password")]
    public async Task<ActionResult<ApiResponse<bool>>> ResetPassword(int id, [FromBody] ResetPasswordDto dto)
    {
        var resetDto = new ResetPasswordDto { UserId = id, NewPassword = dto.NewPassword };
        var result = await _userService.ResetPasswordAsync(resetDto);
        return Ok(HandleResult(result));
    }

    [HttpPost("{id}/roles")]
    public async Task<ActionResult<ApiResponse<bool>>> AssignRoles(int id, [FromBody] List<int> roleIds)
    {
        var result = await _userService.AssignRolesAsync(id, roleIds);
        return Ok(HandleResult(result));
    }

    [HttpGet("{id}/permissions")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetPermissions(int id)
    {
        var result = await _userService.GetUserPermissionsAsync(id);
        return Ok(HandleResult(result));
    }

    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetProfile()
    {
        var userId = GetUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var result = await _userService.GetByIdAsync(userId.Value);
        return Ok(HandleResult(result));
    }
}

[Authorize]
[Route("api/[controller]")]
public class RolesController : BaseApiController
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RoleListDto>>>> GetAll()
    {
        var result = await _roleService.GetAllAsync();
        return Ok(HandleResult(result));
    }

    [HttpGet("paged")]
    public async Task<ActionResult<ApiResponse<PaginatedList<RoleListDto>>>> GetPaged([FromQuery] PaginationParams pagination)
    {
        var result = await _roleService.GetPagedAsync(pagination);
        return Ok(HandleResult(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetById(int id)
    {
        var result = await _roleService.GetByIdAsync(id);
        return Ok(HandleResult(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Create([FromBody] CreateRoleDto dto)
    {
        var result = await _roleService.CreateAsync(dto);
        return Ok(HandleResult(result));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Update(int id, [FromBody] UpdateRoleDto dto)
    {
        var result = await _roleService.UpdateAsync(id, dto);
        return Ok(HandleResult(result));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var result = await _roleService.DeleteAsync(id);
        return Ok(HandleResult(result));
    }

    [HttpGet("{id}/permissions")]
    public async Task<ActionResult<ApiResponse<List<PermissionDto>>>> GetPermissions(int id)
    {
        var result = await _roleService.GetRolePermissionsAsync(id);
        return Ok(HandleResult(result));
    }

    [HttpPost("{id}/permissions")]
    public async Task<ActionResult<ApiResponse<bool>>> AssignPermissions(int id, [FromBody] List<PermissionAssignmentDto> permissions)
    {
        var result = await _roleService.AssignPermissionsAsync(id, permissions);
        return Ok(HandleResult(result));
    }
}

[Authorize]
[Route("api/[controller]")]
public class PermissionsController : BaseApiController
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PermissionDto>>>> GetAll()
    {
        var result = await _permissionService.GetAllAsync();
        return Ok(HandleResult(result));
    }

    [HttpGet("grouped")]
    public async Task<ActionResult<ApiResponse<List<PermissionGroupDto>>>> GetGrouped()
    {
        var result = await _permissionService.GetGroupedAsync();
        return Ok(HandleResult(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PermissionDto>>> GetById(int id)
    {
        var result = await _permissionService.GetByIdAsync(id);
        return Ok(HandleResult(result));
    }

    [HttpGet("for-role/{roleId}")]
    public async Task<ActionResult<ApiResponse<List<PermissionDto>>>> GetForRole(int roleId)
    {
        var result = await _permissionService.GetPermissionsForRoleAsync(roleId);
        return Ok(HandleResult(result));
    }

    [HttpGet("my-permissions")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetMyPermissions()
    {
        var userId = GetUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var result = await _permissionService.GetUserPermissionsAsync(userId.Value);
        return Ok(HandleResult(result));
    }
}

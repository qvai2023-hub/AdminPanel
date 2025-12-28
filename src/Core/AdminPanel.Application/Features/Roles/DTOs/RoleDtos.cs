namespace AdminPanel.Application.Features.Roles.DTOs;

public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; }
    public int UsersCount { get; set; }
    public int PermissionsCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RoleListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; }
    public int UsersCount { get; set; }
    public int PermissionsCount { get; set; }
}

public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<int>? PermissionIds { get; set; }
}

public class UpdateRoleDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class RoleFilterDto
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsSystemRole { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class AssignPermissionsDto
{
    public int RoleId { get; set; }
    public List<PermissionAssignmentDto> Permissions { get; set; } = new();
}

public class PermissionAssignmentDto
{
    public int PermissionId { get; set; }
    public bool IsGranted { get; set; }
}

// Permission Matrix DTOs
public class RolePermissionMatrixDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsSystemRole { get; set; }
    public List<PagePermissionDto> Pages { get; set; } = new();
    public List<ActionHeaderDto> Actions { get; set; } = new();
}

public class PagePermissionDto
{
    public int PageId { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public List<ActionPermissionDto> ActionPermissions { get; set; } = new();
}

public class ActionHeaderDto
{
    public int ActionId { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
}

public class ActionPermissionDto
{
    public int ActionId { get; set; }
    public int? PageActionId { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsGranted { get; set; }
}

public class SaveRolePermissionsDto
{
    public int RoleId { get; set; }
    public List<PageActionPermissionDto> Permissions { get; set; } = new();
}

public class PageActionPermissionDto
{
    public int PageActionId { get; set; }
    public bool IsGranted { get; set; }
}

namespace AdminPanel.Application.Features.Roles.DTOs;

public class RoleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
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

namespace AdminPanel.Application.Features.Permissions.DTOs;

public class PermissionDto
{
    public int Id { get; set; }
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? DisplayNameAr { get; set; }
    public string? DisplayNameEn { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsGranted { get; set; }
}

public class PermissionGroupDto
{
    public string Module { get; set; } = string.Empty;
    public string ModuleDisplayName { get; set; } = string.Empty;
    public List<PermissionDto> Permissions { get; set; } = new();
}

public class UpdatePermissionDto
{
    public string? DisplayNameAr { get; set; }
    public string? DisplayNameEn { get; set; }
    public int? DisplayOrder { get; set; }
}

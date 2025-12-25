namespace AdminPanel.Application.Features.Tenants.DTOs;

public class TenantDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public int UsersCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTenantDto
{
    public string Name { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? SubscriptionEndDate { get; set; }
}

public class UpdateTenantDto
{
    public string? Name { get; set; }
    public string? Domain { get; set; }
    public string? LogoUrl { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
}

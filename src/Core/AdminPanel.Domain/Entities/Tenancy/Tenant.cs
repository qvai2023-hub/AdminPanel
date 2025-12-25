using AdminPanel.Domain.Entities.Common;
using AdminPanel.Domain.Entities.Identity;

namespace AdminPanel.Domain.Entities.Tenancy;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? SubscriptionEndDate { get; set; }
    public string? Settings { get; set; }

    // Navigation
    public ICollection<User> Users { get; set; } = new List<User>();
}

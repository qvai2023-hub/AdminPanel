namespace AdminPanel.Domain.Entities.Common;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    int? CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    int? UpdatedBy { get; set; }
}

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    int? DeletedBy { get; set; }
}

public interface ITenantEntity
{
    int TenantId { get; set; }
}

public abstract class BaseEntity : IAuditableEntity, ISoftDelete
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
}

public abstract class TenantBaseEntity : BaseEntity, ITenantEntity
{
    public int TenantId { get; set; }
}

using AdminPanel.Domain.Enums;

namespace AdminPanel.Application.Features.AuditLogs.DTOs;

public class AuditLogDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public AuditAction Action { get; set; }
    public string ActionName { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? AffectedColumns { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AuditLogDetailDto : AuditLogDto
{
    public string? UserAgent { get; set; }
    public string? AdditionalInfo { get; set; }
}

public class AuditLogFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public int? UserId { get; set; }
    public string? EntityName { get; set; }
    public AuditAction? Action { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}

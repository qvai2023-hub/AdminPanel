using AdminPanel.Domain.Entities.Common;

namespace AdminPanel.Domain.Entities.Identity;

/// <summary>
/// Represents an action that can be performed (View, Create, Edit, Delete, etc.)
/// </summary>
public class Action : BaseEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<PageAction> PageActions { get; set; } = new List<PageAction>();
}

/// <summary>
/// Represents a page/module in the system (Users, Roles, Calendar, etc.)
/// </summary>
public class Page : BaseEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int? ParentId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsInMenu { get; set; } = true;

    // Navigation
    public Page? Parent { get; set; }
    public ICollection<Page> Children { get; set; } = new List<Page>();
    public ICollection<PageAction> PageActions { get; set; } = new List<PageAction>();
}

/// <summary>
/// Junction table linking Pages to Actions (which actions are available on which pages)
/// </summary>
public class PageAction
{
    public int Id { get; set; }
    public int PageId { get; set; }
    public int ActionId { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Page? Page { get; set; }
    public Action? Action { get; set; }
    public ICollection<RolePageAction> RolePageActions { get; set; } = new List<RolePageAction>();
}

/// <summary>
/// Assigns page-action permissions to roles
/// </summary>
public class RolePageAction
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public int PageActionId { get; set; }
    public bool IsGranted { get; set; } = true;

    // Navigation
    public Role? Role { get; set; }
    public PageAction? PageAction { get; set; }
}

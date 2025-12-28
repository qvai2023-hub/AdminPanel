using AdminPanel.Application.Features.Actions.DTOs;

namespace AdminPanel.Application.Features.Pages.DTOs;

/// <summary>
/// DTO for displaying Page details
/// </summary>
public class PageDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int? ParentId { get; set; }
    public string? ParentNameAr { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsInMenu { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PageDto> Children { get; set; } = new();
    public List<ActionListDto> AvailableActions { get; set; } = new();
}

/// <summary>
/// DTO for Page list items
/// </summary>
public class PageListDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int? ParentId { get; set; }
    public string? ParentNameAr { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsInMenu { get; set; }
    public int ActionsCount { get; set; }
    public int ChildrenCount { get; set; }
}

/// <summary>
/// DTO for creating a new Page
/// </summary>
public class CreatePageDto
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int? ParentId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsInMenu { get; set; } = true;
}

/// <summary>
/// DTO for updating an existing Page
/// </summary>
public class UpdatePageDto
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int? ParentId { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsInMenu { get; set; }
}

/// <summary>
/// DTO for filtering Pages
/// </summary>
public class PageFilterDto
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsInMenu { get; set; }
    public int? ParentId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// DTO for assigning actions to a page
/// </summary>
public class PageActionAssignDto
{
    public int PageId { get; set; }
    public List<int> ActionIds { get; set; } = new();
}

/// <summary>
/// Simple DTO for dropdown lists
/// </summary>
public class PageDropdownDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public int? ParentId { get; set; }
}

namespace AdminPanel.Application.Features.Menu.DTOs;

public class MenuItemDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public List<MenuItemDto> Children { get; set; } = new();
}

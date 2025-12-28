using AdminPanel.Application.Common.Interfaces;
using AdminPanel.Application.Features.Menu.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdminPanel.Web.ViewComponents;

public class SidebarMenuViewComponent : ViewComponent
{
    private readonly IMenuService _menuService;

    public SidebarMenuViewComponent(IMenuService menuService)
    {
        _menuService = menuService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Get current user ID from claims
        var userIdClaim = HttpContext.User.FindFirst("UserId")
            ?? HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return View(new List<MenuItemDto>());
        }

        var menuItems = await _menuService.GetUserMenuAsync(userId);
        return View(menuItems);
    }
}

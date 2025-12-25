using AdminPanel.Application.Common.Interfaces;
using AdminPanel.Application.Features.Roles.DTOs;
using AdminPanel.Web.Controllers;
using AdminPanel.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

public class RolesController : BaseController
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _roleService.GetAllAsync();
        var viewModel = new RolesViewModel
        {
            Roles = result.IsSuccess ? result.Data! : new List<RoleListDto>()
        };
        return View(viewModel);
    }
}
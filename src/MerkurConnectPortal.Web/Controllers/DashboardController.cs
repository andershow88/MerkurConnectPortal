using Microsoft.AspNetCore.Mvc;
using MerkurConnectPortal.Application.Interfaces;
using MerkurConnectPortal.Web.ViewModels;

namespace MerkurConnectPortal.Web.Controllers;

public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService) =>
        _dashboardService = dashboardService;

    public async Task<IActionResult> Index()
    {
        var dashboard = await _dashboardService.GetDashboardAsync(GetPartnerBankId());
        return View(new DashboardViewModel { Dashboard = dashboard });
    }
}

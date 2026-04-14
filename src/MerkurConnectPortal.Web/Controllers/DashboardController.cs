using Microsoft.AspNetCore.Mvc;
using MerkurConnectPortal.Application.Interfaces;
using MerkurConnectPortal.Web.ViewModels;

namespace MerkurConnectPortal.Web.Controllers;

public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboardService;
    private readonly INachrichtService _nachrichtService;
    private readonly IDokumentService _dokumentService;

    public DashboardController(
        IDashboardService dashboardService,
        INachrichtService nachrichtService,
        IDokumentService dokumentService)
    {
        _dashboardService = dashboardService;
        _nachrichtService = nachrichtService;
        _dokumentService = dokumentService;
    }

    public async Task<IActionResult> Index()
    {
        var dashboard = await _dashboardService.GetDashboardAsync(GetPartnerBankId());
        return View(new DashboardViewModel { Dashboard = dashboard });
    }

    // ── Polling-Endpunkt für Benachrichtigungsglocke ─────────────────────────

    [HttpGet]
    public async Task<IActionResult> UngeleseneAnzahlJson()
    {
        var partnerBankId = GetPartnerBankId();
        var nachrichten = await _nachrichtService.GetUngeleseneAnzahlForPartnerBankAsync(partnerBankId);
        var dokumente = await _dokumentService.GetUngeleseneAnzahlForPartnerBankAsync(partnerBankId);
        return Json(new { anzahl = nachrichten + dokumente });
    }

    // ── Aktivitäten-Übersicht ─────────────────────────────────────────────────

    public async Task<IActionResult> Aktivitaeten()
    {
        var partnerBankId = GetPartnerBankId();
        var aktivitaeten = await _dashboardService.GetUngeleseneAktivitaetenAsync(partnerBankId);
        var ungelesen = aktivitaeten.Count(a => !a.Gelesen);

        ViewData["Breadcrumb"] = "<a href='/Dashboard'>Dashboard</a><span class='separator'>/</span><span>Aktivitäten</span>";
        return View(new AktivitaetenViewModel
        {
            Aktivitaeten = aktivitaeten,
            UngeleseneAnzahl = ungelesen
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlleGelesenMarkieren()
    {
        await _dashboardService.MarkiereAlleGelesenAsync(GetPartnerBankId());
        TempData["Erfolgsmeldung"] = "Alle Aktivitäten wurden als gelesen markiert.";
        return RedirectToAction("Aktivitaeten");
    }
}

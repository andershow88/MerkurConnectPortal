using Microsoft.AspNetCore.Mvc;
using MerkurConnectPortal.Application.Interfaces;
using MerkurConnectPortal.Web.ViewModels;

namespace MerkurConnectPortal.Web.Controllers;

public class ObjekteController : BaseController
{
    private readonly IObjektService _objektService;
    private readonly IDokumentService _dokumentService;
    private readonly INachrichtService _nachrichtService;

    public ObjekteController(
        IObjektService objektService,
        IDokumentService dokumentService,
        INachrichtService nachrichtService)
    {
        _objektService = objektService;
        _dokumentService = dokumentService;
        _nachrichtService = nachrichtService;
    }

    public async Task<IActionResult> Index(string? suche, string? status, string? sortierung)
    {
        var objekte = await _objektService.GetObjekteByPartnerBankAsync(
            GetPartnerBankId(), suche, status, sortierung);

        return View(new ObjektListViewModel
        {
            Objekte = objekte,
            Suchbegriff = suche,
            StatusFilter = status,
            Sortierung = sortierung
        });
    }

    public async Task<IActionResult> Details(int id)
    {
        var objekt = await _objektService.GetObjektDetailAsync(id, GetPartnerBankId());
        if (objekt is null) return NotFound();

        var dokumente = await _dokumentService.GetDokumenteByObjektAsync(id, GetPartnerBankId());
        var nachrichten = await _nachrichtService.GetNachrichtenByObjektAsync(id, GetPartnerBankId());

        return View(new ObjektDetailViewModel
        {
            Objekt = objekt,
            LetzteDokumente = dokumente.Take(5).ToList(),
            LetzteNachrichten = nachrichten.TakeLast(3).ToList()
        });
    }
}

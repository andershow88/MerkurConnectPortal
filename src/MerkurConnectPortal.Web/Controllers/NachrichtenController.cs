using Microsoft.AspNetCore.Mvc;
using MerkurConnectPortal.Application.Interfaces;
using MerkurConnectPortal.Web.ViewModels;

namespace MerkurConnectPortal.Web.Controllers;

public class NachrichtenController : BaseController
{
    private readonly INachrichtService _nachrichtService;
    private readonly IObjektService _objektService;

    public NachrichtenController(INachrichtService nachrichtService, IObjektService objektService)
    {
        _nachrichtService = nachrichtService;
        _objektService = objektService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int objektId)
    {
        var objekt = await _objektService.GetObjektDetailAsync(objektId, GetPartnerBankId());
        if (objekt is null) return NotFound();

        var nachrichten = await _nachrichtService.GetNachrichtenByObjektAsync(objektId, GetPartnerBankId());

        return View(new NachrichtViewModel
        {
            ObjektId = objektId,
            ObjektName = objekt.Objektname,
            Nachrichten = nachrichten
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Senden(NachrichtViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var nachrichten = await _nachrichtService.GetNachrichtenByObjektAsync(model.ObjektId, GetPartnerBankId());
            model.Nachrichten = nachrichten;
            return View("Index", model);
        }

        await _nachrichtService.SendeNachrichtAsync(
            model.ObjektId,
            GetPartnerBankId(),
            GetAnzeigename(),
            model.NeueNachricht);

        return RedirectToAction("Index", new { objektId = model.ObjektId });
    }
}

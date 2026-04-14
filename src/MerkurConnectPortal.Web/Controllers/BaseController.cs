using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MerkurConnectPortal.Web.Controllers;

[Authorize]
public abstract class BaseController : Controller
{
    protected int GetPartnerBankId()
    {
        var claim = User.FindFirst("PartnerBankId");
        return claim is not null && int.TryParse(claim.Value, out var id) ? id : 0;
    }

    protected string GetAnzeigename() =>
        User.FindFirst(ClaimTypes.GivenName)?.Value ?? User.Identity?.Name ?? string.Empty;

    protected string GetPartnerBankName() =>
        User.FindFirst("PartnerBankName")?.Value ?? string.Empty;

    protected bool IsAdmin => User.IsInRole("Admin");

    // Admin-Benutzer haben im Partnerbank-Bereich nichts zu suchen
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (IsAdmin)
        {
            context.Result = RedirectToAction("Index", "Admin");
            return;
        }
        base.OnActionExecuting(context);
    }
}

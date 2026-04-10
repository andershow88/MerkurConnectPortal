using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
}

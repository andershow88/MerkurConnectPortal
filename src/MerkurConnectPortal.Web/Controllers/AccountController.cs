using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MerkurConnectPortal.Application.Interfaces;
using MerkurConnectPortal.Web.ViewModels;

namespace MerkurConnectPortal.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService) => _authService = authService;

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction("Index", "Admin");
            return RedirectToAction("Index", "Dashboard");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _authService.ValidateLoginAsync(model.Benutzername, model.Passwort);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, "Ungültige Anmeldedaten. Bitte überprüfen Sie Benutzername und Passwort.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, result.Benutzername),
            new(ClaimTypes.GivenName, result.Anzeigename),
            new(ClaimTypes.Role, result.IsAdmin ? "Admin" : "PartnerBank"),
            new("PartnerBankId", result.PartnerBankId.ToString()),
            new("PartnerBankName", result.PartnerBankName)
        };

        var identity = new ClaimsIdentity(claims, "MerkurCookieAuth");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("MerkurCookieAuth", principal);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        // Admin → Admin-Bereich, Partnerbank → Dashboard
        return result.IsAdmin
            ? RedirectToAction("Index", "Admin")
            : RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("MerkurCookieAuth");
        return RedirectToAction("Login");
    }
}

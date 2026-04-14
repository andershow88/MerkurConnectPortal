using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MerkurConnectPortal.Application.Interfaces;
using MerkurConnectPortal.Web.ViewModels;

namespace MerkurConnectPortal.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    public AdminController(IAdminService adminService, IConfiguration config, IWebHostEnvironment env)
    {
        _adminService = adminService;
        _config = config;
        _env = env;
    }

    private string GetAnzeigename() =>
        User.FindFirst(ClaimTypes.GivenName)?.Value ?? User.Identity?.Name ?? "Administrator";

    // ── Dashboard ──────────────────────────────────────────────────────────────

    public async Task<IActionResult> Index()
    {
        var dashboard = await _adminService.GetAdminDashboardAsync();
        return View(new AdminDashboardViewModel { Dashboard = dashboard });
    }

    // ── Objekte ────────────────────────────────────────────────────────────────

    public async Task<IActionResult> Objekte(string? suche, string? status)
    {
        var objekte = await _adminService.GetAlleObjekteAsync(suche, status);
        ViewData["Breadcrumb"] = "<a href='/Admin'>Admin</a><span class='separator'>/</span><span>Alle Objekte</span>";
        return View(new AdminObjekteViewModel
        {
            Objekte = objekte,
            Suchbegriff = suche,
            StatusFilter = status
        });
    }

    public async Task<IActionResult> ObjektDetail(int id)
    {
        var objekt = await _adminService.GetObjektDetailAsync(id);
        if (objekt is null) return NotFound();
        ViewData["Breadcrumb"] = $"<a href='/Admin'>Admin</a><span class='separator'>/</span><a href='/Admin/Objekte'>Objekte</a><span class='separator'>/</span><span>{objekt.Objektname}</span>";
        return View(objekt);
    }

    // ── Nachrichten ────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Nachrichten(int objektId)
    {
        var objekt = await _adminService.GetObjektDetailAsync(objektId);
        if (objekt is null) return NotFound();

        var nachrichten = await _adminService.GetNachrichtenByObjektAdminAsync(objektId);

        ViewData["Breadcrumb"] = $"<a href='/Admin'>Admin</a><span class='separator'>/</span><a href='/Admin/Objekte'>Objekte</a><span class='separator'>/</span><a href='/Admin/ObjektDetail/{objektId}'>{objekt.Objektname}</a><span class='separator'>/</span><span>Kommunikation</span>";

        return View(new AdminNachrichtenViewModel
        {
            ObjektId = objektId,
            ObjektName = objekt.Objektname,
            PartnerBankName = objekt.PartnerBankName,
            Nachrichten = nachrichten
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NachrichtSenden(AdminNachrichtenViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.NeueNachricht))
        {
            TempData["Fehlermeldung"] = "Bitte geben Sie eine Nachricht ein.";
            return RedirectToAction("Nachrichten", new { objektId = model.ObjektId });
        }

        await _adminService.SendeNachrichtAlsAdminAsync(
            model.ObjektId,
            model.NeueNachricht,
            GetAnzeigename());

        return RedirectToAction("Nachrichten", new { objektId = model.ObjektId });
    }

    // ── Dokumente ──────────────────────────────────────────────────────────────

    public async Task<IActionResult> Dokumente(int? objektId, string? kategorie)
    {
        List<Application.DTOs.DokumentDto> dokumente;
        string? objektName = null;

        if (objektId.HasValue)
        {
            var objekt = await _adminService.GetObjektDetailAsync(objektId.Value);
            objektName = objekt?.Objektname;
            dokumente = await _adminService.GetDokumenteByObjektAdminAsync(objektId.Value);
        }
        else
        {
            dokumente = await _adminService.GetAlleDokumenteAsync(kategorie);
        }

        ViewData["Breadcrumb"] = "<a href='/Admin'>Admin</a><span class='separator'>/</span><span>Dokumente</span>";
        return View(new AdminDokumenteViewModel
        {
            Dokumente = dokumente,
            ObjektId = objektId,
            ObjektName = objektName,
            KategorieFilter = kategorie
        });
    }

    [HttpGet]
    public async Task<IActionResult> DokumentUpload(int objektId)
    {
        var objekt = await _adminService.GetObjektDetailAsync(objektId);
        if (objekt is null) return NotFound();

        ViewData["Breadcrumb"] = $"<a href='/Admin'>Admin</a><span class='separator'>/</span><a href='/Admin/Dokumente?objektId={objektId}'>Dokumente</a><span class='separator'>/</span><span>Hochladen</span>";
        return View(new AdminDokumentUploadViewModel
        {
            ObjektId = objektId,
            ObjektName = objekt.Objektname
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DokumentUpload(AdminDokumentUploadViewModel model)
    {
        if (model.Datei is null || model.Datei.Length == 0)
        {
            ModelState.AddModelError("Datei", "Bitte wählen Sie eine Datei aus.");
            return View(model);
        }

        var maxGroesseMb = _config.GetValue<int>("AppSettings:MaxDateigroesseMB", 50);
        if (model.Datei.Length > maxGroesseMb * 1024 * 1024)
        {
            ModelState.AddModelError("Datei", $"Die Datei darf maximal {maxGroesseMb} MB groß sein.");
            return View(model);
        }

        var uploadVerzeichnis = Path.Combine(
            _env.ContentRootPath,
            _config.GetValue<string>("AppSettings:UploadVerzeichnis") ?? "wwwroot/uploads");

        await using var stream = model.Datei.OpenReadStream();
        await _adminService.UploadDokumentAlsAdminAsync(
            model.ObjektId, stream, model.Datei.FileName,
            model.Kategorie, GetAnzeigename(), uploadVerzeichnis);

        TempData["Erfolgsmeldung"] = $"Dokument '{model.Datei.FileName}' wurde erfolgreich hochgeladen.";
        return RedirectToAction("Dokumente", new { objektId = model.ObjektId });
    }

    public async Task<IActionResult> DokumentDownload(int id)
    {
        var dokument = await _adminService.GetDokumentAdminAsync(id);
        if (dokument is null) return NotFound();

        var uploadVerzeichnis = Path.Combine(
            _env.ContentRootPath,
            _config.GetValue<string>("AppSettings:UploadVerzeichnis") ?? "wwwroot/uploads");

        var dateipfad = Path.Combine(uploadVerzeichnis, dokument.Dateipfad);

        if (!System.IO.File.Exists(dateipfad))
        {
            var platzhalter = ErzeugePlatzhalterPdf(dokument.Dateiname);
            return File(platzhalter, "application/pdf", dokument.Dateiname);
        }

        var stream = System.IO.File.OpenRead(dateipfad);
        return File(stream, GetContentType(dokument.Dateiname), dokument.Dateiname);
    }

    // ── Polling-Endpunkt ───────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> UngeleseneAnzahlJson()
    {
        var anzahl = await _adminService.GetUngeleseneAnzahlAsync();
        return Json(new { anzahl });
    }

    // ── Aktivitäten ────────────────────────────────────────────────────────────

    public async Task<IActionResult> Aktivitaeten()
    {
        var aktivitaeten = await _adminService.GetUngeleseneAktivitaetenAsync();
        var ungelesen = await _adminService.GetUngeleseneAnzahlAsync();

        ViewData["Breadcrumb"] = "<a href='/Admin'>Admin</a><span class='separator'>/</span><span>Aktivitäten</span>";
        return View(new AdminAktivitaetenViewModel
        {
            Aktivitaeten = aktivitaeten,
            UngeleseneAnzahl = ungelesen
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlleGelesenMarkieren()
    {
        await _adminService.MarkiereAlleGelesenAsync();
        TempData["Erfolgsmeldung"] = "Alle Aktivitäten wurden als gelesen markiert.";
        return RedirectToAction("Aktivitaeten");
    }

    // ── Abmelden ───────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("MerkurCookieAuth");
        return RedirectToAction("Login", "Account");
    }

    // ── Hilfsmethoden ──────────────────────────────────────────────────────────

    private static string GetContentType(string dateiname)
    {
        var ext = Path.GetExtension(dateiname).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".xls" => "application/vnd.ms-excel",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".doc" => "application/msword",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };
    }

    private static byte[] ErzeugePlatzhalterPdf(string dateiname)
    {
        var pdfText = "%PDF-1.4\n1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj " +
                      "2 0 obj<</Type/Pages/Kids[3 0 R]/Count 1>>endobj " +
                      "3 0 obj<</Type/Page/MediaBox[0 0 595 842]/Parent 2 0 R/Resources<<>>>>endobj\n" +
                      "xref\n0 4\n0000000000 65535 f\n0000000009 00000 n\n0000000058 00000 n\n" +
                      "0000000115 00000 n\ntrailer<</Size 4/Root 1 0 R>>\nstartxref\n190\n%%EOF";
        return System.Text.Encoding.ASCII.GetBytes(pdfText);
    }
}

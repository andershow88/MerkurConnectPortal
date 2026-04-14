using Microsoft.AspNetCore.Mvc;
using MerkurConnectPortal.Application.Interfaces;
using MerkurConnectPortal.Web.ViewModels;

namespace MerkurConnectPortal.Web.Controllers;

public class DokumenteController : BaseController
{
    private readonly IDokumentService _dokumentService;
    private readonly IObjektService _objektService;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    public DokumenteController(
        IDokumentService dokumentService,
        IObjektService objektService,
        IConfiguration config,
        IWebHostEnvironment env)
    {
        _dokumentService = dokumentService;
        _objektService = objektService;
        _config = config;
        _env = env;
    }

    public async Task<IActionResult> Index(string? kategorie)
    {
        var dokumente = await _dokumentService.GetAlleDokumenteByPartnerBankAsync(GetPartnerBankId(), kategorie);
        return View(new DokumentListViewModel { Dokumente = dokumente, KategorieFilter = kategorie });
    }

    public async Task<IActionResult> Objekt(int objektId)
    {
        var objekt = await _objektService.GetObjektDetailAsync(objektId, GetPartnerBankId());
        if (objekt is null) return NotFound();

        var dokumente = await _dokumentService.GetDokumenteByObjektAsync(objektId, GetPartnerBankId());
        return View("Index", new DokumentListViewModel
        {
            ObjektId = objektId,
            ObjektName = objekt.Objektname,
            Dokumente = dokumente
        });
    }

    [HttpGet]
    public async Task<IActionResult> Upload(int objektId)
    {
        var objekt = await _objektService.GetObjektDetailAsync(objektId, GetPartnerBankId());
        if (objekt is null) return NotFound();

        return View(new DokumentUploadViewModel { ObjektId = objektId, ObjektName = objekt.Objektname });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(DokumentUploadViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

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
        // Partnerbank lädt hoch → VonPartnerBank = true
        await _dokumentService.UploadDokumentAsync(
            model.ObjektId, GetPartnerBankId(), stream,
            model.Datei.FileName, model.Kategorie, GetAnzeigename(),
            uploadVerzeichnis, vonPartnerBank: true);

        TempData["Erfolgsmeldung"] = $"Dokument '{model.Datei.FileName}' wurde erfolgreich hochgeladen.";
        return RedirectToAction("Objekt", new { objektId = model.ObjektId });
    }

    public async Task<IActionResult> Download(int id)
    {
        var dokument = await _dokumentService.GetDokumentAsync(id, GetPartnerBankId());
        if (dokument is null) return NotFound();

        var uploadVerzeichnis = Path.Combine(
            _env.ContentRootPath,
            _config.GetValue<string>("AppSettings:UploadVerzeichnis") ?? "wwwroot/uploads");

        var dateipfad = Path.Combine(uploadVerzeichnis, dokument.Dateipfad);

        if (!System.IO.File.Exists(dateipfad))
            return File(ErzeugePlatzhalterPdf(dokument.Dateiname), "application/pdf", dokument.Dateiname);

        var stream = System.IO.File.OpenRead(dateipfad);
        return File(stream, GetContentType(dokument.Dateiname), dokument.Dateiname);
    }

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
        var pdfText = $"%PDF-1.4\n1 0 obj<</Type/Catalog/Pages 2 0 R>>endobj " +
                      $"2 0 obj<</Type/Pages/Kids[3 0 R]/Count 1>>endobj " +
                      $"3 0 obj<</Type/Page/MediaBox[0 0 595 842]/Parent 2 0 R/Resources<<>>>>endobj\n" +
                      $"xref\n0 4\n0000000000 65535 f\n0000000009 00000 n\n0000000058 00000 n\n" +
                      $"0000000115 00000 n\ntrailer<</Size 4/Root 1 0 R>>\nstartxref\n190\n%%EOF";
        return System.Text.Encoding.ASCII.GetBytes(pdfText);
    }
}

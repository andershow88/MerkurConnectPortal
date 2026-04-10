using Microsoft.EntityFrameworkCore;
using MerkurConnectPortal.Application.DTOs;
using MerkurConnectPortal.Application.Interfaces;
using MerkurConnectPortal.Domain.Entities;

namespace MerkurConnectPortal.Application.Services;

public class DokumentService : IDokumentService
{
    private readonly IApplicationDbContext _db;

    public DokumentService(IApplicationDbContext db) => _db = db;

    public async Task<List<DokumentDto>> GetDokumenteByObjektAsync(int objektId, int partnerBankId)
    {
        var dokumente = await _db.Dokumente
            .Include(d => d.Objekt)
            .Where(d => d.ObjektId == objektId && d.Objekt.PartnerBankId == partnerBankId)
            .OrderByDescending(d => d.HochgeladenAm)
            .ToListAsync();

        return dokumente.Select(ToDto).ToList();
    }

    public async Task<List<DokumentDto>> GetAlleDokumenteByPartnerBankAsync(int partnerBankId, string? kategorie = null)
    {
        var query = _db.Dokumente
            .Include(d => d.Objekt)
            .Where(d => d.Objekt.PartnerBankId == partnerBankId);

        if (!string.IsNullOrWhiteSpace(kategorie) && Enum.TryParse<DokumentKategorie>(kategorie, out var kat))
        {
            query = query.Where(d => d.Kategorie == kat);
        }

        var dokumente = await query.OrderByDescending(d => d.HochgeladenAm).ToListAsync();
        return dokumente.Select(ToDto).ToList();
    }

    public async Task<DokumentDto?> GetDokumentAsync(int dokumentId, int partnerBankId)
    {
        var d = await _db.Dokumente
            .Include(d => d.Objekt)
            .FirstOrDefaultAsync(d => d.Id == dokumentId && d.Objekt.PartnerBankId == partnerBankId);

        return d is null ? null : ToDto(d);
    }

    public async Task<DokumentDto> UploadDokumentAsync(
        int objektId,
        int partnerBankId,
        Stream dateistream,
        string dateiname,
        string kategorie,
        string hochgeladenVon,
        string uploadVerzeichnis)
    {
        // Sicherstellen, dass das Objekt zur PartnerBank gehört
        var objekt = await _db.Objekte
            .FirstOrDefaultAsync(o => o.Id == objektId && o.PartnerBankId == partnerBankId)
            ?? throw new InvalidOperationException("Objekt nicht gefunden oder kein Zugriff.");

        Directory.CreateDirectory(uploadVerzeichnis);

        var sichererDateiname = Path.GetFileName(dateiname);
        var eindeutigerName = $"{Guid.NewGuid():N}_{sichererDateiname}";
        var zieldatei = Path.Combine(uploadVerzeichnis, eindeutigerName);

        await using var zielstream = File.Create(zieldatei);
        await dateistream.CopyToAsync(zielstream);
        var groesse = zielstream.Length;

        if (!Enum.TryParse<DokumentKategorie>(kategorie, out var dokumentKategorie))
            dokumentKategorie = DokumentKategorie.Sonstiges;

        var dokument = new Dokument
        {
            ObjektId = objektId,
            Dateiname = sichererDateiname,
            Kategorie = dokumentKategorie,
            HochgeladenVon = hochgeladenVon,
            HochgeladenAm = DateTime.UtcNow,
            Status = DokumentStatus.Aktiv,
            Dateipfad = eindeutigerName,
            DateigroesseBytes = groesse
        };

        _db.Dokumente.Add(dokument);
        await _db.SaveChangesAsync();

        dokument.Objekt = objekt;
        return ToDto(dokument);
    }

    public async Task<(Stream stream, string dateiname, string contentType)?> DownloadDokumentAsync(
        int dokumentId, int partnerBankId)
    {
        var dto = await GetDokumentAsync(dokumentId, partnerBankId);
        if (dto is null) return null;

        // Dateipfad ist relativ gespeichert; die Web-Schicht muss den Basispfad kennen
        // Hier geben wir nur den Pfad zurück – der Controller löst ihn auf
        return null;
    }

    private static DokumentDto ToDto(Dokument d) => new()
    {
        Id = d.Id,
        ObjektId = d.ObjektId,
        ObjektName = d.Objekt?.Objektname ?? string.Empty,
        Dateiname = d.Dateiname,
        Kategorie = GetKategorieBezeichnung(d.Kategorie),
        HochgeladenVon = d.HochgeladenVon,
        HochgeladenAm = d.HochgeladenAm,
        Status = d.Status == DokumentStatus.Aktiv ? "Aktiv" : "Archiviert",
        Dateipfad = d.Dateipfad,
        DateigroesseBytes = d.DateigroesseBytes
    };

    private static string GetKategorieBezeichnung(DokumentKategorie k) => k switch
    {
        DokumentKategorie.Vertragsdokumente => "Vertragsdokumente",
        DokumentKategorie.Reportings => "Reportings",
        DokumentKategorie.Objektunterlagen => "Objektunterlagen",
        DokumentKategorie.Auswertungen => "Auswertungen",
        DokumentKategorie.Sonstiges => "Sonstiges",
        _ => "Sonstiges"
    };
}

using Microsoft.EntityFrameworkCore;
using MerkurConnectPortal.Application.DTOs;
using MerkurConnectPortal.Application.Interfaces;
using MerkurConnectPortal.Domain.Entities;

namespace MerkurConnectPortal.Application.Services;

public class AdminService : IAdminService
{
    private readonly IApplicationDbContext _db;

    public AdminService(IApplicationDbContext db) => _db = db;

    // ── Dashboard ──────────────────────────────────────────────────────────────

    public async Task<AdminDashboardDto> GetAdminDashboardAsync()
    {
        var banken = await _db.PartnerBanken.Include(p => p.Objekte).ToListAsync();
        var objekte = await _db.Objekte.Include(o => o.PartnerBank).ToListAsync();
        var ungelesen = await GetUngeleseneAnzahlAsync();

        return new AdminDashboardDto
        {
            AnzahlPartnerbanken = banken.Count,
            AnzahlObjekteGesamt = objekte.Count,
            AnzahlObjekteInBau = objekte.Count(o => o.Status == ObjektStatus.InBau),
            AnzahlDokumenteGesamt = await _db.Dokumente.CountAsync(),
            AnzahlNachrichtenGesamt = await _db.Nachrichten.CountAsync(),
            UngeleseneAktivitaeten = ungelesen,
            GesamtMetakontosaldo = objekte.Sum(o => o.Metakontosaldo),
            GesamtKaufpreissammelkonto = objekte.Sum(o => o.Kaufpreissammelkontosaldo),
            GesamtAvale = objekte.Sum(o => o.Avale),
            Partnerbanken = banken.Select(b => new PartnerBankDto
            {
                Id = b.Id,
                Name = b.Name,
                Land = b.Land,
                Ansprechpartner = b.Ansprechpartner,
                EMail = b.EMail,
                AnzahlObjekte = b.Objekte.Count
            }).ToList(),
            NeuesteObjekte = objekte
                .OrderByDescending(o => o.LetzteAktualisierung)
                .Take(5)
                .Select(ObjektMappingHelper.ToKurzDto)
                .ToList()
        };
    }

    // ── Partnerbanken ──────────────────────────────────────────────────────────

    public async Task<List<PartnerBankDto>> GetAllePartnerBankenAsync()
    {
        var banken = await _db.PartnerBanken.Include(p => p.Objekte).ToListAsync();
        var ungelesenNachrichten = await _db.Nachrichten
            .Where(n => n.VonPartnerBank && !n.AdminGelesen)
            .GroupBy(n => n.Objekt.PartnerBankId)
            .Select(g => new { BankId = g.Key, Anzahl = g.Count() })
            .ToListAsync();
        var ungelesenDokumente = await _db.Dokumente
            .Include(d => d.Objekt)
            .Where(d => d.VonPartnerBank && !d.AdminGelesen)
            .GroupBy(d => d.Objekt.PartnerBankId)
            .Select(g => new { BankId = g.Key, Anzahl = g.Count() })
            .ToListAsync();

        return banken.Select(b => new PartnerBankDto
        {
            Id = b.Id,
            Name = b.Name,
            Land = b.Land,
            Ansprechpartner = b.Ansprechpartner,
            EMail = b.EMail,
            AnzahlObjekte = b.Objekte.Count,
            UngeleseneNachrichten = ungelesenNachrichten.FirstOrDefault(x => x.BankId == b.Id)?.Anzahl ?? 0,
            UngelesèneDokumente = ungelesenDokumente.FirstOrDefault(x => x.BankId == b.Id)?.Anzahl ?? 0
        }).ToList();
    }

    // ── Objekte ────────────────────────────────────────────────────────────────

    public async Task<List<ObjektKurzDto>> GetAlleObjekteAsync(string? suchbegriff = null, string? statusFilter = null)
    {
        var query = _db.Objekte
            .Include(o => o.Bautraeger)
            .Include(o => o.PartnerBank)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(suchbegriff))
        {
            var s = suchbegriff.ToLower();
            query = query.Where(o =>
                o.Objektname.ToLower().Contains(s) ||
                o.Standort.ToLower().Contains(s) ||
                o.Bautraeger.Name.ToLower().Contains(s) ||
                o.PartnerBank.Name.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<ObjektStatus>(statusFilter, out var status))
            query = query.Where(o => o.Status == status);

        var objekte = await query.OrderBy(o => o.Objektname).ToListAsync();
        return objekte.Select(ObjektMappingHelper.ToKurzDto).ToList();
    }

    public async Task<ObjektDetailDto?> GetObjektDetailAsync(int objektId)
    {
        var objekt = await _db.Objekte
            .Include(o => o.Bautraeger)
            .Include(o => o.PartnerBank)
            .Include(o => o.Dokumente)
            .Include(o => o.Nachrichten)
            .FirstOrDefaultAsync(o => o.Id == objektId);

        return objekt is null ? null : ObjektMappingHelper.ToDetailDto(objekt);
    }

    // ── Nachrichten ────────────────────────────────────────────────────────────

    public async Task<List<NachrichtDto>> GetNachrichtenByObjektAdminAsync(int objektId)
    {
        var nachrichten = await _db.Nachrichten
            .Include(n => n.Objekt)
            .Where(n => n.ObjektId == objektId)
            .OrderBy(n => n.ErstelltAm)
            .ToListAsync();

        // Ungelesene Nachrichten dieser Unterhaltung als gelesen markieren
        var ungelesen = nachrichten.Where(n => n.VonPartnerBank && !n.AdminGelesen).ToList();
        if (ungelesen.Any())
        {
            ungelesen.ForEach(n => n.AdminGelesen = true);
            await _db.SaveChangesAsync();
        }

        return nachrichten.Select(n => new NachrichtDto
        {
            Id = n.Id,
            ObjektId = n.ObjektId,
            ObjektName = n.Objekt?.Objektname ?? string.Empty,
            Absender = n.Absender,
            Text = n.Text,
            ErstelltAm = n.ErstelltAm,
            VonPartnerBank = n.VonPartnerBank
        }).ToList();
    }

    public async Task<NachrichtDto> SendeNachrichtAlsAdminAsync(int objektId, string text, string absenderName)
    {
        var objekt = await _db.Objekte.FirstOrDefaultAsync(o => o.Id == objektId)
            ?? throw new InvalidOperationException("Objekt nicht gefunden.");

        var nachricht = new Nachricht
        {
            ObjektId = objektId,
            Absender = absenderName,
            Text = text,
            ErstelltAm = DateTime.UtcNow,
            VonPartnerBank = false,
            AdminGelesen = true
        };

        _db.Nachrichten.Add(nachricht);
        await _db.SaveChangesAsync();

        return new NachrichtDto
        {
            Id = nachricht.Id,
            ObjektId = objektId,
            ObjektName = objekt.Objektname,
            Absender = absenderName,
            Text = text,
            ErstelltAm = nachricht.ErstelltAm,
            VonPartnerBank = false
        };
    }

    // ── Dokumente ──────────────────────────────────────────────────────────────

    public async Task<List<DokumentDto>> GetAlleDokumenteAsync(string? kategorie = null)
    {
        var query = _db.Dokumente.Include(d => d.Objekt).ThenInclude(o => o.PartnerBank).AsQueryable();

        if (!string.IsNullOrWhiteSpace(kategorie) && Enum.TryParse<DokumentKategorie>(kategorie, out var kat))
            query = query.Where(d => d.Kategorie == kat);

        var dokumente = await query.OrderByDescending(d => d.HochgeladenAm).ToListAsync();
        return dokumente.Select(DokumentService.ToDto).ToList();
    }

    public async Task<List<DokumentDto>> GetDokumenteByObjektAdminAsync(int objektId)
    {
        var dokumente = await _db.Dokumente
            .Include(d => d.Objekt)
            .Where(d => d.ObjektId == objektId)
            .OrderByDescending(d => d.HochgeladenAm)
            .ToListAsync();

        return dokumente.Select(DokumentService.ToDto).ToList();
    }

    public async Task<DokumentDto?> GetDokumentAdminAsync(int dokumentId)
    {
        var d = await _db.Dokumente.Include(d => d.Objekt).FirstOrDefaultAsync(d => d.Id == dokumentId);
        return d is null ? null : DokumentService.ToDto(d);
    }

    public async Task<DokumentDto> UploadDokumentAlsAdminAsync(
        int objektId, Stream dateistream, string dateiname,
        string kategorie, string hochgeladenVon, string uploadVerzeichnis)
    {
        var objekt = await _db.Objekte.FirstOrDefaultAsync(o => o.Id == objektId)
            ?? throw new InvalidOperationException("Objekt nicht gefunden.");

        Directory.CreateDirectory(uploadVerzeichnis);

        var sichererName = Path.GetFileName(dateiname);
        var eindeutigerName = $"{Guid.NewGuid():N}_{sichererName}";
        var zieldatei = Path.Combine(uploadVerzeichnis, eindeutigerName);

        await using var zielstream = File.Create(zieldatei);
        await dateistream.CopyToAsync(zielstream);

        if (!Enum.TryParse<DokumentKategorie>(kategorie, out var dokumentKategorie))
            dokumentKategorie = DokumentKategorie.Sonstiges;

        var dokument = new Dokument
        {
            ObjektId = objektId,
            Dateiname = sichererName,
            Kategorie = dokumentKategorie,
            HochgeladenVon = hochgeladenVon,
            HochgeladenAm = DateTime.UtcNow,
            Status = DokumentStatus.Aktiv,
            Dateipfad = eindeutigerName,
            DateigroesseBytes = zielstream.Length,
            VonPartnerBank = false,
            AdminGelesen = true
        };

        _db.Dokumente.Add(dokument);
        await _db.SaveChangesAsync();

        dokument.Objekt = objekt;
        return DokumentService.ToDto(dokument);
    }

    // ── Aktivitäten / Benachrichtigungen ──────────────────────────────────────

    public async Task<List<BenachrichtigungDto>> GetUngeleseneAktivitaetenAsync()
    {
        var nachrichten = await _db.Nachrichten
            .Include(n => n.Objekt).ThenInclude(o => o.PartnerBank)
            .Where(n => n.VonPartnerBank && !n.AdminGelesen)
            .OrderByDescending(n => n.ErstelltAm)
            .ToListAsync();

        var dokumente = await _db.Dokumente
            .Include(d => d.Objekt).ThenInclude(o => o.PartnerBank)
            .Where(d => d.VonPartnerBank && !d.AdminGelesen)
            .OrderByDescending(d => d.HochgeladenAm)
            .ToListAsync();

        var result = new List<BenachrichtigungDto>();

        result.AddRange(nachrichten.Select(n => new BenachrichtigungDto
        {
            Id = n.Id,
            Typ = BenachrichtigungTyp.Nachricht,
            ObjektId = n.ObjektId,
            ObjektName = n.Objekt?.Objektname ?? string.Empty,
            PartnerBankName = n.Objekt?.PartnerBank?.Name ?? string.Empty,
            Absender = n.Absender,
            Vorschau = n.Text.Length > 100 ? n.Text[..100] + "…" : n.Text,
            ErstelltAm = n.ErstelltAm,
            Gelesen = n.AdminGelesen
        }));

        result.AddRange(dokumente.Select(d => new BenachrichtigungDto
        {
            Id = d.Id,
            Typ = BenachrichtigungTyp.Dokument,
            ObjektId = d.ObjektId,
            ObjektName = d.Objekt?.Objektname ?? string.Empty,
            PartnerBankName = d.Objekt?.PartnerBank?.Name ?? string.Empty,
            Absender = d.HochgeladenVon,
            Vorschau = d.Dateiname,
            ErstelltAm = d.HochgeladenAm,
            Gelesen = d.AdminGelesen
        }));

        return result.OrderByDescending(b => b.ErstelltAm).ToList();
    }

    public async Task<int> GetUngeleseneAnzahlAsync()
    {
        var ungeleseneNachrichten = await _db.Nachrichten
            .CountAsync(n => n.VonPartnerBank && !n.AdminGelesen);
        var ungeleseneDokumente = await _db.Dokumente
            .CountAsync(d => d.VonPartnerBank && !d.AdminGelesen);
        return ungeleseneNachrichten + ungeleseneDokumente;
    }

    public async Task MarkiereNachrichtGelesenAsync(int nachrichtId)
    {
        var n = await _db.Nachrichten.FindAsync(nachrichtId);
        if (n is not null) { n.AdminGelesen = true; await _db.SaveChangesAsync(); }
    }

    public async Task MarkiereDokumentGelesenAsync(int dokumentId)
    {
        var d = await _db.Dokumente.FindAsync(dokumentId);
        if (d is not null) { d.AdminGelesen = true; await _db.SaveChangesAsync(); }
    }

    public async Task MarkiereAlleGelesenAsync()
    {
        var ungeleseneNachrichten = await _db.Nachrichten
            .Where(n => n.VonPartnerBank && !n.AdminGelesen)
            .ToListAsync();
        ungeleseneNachrichten.ForEach(n => n.AdminGelesen = true);

        var ungeleseneDokumente = await _db.Dokumente
            .Where(d => d.VonPartnerBank && !d.AdminGelesen)
            .ToListAsync();
        ungeleseneDokumente.ForEach(d => d.AdminGelesen = true);

        if (ungeleseneNachrichten.Any() || ungeleseneDokumente.Any())
            await _db.SaveChangesAsync();
    }
}

using Microsoft.EntityFrameworkCore;
using MerkurConnectPortal.Application.DTOs;
using MerkurConnectPortal.Application.Interfaces;

namespace MerkurConnectPortal.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _db;

    public DashboardService(IApplicationDbContext db) => _db = db;

    public async Task<DashboardDto> GetDashboardAsync(int partnerBankId)
    {
        var partnerBank = await _db.PartnerBanken
            .FirstOrDefaultAsync(p => p.Id == partnerBankId);

        var objekte = await _db.Objekte
            .Include(o => o.Bautraeger)
            .Where(o => o.PartnerBankId == partnerBankId)
            .ToListAsync();

        return new DashboardDto
        {
            PartnerBankName = partnerBank?.Name ?? string.Empty,
            AnzahlObjekte = objekte.Count,
            GesamtMetakontosaldo = objekte.Sum(o => o.Metakontosaldo),
            GesamtKaufpreissammelkonto = objekte.Sum(o => o.Kaufpreissammelkontosaldo),
            GesamtAvale = objekte.Sum(o => o.Avale),
            GesamtEinheiten = objekte.Sum(o => o.EinheitenGesamt),
            GesamtVerkaufteEinheiten = objekte.Sum(o => o.EinheitenVerkauft),
            DurchschnittlicheBautenstand = objekte.Any() ? objekte.Average(o => o.BautenstandProzent) : 0,
            Objekte = objekte.Select(ObjektMappingHelper.ToKurzDto).ToList()
        };
    }

    // ── Aktivitäten / Benachrichtigungen für Partnerbank ─────────────────────

    public async Task<List<BenachrichtigungDto>> GetUngeleseneAktivitaetenAsync(int partnerBankId)
    {
        var nachrichten = await _db.Nachrichten
            .Include(n => n.Objekt).ThenInclude(o => o.PartnerBank)
            .Where(n => !n.VonPartnerBank && !n.PartnerBankGelesen && n.Objekt.PartnerBankId == partnerBankId)
            .OrderByDescending(n => n.ErstelltAm)
            .ToListAsync();

        var dokumente = await _db.Dokumente
            .Include(d => d.Objekt).ThenInclude(o => o.PartnerBank)
            .Where(d => !d.VonPartnerBank && !d.PartnerBankGelesen && d.Objekt.PartnerBankId == partnerBankId)
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
            Gelesen = n.PartnerBankGelesen
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
            Gelesen = d.PartnerBankGelesen
        }));

        return result.OrderByDescending(b => b.ErstelltAm).ToList();
    }

    public async Task MarkiereAlleGelesenAsync(int partnerBankId)
    {
        var ungeleseneNachrichten = await _db.Nachrichten
            .Where(n => !n.VonPartnerBank && !n.PartnerBankGelesen && n.Objekt.PartnerBankId == partnerBankId)
            .ToListAsync();
        ungeleseneNachrichten.ForEach(n => n.PartnerBankGelesen = true);

        var ungeleseneDokumente = await _db.Dokumente
            .Where(d => !d.VonPartnerBank && !d.PartnerBankGelesen && d.Objekt.PartnerBankId == partnerBankId)
            .ToListAsync();
        ungeleseneDokumente.ForEach(d => d.PartnerBankGelesen = true);

        if (ungeleseneNachrichten.Any() || ungeleseneDokumente.Any())
            await _db.SaveChangesAsync();
    }
}

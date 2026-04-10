using Microsoft.EntityFrameworkCore;
using MerkurConnectPortal.Application.DTOs;
using MerkurConnectPortal.Application.Interfaces;
using MerkurConnectPortal.Domain.Entities;

namespace MerkurConnectPortal.Application.Services;

public class ObjektService : IObjektService
{
    private readonly IApplicationDbContext _db;

    public ObjektService(IApplicationDbContext db) => _db = db;

    public async Task<List<ObjektKurzDto>> GetObjekteByPartnerBankAsync(
        int partnerBankId,
        string? suchbegriff = null,
        string? statusFilter = null,
        string? sortierung = null)
    {
        var query = _db.Objekte
            .Include(o => o.Bautraeger)
            .Where(o => o.PartnerBankId == partnerBankId);

        if (!string.IsNullOrWhiteSpace(suchbegriff))
        {
            var s = suchbegriff.ToLower();
            query = query.Where(o =>
                o.Objektname.ToLower().Contains(s) ||
                o.Standort.ToLower().Contains(s) ||
                o.Bautraeger.Name.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<ObjektStatus>(statusFilter, out var status))
        {
            query = query.Where(o => o.Status == status);
        }

        query = sortierung switch
        {
            "standort" => query.OrderBy(o => o.Standort),
            "bautenstand" => query.OrderByDescending(o => o.BautenstandProzent),
            "quote" => query.OrderByDescending(o => o.Unterbeteiligungsquote),
            _ => query.OrderBy(o => o.Objektname)
        };

        var objekte = await query.ToListAsync();
        return objekte.Select(ObjektMappingHelper.ToKurzDto).ToList();
    }

    public async Task<ObjektDetailDto?> GetObjektDetailAsync(int objektId, int partnerBankId)
    {
        var o = await _db.Objekte
            .Include(o => o.Bautraeger)
            .Include(o => o.Dokumente)
            .Include(o => o.Nachrichten)
            .FirstOrDefaultAsync(o => o.Id == objektId && o.PartnerBankId == partnerBankId);

        if (o is null) return null;

        return new ObjektDetailDto
        {
            Id = o.Id,
            Objektname = o.Objektname,
            Standort = o.Standort,
            Bautraeger = o.Bautraeger?.Name ?? string.Empty,
            Status = ObjektMappingHelper.GetStatusBezeichnung(o.Status),
            StatusCssClass = ObjektMappingHelper.GetStatusCssClass(o.Status),
            Unterbeteiligungsquote = o.Unterbeteiligungsquote,
            Metakontosaldo = o.Metakontosaldo,
            Kaufpreissammelkontosaldo = o.Kaufpreissammelkontosaldo,
            Avale = o.Avale,
            EinheitenGesamt = o.EinheitenGesamt,
            EinheitenVerkauft = o.EinheitenVerkauft,
            Verkaufsquote = o.Verkaufsquote,
            BautenstandProzent = o.BautenstandProzent,
            LetzteAktualisierung = o.LetzteAktualisierung,
            AnzahlDokumente = o.Dokumente.Count,
            AnzahlNachrichten = o.Nachrichten.Count
        };
    }
}

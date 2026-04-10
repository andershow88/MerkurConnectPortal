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
}

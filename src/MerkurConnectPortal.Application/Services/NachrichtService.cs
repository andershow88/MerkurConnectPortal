using Microsoft.EntityFrameworkCore;
using MerkurConnectPortal.Application.DTOs;
using MerkurConnectPortal.Application.Interfaces;
using MerkurConnectPortal.Domain.Entities;

namespace MerkurConnectPortal.Application.Services;

public class NachrichtService : INachrichtService
{
    private readonly IApplicationDbContext _db;

    public NachrichtService(IApplicationDbContext db) => _db = db;

    public async Task<List<NachrichtDto>> GetNachrichtenByObjektAsync(int objektId, int partnerBankId)
    {
        var nachrichten = await _db.Nachrichten
            .Include(n => n.Objekt)
            .Where(n => n.ObjektId == objektId && n.Objekt.PartnerBankId == partnerBankId)
            .OrderBy(n => n.ErstelltAm)
            .ToListAsync();

        return nachrichten.Select(ToDto).ToList();
    }

    public async Task<NachrichtDto> SendeNachrichtAsync(
        int objektId, int partnerBankId, string absender, string text, bool vonPartnerBank = true)
    {
        var objekt = await _db.Objekte
            .FirstOrDefaultAsync(o => o.Id == objektId && o.PartnerBankId == partnerBankId)
            ?? throw new InvalidOperationException("Objekt nicht gefunden oder kein Zugriff.");

        var nachricht = new Nachricht
        {
            ObjektId = objektId,
            Absender = absender,
            Text = text,
            ErstelltAm = DateTime.UtcNow,
            VonPartnerBank = vonPartnerBank,
            AdminGelesen = !vonPartnerBank // Admin-eigene Nachrichten gelten als bereits gelesen
        };

        _db.Nachrichten.Add(nachricht);
        await _db.SaveChangesAsync();

        return new NachrichtDto
        {
            Id = nachricht.Id,
            ObjektId = objektId,
            ObjektName = objekt.Objektname,
            Absender = absender,
            Text = text,
            ErstelltAm = nachricht.ErstelltAm,
            VonPartnerBank = nachricht.VonPartnerBank
        };
    }

    private static NachrichtDto ToDto(Nachricht n) => new()
    {
        Id = n.Id,
        ObjektId = n.ObjektId,
        ObjektName = n.Objekt?.Objektname ?? string.Empty,
        Absender = n.Absender,
        Text = n.Text,
        ErstelltAm = n.ErstelltAm,
        VonPartnerBank = n.VonPartnerBank
    };
}

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

        // Ungelesene Merkur-Nachrichten dieser Unterhaltung als gelesen markieren
        var ungelesen = nachrichten.Where(n => !n.VonPartnerBank && !n.PartnerBankGelesen).ToList();
        if (ungelesen.Any())
        {
            ungelesen.ForEach(n => n.PartnerBankGelesen = true);
            await _db.SaveChangesAsync();
        }

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
            // Wenn Partnerbank sendet: Partnerbank hat selbst gelesen, Admin noch nicht
            // Wenn Admin sendet: Admin hat gelesen, Partnerbank noch nicht
            AdminGelesen = !vonPartnerBank,
            PartnerBankGelesen = vonPartnerBank
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

    public async Task<int> GetUngeleseneAnzahlForPartnerBankAsync(int partnerBankId)
    {
        return await _db.Nachrichten
            .Where(n => !n.VonPartnerBank
                     && !n.PartnerBankGelesen
                     && n.Objekt.PartnerBankId == partnerBankId)
            .CountAsync();
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

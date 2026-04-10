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

        return nachrichten.Select(n => new NachrichtDto
        {
            Id = n.Id,
            ObjektId = n.ObjektId,
            ObjektName = n.Objekt?.Objektname ?? string.Empty,
            Absender = n.Absender,
            Text = n.Text,
            ErstelltAm = n.ErstelltAm
        }).ToList();
    }

    public async Task<NachrichtDto> SendeNachrichtAsync(int objektId, int partnerBankId, string absender, string text)
    {
        var objekt = await _db.Objekte
            .FirstOrDefaultAsync(o => o.Id == objektId && o.PartnerBankId == partnerBankId)
            ?? throw new InvalidOperationException("Objekt nicht gefunden oder kein Zugriff.");

        var nachricht = new Nachricht
        {
            ObjektId = objektId,
            Absender = absender,
            Text = text,
            ErstelltAm = DateTime.UtcNow
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
            ErstelltAm = nachricht.ErstelltAm
        };
    }
}

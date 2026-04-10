using Microsoft.EntityFrameworkCore;
using MerkurConnectPortal.Domain.Entities;

namespace MerkurConnectPortal.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<PartnerBank> PartnerBanken { get; }
    DbSet<Bautraeger> Bautraeger { get; }
    DbSet<Objekt> Objekte { get; }
    DbSet<Dokument> Dokumente { get; }
    DbSet<Nachricht> Nachrichten { get; }
    DbSet<Benutzer> Benutzer { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

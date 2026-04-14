using Microsoft.EntityFrameworkCore;
using MerkurConnectPortal.Application.Interfaces;
using MerkurConnectPortal.Domain.Entities;

namespace MerkurConnectPortal.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<PartnerBank> PartnerBanken => Set<PartnerBank>();
    public DbSet<Bautraeger> Bautraeger => Set<Bautraeger>();
    public DbSet<Objekt> Objekte => Set<Objekt>();
    public DbSet<Dokument> Dokumente => Set<Dokument>();
    public DbSet<Nachricht> Nachrichten => Set<Nachricht>();
    public DbSet<Benutzer> Benutzer => Set<Benutzer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PartnerBank>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).IsRequired().HasMaxLength(200);
            e.Property(p => p.Land).HasMaxLength(100);
            e.Property(p => p.Ansprechpartner).HasMaxLength(200);
            e.Property(p => p.EMail).HasMaxLength(200);
        });

        modelBuilder.Entity<Bautraeger>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.Name).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Objekt>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Objektname).IsRequired().HasMaxLength(300);
            e.Property(o => o.Standort).HasMaxLength(300);
            e.Property(o => o.Unterbeteiligungsquote).HasPrecision(18, 4);
            e.Property(o => o.Metakontosaldo).HasPrecision(18, 2);
            e.Property(o => o.Kaufpreissammelkontosaldo).HasPrecision(18, 2);
            e.Property(o => o.Avale).HasPrecision(18, 2);
            e.Property(o => o.Verkaufsquote).HasPrecision(18, 4);
            e.Property(o => o.BautenstandProzent).HasPrecision(18, 2);
            e.HasOne(o => o.Bautraeger).WithMany(b => b.Objekte).HasForeignKey(o => o.BautraegerId);
            e.HasOne(o => o.PartnerBank).WithMany(p => p.Objekte).HasForeignKey(o => o.PartnerBankId);
        });

        modelBuilder.Entity<Dokument>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.Dateiname).IsRequired().HasMaxLength(500);
            e.Property(d => d.HochgeladenVon).HasMaxLength(200);
            e.Property(d => d.Dateipfad).HasMaxLength(1000);
            e.Property(d => d.VonPartnerBank).HasDefaultValue(false);
            e.Property(d => d.AdminGelesen).HasDefaultValue(false);
            e.Property(d => d.PartnerBankGelesen).HasDefaultValue(false);
            e.HasOne(d => d.Objekt).WithMany(o => o.Dokumente).HasForeignKey(d => d.ObjektId);
        });

        modelBuilder.Entity<Nachricht>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Absender).HasMaxLength(200);
            e.Property(n => n.Text).HasMaxLength(4000);
            e.Property(n => n.VonPartnerBank).HasDefaultValue(false);
            e.Property(n => n.AdminGelesen).HasDefaultValue(false);
            e.Property(n => n.PartnerBankGelesen).HasDefaultValue(false);
            e.HasOne(n => n.Objekt).WithMany(o => o.Nachrichten).HasForeignKey(n => n.ObjektId);
        });

        modelBuilder.Entity<Benutzer>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.Benutzername).IsRequired().HasMaxLength(100);
            e.HasIndex(b => b.Benutzername).IsUnique();
            e.Property(b => b.PasswortHash).IsRequired().HasMaxLength(100);
            e.Property(b => b.Anzeigename).HasMaxLength(200);
            e.Property(b => b.EMail).HasMaxLength(200);
            e.Property(b => b.IsAdmin).HasDefaultValue(false);
            // PartnerBankId ist nullable (Admin-Benutzer haben keine Partnerbank)
            e.HasOne(b => b.PartnerBank)
             .WithMany(p => p.Benutzer)
             .HasForeignKey(b => b.PartnerBankId)
             .IsRequired(false);
        });
    }
}

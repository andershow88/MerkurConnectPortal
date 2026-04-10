namespace MerkurConnectPortal.Domain.Entities;

public class Objekt
{
    public int Id { get; set; }
    public string Objektname { get; set; } = string.Empty;
    public string Standort { get; set; } = string.Empty;
    public int BautraegerId { get; set; }
    public Bautraeger Bautraeger { get; set; } = null!;
    public ObjektStatus Status { get; set; }

    // Finanzdaten
    public decimal Unterbeteiligungsquote { get; set; }
    public decimal Metakontosaldo { get; set; }
    public decimal Kaufpreissammelkontosaldo { get; set; }
    public decimal Avale { get; set; }

    // Vermarktung
    public int EinheitenGesamt { get; set; }
    public int EinheitenVerkauft { get; set; }
    public decimal Verkaufsquote { get; set; }

    // Baufortschritt
    public decimal BautenstandProzent { get; set; }

    public DateTime LetzteAktualisierung { get; set; }

    // Mandantenzuordnung
    public int PartnerBankId { get; set; }
    public PartnerBank PartnerBank { get; set; } = null!;

    public ICollection<Dokument> Dokumente { get; set; } = new List<Dokument>();
    public ICollection<Nachricht> Nachrichten { get; set; } = new List<Nachricht>();
}

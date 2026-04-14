namespace MerkurConnectPortal.Domain.Entities;

public class Nachricht
{
    public int Id { get; set; }
    public int ObjektId { get; set; }
    public Objekt Objekt { get; set; } = null!;
    public string Absender { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime ErstelltAm { get; set; }

    /// <summary>true = wurde von einer Partnerbank gesendet (für Admin-Benachrichtigung).</summary>
    public bool VonPartnerBank { get; set; }

    /// <summary>true = Admin hat diese Nachricht bereits gesehen.</summary>
    public bool AdminGelesen { get; set; }

    /// <summary>true = Partnerbank hat diese Nachricht bereits gesehen.</summary>
    public bool PartnerBankGelesen { get; set; }
}

namespace MerkurConnectPortal.Domain.Entities;

public class Benutzer
{
    public int Id { get; set; }
    public string Benutzername { get; set; } = string.Empty;
    public string PasswortHash { get; set; } = string.Empty;
    public string Anzeigename { get; set; } = string.Empty;
    public string EMail { get; set; } = string.Empty;

    /// <summary>Null bei Admin-Benutzern (keine Partnerbank-Zuordnung).</summary>
    public int? PartnerBankId { get; set; }
    public PartnerBank? PartnerBank { get; set; }

    public bool IsAdmin { get; set; }
}

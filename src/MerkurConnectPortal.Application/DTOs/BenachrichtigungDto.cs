namespace MerkurConnectPortal.Application.DTOs;

public enum BenachrichtigungTyp { Nachricht, Dokument }

public class BenachrichtigungDto
{
    public int Id { get; set; }
    public BenachrichtigungTyp Typ { get; set; }
    public int ObjektId { get; set; }
    public string ObjektName { get; set; } = string.Empty;
    public string PartnerBankName { get; set; } = string.Empty;
    public string Absender { get; set; } = string.Empty;
    public string Vorschau { get; set; } = string.Empty;
    public DateTime ErstelltAm { get; set; }
    public bool Gelesen { get; set; }

    public string TypBezeichnung => Typ == BenachrichtigungTyp.Nachricht ? "Nachricht" : "Dokument";
    public string TypIcon => Typ == BenachrichtigungTyp.Nachricht ? "bi-chat-dots" : "bi-file-earmark-arrow-up";
}

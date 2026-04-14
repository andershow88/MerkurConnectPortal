namespace MerkurConnectPortal.Application.DTOs;

public class PartnerBankDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Land { get; set; } = string.Empty;
    public string Ansprechpartner { get; set; } = string.Empty;
    public string EMail { get; set; } = string.Empty;
    public int AnzahlObjekte { get; set; }
    public int UngeleseneNachrichten { get; set; }
    public int UngelesèneDokumente { get; set; }
}

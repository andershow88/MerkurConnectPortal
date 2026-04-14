namespace MerkurConnectPortal.Application.DTOs;

public class NachrichtDto
{
    public int Id { get; set; }
    public int ObjektId { get; set; }
    public string ObjektName { get; set; } = string.Empty;
    public string Absender { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime ErstelltAm { get; set; }
    public bool VonPartnerBank { get; set; }
}

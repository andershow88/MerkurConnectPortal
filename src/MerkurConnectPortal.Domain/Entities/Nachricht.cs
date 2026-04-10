namespace MerkurConnectPortal.Domain.Entities;

public class Nachricht
{
    public int Id { get; set; }
    public int ObjektId { get; set; }
    public Objekt Objekt { get; set; } = null!;
    public string Absender { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime ErstelltAm { get; set; }
}

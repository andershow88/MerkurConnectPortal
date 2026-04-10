namespace MerkurConnectPortal.Domain.Entities;

public class PartnerBank
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Land { get; set; } = string.Empty;
    public string Ansprechpartner { get; set; } = string.Empty;
    public string EMail { get; set; } = string.Empty;

    public ICollection<Objekt> Objekte { get; set; } = new List<Objekt>();
    public ICollection<Benutzer> Benutzer { get; set; } = new List<Benutzer>();
}

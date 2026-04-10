namespace MerkurConnectPortal.Domain.Entities;

public class Bautraeger
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Objekt> Objekte { get; set; } = new List<Objekt>();
}

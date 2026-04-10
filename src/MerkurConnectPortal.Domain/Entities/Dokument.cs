namespace MerkurConnectPortal.Domain.Entities;

public class Dokument
{
    public int Id { get; set; }
    public int ObjektId { get; set; }
    public Objekt Objekt { get; set; } = null!;
    public string Dateiname { get; set; } = string.Empty;
    public DokumentKategorie Kategorie { get; set; }
    public string HochgeladenVon { get; set; } = string.Empty;
    public DateTime HochgeladenAm { get; set; }
    public DokumentStatus Status { get; set; }
    public string Dateipfad { get; set; } = string.Empty;
    public long DateigroesseBytes { get; set; }
}

namespace MerkurConnectPortal.Application.DTOs;

public class DokumentDto
{
    public int Id { get; set; }
    public int ObjektId { get; set; }
    public string ObjektName { get; set; } = string.Empty;
    public string Dateiname { get; set; } = string.Empty;
    public string Kategorie { get; set; } = string.Empty;
    public string HochgeladenVon { get; set; } = string.Empty;
    public DateTime HochgeladenAm { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Dateipfad { get; set; } = string.Empty;
    public long DateigroesseBytes { get; set; }
    public bool VonPartnerBank { get; set; }

    public string DateigroesseFormatiert =>
        DateigroesseBytes < 1024 ? $"{DateigroesseBytes} B"
        : DateigroesseBytes < 1024 * 1024 ? $"{DateigroesseBytes / 1024:F1} KB"
        : $"{DateigroesseBytes / (1024.0 * 1024.0):F1} MB";
}

using MerkurConnectPortal.Application.DTOs;

namespace MerkurConnectPortal.Web.ViewModels;

public class DokumentListViewModel
{
    public int? ObjektId { get; set; }
    public string? ObjektName { get; set; }
    public List<DokumentDto> Dokumente { get; set; } = new();
    public string? KategorieFilter { get; set; }

    public List<(string Wert, string Bezeichnung)> KategorieOptionen { get; } = new()
    {
        ("", "Alle Kategorien"),
        ("Vertragsdokumente", "Vertragsdokumente"),
        ("Reportings", "Reportings"),
        ("Objektunterlagen", "Objektunterlagen"),
        ("Auswertungen", "Auswertungen"),
        ("Sonstiges", "Sonstiges")
    };
}

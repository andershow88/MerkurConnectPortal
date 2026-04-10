using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MerkurConnectPortal.Web.ViewModels;

public class DokumentUploadViewModel
{
    public int ObjektId { get; set; }
    public string ObjektName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte wählen Sie eine Datei aus")]
    [Display(Name = "Datei")]
    public IFormFile? Datei { get; set; }

    [Required(ErrorMessage = "Bitte wählen Sie eine Kategorie aus")]
    [Display(Name = "Kategorie")]
    public string Kategorie { get; set; } = "Sonstiges";

    public List<(string Wert, string Bezeichnung)> KategorieOptionen { get; } = new()
    {
        ("Vertragsdokumente", "Vertragsdokumente"),
        ("Reportings", "Reportings"),
        ("Objektunterlagen", "Objektunterlagen"),
        ("Auswertungen", "Auswertungen"),
        ("Sonstiges", "Sonstiges")
    };
}

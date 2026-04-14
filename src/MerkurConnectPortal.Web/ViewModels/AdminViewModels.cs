using MerkurConnectPortal.Application.DTOs;

namespace MerkurConnectPortal.Web.ViewModels;

public class AdminDashboardViewModel
{
    public AdminDashboardDto Dashboard { get; set; } = new();
}

public class AdminObjekteViewModel
{
    public List<ObjektKurzDto> Objekte { get; set; } = new();
    public string? Suchbegriff { get; set; }
    public string? StatusFilter { get; set; }

    public List<(string Value, string Text)> StatusOptionen { get; set; } = new()
    {
        ("", "Alle Status"),
        ("InPlanung", "In Planung"),
        ("InBau", "In Bau"),
        ("Fertiggestellt", "Fertiggestellt"),
        ("Abgeschlossen", "Abgeschlossen")
    };
}

public class AdminNachrichtenViewModel
{
    public int ObjektId { get; set; }
    public string ObjektName { get; set; } = string.Empty;
    public string PartnerBankName { get; set; } = string.Empty;
    public List<NachrichtDto> Nachrichten { get; set; } = new();
    public string NeueNachricht { get; set; } = string.Empty;
}

public class AdminDokumenteViewModel
{
    public List<DokumentDto> Dokumente { get; set; } = new();
    public int? ObjektId { get; set; }
    public string? ObjektName { get; set; }
    public string? KategorieFilter { get; set; }

    public List<(string Value, string Text)> KategorieOptionen { get; set; } = new()
    {
        ("", "Alle Kategorien"),
        ("Vertragsdokumente", "Vertragsdokumente"),
        ("Reportings", "Reportings"),
        ("Objektunterlagen", "Objektunterlagen"),
        ("Auswertungen", "Auswertungen"),
        ("Sonstiges", "Sonstiges")
    };
}

public class AdminDokumentUploadViewModel
{
    public int ObjektId { get; set; }
    public string ObjektName { get; set; } = string.Empty;
    public IFormFile? Datei { get; set; }
    public string Kategorie { get; set; } = "Sonstiges";

    public List<(string Value, string Text)> KategorieOptionen { get; set; } = new()
    {
        ("Vertragsdokumente", "Vertragsdokumente"),
        ("Reportings", "Reportings"),
        ("Objektunterlagen", "Objektunterlagen"),
        ("Auswertungen", "Auswertungen"),
        ("Sonstiges", "Sonstiges")
    };
}

public class AdminAktivitaetenViewModel
{
    public List<BenachrichtigungDto> Aktivitaeten { get; set; } = new();
    public int UngeleseneAnzahl { get; set; }
}

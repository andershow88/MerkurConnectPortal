using MerkurConnectPortal.Application.DTOs;

namespace MerkurConnectPortal.Web.ViewModels;

public class ObjektListViewModel
{
    public List<ObjektKurzDto> Objekte { get; set; } = new();
    public string? Suchbegriff { get; set; }
    public string? StatusFilter { get; set; }
    public string? Sortierung { get; set; }

    public List<(string Wert, string Bezeichnung)> StatusOptionen { get; } = new()
    {
        ("", "Alle Status"),
        ("InPlanung", "In Planung"),
        ("InBau", "In Bau"),
        ("Fertiggestellt", "Fertiggestellt"),
        ("Abgeschlossen", "Abgeschlossen")
    };

    public List<(string Wert, string Bezeichnung)> SortierOptionen { get; } = new()
    {
        ("", "Objektname"),
        ("standort", "Standort"),
        ("quote", "Beteiligungsquote"),
        ("bautenstand", "Bautenstand")
    };
}

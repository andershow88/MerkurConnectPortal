namespace MerkurConnectPortal.Application.DTOs;

public class ObjektKurzDto
{
    public int Id { get; set; }
    public string Objektname { get; set; } = string.Empty;
    public string Standort { get; set; } = string.Empty;
    public string Bautraeger { get; set; } = string.Empty;
    public string BautraegerName { get; set; } = string.Empty;
    public string PartnerBankName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public string StatusCssClass { get; set; } = string.Empty;
    public string StatusBadgeClass { get; set; } = string.Empty;
    public decimal Unterbeteiligungsquote { get; set; }
    public decimal Metakontosaldo { get; set; }
    public decimal Kaufpreissammelkontosaldo { get; set; }
    public decimal Avale { get; set; }
    public int EinheitenGesamt { get; set; }
    public int EinheitenVerkauft { get; set; }
    public decimal Verkaufsquote { get; set; }
    public decimal BautenstandProzent { get; set; }
    public DateTime LetzteAktualisierung { get; set; }
}

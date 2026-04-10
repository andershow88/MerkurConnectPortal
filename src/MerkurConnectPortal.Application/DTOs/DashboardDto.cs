namespace MerkurConnectPortal.Application.DTOs;

public class DashboardDto
{
    public string PartnerBankName { get; set; } = string.Empty;
    public int AnzahlObjekte { get; set; }
    public decimal GesamtMetakontosaldo { get; set; }
    public decimal GesamtKaufpreissammelkonto { get; set; }
    public decimal GesamtAvale { get; set; }
    public int GesamtEinheiten { get; set; }
    public int GesamtVerkaufteEinheiten { get; set; }
    public decimal DurchschnittlicheBautenstand { get; set; }
    public List<ObjektKurzDto> Objekte { get; set; } = new();
}

namespace MerkurConnectPortal.Application.DTOs;

public class AdminDashboardDto
{
    public int AnzahlPartnerbanken { get; set; }
    public int AnzahlObjekteGesamt { get; set; }
    public int AnzahlObjekteInBau { get; set; }
    public int AnzahlDokumenteGesamt { get; set; }
    public int AnzahlNachrichtenGesamt { get; set; }
    public int UngeleseneAktivitaeten { get; set; }

    public decimal GesamtMetakontosaldo { get; set; }
    public decimal GesamtKaufpreissammelkonto { get; set; }
    public decimal GesamtAvale { get; set; }

    public List<PartnerBankDto> Partnerbanken { get; set; } = new();
    public List<ObjektKurzDto> NeuesteObjekte { get; set; } = new();
}

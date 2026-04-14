using MerkurConnectPortal.Application.DTOs;

namespace MerkurConnectPortal.Web.ViewModels;

public class AktivitaetenViewModel
{
    public List<BenachrichtigungDto> Aktivitaeten { get; set; } = new();
    public int UngeleseneAnzahl { get; set; }
}

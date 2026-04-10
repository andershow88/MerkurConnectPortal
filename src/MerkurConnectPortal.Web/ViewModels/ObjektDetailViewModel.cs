using MerkurConnectPortal.Application.DTOs;

namespace MerkurConnectPortal.Web.ViewModels;

public class ObjektDetailViewModel
{
    public ObjektDetailDto Objekt { get; set; } = null!;
    public List<DokumentDto> LetzteDokumente { get; set; } = new();
    public List<NachrichtDto> LetzteNachrichten { get; set; } = new();
}

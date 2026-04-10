using System.ComponentModel.DataAnnotations;
using MerkurConnectPortal.Application.DTOs;

namespace MerkurConnectPortal.Web.ViewModels;

public class NachrichtViewModel
{
    public int ObjektId { get; set; }
    public string ObjektName { get; set; } = string.Empty;
    public List<NachrichtDto> Nachrichten { get; set; } = new();

    [Required(ErrorMessage = "Bitte geben Sie eine Nachricht ein")]
    [StringLength(4000, ErrorMessage = "Nachricht darf maximal 4000 Zeichen lang sein")]
    [Display(Name = "Ihre Nachricht")]
    public string NeueNachricht { get; set; } = string.Empty;
}

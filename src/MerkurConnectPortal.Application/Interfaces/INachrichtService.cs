using MerkurConnectPortal.Application.DTOs;

namespace MerkurConnectPortal.Application.Interfaces;

public interface INachrichtService
{
    Task<List<NachrichtDto>> GetNachrichtenByObjektAsync(int objektId, int partnerBankId);
    Task<NachrichtDto> SendeNachrichtAsync(int objektId, int partnerBankId, string absender, string text);
}

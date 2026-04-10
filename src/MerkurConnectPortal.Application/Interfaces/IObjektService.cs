using MerkurConnectPortal.Application.DTOs;

namespace MerkurConnectPortal.Application.Interfaces;

public interface IObjektService
{
    Task<List<ObjektKurzDto>> GetObjekteByPartnerBankAsync(
        int partnerBankId,
        string? suchbegriff = null,
        string? statusFilter = null,
        string? sortierung = null);

    Task<ObjektDetailDto?> GetObjektDetailAsync(int objektId, int partnerBankId);
}

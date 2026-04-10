using MerkurConnectPortal.Application.DTOs;

namespace MerkurConnectPortal.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(int partnerBankId);
}

namespace MerkurConnectPortal.Application.Interfaces;

public record LoginResult(
    bool Success,
    int PartnerBankId,
    bool IsAdmin,
    string Anzeigename,
    string Benutzername,
    string PartnerBankName);

public interface IAuthService
{
    Task<LoginResult> ValidateLoginAsync(string benutzername, string passwort);
}

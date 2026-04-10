using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MerkurConnectPortal.Application.Interfaces;

namespace MerkurConnectPortal.Application.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _db;

    public AuthService(IApplicationDbContext db) => _db = db;

    public async Task<LoginResult> ValidateLoginAsync(string benutzername, string passwort)
    {
        var hash = HashPasswort(passwort);

        var benutzer = await _db.Benutzer
            .Include(b => b.PartnerBank)
            .FirstOrDefaultAsync(b =>
                b.Benutzername == benutzername &&
                b.PasswortHash == hash);

        if (benutzer is null)
            return new LoginResult(false, 0, string.Empty, string.Empty, string.Empty);

        return new LoginResult(
            true,
            benutzer.PartnerBankId,
            benutzer.Anzeigename,
            benutzer.Benutzername,
            benutzer.PartnerBank?.Name ?? string.Empty);
    }

    public static string HashPasswort(string passwort)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(passwort));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

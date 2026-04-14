using MerkurConnectPortal.Application.DTOs;

namespace MerkurConnectPortal.Application.Interfaces;

public interface IAdminService
{
    // Dashboard
    Task<AdminDashboardDto> GetAdminDashboardAsync();

    // Partnerbanken
    Task<List<PartnerBankDto>> GetAllePartnerBankenAsync();

    // Objekte (bankübergreifend)
    Task<List<ObjektKurzDto>> GetAlleObjekteAsync(string? suchbegriff = null, string? statusFilter = null);
    Task<ObjektDetailDto?> GetObjektDetailAsync(int objektId);

    // Nachrichten (bankübergreifend, kein partnerBankId-Filter)
    Task<List<NachrichtDto>> GetNachrichtenByObjektAdminAsync(int objektId);
    Task<NachrichtDto> SendeNachrichtAlsAdminAsync(int objektId, string text, string absenderName);

    // Dokumente (bankübergreifend)
    Task<List<DokumentDto>> GetAlleDokumenteAsync(string? kategorie = null);
    Task<List<DokumentDto>> GetDokumenteByObjektAdminAsync(int objektId);
    Task<DokumentDto?> GetDokumentAdminAsync(int dokumentId);
    Task<DokumentDto> UploadDokumentAlsAdminAsync(
        int objektId, Stream dateistream, string dateiname,
        string kategorie, string hochgeladenVon, string uploadVerzeichnis);

    // Aktivitäten / Benachrichtigungen
    Task<List<BenachrichtigungDto>> GetUngeleseneAktivitaetenAsync();
    Task<int> GetUngeleseneAnzahlAsync();
    Task MarkiereNachrichtGelesenAsync(int nachrichtId);
    Task MarkiereDokumentGelesenAsync(int dokumentId);
    Task MarkiereAlleGelesenAsync();
}

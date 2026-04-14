using MerkurConnectPortal.Application.DTOs;

namespace MerkurConnectPortal.Application.Interfaces;

public interface IDokumentService
{
    Task<List<DokumentDto>> GetDokumenteByObjektAsync(int objektId, int partnerBankId);
    Task<List<DokumentDto>> GetAlleDokumenteByPartnerBankAsync(int partnerBankId, string? kategorie = null);
    Task<DokumentDto?> GetDokumentAsync(int dokumentId, int partnerBankId);
    Task<DokumentDto> UploadDokumentAsync(
        int objektId,
        int partnerBankId,
        Stream dateistream,
        string dateiname,
        string kategorie,
        string hochgeladenVon,
        string uploadVerzeichnis,
        bool vonPartnerBank = true);
    Task<(Stream stream, string dateiname, string contentType)?> DownloadDokumentAsync(int dokumentId, int partnerBankId);
    Task<int> GetUngeleseneAnzahlForPartnerBankAsync(int partnerBankId);
}

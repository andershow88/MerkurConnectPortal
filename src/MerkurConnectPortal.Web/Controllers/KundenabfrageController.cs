using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MerkurConnectPortal.Web.Controllers;

[Authorize(Roles = "Admin")]
public class KundenabfrageController : Controller
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    private const string BaseUrl = "https://api.openregister.de/v1";

    public KundenabfrageController(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    private string? ApiKey =>
        Environment.GetEnvironmentVariable("OPENREGISTER_API_KEY")
        ?? _config["OpenRegisterApiKey"]
        ?? _config["OpenRegister:ApiKey"];

    public IActionResult Index()
    {
        ViewData["Breadcrumb"] = "<a href='/Admin'>Administration</a><span class='separator'>/</span><span>Kundenabfrage</span>";
        return View();
    }

    /// <summary>
    /// Autocomplete-Vorschläge ab 3 Zeichen (Live-Dropdown).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Vorschlaege(string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 3)
            return Json(new { results = Array.Empty<object>() });

        if (string.IsNullOrWhiteSpace(ApiKey))
            return Json(new { error = "Kein OPENREGISTER_API_KEY konfiguriert." });

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");

            var url = $"{BaseUrl}/autocomplete/company?query={Uri.EscapeDataString(q.Trim())}";
            var resp = await client.GetAsync(url);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                Console.Error.WriteLine($"[OpenRegister] autocomplete {resp.StatusCode}: {body}");
                return Json(new { error = $"API-Fehler ({(int)resp.StatusCode})." });
            }

            var json = await resp.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[OpenRegister] autocomplete exception: {ex.Message}");
            return Json(new { error = "Verbindungsfehler zur OpenRegister-API." });
        }
    }

    /// <summary>
    /// Volltextsuche via POST /search/company — liefert ausführliche Treffer für die Popup-UI.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Suchen([FromBody] SuchAnfrageDto anfrage)
    {
        if (string.IsNullOrWhiteSpace(anfrage?.Query))
            return Json(new { error = "Bitte geben Sie einen Firmennamen ein." });

        if (string.IsNullOrWhiteSpace(ApiKey))
            return Json(new { error = "Kein OPENREGISTER_API_KEY konfiguriert." });

        try
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");

            var requestBody = new
            {
                query = new { value = anfrage.Query.Trim() },
                pagination = new { page = 1, per_page = 25 }
            };

            var json = JsonSerializer.Serialize(requestBody,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await client.PostAsync($"{BaseUrl}/search/company", content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                Console.Error.WriteLine($"[OpenRegister] search {resp.StatusCode}: {body}");
                return Json(new { error = $"API-Fehler ({(int)resp.StatusCode})." });
            }

            var respJson = await resp.Content.ReadAsStringAsync();
            return Content(respJson, "application/json");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[OpenRegister] search exception: {ex.Message}");
            return Json(new { error = "Verbindungsfehler zur OpenRegister-API." });
        }
    }
}

public record SuchAnfrageDto(string Query);

using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using MerkurConnectPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MerkurConnectPortal.Web.Controllers;

[Authorize]
public class AiAssistentController : Controller
{
    private readonly IApplicationDbContext _db;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public AiAssistentController(
        IApplicationDbContext db,
        IConfiguration config,
        IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    private int AktuellerPartnerBankId =>
        int.TryParse(User.FindFirst("PartnerBankId")?.Value, out var id) ? id : 0;

    private bool IstAdmin => User.IsInRole("Admin");

    [HttpPost]
    public async Task<IActionResult> Fragen([FromBody] ChatAnfrageDto anfrage)
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            ?? _config["OpenAiApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
            return Json(new { error = "Kein OPENAI_API_KEY konfiguriert. Bitte in Railway-Umgebungsvariablen eintragen." });

        if (string.IsNullOrWhiteSpace(anfrage?.Frage))
            return Json(new { error = "Keine Frage angegeben." });

        try
        {
            var kontext = await BaueKontextAsync(anfrage.Frage);

            var messages = new List<object>();
            foreach (var msg in anfrage.Verlauf ?? [])
                messages.Add(new { role = msg.Rolle, content = msg.Text });

            var rollenHinweis = IstAdmin
                ? "mandantenübergreifend, alle Partnerbanken"
                : "ausschließlich für Ihre Partnerbank";
            var userContent = string.IsNullOrWhiteSpace(kontext)
                ? anfrage.Frage
                : $"{anfrage.Frage}\n\n---\n*Aktuelle Systemdaten (automatisch geladen, {rollenHinweis}):*\n{kontext}";

            messages.Add(new { role = "user", content = userContent });

            var systemPrompt = $$"""
                Du bist der KI-Assistent des MerkurConnectPortals der Merkur Privatbank KGaA.
                Das Portal ist ein Unterbeteiligungs- und Informationsportal für Partnerbanken
                (Volksbanken, Sparkassen) bei gemeinsamen Bauträgerfinanzierungen.

                **Rolle des aktuellen Benutzers:** {{(IstAdmin ? "Admin (Merkur Privatbank) – sieht mandantenübergreifend alle Partnerbanken, Objekte, Dokumente und Nachrichten." : "Partnerbank-Mitarbeiter – sieht ausschließlich die eigene Partnerbank.")}}

                **Software-Übersicht:**
                - Partnerbanken sehen ausschließlich ihre eigenen Objekte, Dokumente und Nachrichten (Mandantentrennung)
                - Admins der Merkur Privatbank sehen alle Partnerbanken und deren Daten
                - Ein Objekt ist ein Bauträger-Finanzierungsprojekt mit Unterbeteiligungsquote
                - Je Objekt gibt es Dokumente, Nachrichten, Bautenstand und finanzielle Kennzahlen

                **Kern-Entitäten:**
                - Objekt: Objektname, Standort, Status, Bauträger, Unterbeteiligungsquote,
                  Metakontosaldo, Kaufpreissammelkontosaldo, Avale, Einheiten gesamt/verkauft,
                  Verkaufsquote, Bautenstand in Prozent, letzte Aktualisierung
                - Dokument: Dateiname, Kategorie, hochgeladen von/am, Status,
                  Dateigröße, ob von Partnerbank oder Merkur, Lesestatus
                - Nachricht: Text, Absender, Datum, ob von Partnerbank oder Merkur, Lesestatus
                - Bauträger: Firmierung eines Bauträgers
                - PartnerBank: Name, Ansprechpartner, E-Mail (eigene Bank und ggf. Merkur)

                **Status (Objekt):**
                Entwurf, InPlanung, InBau, Vertrieb, Abgeschlossen, Archiviert (je nach Projekt-Modell)

                **Dokumentkategorien:**
                Bauzeichnungen, Verträge, Prüfberichte, Bautenstandsberichte, Finanzunterlagen,
                Sonstige (siehe Projektenum)

                Antworte immer auf Deutsch. Sei präzise, freundlich und strukturiert.
                Nutze Markdown für Listen und Hervorhebungen.
                Bei Objektlisten: zeige Objektname, Standort, Status, Bautenstand und Verkaufsquote.
                Wenn du keine Daten zu einer spezifischen Anfrage hast, sage es klar.
                Beantworte ausschließlich Fragen, die sich auf dieses Portal, seine Workflows
                oder die zugeordneten Daten beziehen.
                """;

            messages.Insert(0, new { role = "system", content = systemPrompt });

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var requestBody = new
            {
                model = "gpt-4o-mini",
                max_tokens = 1500,
                messages
            };

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(requestBody, jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                Console.Error.WriteLine($"[AI] OpenAI error {response.StatusCode}: {body}");
                return Json(new { error = $"API-Fehler ({response.StatusCode}). Bitte API-Key prüfen." });
            }

            using var stream = await response.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<OpenAiResponse>(stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var antwort = result?.Choices?.FirstOrDefault()?.Message?.Content ?? "Keine Antwort erhalten.";
            return Json(new { antwort });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[AI] Exception: {ex.Message}");
            return Json(new { error = "Interner Fehler beim Aufrufen des KI-Assistenten." });
        }
    }

    // ── Datenbankkontext aufbauen (Mandanten-gefiltert) ──────────────────────

    private async Task<string> BaueKontextAsync(string frage)
    {
        var sb = new StringBuilder();
        var frageLower = frage.ToLowerInvariant();
        var pbId = AktuellerPartnerBankId;

        // ── Immer: Statistik (Admin: alle, Partnerbank: gefiltert) ───────────
        var objekteQ = IstAdmin ? _db.Objekte : _db.Objekte.Where(o => o.PartnerBankId == pbId);
        var dokumenteQ = IstAdmin ? _db.Dokumente : _db.Dokumente.Where(d => d.Objekt.PartnerBankId == pbId);
        var nachrichtenQ = IstAdmin ? _db.Nachrichten : _db.Nachrichten.Where(n => n.Objekt.PartnerBankId == pbId);

        var objekteAnzahl = await objekteQ.CountAsync();
        var dokumenteAnzahl = await dokumenteQ.CountAsync();
        var nachrichtenAnzahl = await nachrichtenQ.CountAsync();

        sb.AppendLine(IstAdmin ? "**Gesamtbestand (Admin-Sicht):**" : "**Ihre Partnerbank – Bestand:**");
        sb.AppendLine($"- Objekte: {objekteAnzahl}");
        sb.AppendLine($"- Dokumente: {dokumenteAnzahl}");
        sb.AppendLine($"- Nachrichten: {nachrichtenAnzahl}");

        if (IstAdmin)
        {
            var partnerBanken = await _db.PartnerBanken
                .OrderBy(p => p.Name)
                .Select(p => new { p.Name, ObjekteAnzahl = p.Objekte.Count() })
                .ToListAsync();
            sb.AppendLine();
            sb.AppendLine("**Partnerbanken:**");
            foreach (var p in partnerBanken)
                sb.AppendLine($"- {p.Name} ({p.ObjekteAnzahl} Objekte)");
        }
        else
        {
            var ungeleseneDoks = await dokumenteQ.CountAsync(d => !d.PartnerBankGelesen);
            var ungeleseneNachr = await nachrichtenQ.CountAsync(n => !n.PartnerBankGelesen);
            sb.AppendLine($"- Ungelesene Dokumente: {ungeleseneDoks}");
            sb.AppendLine($"- Ungelesene Nachrichten: {ungeleseneNachr}");
        }
        sb.AppendLine();

        // ── Stopwörter für die Begriffsuche ──────────────────────────────────
        var stoppwoerter = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "wer","was","wie","wo","wann","warum","welche","welcher","welches",
            "ist","sind","hat","hatte","haben","war","wird","kann",
            "der","die","das","den","dem","des","ein","eine","einer","einen",
            "und","oder","aber","auch","nur","mit","von","zu","zur","zum",
            "im","in","am","an","auf","aus","bei","bis","für","über","unter",
            "zeig","zeige","zeigen","liste","listen","finde","findet","finden",
            "such","suche","suchen","gib","gibt","nenn","nenne","nennen",
            "bitte","jetzt","doch","noch","schon","sehr","ganz","alle","meine",
            "objekt","objekte","dokument","dokumente","nachricht","nachrichten",
            "bauträger","partnerbank","standort","status","quote","saldo"
        };

        var suchBegriffe = Regex.Matches(frage, @"[\p{L}][\p{L}\-]{2,}")
            .Select(m => m.Value)
            .Where(w => !stoppwoerter.Contains(w))
            .Distinct()
            .Take(5)
            .ToList();

        bool sucheObjekte = frageLower.Contains("objekt") || frageLower.Contains("projekt")
            || frageLower.Contains("bauträger") || frageLower.Contains("bautraeger")
            || frageLower.Contains("standort") || frageLower.Contains("bautenstand")
            || frageLower.Contains("verkauf") || frageLower.Contains("obligo")
            || frageLower.Contains("saldo") || frageLower.Contains("quote");
        bool sucheDokumente = frageLower.Contains("dokument") || frageLower.Contains("datei")
            || frageLower.Contains("pdf") || frageLower.Contains("upload")
            || frageLower.Contains("hochgeladen");
        bool sucheNachrichten = frageLower.Contains("nachricht") || frageLower.Contains("nachrichten")
            || frageLower.Contains("chat") || frageLower.Contains("mitteilung");
        bool sucheListe = frageLower.Contains("liste") || frageLower.Contains("zeig")
            || frageLower.Contains("aktuell") || frageLower.Contains("neueste")
            || frageLower.Contains("letzte") || frageLower.Contains("überblick")
            || frageLower.Contains("übersicht");

        // ── Begriffsuche: Objekt/Bauträger/Dokument/Nachricht ────────────────
        bool trefferGefunden = false;

        foreach (var begriff in suchBegriffe)
        {
            var term = begriff;

            var objekteTreffer = await _db.Objekte
                .Include(o => o.Bautraeger)
                .Include(o => o.PartnerBank)
                .Where(o => (IstAdmin || o.PartnerBankId == pbId) &&
                    (o.Objektname.Contains(term) ||
                     o.Standort.Contains(term) ||
                     o.Bautraeger.Name.Contains(term) ||
                     (IstAdmin && o.PartnerBank.Name.Contains(term))))
                .OrderBy(o => o.Objektname)
                .Take(6)
                .ToListAsync();

            var dokumenteTreffer = await _db.Dokumente
                .Include(d => d.Objekt).ThenInclude(o => o.PartnerBank)
                .Where(d => (IstAdmin || d.Objekt.PartnerBankId == pbId) &&
                    (d.Dateiname.Contains(term) ||
                     d.HochgeladenVon.Contains(term)))
                .OrderByDescending(d => d.HochgeladenAm)
                .Take(6)
                .ToListAsync();

            if (objekteTreffer.Any() || dokumenteTreffer.Any())
            {
                trefferGefunden = true;
                sb.AppendLine($"**Treffer für \"{term}\":**");

                foreach (var o in objekteTreffer)
                {
                    var pbKennung = IstAdmin && o.PartnerBank != null ? $" [{o.PartnerBank.Name}]" : "";
                    sb.AppendLine($"- **{o.Objektname}**{pbKennung} ({o.Standort}) – Bauträger: {o.Bautraeger?.Name ?? "–"}");
                    sb.AppendLine($"  Status: {o.Status} | Bautenstand: {o.BautenstandProzent:N1} %"
                        + $" | Verkauft: {o.EinheitenVerkauft}/{o.EinheitenGesamt}"
                        + $" | Quote: {o.Verkaufsquote:N1} %");
                    sb.AppendLine($"  Unterbeteiligung: {o.Unterbeteiligungsquote:N2} %"
                        + $" | Metakto-Saldo: {o.Metakontosaldo:N0} €"
                        + $" | KP-Sammelkto: {o.Kaufpreissammelkontosaldo:N0} €"
                        + $" | Avale: {o.Avale:N0} €");
                }

                foreach (var d in dokumenteTreffer)
                {
                    var pbKennung = IstAdmin && d.Objekt?.PartnerBank != null ? $" [{d.Objekt.PartnerBank.Name}]" : "";
                    sb.AppendLine($"- 📄 **{d.Dateiname}** (Dokument){pbKennung} – Objekt: {d.Objekt.Objektname}");
                    sb.AppendLine($"  Kategorie: {d.Kategorie} | Status: {d.Status}"
                        + $" | hochgeladen von {d.HochgeladenVon} am {d.HochgeladenAm:dd.MM.yyyy}");
                }
                sb.AppendLine();
            }
        }

        // ── Fallback-Listen ──────────────────────────────────────────────────
        if (!trefferGefunden && (sucheObjekte || (sucheListe && !sucheDokumente && !sucheNachrichten)))
        {
            var objekte = await _db.Objekte
                .Include(o => o.Bautraeger)
                .Include(o => o.PartnerBank)
                .Where(o => IstAdmin || o.PartnerBankId == pbId)
                .OrderByDescending(o => o.LetzteAktualisierung)
                .Take(15)
                .ToListAsync();

            sb.AppendLine("**Objekte (zuletzt aktualisiert):**");
            foreach (var o in objekte)
            {
                var pbKennung = IstAdmin && o.PartnerBank != null ? $" [{o.PartnerBank.Name}]" : "";
                sb.AppendLine($"- {o.Objektname}{pbKennung} ({o.Standort}) | Status: {o.Status}"
                    + $" | Bautenstand: {o.BautenstandProzent:N1} %"
                    + $" | Verkauf: {o.Verkaufsquote:N1} %"
                    + $" | Bauträger: {o.Bautraeger?.Name ?? "–"}");
            }
            sb.AppendLine();
        }

        if (!trefferGefunden && sucheDokumente)
        {
            var dokumente = await _db.Dokumente
                .Include(d => d.Objekt).ThenInclude(o => o.PartnerBank)
                .Where(d => IstAdmin || d.Objekt.PartnerBankId == pbId)
                .OrderByDescending(d => d.HochgeladenAm)
                .Take(15)
                .ToListAsync();

            sb.AppendLine("**Dokumente (zuletzt hochgeladen):**");
            foreach (var d in dokumente)
            {
                var pbKennung = IstAdmin && d.Objekt?.PartnerBank != null ? $" [{d.Objekt.PartnerBank.Name}]" : "";
                var ungelesen = !d.PartnerBankGelesen ? "● " : "";
                sb.AppendLine($"- {ungelesen}{d.Dateiname}{pbKennung} | Objekt: {d.Objekt.Objektname}"
                    + $" | Kategorie: {d.Kategorie} | Status: {d.Status}"
                    + $" | {d.HochgeladenAm:dd.MM.yyyy}");
            }
            sb.AppendLine();
        }

        if (!trefferGefunden && sucheNachrichten)
        {
            var nachrichten = await _db.Nachrichten
                .Include(n => n.Objekt).ThenInclude(o => o.PartnerBank)
                .Where(n => IstAdmin || n.Objekt.PartnerBankId == pbId)
                .OrderByDescending(n => n.ErstelltAm)
                .Take(15)
                .ToListAsync();

            sb.AppendLine("**Nachrichten (zuletzt eingegangen):**");
            foreach (var n in nachrichten)
            {
                var pbKennung = IstAdmin && n.Objekt?.PartnerBank != null ? $" [{n.Objekt.PartnerBank.Name}]" : "";
                var ungelesen = !n.PartnerBankGelesen ? "● " : "";
                var auszug = n.Text.Length > 120 ? n.Text[..120] + "…" : n.Text;
                sb.AppendLine($"- {ungelesen}[{n.ErstelltAm:dd.MM.yyyy HH:mm}] {n.Absender}{pbKennung} @ {n.Objekt.Objektname}: {auszug}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}

// ── DTOs ─────────────────────────────────────────────────────────────────────

public record ChatAnfrageDto(string Frage, List<ChatNachrichtDto>? Verlauf);
public record ChatNachrichtDto(string Rolle, string Text);

internal class OpenAiResponse
{
    public List<OpenAiChoice>? Choices { get; set; }
}
internal class OpenAiChoice
{
    public OpenAiMessage? Message { get; set; }
}
internal class OpenAiMessage
{
    public string? Content { get; set; }
}

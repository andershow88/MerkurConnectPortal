using System.Security.Cryptography;
using System.Text;
using MerkurConnectPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MerkurConnectPortal.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (await db.PartnerBanken.AnyAsync()) return;

        // --- Partnerbanken ---
        var bank1 = new PartnerBank
        {
            Name = "Volksbank Rheinland eG",
            Land = "Deutschland",
            Ansprechpartner = "Thomas Becker",
            EMail = "t.becker@volksbank-rheinland.de"
        };
        var bank2 = new PartnerBank
        {
            Name = "Sparkasse Westfalen-Lippe",
            Land = "Deutschland",
            Ansprechpartner = "Sandra Müller",
            EMail = "s.mueller@sparkasse-wl.de"
        };
        db.PartnerBanken.AddRange(bank1, bank2);

        // --- Bauträger ---
        var bt1 = new Bautraeger { Name = "Nordbau Projektentwicklung GmbH" };
        var bt2 = new Bautraeger { Name = "RheinWest Immobilien AG" };
        var bt3 = new Bautraeger { Name = "Metropol Development KG" };
        var bt4 = new Bautraeger { Name = "Grünland Wohnbau GmbH" };
        db.Bautraeger.AddRange(bt1, bt2, bt3, bt4);

        // --- Objekte für Bank 1 ---
        var objekte1 = new List<Objekt>
        {
            new()
            {
                Objektname = "Wohnpark Nord",
                Standort = "Hamburg, Langenhorner Chaussee 145",
                Bautraeger = bt1,
                PartnerBank = bank1,
                Status = ObjektStatus.InBau,
                Unterbeteiligungsquote = 35.00m,
                Metakontosaldo = 4_250_000m,
                Kaufpreissammelkontosaldo = 1_820_000m,
                Avale = 650_000m,
                EinheitenGesamt = 48,
                EinheitenVerkauft = 32,
                Verkaufsquote = 66.67m,
                BautenstandProzent = 72m,
                LetzteAktualisierung = new DateTime(2026, 3, 28)
            },
            new()
            {
                Objektname = "Quartier Süd",
                Standort = "Köln, Deutzer Freiheit 87",
                Bautraeger = bt2,
                PartnerBank = bank1,
                Status = ObjektStatus.InBau,
                Unterbeteiligungsquote = 40.00m,
                Metakontosaldo = 6_100_000m,
                Kaufpreissammelkontosaldo = 2_340_000m,
                Avale = 980_000m,
                EinheitenGesamt = 72,
                EinheitenVerkauft = 54,
                Verkaufsquote = 75.00m,
                BautenstandProzent = 58m,
                LetzteAktualisierung = new DateTime(2026, 4, 2)
            },
            new()
            {
                Objektname = "Parkblick",
                Standort = "Düsseldorf, Grafenberger Allee 210",
                Bautraeger = bt2,
                PartnerBank = bank1,
                Status = ObjektStatus.Fertiggestellt,
                Unterbeteiligungsquote = 25.00m,
                Metakontosaldo = 2_800_000m,
                Kaufpreissammelkontosaldo = 2_800_000m,
                Avale = 0m,
                EinheitenGesamt = 36,
                EinheitenVerkauft = 36,
                Verkaufsquote = 100.00m,
                BautenstandProzent = 100m,
                LetzteAktualisierung = new DateTime(2026, 2, 15)
            },
            new()
            {
                Objektname = "Seegärten",
                Standort = "Hannover, Am Maschsee 12",
                Bautraeger = bt3,
                PartnerBank = bank1,
                Status = ObjektStatus.InPlanung,
                Unterbeteiligungsquote = 30.00m,
                Metakontosaldo = 500_000m,
                Kaufpreissammelkontosaldo = 0m,
                Avale = 200_000m,
                EinheitenGesamt = 60,
                EinheitenVerkauft = 8,
                Verkaufsquote = 13.33m,
                BautenstandProzent = 5m,
                LetzteAktualisierung = new DateTime(2026, 4, 1)
            }
        };

        // --- Objekte für Bank 2 ---
        var objekte2 = new List<Objekt>
        {
            new()
            {
                Objektname = "Lindenhof Residences",
                Standort = "Münster, Lindenhof 4-8",
                Bautraeger = bt4,
                PartnerBank = bank2,
                Status = ObjektStatus.InBau,
                Unterbeteiligungsquote = 45.00m,
                Metakontosaldo = 8_400_000m,
                Kaufpreissammelkontosaldo = 3_100_000m,
                Avale = 1_200_000m,
                EinheitenGesamt = 96,
                EinheitenVerkauft = 71,
                Verkaufsquote = 73.96m,
                BautenstandProzent = 45m,
                LetzteAktualisierung = new DateTime(2026, 4, 5)
            },
            new()
            {
                Objektname = "Stadtgarten Dortmund",
                Standort = "Dortmund, Brüderweg 22",
                Bautraeger = bt3,
                PartnerBank = bank2,
                Status = ObjektStatus.InBau,
                Unterbeteiligungsquote = 38.50m,
                Metakontosaldo = 5_650_000m,
                Kaufpreissammelkontosaldo = 1_980_000m,
                Avale = 750_000m,
                EinheitenGesamt = 56,
                EinheitenVerkauft = 39,
                Verkaufsquote = 69.64m,
                BautenstandProzent = 62m,
                LetzteAktualisierung = new DateTime(2026, 3, 20)
            },
            new()
            {
                Objektname = "Residenz am Kanal",
                Standort = "Dortmund, Hafenpromenade 1",
                Bautraeger = bt1,
                PartnerBank = bank2,
                Status = ObjektStatus.Abgeschlossen,
                Unterbeteiligungsquote = 20.00m,
                Metakontosaldo = 0m,
                Kaufpreissammelkontosaldo = 0m,
                Avale = 0m,
                EinheitenGesamt = 24,
                EinheitenVerkauft = 24,
                Verkaufsquote = 100.00m,
                BautenstandProzent = 100m,
                LetzteAktualisierung = new DateTime(2025, 11, 30)
            }
        };

        db.Objekte.AddRange(objekte1);
        db.Objekte.AddRange(objekte2);
        await db.SaveChangesAsync();

        // --- Benutzer ---
        var passwortHash1 = HashPasswort("Demo1234!");
        var passwortHash2 = HashPasswort("Demo1234!");

        db.Benutzer.AddRange(
            new Benutzer
            {
                Benutzername = "volksbank.rheinland",
                PasswortHash = passwortHash1,
                Anzeigename = "Thomas Becker",
                EMail = "t.becker@volksbank-rheinland.de",
                PartnerBank = bank1
            },
            new Benutzer
            {
                Benutzername = "sparkasse.wl",
                PasswortHash = passwortHash2,
                Anzeigename = "Sandra Müller",
                EMail = "s.mueller@sparkasse-wl.de",
                PartnerBank = bank2
            }
        );

        // --- Dokumente ---
        db.Dokumente.AddRange(
            new Dokument
            {
                Objekt = objekte1[0],
                Dateiname = "Quartalsbericht_Q1_2026_WohnparkNord.pdf",
                Kategorie = DokumentKategorie.Reportings,
                HochgeladenVon = "Merkur Privatbank",
                HochgeladenAm = new DateTime(2026, 4, 3),
                Status = DokumentStatus.Aktiv,
                Dateipfad = "seed_report_q1_wohnpark.pdf",
                DateigroesseBytes = 1_248_576,
                VonPartnerBank = false, AdminGelesen = true, PartnerBankGelesen = true
            },
            new Dokument
            {
                Objekt = objekte1[0],
                Dateiname = "Unterbeteiligungsvertrag_WohnparkNord.pdf",
                Kategorie = DokumentKategorie.Vertragsdokumente,
                HochgeladenVon = "Merkur Privatbank",
                HochgeladenAm = new DateTime(2025, 6, 15),
                Status = DokumentStatus.Aktiv,
                Dateipfad = "seed_vertrag_wohnpark.pdf",
                DateigroesseBytes = 3_245_824,
                VonPartnerBank = false, AdminGelesen = true, PartnerBankGelesen = true
            },
            new Dokument
            {
                Objekt = objekte1[1],
                Dateiname = "Bauzeitenplan_QuartierSued_v3.xlsx",
                Kategorie = DokumentKategorie.Objektunterlagen,
                HochgeladenVon = "Merkur Privatbank",
                HochgeladenAm = new DateTime(2026, 3, 10),
                Status = DokumentStatus.Aktiv,
                Dateipfad = "seed_bau_quartiersued.xlsx",
                DateigroesseBytes = 284_512,
                VonPartnerBank = false, AdminGelesen = true, PartnerBankGelesen = true
            },
            new Dokument
            {
                Objekt = objekte1[1],
                Dateiname = "Quartalsbericht_Q1_2026_QuartierSued.pdf",
                Kategorie = DokumentKategorie.Reportings,
                HochgeladenVon = "Merkur Privatbank",
                HochgeladenAm = new DateTime(2026, 4, 4),
                Status = DokumentStatus.Aktiv,
                Dateipfad = "seed_report_q1_quartiersued.pdf",
                DateigroesseBytes = 987_136,
                VonPartnerBank = false, AdminGelesen = true, PartnerBankGelesen = true
            },
            new Dokument
            {
                Objekt = objekte1[2],
                Dateiname = "Schlussabrechnung_Parkblick.pdf",
                Kategorie = DokumentKategorie.Auswertungen,
                HochgeladenVon = "Merkur Privatbank",
                HochgeladenAm = new DateTime(2026, 3, 1),
                Status = DokumentStatus.Aktiv,
                Dateipfad = "seed_schluss_parkblick.pdf",
                DateigroesseBytes = 2_156_032,
                VonPartnerBank = false, AdminGelesen = true, PartnerBankGelesen = true
            },
            new Dokument
            {
                Objekt = objekte2[0],
                Dateiname = "Quartalsbericht_Q1_2026_LindenhofResidences.pdf",
                Kategorie = DokumentKategorie.Reportings,
                HochgeladenVon = "Merkur Privatbank",
                HochgeladenAm = new DateTime(2026, 4, 6),
                Status = DokumentStatus.Aktiv,
                Dateipfad = "seed_report_q1_lindenhof.pdf",
                DateigroesseBytes = 1_456_128,
                VonPartnerBank = false, AdminGelesen = true, PartnerBankGelesen = true
            },
            new Dokument
            {
                Objekt = objekte2[0],
                Dateiname = "Unterbeteiligungsvertrag_LindenhofResidences.pdf",
                Kategorie = DokumentKategorie.Vertragsdokumente,
                HochgeladenVon = "Merkur Privatbank",
                HochgeladenAm = new DateTime(2025, 3, 22),
                Status = DokumentStatus.Aktiv,
                Dateipfad = "seed_vertrag_lindenhof.pdf",
                DateigroesseBytes = 4_123_648,
                VonPartnerBank = false, AdminGelesen = true, PartnerBankGelesen = true
            },
            new Dokument
            {
                Objekt = objekte2[1],
                Dateiname = "Grundrisse_StadtgartenDortmund.pdf",
                Kategorie = DokumentKategorie.Objektunterlagen,
                HochgeladenVon = "Merkur Privatbank",
                HochgeladenAm = new DateTime(2025, 9, 14),
                Status = DokumentStatus.Aktiv,
                Dateipfad = "seed_grundrisse_stadtgarten.pdf",
                DateigroesseBytes = 8_765_440,
                VonPartnerBank = false, AdminGelesen = true, PartnerBankGelesen = true
            },
            // Neue ungelesene Dokumente von Merkur (für Partnerbank-Demo)
            new Dokument
            {
                Objekt = objekte1[0],
                Dateiname = "Kostenkalkulation_Innenausbau_WohnparkNord.pdf",
                Kategorie = DokumentKategorie.Auswertungen,
                HochgeladenVon = "Merkur Privatbank",
                HochgeladenAm = new DateTime(2026, 4, 12, 9, 30, 0),
                Status = DokumentStatus.Aktiv,
                Dateipfad = "seed_kalkulation_wohnpark.pdf",
                DateigroesseBytes = 756_480,
                VonPartnerBank = false, AdminGelesen = true, PartnerBankGelesen = false
            },
            new Dokument
            {
                Objekt = objekte2[0],
                Dateiname = "Bautenstandsbericht_Mai2026_LindenhofResidences.pdf",
                Kategorie = DokumentKategorie.Reportings,
                HochgeladenVon = "Merkur Privatbank",
                HochgeladenAm = new DateTime(2026, 4, 13, 11, 0, 0),
                Status = DokumentStatus.Aktiv,
                Dateipfad = "seed_baubericht_lindenhof.pdf",
                DateigroesseBytes = 1_024_000,
                VonPartnerBank = false, AdminGelesen = true, PartnerBankGelesen = false
            }
        );

        // --- Nachrichten ---
        db.Nachrichten.AddRange(
            new Nachricht
            {
                Objekt = objekte1[0], Absender = "Merkur Privatbank",
                Text = "Der Rohbau ist planmäßig fertiggestellt. Laut Bautenstandsbericht vom 28.03.2026 beträgt der aktuelle Fortschritt 72 %. Die Innenausbauarbeiten beginnen voraussichtlich ab Mai 2026.",
                ErstelltAm = new DateTime(2026, 3, 28, 10, 30, 0),
                VonPartnerBank = false, AdminGelesen = true, PartnerBankGelesen = true
            },
            new Nachricht
            {
                Objekt = objekte1[0], Absender = "Volksbank Rheinland eG",
                Text = "Vielen Dank für die Aktualisierung. Bitte stellen Sie uns den aktualisierten Bauzeitenplan zu.",
                ErstelltAm = new DateTime(2026, 3, 29, 9, 15, 0),
                VonPartnerBank = true, AdminGelesen = true, PartnerBankGelesen = true
            },
            new Nachricht
            {
                Objekt = objekte1[0], Absender = "Merkur Privatbank",
                Text = "Der aktualisierte Bauzeitenplan wurde in den Dokumentenbereich hochgeladen.",
                ErstelltAm = new DateTime(2026, 4, 1, 14, 45, 0),
                VonPartnerBank = false, AdminGelesen = true, PartnerBankGelesen = true
            },
            new Nachricht
            {
                Objekt = objekte1[1], Absender = "Merkur Privatbank",
                Text = "Die Erdarbeiten für Quartier Süd sind abgeschlossen. Die Gründungsarbeiten verlaufen nach Plan.",
                ErstelltAm = new DateTime(2026, 3, 15, 11, 0, 0),
                VonPartnerBank = false, AdminGelesen = true, PartnerBankGelesen = true
            },
            new Nachricht
            {
                Objekt = objekte2[0], Absender = "Merkur Privatbank",
                Text = "Der aktuelle Bautenstand für Lindenhof Residences liegt bei 45 %. Die Rohbauarbeiten schreiten planmäßig voran. Der Quartalsbericht Q1/2026 steht im Dokumentenbereich bereit.",
                ErstelltAm = new DateTime(2026, 4, 6, 8, 0, 0),
                VonPartnerBank = false, AdminGelesen = true, PartnerBankGelesen = true
            },
            new Nachricht
            {
                Objekt = objekte2[0], Absender = "Sparkasse Westfalen-Lippe",
                Text = "Danke für den Bericht. Wann ist mit dem nächsten Bautenstandsbesuch zu rechnen?",
                ErstelltAm = new DateTime(2026, 4, 7, 10, 20, 0),
                VonPartnerBank = true, AdminGelesen = true, PartnerBankGelesen = true
            },
            // Neue ungelesene Nachrichten von Merkur (für Partnerbank-Demo)
            new Nachricht
            {
                Objekt = objekte1[0], Absender = "Merkur Privatbank",
                Text = "Die Kostenkalkulation für die Innenausbauarbeiten wurde im Dokumentenbereich hinterlegt. Bitte prüfen Sie die aktualisierte Übersicht.",
                ErstelltAm = new DateTime(2026, 4, 12, 9, 0, 0),
                VonPartnerBank = false, AdminGelesen = true, PartnerBankGelesen = false
            },
            new Nachricht
            {
                Objekt = objekte2[0], Absender = "Merkur Privatbank",
                Text = "Der nächste Bautenstandsbesuch ist für den 20.05.2026 geplant. Den aktuellen Bautenstandsbericht finden Sie im Dokumentenbereich.",
                ErstelltAm = new DateTime(2026, 4, 13, 10, 15, 0),
                VonPartnerBank = false, AdminGelesen = true, PartnerBankGelesen = false
            }
        );

        await db.SaveChangesAsync();
    }

    private static string HashPasswort(string passwort)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(passwort));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

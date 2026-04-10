# MerkurConnect Portal

**Kundenportal für Unterbeteiligungsbanken im Bauträgergeschäft**

Merkur Privatbank KGaA · MVP v1.0

---

## Überblick

Das MerkurConnect Portal ermöglicht Unterbeteiligungspartnerbanken den strukturierten Zugriff auf Informationen zu Bauträgerprojekten, an denen sie beteiligt sind. Es ersetzt manuelle E-Mail- und Telefonabstimmungen durch ein zentrales, web-basiertes Partnerportal.

### Hauptfunktionen

| Funktion | Beschreibung |
|----------|-------------|
| **Dashboard** | KPI-Übersicht: Metakonten, Kaufpreissammelkonten, Avale, Einheiten, Bautenstand |
| **Objektübersicht** | Tabellarische Liste aller Objekte mit Suche, Filter und Sortierung |
| **Objektdetail** | Stammdaten, Finanzkennzahlen, Vermarktungsstand, Baufortschritt |
| **Dokumentenaustausch** | Upload, Download, Kategorisierung von Dokumenten pro Objekt |
| **Kommunikation** | Objektbezogener Nachrichtenkanal (MVP-Platzhalter) |
| **Hilfe & Kontakt** | FAQ, Ansprechpartner, Portalinformationen |

---

## Voraussetzungen

### Entwicklung (lokal)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 oder Visual Studio Code

### Produktivbetrieb (IIS)
- Windows Server 2019+ oder Windows 10/11
- IIS 10+
- [ASP.NET Core 8 Hosting Bundle](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (enthält .NET Runtime + IIS-Modul)

---

## Lokaler Start

```bash
# 1. Repository klonen
git clone <repository-url>
cd MerkurConnectPortal

# 2. In das Web-Projekt wechseln
cd src/MerkurConnectPortal.Web

# 3. Starten (SQLite wird automatisch angelegt und befüllt)
dotnet run

# 4. Browser öffnen
# https://localhost:5001  oder  http://localhost:5000
```

### Demo-Zugangsdaten

| Benutzer | Passwort | Partnerbank |
|----------|----------|-------------|
| `volksbank.rheinland` | `Demo1234!` | Volksbank Rheinland eG |
| `sparkasse.wl` | `Demo1234!` | Sparkasse Westfalen-Lippe |

---

## Datenbank-Setup

Die Anwendung nutzt **SQLite** (für MVP/Demo) und initialisiert die Datenbank automatisch beim ersten Start:

- Tabellen werden per `EnsureCreated()` angelegt
- Seed-Daten (2 Banken, 7 Objekte, 8 Dokumente, 6 Nachrichten) werden automatisch eingespielt
- Die Datenbankdatei `merkurconnect.db` liegt im Anwendungsverzeichnis

### Umstieg auf SQL Server

In `appsettings.json` einfach den Connection String anpassen:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=DBSERVER;Database=MerkurConnect;Integrated Security=true;TrustServerCertificate=true"
  }
}
```

Die Datenbankprovider-Auswahl in `Program.cs` erkennt automatisch SQL Server vs. SQLite anhand des Connection Strings.

---

## IIS-Deployment

### Schritt 1 – Anwendung publizieren

```bash
cd src/MerkurConnectPortal.Web

# Framework-abhängig (dotnet muss auf dem Server installiert sein)
dotnet publish -c Release -o C:\Publish\MerkurConnect

# ODER Self-contained (kein dotnet auf dem Server erforderlich)
dotnet publish -c Release -r win-x64 --self-contained -o C:\Publish\MerkurConnect
```

### Schritt 2 – IIS konfigurieren

1. **IIS Manager** öffnen → **Sites** → **Neue Website** hinzufügen
2. **Physischer Pfad**: `C:\Publish\MerkurConnect`
3. **Binding**: HTTP Port 80 oder HTTPS Port 443 (mit Zertifikat)
4. **Anwendungspool**: `.NET CLR Version: Kein verwalteter Code` (für In-Process-Hosting)

### Schritt 3 – Dateiberechtigungen

```powershell
# IIS-Benutzer Schreibrecht auf Uploads-Verzeichnis und Datenbank-Verzeichnis geben
icacls "C:\Publish\MerkurConnect\wwwroot\uploads" /grant "IIS AppPool\DefaultAppPool:(OI)(CI)M"
icacls "C:\Publish\MerkurConnect" /grant "IIS AppPool\DefaultAppPool:(OI)(CI)M"
```

### Schritt 4 – Umgebungsvariable setzen

Im IIS Manager → **Anwendungspool** → **Erweiterte Einstellungen** → **Umgebungsvariablen**:
```
ASPNETCORE_ENVIRONMENT = Production
```

### Schritt 5 – web.config prüfen

Die `web.config` ist im Publish-Ausgabeverzeichnis enthalten und vorkonfiguriert:
- `processPath`: `dotnet`
- `arguments`: `.\MerkurConnectPortal.Web.dll`
- `hostingModel`: `inprocess`

---

## Architekturübersicht

```
MerkurConnectPortal.sln
└── src/
    ├── MerkurConnectPortal.Domain/          # Domänenmodell
    │   └── Entities/                        # PartnerBank, Bautraeger, Objekt, Dokument, Nachricht, Benutzer
    │
    ├── MerkurConnectPortal.Application/     # Anwendungslogik
    │   ├── Interfaces/                      # IApplicationDbContext, IDashboardService, ...
    │   ├── Services/                        # DashboardService, ObjektService, DokumentService, ...
    │   └── DTOs/                            # Datentransferobjekte
    │
    ├── MerkurConnectPortal.Infrastructure/  # Datenzugriff
    │   └── Data/                            # ApplicationDbContext (EF Core), DataSeeder
    │
    └── MerkurConnectPortal.Web/             # ASP.NET Core MVC
        ├── Controllers/                     # Account, Dashboard, Objekte, Dokumente, Nachrichten, Hilfe
        ├── ViewModels/                      # Seitenspezifische ViewModels
        ├── Views/                           # Razor Views (deutschsprachig)
        └── wwwroot/                         # CSS, JS, Uploads
```

### Schichtentrennung

| Schicht | Abhängigkeiten | Zweck |
|---------|---------------|-------|
| Domain | keine | Entitäten, Enums |
| Application | Domain | Business Logic, Interfaces, DTOs |
| Infrastructure | Domain, Application | EF Core, Datenbankzugriff |
| Web | Application, Infrastructure | Controller, Views, Authentifizierung |

---

## Sicherheitshinweise (MVP)

Das MVP nutzt:
- **Cookie-Authentifizierung** (ASP.NET Core, HTTP-Only, SameSite=Lax)
- **SHA-256 Passwort-Hashing** (Demo-Zwecke; in Produktion: BCrypt oder ASP.NET Core Identity)
- **CSRF-Schutz** via AntiForgeryToken auf allen POST-Formularen
- **Mandantentrennung** über PartnerBankId-Filterung in allen Queries

**Für Produktion empfohlen:**
- HTTPS erzwingen (HSTS)
- ASP.NET Core Identity mit BCrypt
- Content Security Policy Header
- Rate Limiting für Login
- Auditlogging

---

## Erweiterungspunkte für spätere Ausbaustufen

| Thema | Vorbereiteter Einstiegspunkt |
|-------|------------------------------|
| **BauPro/BauProWeb-Anbindung** | `IObjektService` durch API-Implementierung ersetzen |
| **Dokumentenablage (Azure Blob / SharePoint)** | `IDokumentService.UploadDokumentAsync()` adaptieren |
| **E-Mail-Benachrichtigungen** | `INachrichtService` um E-Mail-Versand erweitern |
| **SQL Server (Produktion)** | Connection String anpassen, Migrations ergänzen |
| **Rollensystem** | Claims-basiert in `BaseController` erweiterbar |
| **2FA** | In `AccountController` und `AuthService` integrierbar |
| **Multi-Mandant** | PartnerBankId-Filterlogik bereits vollständig implementiert |

---

## Tech Stack

| Komponente | Technologie |
|-----------|-------------|
| Framework | ASP.NET Core 8 MVC |
| ORM | Entity Framework Core 8 |
| Datenbank | SQLite (MVP) / SQL Server (Produktion) |
| Authentifizierung | ASP.NET Core Cookie Auth |
| UI | Bootstrap 5.3 + Custom CSS (Dunkelblau/Corporate) |
| Icons | Bootstrap Icons 1.11 |
| Hosting | IIS (ASP.NET Core Module v2, In-Process) |

---

*MerkurConnect Portal · Merkur Privatbank KGaA · Vertraulich · Nur für autorisierte Partnerbanken*

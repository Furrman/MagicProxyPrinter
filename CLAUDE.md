# MagicProxyPrinter

A .NET 10 console application that generates printable Word (.docx) documents of Magic: The Gathering proxy cards, pulling deck lists from online deck-builder sites or local exported files.

## Solution structure

```
MagicProxyPrinter.sln
├── ConsoleApp/     — CLI entry point (Cocona.Lite, NLog)
├── Domain/         — all business logic (no framework coupling)
└── UnitTests/      — xUnit + Moq unit tests
```

## Key architecture

- **`IMagicProxyPrinter`** (`Domain/MagicProxyPrinter.cs`) — top-level facade; the only public entry point for orchestrating a full run
- **`IDeckRetrieveStrategy`** — strategy pattern: picks the right site-specific service based on URL domain
- **`IServiceFactory`** (`Domain/Factories/ServiceFactory.cs`) — resolves the correct deck-build service from the DI container by matching the URL against known domains
- **Site services** (`Domain/Services/`) — one per supported site, each implements `IDeckBuildService`; EDHRec is special (HTML scraping + link forwarding)
- **`IScryfallService`** — enriches card list with image URLs from Scryfall API; handles language, tokens, emblems
- **`IWordGeneratorService`** — downloads images and assembles the `.docx` output via `OfficeIMO.Word`
- **`ICardListFileParser`** — parses plain-text deck export files (format used by Archidekt et al.)
- **DI wiring**: `Domain/DependencyInjection/ServicesRegistration.cs` → `ConsoleApp/Configuration/DependencyInjectionConfigurator.cs`

## Supported deck-builder sites

| Site | Domain constant | Notes |
|---|---|---|
| Archidekt | `archidekt.com` | No per-card language; no card number in exports |
| Moxfield | `moxfield.com` | Full API support |
| EDHRec | `edhrec.com` | HTML scraping; only `/deckpreview` and `/commanders` paths |
| MTGGoldFish | `mtggoldfish.com` | Added in v26 |
| CubeCobra | `cubecobra.com` | Added in v28 |

Domain URL constants live in `Domain/Constants/DeckBuilders.cs`.

## Adding a new deck-builder site

1. Add the URL domain constant to `Domain/Constants/DeckBuilders.cs`
2. Create `Domain/Clients/<Site>Client.cs` (HTTP client wrapper)
3. Create `Domain/Services/<Site>Service.cs` implementing `IDeckBuildService`
4. Register the HTTP client in `Domain/DependencyInjection/HttpClientFactorySetup.cs`
5. Register the service in `Domain/DependencyInjection/ServicesRegistration.cs`
6. Add URL → service mapping in `Domain/Factories/ServiceFactory.cs`
7. Add unit tests in `UnitTests/Domain/Factories/ServiceFactoryTests.cs`

## Build & run

```bash
dotnet build
dotnet run --project ConsoleApp -- --deck-url <URL>
dotnet run --project ConsoleApp -- --deck-file-path <path>
dotnet test
```

Publish self-contained single-file binary:
```bash
dotnet publish --configuration Release
```

## CLI parameters

| Parameter | Type | Description |
|---|---|---|
| `--deck-url` | string | URL to an online deck |
| `--deck-file-path` | string | Path to a locally exported deck file |
| `--language-code` | string | ISO language code for card images |
| `--token-copies` | int | Number of copies per token (1–100) |
| `--group-tokens` | flag | Deduplicate tokens by name |
| `--output-path` | string | Output directory |
| `--output-file-name` | string | Output `.docx` filename |
| `--include-emblems` | flag | Include card emblems in output |
| `--store-original-images` | flag | Save raw card images alongside the `.docx` |

## Key dependencies

- **Cocona.Lite** — CLI argument parsing
- **OfficeIMO.Word** — `.docx` generation
- **HtmlAgilityPack** — HTML scraping (EDHRec)
- **Microsoft.Extensions.Http.Polly** — resilient HTTP with retry policies
- **NLog** — logging to `log.txt`
- **xUnit + Moq** — unit testing

## Notable constraints / limitations

- Custom cards not supported
- Foil cards not supported (Scryfall API limitation, except etched/unique foils)
- EDHRec: only `/deckpreview` and `/commanders` paths work
- Archidekt: no card language selection; file exports lack card number info
- Long URLs from web-scraped links are restricted (see commit `23704f4`)
- URL must be short enough to resolve correctly when used via web scraper

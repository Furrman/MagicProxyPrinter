# Refactoring Roadmap

Findings from a full solution scan (all `.cs` files in `ConsoleApp/`, `Domain/`, `UnitTests/`, plus the three `.csproj` files). Grouped by theme, ordered roughly by impact within each group. Each item has file/line references, why it matters, and a rough effort estimate so this can be worked through step by step.

Suggested order: **Phase 1 → 2 → 3 → 4 → 5 → 6**, since later phases (de-duplicating clients/services) are safer once client-level tests exist to catch regressions.

---

## Phase 1 — Correctness bugs (small effort, do first)

These aren't tech debt, they're actual bugs found while scanning. Fixing them is cheap and unblocks trusting the rest of the codebase.

1. **`CardEntryDTO.Equals` compares a field to itself, not to `other`**
   `Domain/Models/DTO/CardEntryDTO.cs:44-47`
   ```csharp
   return Name == other.Name
       && Quantity == other.Quantity
       && CardSides == CardSides;   // always true — should be `other.CardSides`
   ```
   This silently breaks equality/dedup for `CardEntryDTO` — any code relying on `Equals`/`HashSet<CardEntryDTO>` will treat cards with different `CardSides` as equal. Fix: `CardSides.SetEquals(other.CardSides)`. Effort: small.

2. **`ScryfallClient.FindCard` builds a malformed URL for language-specific lookups**
   `Domain/Clients/ScryfallClient.cs:104-108`
   ```csharp
   var requestUrl = $"cards/{expansionCode}/{collectorNumber}";
   if (languageCode is not null)
   {
       requestUrl += $"{languageCode}";   // concatenated with no separator
   }
   ```
   Produces e.g. `cards/MH3/123en` instead of the Scryfall-documented `cards/MH3/123/en`. This likely means language-specific card lookups via `FindCard` never actually resolve. Effort: small.

3. **`ScryfallClient.SearchCard` has a stray `$` in the query string**
   `Domain/Clients/ScryfallClient.cs:138`
   ```csharp
   var requestUrl = $"cards/search?q=${cardName}";
   ```
   The `$` right before `{cardName}` is almost certainly a leftover from converting a plain string to an interpolated one — it sends `q=$CardName` literally. Also, `cardName` isn't URL-encoded, so names containing `//` (dual-faced cards), apostrophes, or spaces can break the query. Effort: small.

4. **Root `IServiceProvider` is never disposed**
   `ConsoleApp/Program.cs:16`, `ConsoleApp/Configuration/DependencyInjectionConfigurator.cs`
   `DependencyInjectionConfigurator.Setup()` builds the provider but nothing ever calls `.Dispose()` on it. `IWordDocumentWrapper` (registered `Scoped`, implements `IDisposable`) is resolved straight from the root container without an explicit scope, so it's tracked and only cleaned up when the root provider disposes — which currently never happens. Given the branch this repo is on just fixed a Word-document-disposal bug, this is the same class of issue, left unfinished. Fix: wrap `Program.Main`'s body in `using var serviceProvider = ...` or call `serviceProvider.Dispose()` before returning. Effort: small.

---

## Phase 2 — Test coverage gaps (do before de-duplicating in Phase 3/4) ✅ Done

Comparing `Domain/` against `UnitTests/Domain/` shows entire layers with zero coverage:

| Untested class | Why it matters |
|---|---|
| `Domain/Clients/ArchidektClient.cs` | Contains the request-building logic; no client has any test |
| `Domain/Clients/CubeCobraClient.cs` | " |
| `Domain/Clients/EdhrecClient.cs` | " |
| `Domain/Clients/GoldfishClient.cs` | " |
| `Domain/Clients/MoxfieldClient.cs` | " |
| `Domain/Clients/ScryfallClient.cs` | Would have caught bugs #2 and #3 above |
| `Domain/Strategies/DeckRetrieveStrategy.cs` | Contains the non-trivial EDHRec-forwarding branch (CLAUDE.md's "EDHRec is special" case) — the trickiest piece of orchestration logic in the app, untested |
| `Domain/IO/FileManager.cs` | I/O boundary class; no coverage of `ReturnCorrectWordFilePath`'s default-name/invalid-path branches |
| `Domain/IO/WordDocumentWrapper.cs` | Directly relevant to the recent disposal bugfix — no test guards against a regression |
| `Domain/Helpers/UrlHelper.cs` | Small pure function (`GetGuidFromLastPartOfUrl`), cheap to cover, used by `ScryfallService.UpdateTokens` |
| `ConsoleApp/*` (whole project) | No test project at all. `Program.cs`'s argument-validation logic (token-copies bounds, language-code validation, missing-input checks) lives inside the `CoconaApp.Run` lambda and can't be unit tested as-is — consider extracting it to an internal static method first |

Recommended approach for the clients: introduce a fake/mockable `HttpMessageHandler` (or wrap `HttpClient` behind a thin seam) so tests can assert on the actual request URL built — this is what would have caught bugs #2/#3. Effort: medium (one test class per client, ~5 clients + strategy + IO classes).

---

## Phase 3 — De-duplicate the HTTP clients ✅ Done

`Domain/Clients/CubeCobraClient.cs`, `EdhrecClient.cs`, and `GoldfishClient.cs` are structurally identical — same interface shape (`Task<string?> GetCardsInHtml(string relativePath)`), same try/catch/log pattern, differing only in the request path prefix. There's already a self-acknowledged TODO for this:

`Domain/Clients/CubeCobraClient.cs:18`
```csharp
// TODO Create generic client for clients that only retrieve directly HTML code
```

Similarly, `ArchidektClient.cs` and `MoxfieldClient.cs` share an identical JSON-fetch pattern (`GetAsync` → check status → `ReadFromJsonAsync<T>` → log on failure), differing only in the DTO type and URL template.

**Plan:**
- Extract a base class (or a single generic helper, e.g. `HtmlScrapingClientBase`) with the shared try/catch/log/status-check skeleton for the 3 HTML-scraping clients.
- Extract a generic `JsonApiClientBase<TDto>` (or a shared helper method) for the Archidekt/Moxfield JSON-fetch pattern.
- Do this *after* Phase 2 gives these classes test coverage, so the refactor is verifiable.

Effort: medium. Removes roughly 150 lines of duplicated boilerplate.

---

## Phase 4 — De-duplicate the site services ✅ Done (base covers CubeCobraService too; secondary regex finding still open)

`ArchidektService`, `CubeCobraService`, `EdhrecService`, `GoldfishService`, `MoxfieldService` (`Domain/Services/*.cs`) all follow the same shape:

```
RetrieveDeckFromWeb(url)
  → TryExtract{DeckId|RelativePath}FromUrl(url)
  → client call
  → parse response into DeckDetailsDTO
```

`EdhrecService` and `GoldfishService` in particular are near line-for-line identical (`RetrieveDeckFromWeb` → `GetDeckHtmlContent` → `TryExtractRelativePath`), differing only in the scraping regex/HTML node selectors. `CubeCobraService` follows the same skeleton but parses embedded JSON instead of HTML nodes.

**Plan:**
- Extract an abstract base (e.g. `HtmlScrapingDeckServiceBase`) with the `RetrieveDeckFromWeb`/`GetDeckHtmlContent` skeleton as a template method, leaving `TryExtractRelativePath` and the HTML/JSON-parsing step as abstract/overridable.
- Keep `ArchidektService` and `MoxfieldService` separate since they're genuinely JSON-API-shaped, not scraping-shaped — don't force a false unification.

Effort: medium-large. This is the highest-value structural cleanup in the codebase and directly reduces the cost of "Adding a new deck-builder site" (CLAUDE.md's documented 7-step process) for the next site added.

**Secondary, smaller finding in the same files:** every `TryExtract*` method does `new Regex(pattern)` inline on each call (`ArchidektService.cs:58`, `CubeCobraService.cs:108`, `EdhrecService.cs:122`, `GoldfishService.cs:91`, `MoxfieldService.cs:57`). Switch these to `[GeneratedRegex]` source-generated regex (available on net10.0) or `static readonly Regex` fields. Effort: small, mechanical, can be done independently of the bigger de-dup.

---

## Phase 5 — Resource management cleanup ✅ Done

1. **Double disposal path for the Word document.**
   `Domain/Services/WordGeneratorService.cs:65` does `using WordDocument document = _wordDocumentWrapper.Create(wordFilePath);` while `WordDocumentWrapper` (`Domain/IO/WordDocumentWrapper.cs:56-104`) *also* implements `IDisposable` and disposes the same underlying `_wordDocument` in its own `Dispose()`. Two independent owners for one resource — works today because OfficeIMO's dispose is presumably idempotent, but it's fragile. Pick one clear owner (recommend: the wrapper owns disposal; drop the `using` on the raw `WordDocument`, or don't implement `IDisposable` on the wrapper and let the `using` at the call site be the only disposal path). Effort: small.

2. **`WordDocumentWrapper.AddImage`'s `MemoryStream` is never disposed.**
   `Domain/IO/WordDocumentWrapper.cs:90-94`
   ```csharp
   var stream = new MemoryStream(imageContent);
   paragraph.AddImage(stream, fileName, width, height);
   ```
   Low real impact (`MemoryStream` over a byte array has no unmanaged resources), but wrap in `using` for consistency and to avoid the pattern being copied elsewhere. Effort: small.

3. Ties back to Phase 1 item #4 — the DI scoped-lifetime/root-provider-never-disposed issue is the root cause enabling both of the above to go unnoticed.

---

## Phase 6 — Naming, cleanup, and build hygiene

Small, independent fixes — good for filling gaps between the bigger phases.

- **`ConsoleApp/Helpers/ConsoleExtensions.cs`** — file is named `ConsoleExtensions.cs` but the class inside is `ConsoleUtility`, and it contains no extension methods (no `this` parameters). Rename the file to match the class, or rename the class if extension methods were originally intended.
- **`Domain/Constants/LanguageCodes.cs:14`** — `CHINESE_TRADITIONAL_CODE_CODE` has a duplicated `_CODE` suffix (typo).
- **`Domain/Clients/MoxfieldClient.cs:9`** — XML doc comment on `IMoxfieldClient` says *"interface for interacting with the Archidekt API"* — copy-pasted from `ArchidektClient`, should reference Moxfield.
- **`Domain/DependencyInjection/HttpClientFactorySetup.cs:12`** — `// TODO Use const value for User-Agent with app version`. Also note only 4 of the 6 registered clients set a `User-Agent` header at all (`ArchidektClient`'s registration at line 13-17 has none) — inconsistent, and some sites may rate-limit/block requests without one.
- **`Domain/GlobalSuppressions.cs`** — suppresses `CA1822` ("mark members as static") on 7 methods across `FileManager` and `Services`, with the justification `"Avoid using static methods"` on every entry. That justification is circular (CA1822 exists specifically to *suggest* static) — either give each suppression a real reason (e.g. "kept as instance methods for mock-based testability/consistency with the rest of the class") or reconsider whether these should just be static.
- **Constants visibility is inconsistent** — `CardDetails`, `DeckBuilders`, `FilePaths`, `LanguageCodes` are `internal class`, but `ScryfallParts` (`Domain/Constants/ScryfallParts.cs:3`) is `public class` with no apparent reason for the difference.
- **`Domain/Services/WordGeneratorService.cs`** — the interface (`IWordGeneratorService.GenerateWord`, line 34) names the output-folder parameter `outputFolder`; the implementation (line 49) names it `outputFolderDir`. Harmless today (no named-argument call sites), but worth aligning.
- **No `Directory.Build.props`** — `TargetFramework net10.0`, `ImplicitUsings`, and `Nullable enable` are duplicated identically across all three `.csproj` files. A solution-level `Directory.Build.props` would remove the duplication and reduce drift risk as the solution grows (already 5 separate `10.0.8` version pins across `ConsoleApp.csproj`/`Domain.csproj`). Consider pairing with `Directory.Packages.props` (central package management) at the same time.
- **DI lifetime choice is arbitrary for a CLI tool.** Every Domain service in `ServicesRegistration.cs` is registered `Scoped`, but `Program.cs` never creates a scope — it resolves directly from the root provider, so in practice everything behaves like a singleton for the process's lifetime anyway (see Phase 1 #4 / Phase 5 #3). Worth either creating an explicit scope per run, or switching registrations to `Singleton`/`Transient` deliberately instead of `Scoped` by default.

---

## Not investigated further (flag, don't fix blindly)

- **Long-URL restriction** (CLAUDE.md: *"Long URLs from web-scraped links are restricted (see commit `23704f4`)"*) — the length-restriction logic wasn't obviously visible in the files scanned (the EDHRec/Goldfish/CubeCobra regexes just bound path segments with `[^/]+`, not an explicit length check). Worth a dedicated look at commit `23704f4` to confirm the restriction still exists and is covered by a test, before touching any of the scraping services in Phase 4.
- **Retry/timeout budget** — every HTTP client in `HttpClientFactorySetup.cs` gets identical 30s timeout + 3 retries with exponential backoff (2s/4s/8s). Worst case, a single call can take ~1-2 minutes before failing, which for a CLI tool can look like a hang with no feedback. Not a bug, but worth a conscious decision (progress/heartbeat output during retries, or a shorter budget) rather than leaving it as the Polly default.

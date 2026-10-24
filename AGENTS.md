# EchoSpec — Agent Handbook

Zero-dependency .NET library that turns tabular/report data into Console, Markdown, HTML (or custom) output via fluent builders and renderer interfaces.

Package: `EchoSpec` on NuGet · License: MIT · Repo: https://github.com/tzinmein/EchoSpec

## Quick commands

```bash
# Restore / build (solution is EchoSpec.slnx)
dotnet restore
dotnet build -c Release

# Tests (multi-targets net8.0 + net10.0)
dotnet test -c Release

# Test only the TFM available on the machine (local Homebrew often has net10 only)
dotnet test -c Release -f net10.0

# Pack
dotnet pack src/EchoSpec/EchoSpec.csproj -c Release -o ./artifacts
```

CI: `.github/workflows/dotnet.yml` — build/test on `master` and `v*.*.*` tags; packs NuGet; publishes with `ECHOSPEC_NUGET_KEY`. Tag pushes set version from the tag; master pushes use `1.0.1-beta.<run>`.

## Layout

```
src/EchoSpec/                 # Library (packable)
  Interfaces.cs               # ITable, ITableRenderer, IReportRenderer, ColumnAlignment
  ReportFormat.cs             # Console | Markdown | Both  (HTML is NOT in this enum)
  ReportOutput.cs             # ConsoleText + MarkdownText helpers only
  ConsoleRenderer.cs
  MarkdownRenderer.cs
  HtmlRenderer.cs             # Name == "HTML"; CombineParts wraps full HTML document
  Builders/TableBuilder.cs    # Fluent table; also implements ITable
  Builders/ReportBuilder.cs   # Generic ReportBuilder<T>
  Sections/                   # Internal: TableSection, CustomSection
tests/EchoSpec.Tests/         # xUnit
examples/HtmlRendererExample.cs   # Sample Main(); NOT a project (no .csproj)
assets/logo.svg               # Brand mark (vector source)
assets/icon.png               # NuGet PackageIcon (export from logo.svg)
```

Namespaces: `EchoSpec`, `EchoSpec.Builders`. Sections are `internal` under `EchoSpec.Sections`.

## Architecture (mental model)

1. **Data in** → `TableBuilder` rows/headers, or `ReportBuilder<T>` over `IEnumerable<T>`.
2. **Structure** → report sections (`AddTable`, `AddSection`) hold formatters; tables are rebuilt per render.
3. **Render** → `ITableRenderer` / `IReportRenderer` produce strings.
4. **Out** → `Build` / `Generate` use `ReportFormat` (console/markdown); `BuildWith` / `GenerateWith` take custom renderers.

```
ReportBuilder<T>
  ├─ WithTitle / WithReferenceUrl
  ├─ AddTable(configure, title?)     → TableSection → renderer.RenderTable
  └─ AddSection(consoleFn, mdFn, ?)  → CustomSection → renderer.RenderText
         │
         ▼
   Generate(format)     → ReportOutput { ConsoleText?, MarkdownText? }
   GenerateWith(r...)   → string | Dictionary<string,string>  (cast required)
```

### Critical API quirks (do not “simplify” without a version bump)

| Quirk | Detail |
|-------|--------|
| `BuildWith` / `GenerateWith` return `object` | 1 renderer → `string`; 2+ → `Dictionary<string,string>` keyed by `renderer.Name`. Callers cast. |
| HTML is second-class | Not in `ReportFormat` or `ReportOutput`. Use `GenerateWith(..., new HtmlRenderer())`. |
| `TableBuilder.Build(ReportFormat.Both)` | Falls through to Markdown (default arm). Prefer `BuildWith` for multi-format. |
| `HtmlRenderer.Render(ITable)` | Returns a **table fragment** (`<table>…</table>`), not a document. Full document only via `CombineParts` (report path). |
| `CustomSection` | Console vs Markdown formatters by `renderer.Name`; anything else (incl. HTML) uses **markdown** formatter text, then `RenderText` (HTML escapes + wraps `<p>`). |
| Empty tables | Console/HTML `Render` return `""` when `Rows.Count == 0`; Markdown still emits header/separator. |
| Zero deps | Do not add NuGet deps to the library project without explicit product decision. |

### Extending formats

Implement `ITableRenderer` and/or `IReportRenderer` (see QUICKSTART CSV sample). `Name` must be unique when using multi-renderer dictionaries. Prefer matching built-in names only when intentionally replacing Console/Markdown/HTML behavior for `CustomSection` switching.

## Conventions for changes

- Public APIs: XML docs (`GenerateDocumentationFile` is on).
- Fluent builders return `this`; keep that style.
- Nullable enabled; prefer modern C# (collection expressions, primary constructors where already used).
- Update `CHANGELOG.md` (Keep a Changelog + SemVer) for user-facing changes.
- Keep README (landing) and QUICKSTART (tutorial/API) examples compiling conceptually (`using EchoSpec.Builders`).
- Releases: annotated git tags `v*.*.*` (not GitHub Releases UI) — see CHANGELOG note.
- Tests: xUnit; assert on structure/content, not brittle full-string dumps unless necessary.
- Prefer fixing/hardening library code over expanding emoji-heavy marketing copy.

## Local environment notes

- Solution format is **`.slnx`** (SDK-style solution).
- Machine may only have the **.NET 10** runtime; `dotnet test` without `-f` can abort the net8.0 host while net10.0 still passes. CI installs 10.x SDK which can multi-target; ensure net8.0 is available in CI if both TFMs must run.
- `examples/` is documentation-only until a sample `.csproj` exists.
- `EchoSpec.slnx` references missing `GITHUB_ACTIONS.md` — ignore or restore that file; do not assume it exists.

## Test map

| File | Focus |
|------|--------|
| `TableBuilderTests` | Headers, rows, Build console/markdown |
| `ReportBuilderTests` | Generate Both |
| `RendererTests` | Console/Markdown table smoke |
| `HtmlRendererTests` | Strongest coverage: escape, align, CombineParts, BuildWith/GenerateWith |

Gaps: `Clear()`, row/header length validation, `Build(Both)`, duplicate renderer names, CustomSection + HtmlRenderer behavior, Markdown pipe escaping.

---

## Priorities: fixes & enhancements

Ordered for agent/work planning. Prefer P0–P1 before features.

### P0 — Correctness / API footguns

1. **`TableBuilder.Build(ReportFormat.Both)`** — Document as unsupported or throw; do not silently emit Markdown.
2. **Doc accuracy** — Keep README/QUICKSTART aligned: table `BuildWith(HtmlRenderer)` is a fragment; full documents only via report `GenerateWith` → `CombineParts`.
3. **CustomSection + non-Console/Markdown renderers** — Escape/format mismatch for HTML (markdown text → `EscapeHtml` → `<p>`). Design a renderer-agnostic section API (e.g. `AddSection(data => text)` + let renderer format, or per-renderer delegates / `ITextContent`).
4. **Row width validation** — Optional strict mode or pad/truncate consistently when `row.Count != headers.Count`.

### P1 — DX & consistency

5. **Replace `object` returns** — Overloads: `string BuildWith(ITableRenderer)`, `IReadOnlyDictionary<string,string> BuildWith(IEnumerable<ITableRenderer>)` (same for `GenerateWith`). Avoid breaking change: add overloads, obsolete ambiguous `params` if needed.
6. **First-class HTML** — `ReportFormat.Html` and/or `ReportOutput.HtmlText` + `SaveHtml`, or deprecate `Both` in favor of multi-renderer only.
7. **Empty-table policy** — Render header-only tables consistently across Console/Markdown/HTML.
8. **Markdown cell escaping** — Escape `\|` / newlines in cells.
9. **Runnable example project** — `examples/EchoSpec.Examples.csproj` referencing the library; wire VS Code launch to it (today launch points at test DLL).

### P2 — Quality & packaging

10. **Tests for P0/P1** — Cover Both, CustomSection+HTML, empty tables, escaping, multi-renderer name collisions.
11. **CI** — Explicitly install/test net8.0 + net10.0; reconsider publishing beta packages on every `master` push.
12. **Restore or drop `GITHUB_ACTIONS.md`** from the solution folder list.
13. **Remove unused `using System.Text`** in `TableBuilder.cs` (minor).
14. **Console title box** — Handle extremely long titles/URLs without awkward padding; consider min width option.

### P3 — Enhancements (product)

15. Built-in `CsvRenderer` / `JsonRenderer` (still zero deps).
16. Streaming/`TextWriter` sinks to reduce large-string allocations.
17. Theme/CSS injection hooks on `HtmlRenderer`.
18. Column formatters / strongly typed column definitions on `TableBuilder`.
19. Async file helpers on `ReportOutput` if the library grows I/O surface.

---

## Suggested first tasks for agents

1. Fix/document `Build(ReportFormat.Both)` and HTML fragment vs document docs.
2. Add typed `BuildWith`/`GenerateWith` overloads without breaking existing casts.
3. Add example `.csproj` + tighten tests around CustomSection and empty tables.
4. Only then add new renderers or HTML-in-`ReportFormat`.

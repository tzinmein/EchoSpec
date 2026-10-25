# EchoSpec — Agent Handbook

Zero-dependency .NET library that turns tabular/report data into Console, Markdown, HTML (or custom) output via fluent builders and renderer interfaces.

Package: `EchoSpec` on NuGet · License: MIT · Repo: https://github.com/tzinmein/EchoSpec

## Quick commands

```bash
# Restore / build (solution is EchoSpec.slnx)
dotnet restore
dotnet build -c Release

# Tests (net10.0)
dotnet test -c Release

# Pack
dotnet pack src/EchoSpec/EchoSpec.csproj -c Release -o ./artifacts
```

CI: `.github/workflows/dotnet.yml` — build/test on `master` and `v*.*.*` tags; packs NuGet; publishes with `ECHOSPEC_NUGET_KEY`. Tag pushes set version from the tag; master pushes use `1.0.1-beta.<run>`.

## Layout

```
src/EchoSpec/                 # Library (packable)
  Interfaces.cs               # ITable, ITableRenderer, IReportRenderer, ColumnAlignment
  ReportFormat.cs             # Console | Markdown | Html | All
  ReportOutput.cs             # ConsoleText, MarkdownText, HtmlText + Save*
  ConsoleRenderer.cs
  MarkdownRenderer.cs
  HtmlRenderer.cs             # Name == "HTML"; CombineParts wraps full HTML document
  Builders/TableBuilder.cs    # Fluent table; also implements ITable
  Builders/ReportBuilder.cs   # Generic ReportBuilder<T>
  Sections/                   # Internal: TableSection, CustomSection (+ Prepare/Render)
tests/EchoSpec.Tests/         # xUnit
examples/HtmlRendererExample.cs   # Sample Main(); NOT a project (no .csproj)
assets/logo.svg               # Brand mark (vector source)
assets/icon.png               # NuGet PackageIcon (export from logo.svg)
```

Namespaces: `EchoSpec`, `EchoSpec.Builders`. Sections are `internal` under `EchoSpec.Sections`.

## Architecture (mental model)

1. **Data in** → `TableBuilder` rows/headers, or `ReportBuilder<T>` over `IEnumerable<T>`.
2. **Structure** → report sections (`AddTable`, `AddSection`); `Prepare` materializes once.
3. **Render** → prepared sections + `ITableRenderer` / `IReportRenderer` produce strings.
4. **Out** → `Build` / `Generate` use `ReportFormat`; `BuildWith` / `GenerateWith` take custom renderers.

```
ReportBuilder<T>
  ├─ WithTitle / WithReferenceUrl
  ├─ AddTable(Action configure, title?)    → TableSection.Prepare → ITable
  └─ AddSection(plain | console/md/html)   → CustomSection.Prepare → cached strings
         │
         ▼
   Generate(format)     → ReportOutput { ConsoleText?, MarkdownText?, HtmlText? }
   GenerateWith(r)      → string
   GenerateWith(r...)   → IReadOnlyDictionary<string,string> keyed by Name
```

### Critical API notes (2.0)

| Note | Detail |
|------|--------|
| Typed `BuildWith` / `GenerateWith` | Single renderer → `string`; `params` → `IReadOnlyDictionary<string,string>`. No `object` / casts. |
| HTML | In `ReportFormat` / `ReportOutput`. Table path = fragment; report path = full document via `CombineParts`. |
| `TableBuilder.Build(All)` | Throws. Use `BuildWith` or call `Build` per format. No `Both` — subset via `GenerateWith`/`BuildWith`. |
| `HtmlRenderer.Render(ITable)` | Returns a **table fragment** (`<table>…</table>`), not a document. |
| `CustomSection` | Plain formatter, or console+markdown+html. Unknown names fall back plain → markdown → console → html. |
| Empty tables | Header/separator (or empty tbody) rendered consistently across Console/Markdown/HTML. |
| Row widths | Pad/truncate to header count by default; `StrictRowWidths` throws. |
| Duplicate `Name` | Throws when building multi-renderer dictionaries. |
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
- Library and tests target **net10.0** only (aligned with CI’s .NET 10 SDK).
- `examples/` is documentation-only until a sample `.csproj` exists.
- `EchoSpec.slnx` references missing `GITHUB_ACTIONS.md` — ignore or restore that file; do not assume it exists.

## Test map

| File | Focus |
|------|--------|
| `TableBuilderTests` | Headers, rows, Build, All throw, typed BuildWith, row widths, Clear |
| `ReportBuilderTests` | Generate Html/All, GenerateWith subsets, section APIs, prepare-once, duplicates |
| `RendererTests` | Console/Markdown smoke, empty tables, markdown escaping |
| `HtmlRendererTests` | Escape, align, CombineParts, BuildWith/GenerateWith |

---

## Priorities: remaining enhancements

### P2 — Quality & packaging

1. **Runnable example project** — `examples/EchoSpec.Examples.csproj`; wire VS Code launch to it.
2. **CI** — Reconsider publishing beta packages on every `master` push.
3. **Restore or drop `GITHUB_ACTIONS.md`** from the solution folder list.
4. **Console title box** — Handle extremely long titles/URLs; consider min width option.

### P3 — Product

5. Built-in `CsvRenderer` / `JsonRenderer` (still zero deps).
6. Streaming/`TextWriter` sinks to reduce large-string allocations.
7. Theme/CSS injection hooks on `HtmlRenderer`.
8. Column formatters / strongly typed column definitions on `TableBuilder`.
9. Async file helpers on `ReportOutput` if the library grows I/O surface.

# EchoSpec

[![Build & Publish NuGet](https://github.com/tzinmein/EchoSpec/actions/workflows/dotnet.yml/badge.svg)](https://github.com/tzinmein/EchoSpec/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/EchoSpec.svg)](https://www.nuget.org/packages/EchoSpec/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)

A zero-dependency .NET library for turning tabular data into Console, Markdown, HTML, or custom output from a single source.

- Fluent `TableBuilder` and `ReportBuilder<T>`
- Built-in Console, Markdown, and HTML renderers
- Extend via `ITableRenderer` / `IReportRenderer`

## Installation

```bash
dotnet add package EchoSpec
```

Or add a `PackageReference` and set `Version` to the current release on [NuGet](https://www.nuget.org/packages/EchoSpec/).

## Quick example

```csharp
using EchoSpec;
using EchoSpec.Builders;

var table = new TableBuilder()
    .AddHeader("Name")
    .AddHeader("Score", ColumnAlignment.Right)
    .AddRow("Alice", "95")
    .AddRow("Bob", "87");

Console.WriteLine(table.Build(ReportFormat.Console));
File.WriteAllText("results.md", table.Build(ReportFormat.Markdown));
File.WriteAllText("results-table.html", table.Build(ReportFormat.Html)); // <table> fragment
```

```
Name   Score
─────────────
Alice     95
Bob       87
```

Reports over typed data:

```csharp
var builder = new ReportBuilder<TestResult>()
    .WithTitle("Test Results")
    .AddTable((table, data) =>
    {
        table.AddHeader("Test")
             .AddHeader("Status", ColumnAlignment.Center);
        foreach (var row in data)
            table.AddRow(row.Name, row.Passed ? "✓" : "✗");
    })
    .AddSection(data => $"Total: {data.Count()} tests");

var output = builder.Generate(results, ReportFormat.All);
output.WriteToConsole();
output.SaveMarkdown("results.md");
output.SaveHtml("results.html"); // full HTML document
```

## Gotchas

| Topic | Detail |
|-------|--------|
| Table HTML | `Build(ReportFormat.Html)` / `BuildWith(new HtmlRenderer())` → `<table>` fragment |
| Report HTML | `Generate(ReportFormat.Html)` / `GenerateWith(..., new HtmlRenderer())` → full document |
| `TableBuilder.Build` | Single formats only (`Console`, `Markdown`, `Html`). `All` throws — use `BuildWith` |
| Console + Markdown only | `GenerateWith` / `BuildWith` with those two renderers (no `Both` enum) |
| `BuildWith` / `GenerateWith` | One renderer → `string`; `params` → `IReadOnlyDictionary<string, string>` keyed by `Name` |
| Duplicate `Name` | Throws — renderer names must be unique |
| Row widths | Default pad/truncate to header count; set `StrictRowWidths = true` to throw |

## Docs

- [QUICKSTART.md](QUICKSTART.md) — tutorial, API notes, custom renderers
- [examples/](examples/) — additional samples
- [CHANGELOG.md](CHANGELOG.md)

## License

MIT — see [LICENSE](LICENSE).

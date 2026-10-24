# EchoSpec

[![Build & Publish NuGet](https://github.com/tzinmein/EchoSpec/actions/workflows/dotnet.yml/badge.svg)](https://github.com/tzinmein/EchoSpec/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/EchoSpec.svg)](https://www.nuget.org/packages/EchoSpec/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%2010.0-512BD4)](https://dotnet.microsoft.com/)

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
        return table;
    });

var output = builder.Generate(results, ReportFormat.Both);
output.WriteToConsole();
output.SaveMarkdown("results.md");

// Full HTML document (table BuildWith returns a <table> fragment only)
File.WriteAllText(
    "results.html",
    (string)builder.GenerateWith(results, new HtmlRenderer()));
```

## Gotchas

| Topic | Detail |
|-------|--------|
| `ReportFormat` / `ReportOutput` | Console and Markdown only — not HTML |
| Table HTML | `BuildWith(new HtmlRenderer())` → `<table>` fragment |
| Report HTML | `GenerateWith(..., new HtmlRenderer())` → full document |
| `BuildWith` / `GenerateWith` | One renderer → `string`; two or more → `Dictionary<string, string>` keyed by `Name` (cast required) |

## Docs

- [QUICKSTART.md](QUICKSTART.md) — tutorial, API notes, custom renderers
- [examples/](examples/) — additional samples
- [CHANGELOG.md](CHANGELOG.md)

## License

MIT — see [LICENSE](LICENSE).

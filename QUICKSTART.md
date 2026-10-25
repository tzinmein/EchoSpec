# EchoSpec Quick Start

Companion to the [README](README.md). Install with `dotnet add package EchoSpec`.

## Tutorial

### 1. Tables

```csharp
using EchoSpec;
using EchoSpec.Builders;

var table = new TableBuilder()
    .AddHeader("Name")
    .AddHeader("Score", ColumnAlignment.Right)
    .AddHeader("Status")
    .AddRow("Alice", "95", "✓")
    .AddRow("Bob", "87", "✓")
    .AddRow("Charlie", "72", "~");

Console.WriteLine(table.Build(ReportFormat.Console));
File.WriteAllText("results.md", table.Build(ReportFormat.Markdown));

// Fragment only — not a full HTML document
var htmlFragment = table.BuildWith(new HtmlRenderer());
File.WriteAllText("table.html", htmlFragment);
```

```
Name     Score  Status
──────────────────────
Alice       95  ✓
Bob         87  ✓
Charlie     72  ~
```

### 2. Reports

```csharp
using EchoSpec;
using EchoSpec.Builders;

public record TestResult
{
    public string TestName { get; init; } = "";
    public bool Passed { get; init; }
    public TimeSpan Duration { get; init; }
}

var results = new List<TestResult>
{
    new() { TestName = "Login Test", Passed = true, Duration = TimeSpan.FromMilliseconds(250) },
    new() { TestName = "API Test", Passed = true, Duration = TimeSpan.FromMilliseconds(150) },
    new() { TestName = "UI Test", Passed = false, Duration = TimeSpan.FromMilliseconds(500) }
};

var reportBuilder = new ReportBuilder<TestResult>()
    .WithTitle("Test Execution Report")
    .WithReferenceUrl("https://example.com/test-suite")
    .AddTable((table, data) =>
    {
        table.AddHeader("Test Name")
             .AddHeader("Status", ColumnAlignment.Center)
             .AddHeader("Duration (ms)", ColumnAlignment.Right);

        foreach (var result in data)
        {
            table.AddRow(
                result.TestName,
                result.Passed ? "✓" : "✗",
                $"{result.Duration.TotalMilliseconds:F0}");
        }
    })
    .AddSection(
        data => $"Total: {data.Count()} tests, {data.Count(r => r.Passed)} passed");

var reportOutput = reportBuilder.Generate(results, ReportFormat.All);
reportOutput.WriteToConsole();
reportOutput.SaveMarkdown("test-report.md");
reportOutput.SaveHtml("test-report.html");
```

### 3. Multiple renderers

```csharp
var outputs = table.BuildWith(
    new ConsoleRenderer(),
    new MarkdownRenderer(),
    new HtmlRenderer());

Console.WriteLine(outputs["Console"]);
File.WriteAllText("results.md", outputs["Markdown"]);
File.WriteAllText("table.html", outputs["HTML"]); // fragment
```

### 4. Multi-section report

```csharp
var output = new ReportBuilder<BenchmarkResult>()
    .WithTitle("Performance Benchmarks")
    .AddTable((table, data) =>
    {
        table.AddHeader("Operation")
             .AddHeader("Time (ms)", ColumnAlignment.Right);

        foreach (var result in data.OrderBy(r => r.Time))
            table.AddRow(result.Operation, $"{result.Time:F2}");
    }, "Detailed Results")
    .AddSection(
        data => $"Fastest: {data.MinBy(r => r.Time)?.Operation}",
        data => $"**Fastest:** {data.MinBy(r => r.Time)?.Operation}",
        data => $"Fastest: {data.MinBy(r => r.Time)?.Operation}",
        "Summary")
    .Generate(benchmarks, ReportFormat.All);

output.WriteToConsole();
output.SaveMarkdown("benchmarks.md");
output.SaveHtml("benchmarks.html");
```

## API notes

### Formats

```csharp
public enum ReportFormat
{
    Console,   // 0
    Markdown,  // 1
    Html,      // 2
    All        // 3 — Console + Markdown + HTML (ReportBuilder.Generate only)
}
```

| Path | HTML result |
|------|-------------|
| `TableBuilder.Build(ReportFormat.Html)` / `BuildWith(new HtmlRenderer())` | `<table>…</table>` fragment |
| `ReportBuilder.Generate(ReportFormat.Html)` / `GenerateWith(..., new HtmlRenderer())` | Full HTML document via `CombineParts` |

`TableBuilder.Build` supports only single formats (`Console`, `Markdown`, `Html`). `All` throws.
For Console + Markdown without HTML, use `GenerateWith` / `BuildWith` with those renderers.

### Built-in renderers

| Type | `Name` | Notes |
|------|--------|--------|
| `ConsoleRenderer` | `"Console"` | Aligned columns; title box for reports |
| `MarkdownRenderer` | `"Markdown"` | Pipe tables; escapes `\|` and newlines in cells |
| `HtmlRenderer` | `"HTML"` | Fragments on tables; full documents on reports |

### TableBuilder

- `AddHeader(string, ColumnAlignment = Left)`
- `AddRow(params object[])` / `AddRow(params string[])`
- `StrictRowWidths` — default `false` (pad/truncate); `true` throws on width mismatch
- `Build(ReportFormat)` → `string` (single formats only)
- `BuildWith(ITableRenderer)` → `string`
- `BuildWith(params ITableRenderer[])` → `IReadOnlyDictionary<string, string>` keyed by `Name`
- `Clear()`

### ReportBuilder&lt;T&gt;

- `WithTitle` / `WithReferenceUrl`
- `AddTable(Action<TableBuilder, IEnumerable<T>>, string? sectionTitle = null)`
- `AddSection(contentFormatter, sectionTitle?)` — same plain text for every renderer; `RenderText` formats
- `AddSection(console, markdown, html, sectionTitle?)` — per-format content (required when formats differ)
- `Generate` → `ReportOutput` (`ConsoleText`, `MarkdownText`, `HtmlText`, `WriteToConsole()`, `SaveMarkdown`, `SaveHtml`)
- `GenerateWith(data, IReportRenderer)` → `string`
- `GenerateWith(data, params IReportRenderer[])` → `IReadOnlyDictionary<string, string>`

Multi-format `Generate` / `GenerateWith` prepare each section once (tables and formatters run a single time).

### Interfaces

```csharp
public interface ITableRenderer
{
    string Name { get; }
    string Render(ITable table);
}

public interface IReportRenderer
{
    string Name { get; }
    string RenderTitle(string title, string? referenceUrl = null);
    string RenderSectionHeader(string sectionTitle);
    string RenderTable(ITable table);
    string RenderText(string text);
    string CombineParts(IEnumerable<string> parts);
}
```

## Custom renderers

Implement `ITableRenderer` and/or `IReportRenderer`. Keep `Name` unique when using multi-renderer dictionaries.

```csharp
using System.Text;
using EchoSpec;

public class CsvRenderer : ITableRenderer, IReportRenderer
{
    public string Name => "CSV";

    public string Render(ITable table)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", table.Headers.Select(EscapeCsv)));
        foreach (var row in table.Rows)
            sb.AppendLine(string.Join(",", row.Select(EscapeCsv)));
        return sb.ToString();
    }

    public string RenderTitle(string title, string? referenceUrl = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {title}");
        if (referenceUrl != null)
            sb.AppendLine($"# Reference: {referenceUrl}");
        sb.AppendLine();
        return sb.ToString();
    }

    public string RenderSectionHeader(string sectionTitle) => $"\n# {sectionTitle}\n";
    public string RenderTable(ITable table) => Render(table);
    public string RenderText(string text) => $"# {text}\n";
    public string CombineParts(IEnumerable<string> parts) =>
        string.Join("\n", parts.Where(p => !string.IsNullOrWhiteSpace(p)));

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
```

```csharp
var csv = table.BuildWith(new CsvRenderer());
File.WriteAllText("data.csv", csv);
```

## See also

- [examples/](examples/)
- [CHANGELOG.md](CHANGELOG.md)
- [Issues](https://github.com/tzinmein/EchoSpec/issues)

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
var htmlFragment = (string)table.BuildWith(new HtmlRenderer());
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

        return table;
    })
    .AddSection(
        data => $"Total: {data.Count()} tests, {data.Count(r => r.Passed)} passed",
        data => $"Total: {data.Count()} tests, {data.Count(r => r.Passed)} passed");

var reportOutput = reportBuilder.Generate(results, ReportFormat.Both);
reportOutput.WriteToConsole();
reportOutput.SaveMarkdown("test-report.md");

// Full HTML document (DOCTYPE, embedded CSS, body)
var htmlDocument = (string)reportBuilder.GenerateWith(results, new HtmlRenderer());
File.WriteAllText("test-report.html", htmlDocument);
```

`Generate` / `ReportOutput` cover Console and Markdown only.

### 3. Multiple renderers

```csharp
var outputs = (Dictionary<string, string>)table.BuildWith(
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

        return table;
    }, "Detailed Results")
    .AddSection(
        data => $"Fastest: {data.MinBy(r => r.Time)?.Operation}",
        data => $"**Fastest:** {data.MinBy(r => r.Time)?.Operation}",
        "Summary")
    .Generate(benchmarks, ReportFormat.Both);

output.WriteToConsole();
output.SaveMarkdown("benchmarks.md");
```

## API notes

### Formats

```csharp
public enum ReportFormat
{
    Console,
    Markdown,
    Both   // Console + Markdown (not HTML)
}
```

| Path | HTML result |
|------|-------------|
| `TableBuilder.BuildWith(new HtmlRenderer())` | `<table>…</table>` fragment |
| `ReportBuilder<T>.GenerateWith(..., new HtmlRenderer())` | Full HTML document via `CombineParts` |

`TableBuilder.Build` supports Console and Markdown. Prefer `BuildWith` for HTML or multiple custom renderers.

### Built-in renderers

| Type | `Name` | Notes |
|------|--------|--------|
| `ConsoleRenderer` | `"Console"` | Aligned columns; title box for reports |
| `MarkdownRenderer` | `"Markdown"` | Pipe tables with alignment markers |
| `HtmlRenderer` | `"HTML"` | Fragments on tables; full documents on reports |

### TableBuilder

- `AddHeader(string, ColumnAlignment = Left)`
- `AddRow(params object[])`
- `Build(ReportFormat)` → `string`
- `BuildWith(params ITableRenderer[])` → `string` (one) or `Dictionary<string, string>` keyed by `Name` (two or more)
- `Clear()`

### ReportBuilder&lt;T&gt;

- `WithTitle` / `WithReferenceUrl`
- `AddTable(Func<TableBuilder, IEnumerable<T>, TableBuilder>, string? sectionTitle = null)`
- `AddSection(consoleFormatter, markdownFormatter, string? sectionTitle = null)` — non-Console/Markdown renderers use the **markdown** formatter text, then `RenderText`
- `Generate` → `ReportOutput` (`ConsoleText`, `MarkdownText`, `WriteToConsole()`, `SaveMarkdown`)
- `GenerateWith` → same return rules as `BuildWith`

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
var csv = (string)table.BuildWith(new CsvRenderer());
File.WriteAllText("data.csv", csv);
```

## See also

- [examples/](examples/)
- [CHANGELOG.md](CHANGELOG.md)
- [Issues](https://github.com/tzinmein/EchoSpec/issues)

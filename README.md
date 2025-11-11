# EchoSpec

A flexible, generic reporting library for .NET that generates beautiful test reports in multiple output formats from a single source of data.

## Features

- 📊 **Multi-Format Output**: Generate reports in multiple formats (Console, Markdown, HTML, or custom)
- 🎨 **Beautiful Formatting**: Box-drawing characters for console, proper tables for markdown, styled HTML output
- 🔧 **Fully Generic**: Works with any data type using `ReportBuilder<T>`
- 🔌 **Extensible Renderers**: Interface-based design allows custom output formats (JSON, CSV, etc.)
- 🚀 **Zero Dependencies**: Standalone library with no external dependencies
- 📝 **Type-Safe**: Leverages C# generics and LINQ for type safety

## Installation

### Via NuGet Package Manager

```bash
dotnet add package EchoSpec
```

### Via Package Manager Console

```powershell
Install-Package EchoSpec
```

### Via PackageReference

```xml
<ItemGroup>
  <PackageReference Include="EchoSpec" Version="1.0.0" />
</ItemGroup>
```

## Quick Start

### Simple Table

```csharp
using EchoSpec;

var table = new TableBuilder()
    .AddHeader("Name")
    .AddHeader("Score", ColumnAlignment.Right)
    .AddHeader("Status")
    .AddRow("Alice", "95", "✓")
    .AddRow("Bob", "87", "✓")
    .AddRow("Charlie", "72", "~");

// Console output
Console.WriteLine(table.Build(ReportFormat.Console));

// Markdown output
File.WriteAllText("results.md", table.Build(ReportFormat.Markdown));

// HTML output
var html = (string)table.BuildWith(new HtmlRenderer());
File.WriteAllText("results.html", html);
```

**Console Output:**

```
Name     Score  Status
────────────────────────
Alice       95  ✓
Bob         87  ✓
Charlie     72  ~
```

**Markdown Output:**

```markdown
| Name    | Score | Status |
| ------- | ----: | ------ |
| Alice   |    95 | ✓      |
| Bob     |    87 | ✓      |
| Charlie |    72 | ~      |
```

**HTML Output:**

```html
<table>
  <thead>
    <tr>
      <th>Name</th>
      <th>Score</th>
      <th>Status</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>Alice</td>
      <td>95</td>
      <td>✓</td>
    </tr>
    <tr>
      <td>Bob</td>
      <td>87</td>
      <td>✓</td>
    </tr>
    <tr>
      <td>Charlie</td>
      <td>72</td>
      <td>~</td>
    </tr>
  </tbody>
</table>
```

### Generic Report Builder

```csharp
using EchoSpec;

// Your data model
public record TestResult
{
    public string TestName { get; init; }
    public bool Passed { get; init; }
    public TimeSpan Duration { get; init; }
}

// Collect results
var results = new List<TestResult>
{
    new() { TestName = "Login Test", Passed = true, Duration = TimeSpan.FromMilliseconds(250) },
    new() { TestName = "API Test", Passed = true, Duration = TimeSpan.FromMilliseconds(150) },
    new() { TestName = "UI Test", Passed = false, Duration = TimeSpan.FromMilliseconds(500) }
};

// Build report
var report = new ReportBuilder<TestResult>()
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
                $"{result.Duration.TotalMilliseconds:F0}"
            );
        }

        return table;
    })
    .AddSection(
        // Console formatter
        data => $"Total: {data.Count()} tests, {data.Count(r => r.Passed)} passed",
        // Markdown formatter
        data => $"**Total:** {data.Count()} tests, {data.Count(r => r.Passed)} passed"
    )
    .Generate(results, ReportFormat.Both);

// Display and save
report.WriteToConsole();
report.SaveMarkdown("test-report.md");
```

**Generated Report:**

Console:

```
╔══════════════════════════════════════════════════════╗
║  Test Execution Report                               ║
║  https://example.com/test-suite                      ║
╚══════════════════════════════════════════════════════╝

Test Name    Status  Duration (ms)
──────────────────────────────────
Login Test     ✓            250
API Test       ✓            150
UI Test        ✗            500

Total: 3 tests, 2 passed
```

Markdown:

```markdown
# Test Execution Report

Reference: https://example.com/test-suite

## Test Results

| Test Name  | Status | Duration (ms) |
| ---------- | :----: | ------------: |
| Login Test |   ✓    |           250 |
| API Test   |   ✓    |           150 |
| UI Test    |   ✗    |           500 |

**Total:** 3 tests, 2 passed
```

HTML:

```html
<h1>Test Execution Report</h1>
<p>Reference: <a href="https://example.com/test-suite">https://example.com/test-suite</a></p>

<h2>Test Results</h2>
<table>
  <thead>
    <tr>
      <th>Test Name</th>
      <th>Status</th>
      <th>Duration (ms)</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>Login Test</td>
      <td>✓</td>
      <td>250</td>
    </tr>
    <tr>
      <td>API Test</td>
      <td>✓</td>
      <td>150</td>
    </tr>
    <tr>
      <td>UI Test</td>
      <td>✗</td>
      <td>500</td>
    </tr>
  </tbody>
</table>

<p><strong>Total:</strong> 3 tests, 2 passed</p>
```

## Core Components

### Renderer Interfaces

EchoSpec uses an interface-based design for extensibility:

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

**Built-in Renderers:**

- `ConsoleRenderer` - Box-drawing characters for terminal output
- `MarkdownRenderer` - GitHub-compatible markdown tables
- `HtmlRenderer` - Complete HTML documents with embedded CSS styling

### ReportFormat Enum

```csharp
public enum ReportFormat
{
    Console,   // Console output only
    Markdown,  // Markdown output only
    Both       // Generate both formats
}
```

### TableBuilder

Builds formatted tables for any renderer:

```csharp
var table = new TableBuilder()
    .AddHeader("Column 1", ColumnAlignment.Left)
    .AddHeader("Column 2", ColumnAlignment.Right)
    .AddRow("value1", "value2")
    .Build(ReportFormat.Console);

// Or use a custom renderer directly
var customRenderer = new HtmlRenderer();
var html = (string)table.BuildWith(customRenderer);

// Or generate multiple formats at once
var outputs = (Dictionary<string, string>)table.BuildWith(
    new ConsoleRenderer(),
    new MarkdownRenderer(),
    new HtmlRenderer()
);

Console.WriteLine(outputs["Console"]);
File.WriteAllText("report.md", outputs["Markdown"]);
File.WriteAllText("report.html", outputs["HTML"]);
```

**Methods:**

- `AddHeader(string, ColumnAlignment)`: Add column header
- `AddRow(params object[])`: Add data row
- `Build(ReportFormat)`: Generate output using built-in renderers
- `BuildWith(params ITableRenderer[])`: Generate output using one or more custom renderers
  - Single renderer returns `string`
  - Multiple renderers return `Dictionary<string, string>` keyed by renderer name
- `Clear()`: Reset builder state

### ReportBuilder&lt;T&gt;

Generic report generator that works with any data type:

```csharp
var report = new ReportBuilder<MyDataType>()
    .WithTitle("Report Title")
    .WithReferenceUrl("https://...")
    .AddTable((builder, data) => { /* configure table */ })
    .AddSection(consoleFormatter, markdownFormatter)
    .Generate(dataList, ReportFormat.Both);
```

**Methods:**

- `WithTitle(string)`: Set report title
- `WithReferenceUrl(string)`: Add reference URL
- `AddTable(Func<TableBuilder, IEnumerable<T>, TableBuilder>, string?)`: Add table section
- `AddSection(Func<IEnumerable<T>, string>, Func<IEnumerable<T>, string>, string?)`: Add custom section
- `Generate(IEnumerable<T>, ReportFormat)`: Generate report using built-in renderers
- `GenerateWith(IEnumerable<T>, params IReportRenderer[])`: Generate report using one or more custom renderers
  - Single renderer returns `string`
  - Multiple renderer returns `Dictionary<string, string>` keyed by renderer name

### ReportOutput

Container for generated reports:

```csharp
public class ReportOutput
{
    public string? ConsoleText { get; }
    public string? MarkdownText { get; }

    public void WriteToConsole();
    public void SaveMarkdown(string filePath);
}
```

## Advanced Examples

### Multi-Section Report

```csharp
var report = new ReportBuilder<BenchmarkResult>()
    .WithTitle("Performance Benchmarks")
    .AddTable((table, data) =>
    {
        table.AddHeader("Operation")
             .AddHeader("Time (ms)", ColumnAlignment.Right)
             .AddHeader("Memory (MB)", ColumnAlignment.Right);

        foreach (var result in data.OrderBy(r => r.Time))
        {
            table.AddRow(result.Operation, $"{result.Time:F2}", $"{result.Memory:F1}");
        }

        return table;
    }, "Detailed Results")
    .AddSection(
        data => $"Fastest: {data.MinBy(r => r.Time)?.Operation} ({data.Min(r => r.Time):F2}ms)",
        data => $"**Fastest:** {data.MinBy(r => r.Time)?.Operation} ({data.Min(r => r.Time):F2}ms)",
        "Summary"
    )
    .Generate(benchmarks, ReportFormat.Both);
```

### Custom Formatting

```csharp
var report = new ReportBuilder<ErrorLog>()
    .WithTitle("Error Report")
    .AddTable((table, data) =>
    {
        table.AddHeader("Timestamp")
             .AddHeader("Level")
             .AddHeader("Message");

        foreach (var error in data.OrderByDescending(e => e.Timestamp))
        {
            var level = error.Level == LogLevel.Error ? "❌" : "⚠️";
            table.AddRow(
                error.Timestamp.ToString("HH:mm:ss"),
                level,
                error.Message.Length > 50
                    ? error.Message[..47] + "..."
                    : error.Message
            );
        }

        return table;
    })
    .Generate(errorLogs, ReportFormat.Console);

report.WriteToConsole();
```

### Multiple Renderers at Once

Generate all formats in a single call:

```csharp
var table = new TableBuilder()
    .AddHeader("Feature")
    .AddHeader("Status", ColumnAlignment.Center)
    .AddRow("Authentication", "✓")
    .AddRow("API Integration", "✓")
    .AddRow("UI Polish", "⏳");

// Generate console, markdown, and HTML simultaneously
var outputs = (Dictionary<string, string>)table.BuildWith(
    new ConsoleRenderer(),
    new MarkdownRenderer(),
    new HtmlRenderer(),
    new CsvRenderer()
);

// Use each format as needed
Console.WriteLine(outputs["Console"]);
File.WriteAllText("status.md", outputs["Markdown"]);
File.WriteAllText("status.html", outputs["HTML"]);

// Or with reports
var report = new ReportBuilder<FeatureStatus>()
    .WithTitle("Feature Status Report")
    .AddTable((table, data) => { /* ... */ });

var reportOutputs = (Dictionary<string, string>)report.GenerateWith(
    features,
    new ConsoleRenderer(),
    new MarkdownRenderer(),
    new HtmlRenderer()
);
```

## Design Principles

1. **Single Source of Truth**: Data collected once, formatted multiple ways
2. **Format Flexibility**: Easy to extend with new output formats (HTML, JSON, etc.)
3. **Type Safety**: Leverage C# generics for compile-time safety
4. **Separation of Concerns**: Data collection separated from presentation
5. **Developer Experience**: Fluent API for readable code

## Extending EchoSpec

### Creating Custom Renderers

EchoSpec's interface-based design makes it easy to add new output formats. The library includes three built-in renderers (`ConsoleRenderer`, `MarkdownRenderer`, `HtmlRenderer`) as reference implementations.

Here's how to create a custom CSV renderer:

```csharp
using EchoSpec;
using System.Text;

public class CsvRenderer : ITableRenderer, IReportRenderer
{
    public string Name => "CSV";

    public string Render(ITable table)
    {
        var sb = new StringBuilder();
        
        // Header row
        sb.AppendLine(string.Join(",", table.Headers.Select(EscapeCsv)));
        
        // Data rows
        foreach (var row in table.Rows)
        {
            var cells = row.Select(EscapeCsv);
            sb.AppendLine(string.Join(",", cells));
        }
        
        return sb.ToString();
    }

    public string RenderTitle(string title, string? referenceUrl = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {title}");
        if (referenceUrl != null)
        {
            sb.AppendLine($"# Reference: {referenceUrl}");
        }
        sb.AppendLine();
        return sb.ToString();
    }

    public string RenderSectionHeader(string sectionTitle)
    {
        return $"\n# {sectionTitle}\n";
    }

    public string RenderTable(ITable table) => Render(table);

    public string RenderText(string text)
    {
        return $"# {text}\n";
    }

    public string CombineParts(IEnumerable<string> parts)
    {
        return string.Join("\n", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}
```

**Usage:**

```csharp
// Use with TableBuilder
var csv = (string)table.BuildWith(new CsvRenderer());
File.WriteAllText("data.csv", csv);

// Use with ReportBuilder
var report = new ReportBuilder<TestResult>()
    .WithTitle("Test Report")
    .AddTable((table, data) => { /* ... */ })
    .GenerateWith(results, new CsvRenderer());

File.WriteAllText("report.csv", (string)report);

// Generate multiple formats at once
var outputs = (Dictionary<string, string>)table.BuildWith(
    new ConsoleRenderer(),
    new MarkdownRenderer(),
    new HtmlRenderer(),
    new CsvRenderer()
);

File.WriteAllText("data.html", outputs["HTML"]);
File.WriteAllText("data.csv", outputs["CSV"]);
```

Other potential custom renderers:

- **JsonRenderer** - Export to JSON format
- **XmlRenderer** - Export to XML
- **AnsiRenderer** - Rich console colors with ANSI codes
- **LatexRenderer** - Academic papers and documents
- **SqlRenderer** - Generate INSERT statements
- **ExcelRenderer** - XLSX format (requires external library)

## Performance

- **Overhead**: <1ms per report generation
- **Memory**: Minimal allocations, single-pass generation
- **Scalability**: Handles thousands of rows efficiently

## Building & Publishing

### Build the Package

```bash
cd src/EchoSpec
dotnet build -c Release
```

### Create NuGet Package

```bash
dotnet pack -c Release -o ./nupkg
```

### Publish to NuGet.org

```bash
dotnet nuget push ./nupkg/EchoSpec.1.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

### Local Testing

```bash
# Add local package source
dotnet nuget add source /path/to/nupkg --name LocalEchoSpec

# Install from local source
dotnet add package EchoSpec --source LocalEchoSpec
```

## Contributing

Contributions welcome! EchoSpec is part of the zxcvbn-ts project but designed as a standalone library.

### Development Setup

1. Clone the repository
2. Open in Visual Studio 2022 or VS Code with C# extension
3. Build: `dotnet build`
4. Run tests: `dotnet test`

### Guidelines

- Follow existing code style and patterns
- Add XML documentation for public APIs
- Update README.md and CHANGELOG.md
- Ensure zero external dependencies
- Add examples for new features

## License

MIT License - See LICENSE file for details

---

**EchoSpec** - Echo your specs beautifully 📊✨

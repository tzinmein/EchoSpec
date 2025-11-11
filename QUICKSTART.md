# EchoSpec Quick Start Guide

## Installation

```bash
dotnet add package EchoSpec
```

## 5-Minute Tutorial

### 1. Create Your First Table

```csharp
using EchoSpec;

var table = new TableBuilder()
    .AddHeader("Name")
    .AddHeader("Score", ColumnAlignment.Right)
    .AddRow("Alice", "95")
    .AddRow("Bob", "87");

// Console output
Console.WriteLine(table.Build(ReportFormat.Console));

// HTML output
var html = (string)table.BuildWith(new HtmlRenderer());
File.WriteAllText("results.html", html);
```

Output:

```
Name   Score
─────────────
Alice     95
Bob       87
```

### 2. Generate a Complete Report

```csharp
public record TestResult(string Name, bool Passed, double Duration);

var results = new List<TestResult>
{
    new("Login", true, 0.25),
    new("Checkout", false, 1.5)
};

var report = new ReportBuilder<TestResult>()
    .WithTitle("Test Results")
    .AddTable((table, data) =>
    {
        table.AddHeader("Test")
             .AddHeader("Status", ColumnAlignment.Center)
             .AddHeader("Time (s)", ColumnAlignment.Right);

        foreach (var test in data)
            table.AddRow(test.Name, test.Passed ? "✓" : "✗", test.Duration);

        return table;
    })
    .Generate(results, ReportFormat.Both);

report.WriteToConsole();
report.SaveMarkdown("results.md");
```

### 3. Use Built-in HTML Renderer

```csharp
// Generate HTML report with styled output
var html = (string)report.GenerateWith(results, new HtmlRenderer());
File.WriteAllText("report.html", html);
```

The HTML includes embedded CSS for professional formatting with:
- Clean table styling with borders and alternating row colors
- Responsive typography using system fonts
- Proper semantic HTML structure

### 4. Create a Custom Renderer

```csharp
public class CsvRenderer : ITableRenderer, IReportRenderer
{
    public string Name => "CSV";

    public string Render(ITable table)
    {
        var lines = new List<string>
        {
            string.Join(",", table.Headers)
        };

        foreach (var row in table.Rows)
            lines.Add(string.Join(",", row));

        return string.Join(Environment.NewLine, lines);
    }

    // Implement other IReportRenderer methods...
}

// Use it
var csv = (string)table.BuildWith(new CsvRenderer());
File.WriteAllText("data.csv", csv);
```

### 5. Multiple Formats at Once

```csharp
var outputs = (Dictionary<string, string>)table.BuildWith(
    new ConsoleRenderer(),
    new MarkdownRenderer(),
    new HtmlRenderer(),
    new CsvRenderer()
);

Console.WriteLine(outputs["Console"]);
File.WriteAllText("report.md", outputs["Markdown"]);
File.WriteAllText("report.html", outputs["HTML"]);
File.WriteAllText("data.csv", outputs["CSV"]);
```

## Next Steps

- Read the full [README.md](README.md) for advanced features
- Check out the [examples](examples/) directory for more code samples

## Getting Help

- 📝 [Full Documentation](README.md)
- 🐛 [Report Issues](https://github.com/tzinmein/EchoSpec/issues)
- 💬 [Discussions](https://github.com/tzinmein/EchoSpec/discussions)

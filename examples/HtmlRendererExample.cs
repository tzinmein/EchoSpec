using EchoSpec;
using EchoSpec.Builders;

namespace EchoSpec.Examples;

/// <summary>
/// Example demonstrating HtmlRenderer usage.
/// </summary>
public class HtmlRendererExample
{
    public static void Main()
    {
        // Example 1: Simple table with HTML output
        Console.WriteLine("=== Example 1: Simple HTML Table ===\n");
        
        var table = new TableBuilder()
            .AddHeader("Name")
            .AddHeader("Score", ColumnAlignment.Right)
            .AddHeader("Status", ColumnAlignment.Center)
            .AddRow("Alice", "95", "?")
            .AddRow("Bob", "87", "?")
            .AddRow("Charlie", "72", "~");

        var html = (string)table.BuildWith(new HtmlRenderer());
        File.WriteAllText("table-output.html", html);
        Console.WriteLine("HTML table saved to: table-output.html\n");

        // Example 2: Full report with HTML output
        Console.WriteLine("=== Example 2: Full HTML Report ===\n");

        var testResults = new[]
        {
            new { TestName = "Login Test", Passed = true, Duration = 250 },
            new { TestName = "API Test", Passed = true, Duration = 150 },
            new { TestName = "UI Test", Passed = false, Duration = 500 }
        };

        var report = new ReportBuilder<dynamic>()
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
                        result.Passed ? "?" : "?",
                        $"{result.Duration}"
                    );
                }

                return table;
            }, "Test Results")
            .AddSection(
                data => $"Total: {data.Count()} tests, {data.Count(r => r.Passed)} passed",
                data => $"**Total:** {data.Count()} tests, {data.Count(r => r.Passed)} passed",
                "Summary"
            );

        var reportHtml = (string)report.GenerateWith(testResults, new HtmlRenderer());
        File.WriteAllText("report.html", reportHtml);
        Console.WriteLine("HTML report saved to: report.html\n");

        // Example 3: Generate multiple formats at once
        Console.WriteLine("=== Example 3: Multiple Formats ===\n");

        var outputs = (Dictionary<string, string>)table.BuildWith(
            new ConsoleRenderer(),
            new MarkdownRenderer(),
            new HtmlRenderer()
        );

        Console.WriteLine("Console output:");
        Console.WriteLine(outputs["Console"]);
        Console.WriteLine();

        File.WriteAllText("multi-format.md", outputs["Markdown"]);
        File.WriteAllText("multi-format.html", outputs["HTML"]);
        Console.WriteLine("Markdown saved to: multi-format.md");
        Console.WriteLine("HTML saved to: multi-format.html");
    }
}

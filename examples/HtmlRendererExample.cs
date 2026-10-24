using EchoSpec;
using EchoSpec.Builders;

namespace EchoSpec.Examples;

/// <summary>
/// Example demonstrating HtmlRenderer usage.
/// </summary>
public class HtmlRendererExample
{
    private sealed record TestResult(string TestName, bool Passed, int DurationMs);

    public static void Main()
    {
        // Example 1: Simple table with HTML output
        Console.WriteLine("=== Example 1: Simple HTML Table ===\n");

        var table = new TableBuilder()
            .AddHeader("Name")
            .AddHeader("Score", ColumnAlignment.Right)
            .AddHeader("Status", ColumnAlignment.Center)
            .AddRow("Alice", "95", "PASS")
            .AddRow("Bob", "87", "PASS")
            .AddRow("Charlie", "72", "WARN");

        var html = (string)table.BuildWith(new HtmlRenderer());
        File.WriteAllText("table-output.html", html);
        Console.WriteLine("HTML table saved to: table-output.html\n");

        // Example 2: Full report with HTML output
        Console.WriteLine("=== Example 2: Full HTML Report ===\n");

        var testResults = new[]
        {
            new TestResult("Login Test", true, 250),
            new TestResult("API Test", true, 150),
            new TestResult("UI Test", false, 500),
        };

        var reportBuilder = new ReportBuilder<TestResult>()
            .WithTitle("Test Execution Report")
            .WithReferenceUrl("https://example.com/test-suite")
            .AddTable(
                (table, data) =>
                {
                    table
                        .AddHeader("Test Name")
                        .AddHeader("Status", ColumnAlignment.Center)
                        .AddHeader("Duration (ms)", ColumnAlignment.Right);

                    foreach (var result in data)
                    {
                        table.AddRow(
                            result.TestName,
                            result.Passed ? "PASS" : "FAIL",
                            result.DurationMs
                        );
                    }

                    return table;
                },
                "Test Results"
            )
            .AddSection(
                data => $"Total: {data.Count()} tests, {data.Count(r => r.Passed)} passed",
                data => $"Total: {data.Count()} tests, {data.Count(r => r.Passed)} passed",
                "Summary"
            );

        var reportOutput = reportBuilder.Generate(testResults, ReportFormat.Both);
        reportOutput.WriteToConsole();
        reportOutput.SaveMarkdown("report.md");

        var reportHtml = (string)reportBuilder.GenerateWith(testResults, new HtmlRenderer());
        File.WriteAllText("report.html", reportHtml);
        Console.WriteLine("HTML report saved to: report.html\n");
        Console.WriteLine("Markdown report saved to: report.md\n");

        // Example 3: Generate multiple formats at once
        Console.WriteLine("=== Example 3: Multiple Formats ===\n");

        var outputs =
            (Dictionary<string, string>)
                table.BuildWith(new ConsoleRenderer(), new MarkdownRenderer(), new HtmlRenderer());

        Console.WriteLine("Console output:");
        Console.WriteLine(outputs["Console"]);
        Console.WriteLine();

        File.WriteAllText("multi-format.md", outputs["Markdown"]);
        File.WriteAllText("multi-format.html", outputs["HTML"]);
        Console.WriteLine("Markdown saved to: multi-format.md");
        Console.WriteLine("HTML saved to: multi-format.html");
    }
}

using EchoSpec.Builders;
using Xunit;

namespace EchoSpec.Tests;

public class ReportBuilderTests
{
    public record TestResult(string Name, bool Passed, double Duration);

    [Fact]
    public void Generate_ReportOutput_ContainsExpectedSections()
    {
        var results = new[]
        {
            new TestResult("Login", true, 0.25),
            new TestResult("Checkout", false, 1.5),
        };

        var report = new ReportBuilder<TestResult>()
            .WithTitle("Test Results")
            .AddTable(
                (table, data) =>
                {
                    table
                        .AddHeader("Test")
                        .AddHeader("Status", ColumnAlignment.Center)
                        .AddHeader("Time (s)", ColumnAlignment.Right);
                    foreach (var test in data)
                        table.AddRow(test.Name, test.Passed ? "?" : "?", test.Duration);
                    return table;
                }
            )
            .Generate(results, ReportFormat.Both);

        Assert.Contains("Test Results", report.ConsoleText);
        Assert.Contains("Test", report.ConsoleText);
        Assert.Contains("Login", report.ConsoleText);
        Assert.Contains("Checkout", report.ConsoleText);
        Assert.Contains("Test Results", report.MarkdownText);
        Assert.Contains("Test", report.MarkdownText);
        Assert.Contains("Login", report.MarkdownText);
        Assert.Contains("Checkout", report.MarkdownText);
    }
}

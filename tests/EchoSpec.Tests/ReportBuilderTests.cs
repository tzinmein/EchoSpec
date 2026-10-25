using System;
using System.Linq;
using EchoSpec.Builders;
using Xunit;

namespace EchoSpec.Tests;

public class ReportBuilderTests
{
    public record TestResult(string Name, bool Passed, double Duration);

    [Fact]
    public void GenerateWith_ConsoleAndMarkdown_OmitsHtml()
    {
        var results = new[]
        {
            new TestResult("Login", true, 0.25),
            new TestResult("Checkout", false, 1.5),
        };

        var outputs = new ReportBuilder<TestResult>()
            .WithTitle("Test Results")
            .AddTable(
                (table, data) =>
                {
                    table
                        .AddHeader("Test")
                        .AddHeader("Status", ColumnAlignment.Center)
                        .AddHeader("Time (s)", ColumnAlignment.Right);
                    foreach (var test in data)
                        table.AddRow(test.Name, test.Passed ? "ok" : "fail", test.Duration);
                    return table;
                }
            )
            .GenerateWith(results, new ConsoleRenderer(), new MarkdownRenderer());

        Assert.Contains("Test Results", outputs["Console"]);
        Assert.Contains("Login", outputs["Console"]);
        Assert.Contains("Test Results", outputs["Markdown"]);
        Assert.Contains("Login", outputs["Markdown"]);
        Assert.False(outputs.ContainsKey("HTML"));
    }

    [Fact]
    public void Generate_Html_ProducesFullDocument()
    {
        var report = new ReportBuilder<string>()
            .WithTitle("HTML Report")
            .AddTable(
                (table, data) =>
                {
                    table.AddHeader("Name");
                    foreach (var item in data)
                        table.AddRow(item);
                    return table;
                }
            )
            .Generate(["Item1"], ReportFormat.Html);

        Assert.Null(report.ConsoleText);
        Assert.Null(report.MarkdownText);
        Assert.Contains("<!DOCTYPE html>", report.HtmlText);
        Assert.Contains("Item1", report.HtmlText);
    }

    [Fact]
    public void Generate_All_FillsEveryChannel()
    {
        var report = new ReportBuilder<string>()
            .WithTitle("All Formats")
            .AddSection(data => $"Count: {data.Count()}")
            .Generate(["a"], ReportFormat.All);

        Assert.NotNull(report.ConsoleText);
        Assert.NotNull(report.MarkdownText);
        Assert.NotNull(report.HtmlText);
        Assert.Contains("Count: 1", report.ConsoleText);
        Assert.Contains("<p>Count: 1</p>", report.HtmlText);
    }

    [Fact]
    public void GenerateWith_SingleRenderer_ReturnsString()
    {
        var html = new ReportBuilder<string>()
            .WithTitle("T")
            .GenerateWith([], new HtmlRenderer());

        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("<h1>T</h1>", html);
    }

    [Fact]
    public void GenerateWith_MultipleRenderers_PreparesSectionsOnce()
    {
        var builds = 0;
        var report = new ReportBuilder<string>()
            .WithTitle("T")
            .AddTable(
                (table, data) =>
                {
                    builds++;
                    table.AddHeader("Name");
                    foreach (var item in data)
                        table.AddRow(item);
                    return table;
                }
            );

        var outputs = report.GenerateWith(
            ["x"],
            new ConsoleRenderer(),
            new MarkdownRenderer(),
            new HtmlRenderer()
        );

        Assert.Equal(1, builds);
        Assert.Equal(3, outputs.Count);
        Assert.Contains("<!DOCTYPE html>", outputs["HTML"]);
    }

    [Fact]
    public void AddSection_WithHtmlFormatter_UsesHtmlText()
    {
        var html = new ReportBuilder<string>()
            .WithTitle("T")
            .AddSection(
                _ => "console-only",
                _ => "**markdown**",
                _ => "html plain",
                "Summary"
            )
            .GenerateWith(["x"], new HtmlRenderer());

        Assert.Contains("<p>html plain</p>", html);
        Assert.DoesNotContain("markdown", html);
        Assert.DoesNotContain("console-only", html);
    }

    [Fact]
    public void AddSection_Plain_UsesRenderTextForAll()
    {
        var outputs = new ReportBuilder<string>()
            .WithTitle("T")
            .AddSection(_ => "shared")
            .GenerateWith(["x"], new ConsoleRenderer(), new HtmlRenderer());

        Assert.Contains("shared", outputs["Console"]);
        Assert.Contains("<p>shared</p>", outputs["HTML"]);
    }

    [Fact]
    public void GenerateWith_DuplicateRendererNames_Throws()
    {
        var report = new ReportBuilder<string>().WithTitle("T");

        Assert.Throws<ArgumentException>(() =>
            report.GenerateWith([], new HtmlRenderer(), new HtmlRenderer())
        );
    }
}

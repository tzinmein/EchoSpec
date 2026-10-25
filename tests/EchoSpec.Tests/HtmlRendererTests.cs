using EchoSpec.Builders;
using Xunit;

namespace EchoSpec.Tests;

public class HtmlRendererTests
{
    [Fact]
    public void Name_ReturnsHTML()
    {
        Assert.Equal("HTML", new HtmlRenderer().Name);
    }

    [Fact]
    public void Render_SimpleTable_GeneratesValidHtml()
    {
        var renderer = new HtmlRenderer();
        var table = new TableBuilder()
            .AddHeader("Name")
            .AddHeader("Score", ColumnAlignment.Right)
            .AddRow("Alice", "95")
            .AddRow("Bob", "87");

        var html = renderer.Render(table);

        Assert.Contains("<table>", html);
        Assert.Contains("<thead>", html);
        Assert.Contains("<tbody>", html);
        Assert.Contains("<th>Name</th>", html);
        Assert.Contains("<th style=\"text-align: right;\">Score</th>", html);
        Assert.Contains("<td>Alice</td>", html);
        Assert.Contains("<td style=\"text-align: right;\">95</td>", html);
    }

    [Fact]
    public void Render_EmptyRows_StillRendersHeader()
    {
        var table = new TableBuilder().AddHeader("Name").AddHeader("Score");

        var html = new HtmlRenderer().Render(table);

        Assert.Contains("<table>", html);
        Assert.Contains("<th>Name</th>", html);
        Assert.Contains("<tbody>", html);
    }

    [Fact]
    public void Render_WithCenterAlignment_AppliesCenterStyle()
    {
        var table = new TableBuilder().AddHeader("Status", ColumnAlignment.Center).AddRow("?");

        var html = new HtmlRenderer().Render(table);

        Assert.Contains("<th style=\"text-align: center;\">Status</th>", html);
        Assert.Contains("<td style=\"text-align: center;\">?</td>", html);
    }

    [Fact]
    public void Render_EscapesHtmlCharacters()
    {
        var table = new TableBuilder()
            .AddHeader("Code")
            .AddRow("<script>alert('xss')</script>");

        var html = new HtmlRenderer().Render(table);

        Assert.Contains("&lt;script&gt;", html);
        Assert.Contains("&#39;", html);
        Assert.DoesNotContain("<script>", html);
    }

    [Fact]
    public void RenderTitle_WithoutUrl_GeneratesH1()
    {
        var html = new HtmlRenderer().RenderTitle("Test Report");

        Assert.Contains("<h1>Test Report</h1>", html);
        Assert.DoesNotContain("<a href=", html);
    }

    [Fact]
    public void RenderTitle_WithUrl_GeneratesH1AndLink()
    {
        var html = new HtmlRenderer().RenderTitle("Test Report", "https://example.com");

        Assert.Contains("<h1>Test Report</h1>", html);
        Assert.Contains("<a href=\"https://example.com\">https://example.com</a>", html);
    }

    [Fact]
    public void RenderSectionHeader_GeneratesH2()
    {
        Assert.Contains("<h2>Results</h2>", new HtmlRenderer().RenderSectionHeader("Results"));
    }

    [Fact]
    public void RenderText_WrapsInParagraph()
    {
        Assert.Contains("<p>Summary text</p>", new HtmlRenderer().RenderText("Summary text"));
    }

    [Fact]
    public void CombineParts_CreatesCompleteHtmlDocument()
    {
        var html = new HtmlRenderer().CombineParts(["<h1>Title</h1>", "<p>Content</p>"]);

        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("<html>", html);
        Assert.Contains("<head>", html);
        Assert.Contains("<style>", html);
        Assert.Contains("<body>", html);
        Assert.Contains("</html>", html);
        Assert.Contains("<h1>Title</h1>", html);
        Assert.Contains("<p>Content</p>", html);
    }

    [Fact]
    public void CombineParts_SkipsEmptyParts()
    {
        var html = new HtmlRenderer().CombineParts(["<h1>Title</h1>", "", "   ", "<p>Content</p>"]);

        Assert.Contains("<h1>Title</h1>", html);
        Assert.Contains("<p>Content</p>", html);
    }

    [Fact]
    public void TableBuilder_BuildWith_HtmlRenderer_Works()
    {
        var table = new TableBuilder()
            .AddHeader("Feature")
            .AddHeader("Status", ColumnAlignment.Center)
            .AddRow("Authentication", "?")
            .AddRow("API Integration", "?");

        var html = table.BuildWith(new HtmlRenderer());

        Assert.Contains("<table>", html);
        Assert.Contains("Authentication", html);
    }

    [Fact]
    public void ReportBuilder_GenerateWith_HtmlRenderer_Works()
    {
        var data = new[] { "Item1", "Item2" };
        var report = new ReportBuilder<string>()
            .WithTitle("Test Report")
            .WithReferenceUrl("https://example.com")
            .AddTable(
                (table, items) =>
                {
                    table.AddHeader("Name");
                    foreach (var item in items)
                        table.AddRow(item);
                    return table;
                }
            );

        var html = report.GenerateWith(data, new HtmlRenderer());

        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("<h1>Test Report</h1>", html);
        Assert.Contains("https://example.com", html);
        Assert.Contains("<table>", html);
        Assert.Contains("Item1", html);
        Assert.Contains("Item2", html);
    }

    [Fact]
    public void ReportBuilder_GenerateWith_MultipleRenderers_IncludesHtml()
    {
        var data = new[] { "Item1" };
        var report = new ReportBuilder<string>()
            .WithTitle("Test Report")
            .AddTable(
                (table, items) =>
                {
                    table.AddHeader("Name");
                    foreach (var item in items)
                        table.AddRow(item);
                    return table;
                }
            );

        var outputs = report.GenerateWith(
            data,
            new ConsoleRenderer(),
            new MarkdownRenderer(),
            new HtmlRenderer()
        );

        Assert.Equal(3, outputs.Count);
        Assert.True(outputs.ContainsKey("Console"));
        Assert.True(outputs.ContainsKey("Markdown"));
        Assert.True(outputs.ContainsKey("HTML"));
        Assert.Contains("<!DOCTYPE html>", outputs["HTML"]);
    }
}

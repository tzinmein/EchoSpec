using System.Collections.Generic;
using EchoSpec;
using EchoSpec.Builders;
using Xunit;

namespace EchoSpec.Tests;

public class HtmlRendererTests
{
    [Fact]
    public void Name_ReturnsHTML()
    {
        // Arrange
        var renderer = new HtmlRenderer();

        // Act
        var name = renderer.Name;

        // Assert
        Assert.Equal("HTML", name);
    }

    [Fact]
    public void Render_SimpleTable_GeneratesValidHtml()
    {
        // Arrange
        var renderer = new HtmlRenderer();
        var table = new TableBuilder()
            .AddHeader("Name")
            .AddHeader("Score", ColumnAlignment.Right)
            .AddRow("Alice", "95")
            .AddRow("Bob", "87");

        // Act
        var html = renderer.Render(table);

        // Assert
        Assert.Contains("<table>", html);
        Assert.Contains("<thead>", html);
        Assert.Contains("<tbody>", html);
        Assert.Contains("<th>Name</th>", html);
        Assert.Contains("<th style=\"text-align: right;\">Score</th>", html);
        Assert.Contains("<td>Alice</td>", html);
        Assert.Contains("<td style=\"text-align: right;\">95</td>", html);
    }

    [Fact]
    public void Render_EmptyTable_ReturnsEmptyString()
    {
        // Arrange
        var renderer = new HtmlRenderer();
        var table = new TableBuilder()
            .AddHeader("Name")
            .AddHeader("Score");

        // Act
        var html = renderer.Render(table);

        // Assert
        Assert.Equal(string.Empty, html);
    }

    [Fact]
    public void Render_WithCenterAlignment_AppliesCenterStyle()
    {
        // Arrange
        var renderer = new HtmlRenderer();
        var table = new TableBuilder()
            .AddHeader("Status", ColumnAlignment.Center)
            .AddRow("?");

        // Act
        var html = renderer.Render(table);

        // Assert
        Assert.Contains("<th style=\"text-align: center;\">Status</th>", html);
        Assert.Contains("<td style=\"text-align: center;\">?</td>", html);
    }

    [Fact]
    public void Render_EscapesHtmlCharacters()
    {
        // Arrange
        var renderer = new HtmlRenderer();
        var table = new TableBuilder()
            .AddHeader("Code")
            .AddRow("<script>alert('xss')</script>");

        // Act
        var html = renderer.Render(table);

        // Assert
        Assert.Contains("&lt;script&gt;", html);
        Assert.DoesNotContain("<script>", html);
    }

    [Fact]
    public void RenderTitle_WithoutUrl_GeneratesH1()
    {
        // Arrange
        var renderer = new HtmlRenderer();

        // Act
        var html = renderer.RenderTitle("Test Report");

        // Assert
        Assert.Contains("<h1>Test Report</h1>", html);
        Assert.DoesNotContain("<a href=", html);
    }

    [Fact]
    public void RenderTitle_WithUrl_GeneratesH1AndLink()
    {
        // Arrange
        var renderer = new HtmlRenderer();

        // Act
        var html = renderer.RenderTitle("Test Report", "https://example.com");

        // Assert
        Assert.Contains("<h1>Test Report</h1>", html);
        Assert.Contains("<a href=\"https://example.com\">https://example.com</a>", html);
    }

    [Fact]
    public void RenderSectionHeader_GeneratesH2()
    {
        // Arrange
        var renderer = new HtmlRenderer();

        // Act
        var html = renderer.RenderSectionHeader("Results");

        // Assert
        Assert.Contains("<h2>Results</h2>", html);
    }

    [Fact]
    public void RenderText_WrapsInParagraph()
    {
        // Arrange
        var renderer = new HtmlRenderer();

        // Act
        var html = renderer.RenderText("Summary text");

        // Assert
        Assert.Contains("<p>Summary text</p>", html);
    }

    [Fact]
    public void CombineParts_CreatesCompleteHtmlDocument()
    {
        // Arrange
        var renderer = new HtmlRenderer();
        var parts = new[] { "<h1>Title</h1>", "<p>Content</p>" };

        // Act
        var html = renderer.CombineParts(parts);

        // Assert
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
        // Arrange
        var renderer = new HtmlRenderer();
        var parts = new[] { "<h1>Title</h1>", "", "   ", "<p>Content</p>" };

        // Act
        var html = renderer.CombineParts(parts);

        // Assert
        Assert.Contains("<h1>Title</h1>", html);
        Assert.Contains("<p>Content</p>", html);
    }

    [Fact]
    public void TableBuilder_BuildWith_HtmlRenderer_Works()
    {
        // Arrange
        var table = new TableBuilder()
            .AddHeader("Feature")
            .AddHeader("Status", ColumnAlignment.Center)
            .AddRow("Authentication", "?")
            .AddRow("API Integration", "?");

        // Act
        var html = (string)table.BuildWith(new HtmlRenderer());

        // Assert
        Assert.Contains("<table>", html);
        Assert.Contains("Authentication", html);
        Assert.Contains("?", html);
    }

    [Fact]
    public void ReportBuilder_GenerateWith_HtmlRenderer_Works()
    {
        // Arrange
        var data = new[] { "Item1", "Item2" };
        var report = new ReportBuilder<string>()
            .WithTitle("Test Report")
            .WithReferenceUrl("https://example.com")
            .AddTable((table, items) =>
            {
                table.AddHeader("Name");
                foreach (var item in items)
                {
                    table.AddRow(item);
                }
                return table;
            });

        // Act
        var html = (string)report.GenerateWith(data, new HtmlRenderer());

        // Assert
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
        // Arrange
        var data = new[] { "Item1" };
        var report = new ReportBuilder<string>()
            .WithTitle("Test Report")
            .AddTable((table, items) =>
            {
                table.AddHeader("Name");
                foreach (var item in items)
                {
                    table.AddRow(item);
                }
                return table;
            });

        // Act
        var outputs = (Dictionary<string, string>)report.GenerateWith(
            data,
            new ConsoleRenderer(),
            new MarkdownRenderer(),
            new HtmlRenderer()
        );

        // Assert
        Assert.Equal(3, outputs.Count);
        Assert.True(outputs.ContainsKey("Console"));
        Assert.True(outputs.ContainsKey("Markdown"));
        Assert.True(outputs.ContainsKey("HTML"));
        Assert.Contains("<!DOCTYPE html>", outputs["HTML"]);
    }
}

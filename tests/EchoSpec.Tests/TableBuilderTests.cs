using System;
using System.Collections.Generic;
using EchoSpec.Builders;
using Xunit;

namespace EchoSpec.Tests;

public class TableBuilderTests
{
    [Fact]
    public void AddHeader_AddsHeadersCorrectly()
    {
        var table = new TableBuilder().AddHeader("Name").AddHeader("Score", ColumnAlignment.Right);

        Assert.Equal(new[] { "Name", "Score" }, table.Headers);
        Assert.Equal(new[] { ColumnAlignment.Left, ColumnAlignment.Right }, table.Alignments);
    }

    [Fact]
    public void AddRow_AddsRowsCorrectly()
    {
        var table = new TableBuilder()
            .AddHeader("Name")
            .AddHeader("Score")
            .AddRow("Alice", 95)
            .AddRow("Bob", 87);

        Assert.Equal(2, table.Rows.Count);
        Assert.Equal(new[] { "Alice", "95" }, table.Rows[0]);
        Assert.Equal(new[] { "Bob", "87" }, table.Rows[1]);
    }

    [Fact]
    public void AddRow_PadsShortRows_ByDefault()
    {
        var table = new TableBuilder().AddHeader("A").AddHeader("B").AddHeader("C").AddRow("only");

        Assert.Equal(new[] { "only", "", "" }, table.Rows[0]);
    }

    [Fact]
    public void AddRow_TruncatesLongRows_ByDefault()
    {
        var table = new TableBuilder()
            .AddHeader("A")
            .AddHeader("B")
            .AddRow("1", "2", "3", "4");

        Assert.Equal(new[] { "1", "2" }, table.Rows[0]);
    }

    [Fact]
    public void AddRow_StrictRowWidths_ThrowsOnMismatch()
    {
        var table = new TableBuilder { StrictRowWidths = true }
            .AddHeader("A")
            .AddHeader("B");

        Assert.Throws<ArgumentException>(() => table.AddRow("only"));
    }

    [Fact]
    public void Clear_RemovesAllData()
    {
        var table = new TableBuilder().AddHeader("A").AddRow("1").Clear();

        Assert.Empty(table.Headers);
        Assert.Empty(table.Rows);
    }

    [Fact]
    public void Build_ConsoleFormat_ReturnsExpectedString()
    {
        var table = new TableBuilder()
            .AddHeader("Name")
            .AddHeader("Score", ColumnAlignment.Right)
            .AddRow("Alice", 95)
            .AddRow("Bob", 87);

        var output = table.Build(ReportFormat.Console);
        Assert.Contains("Name", output);
        Assert.Contains("Score", output);
        Assert.Contains("Alice", output);
        Assert.Contains("Bob", output);
    }

    [Fact]
    public void Build_MarkdownFormat_ReturnsExpectedString()
    {
        var table = new TableBuilder()
            .AddHeader("Name")
            .AddHeader("Score", ColumnAlignment.Right)
            .AddRow("Alice", 95)
            .AddRow("Bob", 87);

        var output = table.Build(ReportFormat.Markdown);
        Assert.Contains("| Name | Score |", output);
        Assert.Contains("| Alice | 95 |", output);
        Assert.Contains("| Bob | 87 |", output);
    }

    [Fact]
    public void Build_HtmlFormat_ReturnsTableFragment()
    {
        var table = new TableBuilder().AddHeader("Name").AddRow("Alice");

        var html = table.Build(ReportFormat.Html);
        Assert.Contains("<table>", html);
        Assert.DoesNotContain("<!DOCTYPE", html);
    }

    [Fact]
    public void Build_Both_Throws()
    {
        var table = new TableBuilder().AddHeader("Name").AddRow("Alice");

        Assert.Throws<ArgumentOutOfRangeException>(() => table.Build(ReportFormat.Both));
    }

    [Fact]
    public void Build_All_Throws()
    {
        var table = new TableBuilder().AddHeader("Name").AddRow("Alice");

        Assert.Throws<ArgumentOutOfRangeException>(() => table.Build(ReportFormat.All));
    }

    [Fact]
    public void BuildWith_SingleRenderer_ReturnsString()
    {
        var table = new TableBuilder().AddHeader("Name").AddRow("Alice");

        string html = table.BuildWith(new HtmlRenderer());

        Assert.Contains("<table>", html);
    }

    [Fact]
    public void BuildWith_MultipleRenderers_ReturnsDictionary()
    {
        var table = new TableBuilder().AddHeader("Name").AddRow("Alice");

        IReadOnlyDictionary<string, string> outputs = table.BuildWith(
            new ConsoleRenderer(),
            new MarkdownRenderer()
        );

        Assert.Equal(2, outputs.Count);
        Assert.Contains("Alice", outputs["Console"]);
        Assert.Contains("| Alice |", outputs["Markdown"]);
    }

    [Fact]
    public void BuildWith_DuplicateRendererNames_Throws()
    {
        var table = new TableBuilder().AddHeader("Name").AddRow("Alice");

        Assert.Throws<ArgumentException>(() =>
            table.BuildWith(new MarkdownRenderer(), new MarkdownRenderer())
        );
    }
}

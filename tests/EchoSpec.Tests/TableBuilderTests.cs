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
}

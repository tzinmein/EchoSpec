using EchoSpec.Builders;
using Xunit;

namespace EchoSpec.Tests;

public class RendererTests
{
    [Fact]
    public void ConsoleRenderer_RendersTableWithBoxDrawing()
    {
        var table = new TableBuilder()
            .AddHeader("Name")
            .AddHeader("Score", ColumnAlignment.Right)
            .AddRow("Alice", 95)
            .AddRow("Bob", 87);

        var output = new ConsoleRenderer().Render(table);
        Assert.Contains("Name", output);
        Assert.Contains("Score", output);
        Assert.Contains("Alice", output);
        Assert.Contains("Bob", output);
        Assert.Contains("─", output);
    }

    [Fact]
    public void ConsoleRenderer_EmptyRows_StillRendersHeader()
    {
        var table = new TableBuilder().AddHeader("Name").AddHeader("Score");

        var output = new ConsoleRenderer().Render(table);

        Assert.Contains("Name", output);
        Assert.Contains("Score", output);
        Assert.Contains("─", output);
    }

    [Fact]
    public void MarkdownRenderer_RendersTableWithPipes()
    {
        var table = new TableBuilder()
            .AddHeader("Name")
            .AddHeader("Score", ColumnAlignment.Right)
            .AddRow("Alice", 95)
            .AddRow("Bob", 87);

        var output = new MarkdownRenderer().Render(table);
        Assert.Contains("| Name | Score |", output);
        Assert.Contains("| Alice | 95 |", output);
        Assert.Contains("| Bob | 87 |", output);
    }

    [Fact]
    public void MarkdownRenderer_EscapesPipesAndNewlines()
    {
        var table = new TableBuilder()
            .AddHeader("Note")
            .AddRow("a|b\nc");

        var output = new MarkdownRenderer().Render(table);

        Assert.Contains("| a\\|b<br>c |", output);
    }

    [Fact]
    public void MarkdownRenderer_EmptyRows_StillRendersHeader()
    {
        var table = new TableBuilder().AddHeader("Name").AddHeader("Score");

        var output = new MarkdownRenderer().Render(table);

        Assert.Contains("| Name | Score |", output);
        Assert.Contains("| --- | --- |", output);
    }
}

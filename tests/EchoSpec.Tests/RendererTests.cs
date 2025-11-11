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

        var renderer = new ConsoleRenderer();
        var output = renderer.Render(table);
        Assert.Contains("Name", output);
        Assert.Contains("Score", output);
        Assert.Contains("Alice", output);
        Assert.Contains("Bob", output);
        Assert.Contains("─", output); // Box-drawing character
    }

    [Fact]
    public void MarkdownRenderer_RendersTableWithPipes()
    {
        var table = new TableBuilder()
            .AddHeader("Name")
            .AddHeader("Score", ColumnAlignment.Right)
            .AddRow("Alice", 95)
            .AddRow("Bob", 87);

        var renderer = new MarkdownRenderer();
        var output = renderer.Render(table);
        Assert.Contains("| Name | Score |", output);
        Assert.Contains("| Alice | 95 |", output);
        Assert.Contains("| Bob | 87 |", output);
    }
}

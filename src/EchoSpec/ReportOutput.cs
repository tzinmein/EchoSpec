namespace EchoSpec;

/// <summary>
/// Container for report output in multiple formats.
/// </summary>
public class ReportOutput
{
    /// <summary>
    /// Console-formatted text output (if generated).
    /// </summary>
    public string? ConsoleText { get; init; }

    /// <summary>
    /// Markdown-formatted text output (if generated).
    /// </summary>
    public string? MarkdownText { get; init; }

    /// <summary>
    /// Writes console text to standard output.
    /// </summary>
    public void WriteToConsole()
    {
        if (ConsoleText != null)
        {
            Console.WriteLine(ConsoleText);
        }
    }

    /// <summary>
    /// Saves markdown text to a file.
    /// </summary>
    public void SaveMarkdown(string filePath)
    {
        if (MarkdownText != null)
        {
            File.WriteAllText(filePath, MarkdownText);
        }
    }
}

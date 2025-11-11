namespace EchoSpec.Sections;

/// <summary>
/// Custom section implementation.
/// </summary>
internal class CustomSection<T>(
    Func<IEnumerable<T>, string> consoleFormatter,
    Func<IEnumerable<T>, string> markdownFormatter,
    string? title
) : IReportSection<T>
{
    private readonly Func<IEnumerable<T>, IReportRenderer, string> _formatter = (data, renderer) =>
        renderer.Name switch
        {
            "Console" => consoleFormatter(data),
            "Markdown" => markdownFormatter(data),
            _ => markdownFormatter(data),
        };

    public string Generate(IEnumerable<T> data, IReportRenderer renderer)
    {
        var parts = new List<string>();

        if (title != null)
        {
            parts.Add(renderer.RenderSectionHeader(title));
        }

        parts.Add(renderer.RenderText(_formatter(data, renderer)));

        return string.Concat(parts);
    }
}

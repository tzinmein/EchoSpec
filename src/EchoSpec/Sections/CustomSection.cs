namespace EchoSpec.Sections;

/// <summary>
/// Custom section with per-renderer or shared plain-text formatters.
/// </summary>
internal class CustomSection<T> : IReportSection<T>
{
    private readonly Func<IEnumerable<T>, string>? _plainFormatter;
    private readonly Func<IEnumerable<T>, string>? _consoleFormatter;
    private readonly Func<IEnumerable<T>, string>? _markdownFormatter;
    private readonly Func<IEnumerable<T>, string>? _htmlFormatter;
    private readonly string? _title;

    public CustomSection(
        Func<IEnumerable<T>, string> consoleFormatter,
        Func<IEnumerable<T>, string> markdownFormatter,
        Func<IEnumerable<T>, string> htmlFormatter,
        string? title
    )
    {
        _consoleFormatter = consoleFormatter;
        _markdownFormatter = markdownFormatter;
        _htmlFormatter = htmlFormatter;
        _title = title;
    }

    public CustomSection(Func<IEnumerable<T>, string> plainFormatter, string? title)
    {
        _plainFormatter = plainFormatter;
        _title = title;
    }

    public IPreparedSection Prepare(IReadOnlyList<T> data)
    {
        string? plain = _plainFormatter?.Invoke(data);
        string? console = _consoleFormatter?.Invoke(data);
        string? markdown = _markdownFormatter?.Invoke(data);
        string? html = _htmlFormatter?.Invoke(data);

        return new PreparedCustomSection(_title, plain, console, markdown, html);
    }

    private sealed class PreparedCustomSection(
        string? title,
        string? plain,
        string? console,
        string? markdown,
        string? html
    ) : IPreparedSection
    {
        public string Render(IReportRenderer renderer)
        {
            var content = renderer.Name switch
            {
                "Console" => console ?? plain ?? string.Empty,
                "Markdown" => markdown ?? plain ?? string.Empty,
                "HTML" => html ?? plain ?? string.Empty,
                _ => plain ?? markdown ?? console ?? html ?? string.Empty,
            };

            if (title == null)
                return renderer.RenderText(content);

            return string.Concat(renderer.RenderSectionHeader(title), renderer.RenderText(content));
        }
    }
}

using EchoSpec.Sections;

namespace EchoSpec.Builders;

/// <summary>
/// Generic report builder for creating formatted reports with tables and sections.
/// </summary>
/// <typeparam name="T">The type of data items in the report.</typeparam>
public sealed class ReportBuilder<T>
{
    private readonly List<IReportSection<T>> _sections = [];
    private string _title = "Report";
    private string? _referenceUrl;

    /// <summary>
    /// Sets the report title.
    /// </summary>
    /// <param name="title">The title of the report.</param>
    /// <returns>The current <see cref="ReportBuilder{T}"/> instance.</returns>
    public ReportBuilder<T> WithTitle(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>
    /// Sets a reference URL to display in the report.
    /// </summary>
    /// <param name="url">The reference URL.</param>
    /// <returns>The current <see cref="ReportBuilder{T}"/> instance.</returns>
    public ReportBuilder<T> WithReferenceUrl(string url)
    {
        _referenceUrl = url;
        return this;
    }

    /// <summary>
    /// Adds a table section to the report.
    /// </summary>
    /// <param name="configure">Configures the table from the report data.</param>
    /// <param name="sectionTitle">Optional section title.</param>
    /// <returns>The current <see cref="ReportBuilder{T}"/> instance.</returns>
    public ReportBuilder<T> AddTable(
        Action<TableBuilder, IEnumerable<T>> configure,
        string? sectionTitle = null
    )
    {
        _sections.Add(new TableSection<T>(configure, sectionTitle));
        return this;
    }

    /// <summary>
    /// Adds a custom text section using the same plain content for every renderer.
    /// Each renderer applies its own <see cref="IReportRenderer.RenderText"/> formatting.
    /// </summary>
    /// <param name="contentFormatter">Produces plain text content from the data.</param>
    /// <param name="sectionTitle">Optional section title.</param>
    /// <returns>The current <see cref="ReportBuilder{T}"/> instance.</returns>
    public ReportBuilder<T> AddSection(
        Func<IEnumerable<T>, string> contentFormatter,
        string? sectionTitle = null
    )
    {
        _sections.Add(new CustomSection<T>(contentFormatter, sectionTitle));
        return this;
    }

    /// <summary>
    /// Adds a custom text section with Console, Markdown, and HTML formatters.
    /// Other custom renderers fall back to plain-style preference: markdown, then console,
    /// then <see cref="IReportRenderer.RenderText"/>.
    /// </summary>
    /// <param name="consoleFormatter">Formatter for console output.</param>
    /// <param name="markdownFormatter">Formatter for markdown output.</param>
    /// <param name="htmlFormatter">Formatter for HTML output (plain text; HTML renderer escapes and wraps).</param>
    /// <param name="sectionTitle">Optional section title.</param>
    /// <returns>The current <see cref="ReportBuilder{T}"/> instance.</returns>
    public ReportBuilder<T> AddSection(
        Func<IEnumerable<T>, string> consoleFormatter,
        Func<IEnumerable<T>, string> markdownFormatter,
        Func<IEnumerable<T>, string> htmlFormatter,
        string? sectionTitle = null
    )
    {
        _sections.Add(
            new CustomSection<T>(consoleFormatter, markdownFormatter, htmlFormatter, sectionTitle)
        );
        return this;
    }

    /// <summary>
    /// Generates the report with a single renderer.
    /// </summary>
    /// <param name="data">The data to include in the report.</param>
    /// <param name="renderer">The renderer to use.</param>
    /// <returns>The rendered report string.</returns>
    public string GenerateWith(IEnumerable<T> data, IReportRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(renderer);

        var dataList = data as IReadOnlyList<T> ?? data.ToList();
        var prepared = PrepareSections(dataList);
        return GenerateReport(prepared, renderer);
    }

    /// <summary>
    /// Generates the report with one or more renderers. Section tables and formatters run once.
    /// </summary>
    /// <param name="data">The data to include in the report.</param>
    /// <param name="renderers">
    /// Renderers to use. Each <see cref="IReportRenderer.Name"/> must be unique.
    /// For a single renderer, prefer <see cref="GenerateWith(IEnumerable{T}, IReportRenderer)"/>.
    /// </param>
    /// <returns>A dictionary mapping each renderer <see cref="IReportRenderer.Name"/> to its output.</returns>
    public IReadOnlyDictionary<string, string> GenerateWith(
        IEnumerable<T> data,
        params IReportRenderer[] renderers
    )
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(renderers);
        if (renderers.Length == 0)
            throw new ArgumentException("At least one renderer must be provided.", nameof(renderers));

        var dataList = data as IReadOnlyList<T> ?? data.ToList();
        var prepared = PrepareSections(dataList);

        var results = new Dictionary<string, string>(renderers.Length, StringComparer.Ordinal);
        foreach (var renderer in renderers)
        {
            ArgumentNullException.ThrowIfNull(renderer);
            if (!results.TryAdd(renderer.Name, GenerateReport(prepared, renderer)))
            {
                throw new ArgumentException(
                    $"Duplicate renderer name '{renderer.Name}'. Each renderer Name must be unique.",
                    nameof(renderers)
                );
            }
        }

        return results;
    }

    /// <summary>
    /// Generates the report in the specified format(s).
    /// </summary>
    /// <param name="data">The data to include in the report.</param>
    /// <param name="format">The output format(s) to generate.</param>
    /// <returns>A <see cref="ReportOutput"/> containing the generated report text.</returns>
    public ReportOutput Generate(IEnumerable<T> data, ReportFormat format = ReportFormat.All)
    {
        ArgumentNullException.ThrowIfNull(data);

        var dataList = data as IReadOnlyList<T> ?? data.ToList();
        var prepared = PrepareSections(dataList);

        string? consoleText = null;
        string? markdownText = null;
        string? htmlText = null;

        if (format is ReportFormat.Console or ReportFormat.All)
            consoleText = GenerateReport(prepared, new ConsoleRenderer());

        if (format is ReportFormat.Markdown or ReportFormat.All)
            markdownText = GenerateReport(prepared, new MarkdownRenderer());

        if (format is ReportFormat.Html or ReportFormat.All)
            htmlText = GenerateReport(prepared, new HtmlRenderer());

        if (consoleText is null && markdownText is null && htmlText is null)
        {
            throw new ArgumentOutOfRangeException(
                nameof(format),
                format,
                "Unknown report format."
            );
        }

        return new ReportOutput
        {
            ConsoleText = consoleText,
            MarkdownText = markdownText,
            HtmlText = htmlText,
        };
    }

    private List<IPreparedSection> PrepareSections(IReadOnlyList<T> data)
    {
        var prepared = new List<IPreparedSection>(_sections.Count);
        foreach (var section in _sections)
            prepared.Add(section.Prepare(data));
        return prepared;
    }

    private string GenerateReport(IReadOnlyList<IPreparedSection> sections, IReportRenderer renderer)
    {
        var parts = new List<string>(sections.Count + 1)
        {
            renderer.RenderTitle(_title, _referenceUrl),
        };

        foreach (var section in sections)
        {
            var sectionText = section.Render(renderer);
            if (!string.IsNullOrWhiteSpace(sectionText))
                parts.Add(sectionText);
        }

        return renderer.CombineParts(parts);
    }
}

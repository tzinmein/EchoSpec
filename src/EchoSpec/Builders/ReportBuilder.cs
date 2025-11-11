using EchoSpec.Sections;

namespace EchoSpec.Builders;

/// <summary>
/// Generic report builder for creating formatted reports with tables and sections.
/// </summary>
/// <typeparam name="T">The type of data items in the report.</typeparam>
public class ReportBuilder<T>
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
    /// <param name="tableBuilder">A function to configure the table.</param>
    /// <param name="sectionTitle">Optional section title.</param>
    /// <returns>The current <see cref="ReportBuilder{T}"/> instance.</returns>
    public ReportBuilder<T> AddTable(
        Func<TableBuilder, IEnumerable<T>, TableBuilder> tableBuilder,
        string? sectionTitle = null
    )
    {
        _sections.Add(new TableSection<T>(tableBuilder, sectionTitle));
        return this;
    }

    /// <summary>
    /// Adds a custom text section to the report.
    /// </summary>
    /// <param name="consoleFormatter">Formatter for console output.</param>
    /// <param name="markdownFormatter">Formatter for markdown output.</param>
    /// <param name="sectionTitle">Optional section title.</param>
    /// <returns>The current <see cref="ReportBuilder{T}"/> instance.</returns>
    public ReportBuilder<T> AddSection(
        Func<IEnumerable<T>, string> consoleFormatter,
        Func<IEnumerable<T>, string> markdownFormatter,
        string? sectionTitle = null
    )
    {
        _sections.Add(new CustomSection<T>(consoleFormatter, markdownFormatter, sectionTitle));
        return this;
    }

    /// <summary>
    /// Generates the report using one or more renderers.
    /// </summary>
    /// <param name="data">The data to include in the report.</param>
    /// <param name="renderers">One or more renderers to use for generating output.</param>
    /// <returns>If a single renderer is provided, returns the rendered string. If multiple renderers are provided, returns a dictionary mapping renderer names to their output.</returns>
    public object GenerateWith(IEnumerable<T> data, params IReportRenderer[] renderers)
    {
        if (renderers.Length == 0)
            throw new ArgumentException(
                "At least one renderer must be provided.",
                nameof(renderers)
            );

        var dataList = data.ToList();

        if (renderers.Length == 1)
        {
            return GenerateReport(dataList, renderers[0]);
        }

        var results = new Dictionary<string, string>();
        foreach (var renderer in renderers)
        {
            results[renderer.Name] = GenerateReport(dataList, renderer);
        }
        return results;
    }

    /// <summary>
    /// Generates the report in the specified format(s).
    /// </summary>
    /// <param name="data">The data to include in the report.</param>
    /// <param name="format">The output format(s) to generate.</param>
    /// <returns>A <see cref="ReportOutput"/> containing the generated report text.</returns>
    public ReportOutput Generate(IEnumerable<T> data, ReportFormat format = ReportFormat.Both)
    {
        var dataList = data.ToList();
        string? consoleText = null;
        string? markdownText = null;

        if (format == ReportFormat.Console || format == ReportFormat.Both)
        {
            consoleText = GenerateReport(dataList, new ConsoleRenderer());
        }

        if (format == ReportFormat.Markdown || format == ReportFormat.Both)
        {
            markdownText = GenerateReport(dataList, new MarkdownRenderer());
        }

        return new ReportOutput { ConsoleText = consoleText, MarkdownText = markdownText };
    }

    private string GenerateReport(List<T> data, IReportRenderer renderer)
    {
        var parts = new List<string> { renderer.RenderTitle(_title, _referenceUrl) };

        foreach (var section in _sections)
        {
            var sectionText = section.Generate(data, renderer);
            if (!string.IsNullOrWhiteSpace(sectionText))
            {
                parts.Add(sectionText);
            }
        }

        return renderer.CombineParts(parts);
    }
}

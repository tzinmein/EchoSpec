namespace EchoSpec.Builders;

/// <summary>
/// Unified table builder that can output to different sinks.
/// </summary>
public class TableBuilder : ITable
{
    private readonly List<string> _headers = [];
    private readonly List<ColumnAlignment> _alignments = [];
    private readonly List<List<string>> _rows = [];

    /// <summary>
    /// Gets the column headers.
    /// </summary>
    public IReadOnlyList<string> Headers => _headers;

    /// <summary>
    /// Gets the column alignments.
    /// </summary>
    public IReadOnlyList<ColumnAlignment> Alignments => _alignments;

    /// <summary>
    /// Gets the data rows.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<string>> Rows => _rows;

    /// <summary>
    /// When <see langword="true"/>, <see cref="AddRow(object[])"/> and
    /// <see cref="AddRow(string[])"/> throw if the value count does not match
    /// the current header count. When <see langword="false"/> (default), rows are
    /// padded with empty strings or truncated to the header width.
    /// </summary>
    public bool StrictRowWidths { get; set; }

    /// <summary>
    /// Adds a header column.
    /// </summary>
    /// <param name="header">The column header text.</param>
    /// <param name="alignment">The column alignment.</param>
    /// <returns>The current <see cref="TableBuilder"/> instance.</returns>
    public TableBuilder AddHeader(string header, ColumnAlignment alignment = ColumnAlignment.Left)
    {
        _headers.Add(header);
        _alignments.Add(alignment);
        return this;
    }

    /// <summary>
    /// Adds a data row, converting each value with <see cref="object.ToString"/>.
    /// Prefer <see cref="AddRow(string[])"/> when values are already strings.
    /// </summary>
    /// <param name="values">The values for the row.</param>
    /// <returns>The current <see cref="TableBuilder"/> instance.</returns>
    public TableBuilder AddRow(params object[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var row = new List<string>(values.Length);
        foreach (var value in values)
            row.Add(value?.ToString() ?? string.Empty);

        return AddNormalizedRow(row);
    }

    /// <summary>
    /// Adds a data row of string cells without boxing.
    /// </summary>
    /// <param name="values">The values for the row.</param>
    /// <returns>The current <see cref="TableBuilder"/> instance.</returns>
    public TableBuilder AddRow(params string[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var row = new List<string>(values.Length);
        row.AddRange(values);
        return AddNormalizedRow(row);
    }

    private TableBuilder AddNormalizedRow(List<string> row)
    {
        if (_headers.Count > 0)
        {
            if (StrictRowWidths && row.Count != _headers.Count)
            {
                throw new ArgumentException(
                    $"Row has {row.Count} value(s) but table has {_headers.Count} column(s).",
                    nameof(row)
                );
            }

            while (row.Count < _headers.Count)
                row.Add(string.Empty);

            if (row.Count > _headers.Count)
                row.RemoveRange(_headers.Count, row.Count - _headers.Count);
        }

        _rows.Add(row);
        return this;
    }

    /// <summary>
    /// Builds the table with a single renderer.
    /// </summary>
    /// <param name="renderer">The renderer to use.</param>
    /// <returns>The rendered table string.</returns>
    public string BuildWith(ITableRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        EnsureHeaders();
        return renderer.Render(this);
    }

    /// <summary>
    /// Builds the table with one or more renderers.
    /// </summary>
    /// <param name="renderers">
    /// Renderers to use. Each <see cref="ITableRenderer.Name"/> must be unique.
    /// For a single renderer, prefer <see cref="BuildWith(ITableRenderer)"/> which returns <see cref="string"/>.
    /// </param>
    /// <returns>A dictionary mapping each renderer <see cref="ITableRenderer.Name"/> to its output.</returns>
    public IReadOnlyDictionary<string, string> BuildWith(params ITableRenderer[] renderers)
    {
        ArgumentNullException.ThrowIfNull(renderers);
        if (renderers.Length == 0)
            throw new ArgumentException("At least one renderer must be provided.", nameof(renderers));

        EnsureHeaders();

        var results = new Dictionary<string, string>(renderers.Length, StringComparer.Ordinal);
        foreach (var renderer in renderers)
        {
            ArgumentNullException.ThrowIfNull(renderer);
            if (!results.TryAdd(renderer.Name, renderer.Render(this)))
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
    /// Builds the table in the specified format.
    /// </summary>
    /// <param name="format">
    /// <see cref="ReportFormat.Console"/>, <see cref="ReportFormat.Markdown"/>, or
    /// <see cref="ReportFormat.Html"/>. <see cref="ReportFormat.All"/> is not supported —
    /// use <see cref="BuildWith(ITableRenderer[])"/>.
    /// </param>
    /// <returns>The formatted table as a string.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="format"/> is <see cref="ReportFormat.All"/> or unknown.
    /// </exception>
    public string Build(ReportFormat format = ReportFormat.Markdown)
    {
        EnsureHeaders();

        ITableRenderer renderer = format switch
        {
            ReportFormat.Console => new ConsoleRenderer(),
            ReportFormat.Markdown => new MarkdownRenderer(),
            ReportFormat.Html => new HtmlRenderer(),
            ReportFormat.All => throw new ArgumentOutOfRangeException(
                nameof(format),
                format,
                "ReportFormat.All is not supported on TableBuilder.Build. Use BuildWith with multiple renderers, or call Build once per format."
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown report format."),
        };

        return renderer.Render(this);
    }

    /// <summary>
    /// Clears all data from the builder.
    /// </summary>
    /// <returns>The current <see cref="TableBuilder"/> instance.</returns>
    public TableBuilder Clear()
    {
        _headers.Clear();
        _alignments.Clear();
        _rows.Clear();
        return this;
    }

    private void EnsureHeaders()
    {
        if (_headers.Count == 0)
            throw new InvalidOperationException("At least one header must be added.");
    }
}

using System.Text;

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
    /// Adds a data row.
    /// </summary>
    /// <param name="values">The values for the row.</param>
    /// <returns>The current <see cref="TableBuilder"/> instance.</returns>
    public TableBuilder AddRow(params object[] values)
    {
        var row = values.Select(v => v?.ToString() ?? string.Empty).ToList();
        _rows.Add(row);
        return this;
    }

    /// <summary>
    /// Builds the table using one or more renderers.
    /// </summary>
    /// <param name="renderers">One or more renderers to use for generating output.</param>
    /// <returns>If a single renderer is provided, returns the rendered string. If multiple renderers are provided, returns a dictionary mapping renderer names to their output.</returns>
    public object BuildWith(params ITableRenderer[] renderers)
    {
        if (_headers.Count == 0)
            throw new InvalidOperationException("At least one header must be added.");

        if (renderers.Length == 0)
            throw new ArgumentException(
                "At least one renderer must be provided.",
                nameof(renderers)
            );

        if (renderers.Length == 1)
        {
            return renderers[0].Render(this);
        }

        var results = new Dictionary<string, string>();
        foreach (var renderer in renderers)
        {
            results[renderer.Name] = renderer.Render(this);
        }
        return results;
    }

    /// <summary>
    /// Builds the table in the specified format.
    /// </summary>
    /// <param name="format">The output format to generate.</param>
    /// <returns>The formatted table as a string.</returns>
    public string Build(ReportFormat format = ReportFormat.Markdown)
    {
        if (_headers.Count == 0)
            throw new InvalidOperationException("At least one header must be added.");

        ITableRenderer renderer = format switch
        {
            ReportFormat.Console => new ConsoleRenderer(),
            ReportFormat.Markdown => new MarkdownRenderer(),
            _ => new MarkdownRenderer(),
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
}

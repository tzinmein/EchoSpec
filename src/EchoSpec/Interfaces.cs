namespace EchoSpec;

/// <summary>
/// Represents a table of data that can be rendered to different output formats.
/// </summary>
public interface ITable
{
    /// <summary>
    /// Gets the column headers.
    /// </summary>
    IReadOnlyList<string> Headers { get; }

    /// <summary>
    /// Gets the column alignments.
    /// </summary>
    IReadOnlyList<ColumnAlignment> Alignments { get; }

    /// <summary>
    /// Gets the data rows.
    /// </summary>
    IReadOnlyList<IReadOnlyList<string>> Rows { get; }
}

/// <summary>
/// Column alignment options.
/// </summary>
public enum ColumnAlignment
{
    /// <summary>Left aligned.</summary>
    Left,

    /// <summary>Center aligned.</summary>
    Center,

    /// <summary>Right aligned.</summary>
    Right,
}

/// <summary>
/// Interface for rendering tables to specific output formats.
/// </summary>
public interface ITableRenderer
{
    /// <summary>
    /// Gets the name of this renderer (e.g., "Console", "Markdown", "HTML").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Renders a table to this renderer's output format.
    /// </summary>
    /// <param name="table">The table to render.</param>
    /// <returns>The formatted output string.</returns>
    string Render(ITable table);
}

/// <summary>
/// Interface for rendering complete reports to specific output formats.
/// </summary>
public interface IReportRenderer
{
    /// <summary>
    /// Gets the name of this renderer (e.g., "Console", "Markdown", "HTML").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Renders a report title/header.
    /// </summary>
    string RenderTitle(string title, string? referenceUrl = null);

    /// <summary>
    /// Renders a section header.
    /// </summary>
    string RenderSectionHeader(string sectionTitle);

    /// <summary>
    /// Renders a table.
    /// </summary>
    string RenderTable(ITable table);

    /// <summary>
    /// Renders custom text content.
    /// </summary>
    string RenderText(string text);

    /// <summary>
    /// Combines multiple rendered parts into a complete report.
    /// </summary>
    string CombineParts(IEnumerable<string> parts);
}

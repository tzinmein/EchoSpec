using System.Text;

namespace EchoSpec;

/// <summary>
/// Renders tables in console format with box-drawing characters.
/// </summary>
public class ConsoleRenderer : ITableRenderer, IReportRenderer
{
    /// <inheritdoc/>
    public string Name => "Console";

    /// <inheritdoc/>
    public string Render(ITable table)
    {
        if (table.Rows.Count == 0)
            return string.Empty;

        // Calculate column widths
        var columnWidths = new int[table.Headers.Count];
        for (int i = 0; i < table.Headers.Count; i++)
        {
            columnWidths[i] = table.Headers[i].Length;
        }

        foreach (var row in table.Rows)
        {
            for (int i = 0; i < Math.Min(row.Count, columnWidths.Length); i++)
            {
                columnWidths[i] = Math.Max(columnWidths[i], row[i].Length);
            }
        }

        var sb = new StringBuilder();

        // Header row
        for (int i = 0; i < table.Headers.Count; i++)
        {
            var alignment = i < table.Alignments.Count ? table.Alignments[i] : ColumnAlignment.Left;
            sb.Append(FormatCell(table.Headers[i], columnWidths[i], alignment));
            if (i < table.Headers.Count - 1)
                sb.Append("  ");
        }
        sb.AppendLine();

        // Separator
        sb.AppendLine(new string('─', columnWidths.Sum() + (table.Headers.Count - 1) * 2));

        // Data rows
        foreach (var row in table.Rows)
        {
            for (int i = 0; i < table.Headers.Count; i++)
            {
                var value = i < row.Count ? row[i] : string.Empty;
                var alignment =
                    i < table.Alignments.Count ? table.Alignments[i] : ColumnAlignment.Left;
                sb.Append(FormatCell(value, columnWidths[i], alignment));
                if (i < table.Headers.Count - 1)
                    sb.Append("  ");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string FormatCell(string value, int width, ColumnAlignment alignment)
    {
        return alignment switch
        {
            ColumnAlignment.Right => value.PadLeft(width),
            ColumnAlignment.Center => value.PadLeft((width + value.Length) / 2).PadRight(width),
            _ => value.PadRight(width),
        };
    }

    /// <inheritdoc/>
    public string RenderTitle(string title, string? referenceUrl = null)
    {
        var sb = new StringBuilder();
        var boxWidth = Math.Max(Math.Max(title.Length + 4, referenceUrl?.Length + 4 ?? 0), 80);
        var titlePadding = (boxWidth - title.Length - 2) / 2;

        sb.AppendLine("╔" + new string('═', boxWidth - 2) + "╗");
        sb.AppendLine(
            "║"
                + new string(' ', titlePadding)
                + title
                + new string(' ', boxWidth - titlePadding - title.Length - 2)
                + "║"
        );

        if (referenceUrl != null)
        {
            var urlPadding = (boxWidth - referenceUrl.Length - 2) / 2;
            sb.AppendLine(
                "║"
                    + new string(' ', urlPadding)
                    + referenceUrl
                    + new string(' ', boxWidth - urlPadding - referenceUrl.Length - 2)
                    + "║"
            );
        }

        sb.AppendLine("╚" + new string('═', boxWidth - 2) + "╝");
        sb.AppendLine();

        return sb.ToString();
    }

    /// <inheritdoc/>
    public string RenderSectionHeader(string sectionTitle)
    {
        var sb = new StringBuilder();
        sb.AppendLine(sectionTitle);
        sb.AppendLine();
        return sb.ToString();
    }

    /// <inheritdoc/>
    public string RenderTable(ITable table) => Render(table);

    /// <inheritdoc/>
    public string RenderText(string text)
    {
        return text;
    }

    /// <inheritdoc/>
    public string CombineParts(IEnumerable<string> parts)
    {
        return string.Join(Environment.NewLine, parts.Where(p => !string.IsNullOrWhiteSpace(p)));
    }
}

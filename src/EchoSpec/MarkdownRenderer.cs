using System.Text;

namespace EchoSpec;

/// <summary>
/// Renders tables in Markdown format with pipe-delimited tables.
/// </summary>
public class MarkdownRenderer : ITableRenderer, IReportRenderer
{
    /// <inheritdoc/>
    public string Name => "Markdown";

    /// <inheritdoc/>
    public string Render(ITable table)
    {
        var sb = new StringBuilder();

        // Header row
        sb.Append("| ");
        sb.AppendJoin(" | ", table.Headers);
        sb.AppendLine(" |");

        // Separator row with alignment
        sb.Append("| ");
        for (int i = 0; i < table.Headers.Count; i++)
        {
            var alignment = i < table.Alignments.Count ? table.Alignments[i] : ColumnAlignment.Left;
            var separator = alignment switch
            {
                ColumnAlignment.Center => ":---:",
                ColumnAlignment.Right => "---:",
                _ => "---",
            };

            if (i > 0)
                sb.Append(" | ");
            sb.Append(separator);
        }
        sb.AppendLine(" |");

        // Data rows
        foreach (var row in table.Rows)
        {
            sb.Append("| ");

            var paddedRow = new List<string>(row);
            while (paddedRow.Count < table.Headers.Count)
                paddedRow.Add(string.Empty);

            sb.AppendJoin(" | ", paddedRow.Take(table.Headers.Count));
            sb.AppendLine(" |");
        }

        return sb.ToString();
    }

    /// <inheritdoc/>
    public string RenderTitle(string title, string? referenceUrl = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {title}");
        sb.AppendLine();

        if (referenceUrl != null)
        {
            sb.AppendLine($"Reference: {referenceUrl}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <inheritdoc/>
    public string RenderSectionHeader(string sectionTitle)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"## {sectionTitle}");
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

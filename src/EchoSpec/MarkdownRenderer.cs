using System.Text;

namespace EchoSpec;

/// <summary>
/// Renders tables in Markdown format with pipe-delimited tables.
/// </summary>
public sealed class MarkdownRenderer : ITableRenderer, IReportRenderer
{
    /// <inheritdoc/>
    public string Name => "Markdown";

    /// <inheritdoc/>
    public string Render(ITable table)
    {
        var sb = new StringBuilder();

        sb.Append("| ");
        for (int i = 0; i < table.Headers.Count; i++)
        {
            if (i > 0)
                sb.Append(" | ");
            AppendEscapedCell(sb, table.Headers[i]);
        }
        sb.AppendLine(" |");

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

        foreach (var row in table.Rows)
        {
            sb.Append("| ");
            for (int i = 0; i < table.Headers.Count; i++)
            {
                if (i > 0)
                    sb.Append(" | ");
                var value = i < row.Count ? row[i] : string.Empty;
                AppendEscapedCell(sb, value);
            }
            sb.AppendLine(" |");
        }

        return sb.ToString();
    }

    /// <inheritdoc/>
    public string RenderTitle(string title, string? referenceUrl = null)
    {
        var sb = new StringBuilder();
        sb.Append("# ");
        sb.AppendLine(title);
        sb.AppendLine();

        if (referenceUrl != null)
        {
            sb.Append("Reference: ");
            sb.AppendLine(referenceUrl);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <inheritdoc/>
    public string RenderSectionHeader(string sectionTitle)
    {
        var sb = new StringBuilder();
        sb.Append("## ");
        sb.AppendLine(sectionTitle);
        sb.AppendLine();
        return sb.ToString();
    }

    /// <inheritdoc/>
    public string RenderTable(ITable table) => Render(table);

    /// <inheritdoc/>
    public string RenderText(string text) => text;

    /// <inheritdoc/>
    public string CombineParts(IEnumerable<string> parts)
    {
        var sb = new StringBuilder();
        var first = true;
        foreach (var part in parts)
        {
            if (string.IsNullOrWhiteSpace(part))
                continue;
            if (!first)
                sb.AppendLine();
            sb.Append(part);
            first = false;
        }
        return sb.ToString();
    }

    private static void AppendEscapedCell(StringBuilder sb, string value)
    {
        foreach (var c in value)
        {
            switch (c)
            {
                case '|':
                    sb.Append("\\|");
                    break;
                case '\r':
                    break;
                case '\n':
                    sb.Append("<br>");
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }
    }
}

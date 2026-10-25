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
        var columnWidths = new int[table.Headers.Count];
        for (int i = 0; i < table.Headers.Count; i++)
            columnWidths[i] = table.Headers[i].Length;

        foreach (var row in table.Rows)
        {
            for (int i = 0; i < Math.Min(row.Count, columnWidths.Length); i++)
                columnWidths[i] = Math.Max(columnWidths[i], row[i].Length);
        }

        var totalWidth = 0;
        for (int i = 0; i < columnWidths.Length; i++)
            totalWidth += columnWidths[i];
        if (table.Headers.Count > 1)
            totalWidth += (table.Headers.Count - 1) * 2;

        var sb = new StringBuilder();

        for (int i = 0; i < table.Headers.Count; i++)
        {
            var alignment = i < table.Alignments.Count ? table.Alignments[i] : ColumnAlignment.Left;
            AppendCell(sb, table.Headers[i], columnWidths[i], alignment);
            if (i < table.Headers.Count - 1)
                sb.Append("  ");
        }
        sb.AppendLine();

        sb.Append('─', Math.Max(totalWidth, 0));
        sb.AppendLine();

        foreach (var row in table.Rows)
        {
            for (int i = 0; i < table.Headers.Count; i++)
            {
                var value = i < row.Count ? row[i] : string.Empty;
                var alignment =
                    i < table.Alignments.Count ? table.Alignments[i] : ColumnAlignment.Left;
                AppendCell(sb, value, columnWidths[i], alignment);
                if (i < table.Headers.Count - 1)
                    sb.Append("  ");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static void AppendCell(
        StringBuilder sb,
        string value,
        int width,
        ColumnAlignment alignment
    )
    {
        var pad = Math.Max(width - value.Length, 0);
        switch (alignment)
        {
            case ColumnAlignment.Right:
                sb.Append(' ', pad);
                sb.Append(value);
                break;
            case ColumnAlignment.Center:
                var left = pad / 2;
                var right = pad - left;
                sb.Append(' ', left);
                sb.Append(value);
                sb.Append(' ', right);
                break;
            default:
                sb.Append(value);
                sb.Append(' ', pad);
                break;
        }
    }

    /// <inheritdoc/>
    public string RenderTitle(string title, string? referenceUrl = null)
    {
        var sb = new StringBuilder();
        var boxWidth = Math.Max(Math.Max(title.Length + 4, referenceUrl?.Length + 4 ?? 0), 80);
        var titlePadding = (boxWidth - title.Length - 2) / 2;

        sb.Append('╔');
        sb.Append('═', boxWidth - 2);
        sb.AppendLine("╗");

        sb.Append('║');
        sb.Append(' ', titlePadding);
        sb.Append(title);
        sb.Append(' ', boxWidth - titlePadding - title.Length - 2);
        sb.AppendLine("║");

        if (referenceUrl != null)
        {
            var urlPadding = (boxWidth - referenceUrl.Length - 2) / 2;
            sb.Append('║');
            sb.Append(' ', urlPadding);
            sb.Append(referenceUrl);
            sb.Append(' ', boxWidth - urlPadding - referenceUrl.Length - 2);
            sb.AppendLine("║");
        }

        sb.Append('╚');
        sb.Append('═', boxWidth - 2);
        sb.AppendLine("╝");
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
}

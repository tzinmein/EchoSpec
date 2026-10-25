using System.Text;

namespace EchoSpec;

/// <summary>
/// Renders tables and reports in HTML format.
/// </summary>
public sealed class HtmlRenderer : ITableRenderer, IReportRenderer
{
    /// <inheritdoc/>
    public string Name => "HTML";

    /// <inheritdoc/>
    public string Render(ITable table)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<table>");

        sb.AppendLine("  <thead>");
        sb.AppendLine("    <tr>");
        for (int i = 0; i < table.Headers.Count; i++)
        {
            var alignment = i < table.Alignments.Count ? table.Alignments[i] : ColumnAlignment.Left;
            var alignAttr = GetAlignmentStyle(alignment);
            sb.Append("      <th");
            sb.Append(alignAttr);
            sb.Append('>');
            EscapeHtml(sb, table.Headers[i]);
            sb.AppendLine("</th>");
        }
        sb.AppendLine("    </tr>");
        sb.AppendLine("  </thead>");

        sb.AppendLine("  <tbody>");
        foreach (var row in table.Rows)
        {
            sb.AppendLine("    <tr>");
            for (int i = 0; i < table.Headers.Count; i++)
            {
                var value = i < row.Count ? row[i] : string.Empty;
                var alignment =
                    i < table.Alignments.Count ? table.Alignments[i] : ColumnAlignment.Left;
                var alignAttr = GetAlignmentStyle(alignment);
                sb.Append("      <td");
                sb.Append(alignAttr);
                sb.Append('>');
                EscapeHtml(sb, value);
                sb.AppendLine("</td>");
            }
            sb.AppendLine("    </tr>");
        }
        sb.AppendLine("  </tbody>");
        sb.AppendLine("</table>");

        return sb.ToString();
    }

    /// <inheritdoc/>
    public string RenderTitle(string title, string? referenceUrl = null)
    {
        var sb = new StringBuilder();
        sb.Append("<h1>");
        EscapeHtml(sb, title);
        sb.AppendLine("</h1>");

        if (referenceUrl != null)
        {
            sb.Append("<p>Reference: <a href=\"");
            EscapeHtml(sb, referenceUrl);
            sb.Append("\">");
            EscapeHtml(sb, referenceUrl);
            sb.AppendLine("</a></p>");
        }

        return sb.ToString();
    }

    /// <inheritdoc/>
    public string RenderSectionHeader(string sectionTitle)
    {
        var sb = new StringBuilder();
        sb.Append("<h2>");
        EscapeHtml(sb, sectionTitle);
        sb.AppendLine("</h2>");
        return sb.ToString();
    }

    /// <inheritdoc/>
    public string RenderTable(ITable table) => Render(table);

    /// <inheritdoc/>
    public string RenderText(string text)
    {
        var sb = new StringBuilder();
        sb.Append("<p>");
        EscapeHtml(sb, text);
        sb.AppendLine("</p>");
        return sb.ToString();
    }

    /// <inheritdoc/>
    public string CombineParts(IEnumerable<string> parts)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"utf-8\">");
        sb.AppendLine("  <style>");
        sb.AppendLine(
            "    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; margin: 40px; }"
        );
        sb.AppendLine(
            "    table { border-collapse: collapse; width: 100%; margin: 20px 0; }"
        );
        sb.AppendLine(
            "    th, td { border: 1px solid #ddd; padding: 12px 8px; text-align: left; }"
        );
        sb.AppendLine("    th { background-color: #f5f5f5; font-weight: 600; }");
        sb.AppendLine("    tr:nth-child(even) { background-color: #f9f9f9; }");
        sb.AppendLine(
            "    h1 { color: #333; border-bottom: 2px solid #333; padding-bottom: 10px; }"
        );
        sb.AppendLine("    h2 { color: #555; margin-top: 30px; }");
        sb.AppendLine("    a { color: #0066cc; text-decoration: none; }");
        sb.AppendLine("    a:hover { text-decoration: underline; }");
        sb.AppendLine("  </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        foreach (var part in parts)
        {
            if (!string.IsNullOrWhiteSpace(part))
                sb.AppendLine(part);
        }

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private static string GetAlignmentStyle(ColumnAlignment alignment)
    {
        return alignment switch
        {
            ColumnAlignment.Center => " style=\"text-align: center;\"",
            ColumnAlignment.Right => " style=\"text-align: right;\"",
            _ => string.Empty,
        };
    }

    internal static void EscapeHtml(StringBuilder sb, string text)
    {
        foreach (var c in text)
        {
            switch (c)
            {
                case '&':
                    sb.Append("&amp;");
                    break;
                case '<':
                    sb.Append("&lt;");
                    break;
                case '>':
                    sb.Append("&gt;");
                    break;
                case '"':
                    sb.Append("&quot;");
                    break;
                case '\'':
                    sb.Append("&#39;");
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }
    }
}

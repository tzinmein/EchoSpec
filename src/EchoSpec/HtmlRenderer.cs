using System.Text;

namespace EchoSpec;

/// <summary>
/// Renders tables and reports in HTML format.
/// </summary>
public class HtmlRenderer : ITableRenderer, IReportRenderer
{
    /// <inheritdoc/>
    public string Name => "HTML";

    /// <inheritdoc/>
    public string Render(ITable table)
    {
        if (table.Rows.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine("<table>");

        // Header row
        sb.AppendLine("  <thead>");
        sb.AppendLine("    <tr>");
        for (int i = 0; i < table.Headers.Count; i++)
        {
            var alignment = i < table.Alignments.Count ? table.Alignments[i] : ColumnAlignment.Left;
            var alignAttr = GetAlignmentStyle(alignment);
            sb.AppendLine($"      <th{alignAttr}>{EscapeHtml(table.Headers[i])}</th>");
        }
        sb.AppendLine("    </tr>");
        sb.AppendLine("  </thead>");

        // Data rows
        sb.AppendLine("  <tbody>");
        foreach (var row in table.Rows)
        {
            sb.AppendLine("    <tr>");
            for (int i = 0; i < table.Headers.Count; i++)
            {
                var value = i < row.Count ? row[i] : string.Empty;
                var alignment = i < table.Alignments.Count ? table.Alignments[i] : ColumnAlignment.Left;
                var alignAttr = GetAlignmentStyle(alignment);
                sb.AppendLine($"      <td{alignAttr}>{EscapeHtml(value)}</td>");
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
        sb.AppendLine($"<h1>{EscapeHtml(title)}</h1>");

        if (referenceUrl != null)
        {
            sb.AppendLine($"<p>Reference: <a href=\"{EscapeHtml(referenceUrl)}\">{EscapeHtml(referenceUrl)}</a></p>");
        }

        return sb.ToString();
    }

    /// <inheritdoc/>
    public string RenderSectionHeader(string sectionTitle)
    {
        return $"<h2>{EscapeHtml(sectionTitle)}</h2>\n";
    }

    /// <inheritdoc/>
    public string RenderTable(ITable table) => Render(table);

    /// <inheritdoc/>
    public string RenderText(string text)
    {
        return $"<p>{EscapeHtml(text)}</p>\n";
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
        sb.AppendLine("    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; margin: 40px; }");
        sb.AppendLine("    table { border-collapse: collapse; width: 100%; margin: 20px 0; }");
        sb.AppendLine("    th, td { border: 1px solid #ddd; padding: 12px 8px; text-align: left; }");
        sb.AppendLine("    th { background-color: #f5f5f5; font-weight: 600; }");
        sb.AppendLine("    tr:nth-child(even) { background-color: #f9f9f9; }");
        sb.AppendLine("    h1 { color: #333; border-bottom: 2px solid #333; padding-bottom: 10px; }");
        sb.AppendLine("    h2 { color: #555; margin-top: 30px; }");
        sb.AppendLine("    a { color: #0066cc; text-decoration: none; }");
        sb.AppendLine("    a:hover { text-decoration: underline; }");
        sb.AppendLine("  </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        
        foreach (var part in parts.Where(p => !string.IsNullOrWhiteSpace(p)))
        {
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

    private static string EscapeHtml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }
}

namespace EchoSpec;

/// <summary>
/// Output format for reports and tables.
/// </summary>
public enum ReportFormat
{
    /// <summary>
    /// Plain console output with formatting.
    /// </summary>
    Console = 0,

    /// <summary>
    /// Markdown format suitable for files.
    /// </summary>
    Markdown = 1,

    /// <summary>
    /// Console and Markdown (not HTML).
    /// Supported by <c>ReportBuilder.Generate</c> only — not <c>TableBuilder.Build</c>.
    /// </summary>
    Both = 2,

    /// <summary>
    /// HTML format. Tables produce a <c>&lt;table&gt;</c> fragment;
    /// reports produce a full HTML document.
    /// </summary>
    Html = 3,

    /// <summary>
    /// Console, Markdown, and HTML.
    /// Supported by <c>ReportBuilder.Generate</c> only — not <c>TableBuilder.Build</c>.
    /// </summary>
    All = 4,
}

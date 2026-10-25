namespace EchoSpec.Sections;

/// <summary>
/// Interface for report sections.
/// </summary>
internal interface IReportSection<T>
{
    /// <summary>
    /// Materializes section content once so multiple renderers can reuse it.
    /// </summary>
    IPreparedSection Prepare(IReadOnlyList<T> data);
}

/// <summary>
/// A section whose data-dependent work is already done; only rendering remains.
/// </summary>
internal interface IPreparedSection
{
    string Render(IReportRenderer renderer);
}

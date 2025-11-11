namespace EchoSpec.Sections;

/// <summary>
/// Interface for report sections.
/// </summary>
internal interface IReportSection<T>
{
    string Generate(IEnumerable<T> data, IReportRenderer renderer);
}

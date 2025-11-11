using EchoSpec.Builders;

namespace EchoSpec.Sections;

/// <summary>
/// Table section implementation.
/// </summary>
internal class TableSection<T>(
    Func<TableBuilder, IEnumerable<T>, TableBuilder> tableBuilder,
    string? title
) : IReportSection<T>
{
    public string Generate(IEnumerable<T> data, IReportRenderer renderer)
    {
        var parts = new List<string>();

        if (title != null)
        {
            parts.Add(renderer.RenderSectionHeader(title));
        }

        var table = new TableBuilder();
        tableBuilder(table, data);
        parts.Add(renderer.RenderTable(table));

        return string.Concat(parts);
    }
}

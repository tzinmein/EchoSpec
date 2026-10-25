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
    public IPreparedSection Prepare(IReadOnlyList<T> data)
    {
        var table = new TableBuilder();
        tableBuilder(table, data);
        return new PreparedTableSection(title, table);
    }

    private sealed class PreparedTableSection(string? title, TableBuilder table) : IPreparedSection
    {
        public string Render(IReportRenderer renderer)
        {
            if (title == null)
                return renderer.RenderTable(table);

            return string.Concat(renderer.RenderSectionHeader(title), renderer.RenderTable(table));
        }
    }
}

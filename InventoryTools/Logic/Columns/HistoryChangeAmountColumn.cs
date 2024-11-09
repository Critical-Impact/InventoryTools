using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class HistoryChangeAmountColumn : TextColumn
{
    public HistoryChangeAmountColumn(ILogger<HistoryChangeAmountColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
    public override ColumnCategory ColumnCategory => ColumnCategory.History;

    public override string? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        if (searchResult.InventoryChange != null)
        {
            return searchResult.InventoryChange.GetFormattedAmount().ToString();
        }

        return null;
    }

    public override string Name { get; set; } = "History Event Amount";
    public override string RenderName => "Amount";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "The amount that changed(if applicable)";
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    public override FilterType AvailableIn { get; } = Logic.FilterType.HistoryFilter;
    public override FilterType DefaultIn => Logic.FilterType.HistoryFilter;
}
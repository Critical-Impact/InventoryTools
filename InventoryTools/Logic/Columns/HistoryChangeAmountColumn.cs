using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns;

public class HistoryChangeAmountColumn : TextColumn
{
    public override ColumnCategory ColumnCategory => ColumnCategory.History;

    public override string? CurrentValue(InventoryItem item)
    {
        return "";
    }

    public override string? CurrentValue(ItemEx item)
    {
        return "";
    }

    public override string? CurrentValue(SortingResult item)
    {
        return "";
    }

    public override string? CurrentValue(InventoryChange item)
    {
        return item.GetFormattedAmount().ToString();
    }


    public override string Name { get; set; } = "History Event Amount";
    public override string RenderName => "Event";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "The amount that changed(if applicable)";
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    public override FilterType AvailableIn { get; } = Logic.FilterType.HistoryFilter;
}
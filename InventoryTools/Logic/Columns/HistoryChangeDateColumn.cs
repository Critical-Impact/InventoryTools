using System.Globalization;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns;

public class HistoryChangeDateColumn : TextColumn
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

    public override string? CurrentValue(InventoryChange currentValue)
    {
        //TODO: Make a date column
        return currentValue.ChangeDate?.ToString(CultureInfo.InvariantCulture) ?? "Unknown Date";
    }

    public override string Name { get; set; } = "History Event Date/Time";
    public override string RenderName => "Date/Time";
    public override float Width { get; set; } = 50;
    public override string HelpText { get; set; } = "When did the historical inventory event happen?";
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    public override FilterType AvailableIn { get; } = Logic.FilterType.HistoryFilter;
}
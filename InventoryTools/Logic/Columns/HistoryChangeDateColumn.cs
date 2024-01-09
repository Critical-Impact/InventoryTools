using System;
using System.Globalization;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns;

public class HistoryChangeDateColumn : DateTimeColumn
{
    public override ColumnCategory ColumnCategory => ColumnCategory.History;
    public override DateTime? CurrentValue(InventoryItem item)
    {
        return null;
    }

    public override DateTime? CurrentValue(ItemEx item)
    {
        return null;
    }

    public override DateTime? CurrentValue(SortingResult item)
    {
        return null;
    }

    public override DateTime? CurrentValue(InventoryChange currentValue)
    {
        return currentValue.ChangeDate;
    }
    
    public override string CsvExport(InventoryChange item)
    {
        return CurrentValue(item)?.ToString(CultureInfo.InvariantCulture) ?? "";
    }

    public override string Name { get; set; } = "History Event Date/Time";
    public override string RenderName => "Date/Time";
    public override float Width { get; set; } = 50;
    public override string HelpText { get; set; } = "When did the historical inventory event happen?";
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    public override FilterType AvailableIn { get; } = Logic.FilterType.HistoryFilter;
}
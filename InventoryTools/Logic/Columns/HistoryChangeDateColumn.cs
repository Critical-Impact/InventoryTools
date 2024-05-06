using System;
using System.Globalization;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class HistoryChangeDateColumn : DateTimeColumn
{
    public HistoryChangeDateColumn(ILogger<HistoryChangeDateColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
    public override ColumnCategory ColumnCategory => ColumnCategory.History;
    public override DateTime? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
    {
        return null;
    }

    public override DateTime? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
    {
        return null;
    }

    public override DateTime? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
    {
        return null;
    }

    public override DateTime? CurrentValue(ColumnConfiguration columnConfiguration, InventoryChange currentValue)
    {
        return currentValue.ChangeDate;
    }
    
    public override string CsvExport(ColumnConfiguration columnConfiguration, InventoryChange item)
    {
        return CurrentValue(columnConfiguration, item)?.ToString(CultureInfo.InvariantCulture) ?? "";
    }

    public override string Name { get; set; } = "History Event Date/Time";
    public override string RenderName => "Date/Time";
    public override float Width { get; set; } = 50;
    public override string HelpText { get; set; } = "When did the historical inventory event happen?";
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    public override FilterType AvailableIn { get; } = Logic.FilterType.HistoryFilter;
    public override FilterType DefaultIn => Logic.FilterType.HistoryFilter;
}
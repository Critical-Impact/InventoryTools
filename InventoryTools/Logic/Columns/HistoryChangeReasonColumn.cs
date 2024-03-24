using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Plugin.Services;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class HistoryChangeReasonColumn : TextColumn
{
    public HistoryChangeReasonColumn(ILogger<HistoryChangeReasonColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
    public override ColumnCategory ColumnCategory => ColumnCategory.History;

    public override string? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
    {
        return "";
    }

    public override string? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
    {
        return "";
    }

    public override string? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
    {
        return "";
    }

    public override string? CurrentValue(ColumnConfiguration columnConfiguration, InventoryChange item)
    {
        return item.GetFormattedChange();
    }
    
    public override string CsvExport(ColumnConfiguration columnConfiguration, InventoryChange item)
    {
        return CurrentValue(columnConfiguration, item) ?? "";
    }

    public override string Name { get; set; } = "History Event Reason";
    public override string RenderName => "Event";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "The reason the change occurred";
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    public override FilterType AvailableIn { get; } = Logic.FilterType.HistoryFilter;
}
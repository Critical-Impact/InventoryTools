using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns;

public class HasBeenGatheredColumn : CheckboxColumn
{
    public override bool? CurrentValue(InventoryItem item)
    {
        return CurrentValue(item.Item);
    }

    public override bool? CurrentValue(ItemEx item)
    {
        return PluginService.GameInterface.IsItemGathered(item.RowId);
    }

    public override bool? CurrentValue(SortingResult item)
    {
        return CurrentValue(item.InventoryItem);
    }

    public override string Name { get; set; } = "Has been gathered before?";
    public override float Width { get; set; } = 80;

    public override string HelpText { get; set; } =
        "Has this gathering item been gathered at least once by the currently logged in character? This only supports mining and botany at present.";

    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
}
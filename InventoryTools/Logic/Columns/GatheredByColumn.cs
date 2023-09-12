using System.Linq;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns;

public class GatheredByColumn : TextColumn
{
    public override ColumnCategory ColumnCategory { get; } = ColumnCategory.Basic;
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

    public override string? CurrentValue(InventoryItem item)
    {
        return CurrentValue(item.Item);
    }

    public override string? CurrentValue(ItemEx item)
    {
        var currentValue = item.GatheringTypes.Select(c => c.Value!.FormattedName).ToList();
        if (item.ObtainedFishing)
        {
            currentValue.Add("Fishing");
        }

        return string.Join(",", currentValue);
    }

    public override string? CurrentValue(SortingResult item)
    {
        return CurrentValue(item.InventoryItem);
    }

    public override string Name { get; set; } = "Gathered By?";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "How is this item gathered?";
}
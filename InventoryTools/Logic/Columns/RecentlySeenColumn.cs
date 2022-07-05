using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Extensions;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class RecentlySeenColumn : TextColumn
    {
        public override string? CurrentValue(InventoryItem item)
        {
            return PluginService.PluginLogic.GetLastSeenTime(item.ItemId)?.ToHumanReadableString() ?? "";
        }

        public override string? CurrentValue(ItemEx item)
        {
            return PluginService.PluginLogic.GetLastSeenTime(item.RowId)?.ToHumanReadableString() ?? "";
        }

        public override string? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
        }

        public override string Name { get; set; } = "Last Seen";
        public override float Width { get; set; } = 100;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}
using CriticalCommonLib.Models;
using InventoryTools.Extensions;
using InventoryTools.Logic.Columns.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class RecentlySeenColumn : TextColumn
    {
        public override string? CurrentValue(InventoryItem item)
        {
            return PluginService.PluginLogic.GetLastSeenTime(item.ItemId)?.ToHumanReadableString() ?? "";
        }

        public override string? CurrentValue(Item item)
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
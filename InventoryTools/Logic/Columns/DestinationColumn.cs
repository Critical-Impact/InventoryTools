using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using InventoryTools.Extensions;
using InventoryTools.Logic.Columns.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Columns
{
    public class DestinationColumn : TextColumn
    {
        public override string? CurrentValue(InventoryItem item)
        {
            return null;
        }

        public override string? CurrentValue(Item item)
        {
            return null;
        }

        public override string? CurrentValue(SortingResult item)
        {
            var destination = item.DestinationRetainerId.HasValue
                ? PluginService.CharacterMonitor.Characters[item.DestinationRetainerId.Value]?.FormattedName ?? ""
                : "Unknown";
            var destinationBag = item.DestinationBag.FormattedName();
            return destination + " - " + destinationBag;
        }

        public override string Name { get; set; } = "Destination";
        public override float Width { get; set; } = 100.0f;
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override FilterType AvailableIn => Logic.FilterType.SortingFilter;
    }
}
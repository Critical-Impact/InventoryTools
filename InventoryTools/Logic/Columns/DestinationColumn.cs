using CriticalCommonLib.Models;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class DestinationColumn : TextColumn
    {
        private readonly ICharacterMonitor _characterMonitor;

        public DestinationColumn(ILogger<DestinationColumn> logger, ImGuiService imGuiService, ICharacterMonitor characterMonitor) : base(logger, imGuiService)
        {
            _characterMonitor = characterMonitor;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Inventory;
        public override string? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return null;
        }

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return null;
        }

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            var destination = item.DestinationRetainerId.HasValue
                ? _characterMonitor.Characters.ContainsKey(item.DestinationRetainerId.Value) ? _characterMonitor.Characters[item.DestinationRetainerId.Value].FormattedName : ""
                : "Unknown";
            var destinationBag = item.DestinationBag?.ToInventoryCategory().FormattedName() ?? "";
            return destination + " - " + destinationBag;
        }

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, InventoryChange item)
        {
            var destination = item.ToItem != null
                ? _characterMonitor.Characters.ContainsKey(item.ToItem.RetainerId) ? _characterMonitor.Characters[item.ToItem.RetainerId].FormattedName : ""
                : "Unknown";
            var destinationBag = item.ToItem?.FormattedBagLocation ?? "";
            return destination + " - " + destinationBag;
        }

        public override string Name { get; set; } = "Destination";
        public override float Width { get; set; } = 100.0f;
        public override string HelpText { get; set; } = "Shows where the item should be moved to or where the item was moved to in the case of a history filter.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override FilterType AvailableIn => Logic.FilterType.SortingFilter | Logic.FilterType.CraftFilter | Logic.FilterType.HistoryFilter;
        
        public override FilterType DefaultIn => Logic.FilterType.SortingFilter;

    }
}
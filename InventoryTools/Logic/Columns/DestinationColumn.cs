using CriticalCommonLib.Extensions;
using CriticalCommonLib.Services;

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

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            if (searchResult.InventoryChange != null)
            {
                var destination = searchResult.InventoryChange.ToItem != null
                    ? _characterMonitor.Characters.ContainsKey(searchResult.InventoryChange.ToItem.RetainerId) ? _characterMonitor.Characters[searchResult.InventoryChange.ToItem.RetainerId].FormattedName : ""
                    : "Unknown";
                var destinationBag = searchResult.InventoryChange.ToItem?.FormattedBagLocation ?? "";
                return destination + " - " + destinationBag;
            }

            if (searchResult.SortingResult != null)
            {

                var destination = searchResult.SortingResult.DestinationRetainerId.HasValue
                    ? _characterMonitor.Characters.ContainsKey(searchResult.SortingResult.DestinationRetainerId
                        .Value)
                        ? _characterMonitor.Characters[searchResult.SortingResult.DestinationRetainerId.Value].FormattedName
                        : ""
                    : "Unknown";
                var destinationBag = searchResult.SortingResult.DestinationBag?.ToInventoryCategory().FormattedName() ??
                                     "";
                return destination + " - " + destinationBag;
            }

            return null;
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
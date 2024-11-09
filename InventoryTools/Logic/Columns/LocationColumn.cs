using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class LocationColumn : TextColumn
    {
        public LocationColumn(ILogger<LocationColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Inventory;

        public override FilterType AvailableIn => Logic.FilterType.SearchFilter | Logic.FilterType.SortingFilter | Logic.FilterType.CraftFilter;

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            if (searchResult.InventoryItem != null)
            {
                return searchResult.InventoryItem.FormattedBagLocation;
            }

            return null;
        }
        public override string Name { get; set; } = "Inventory Location";
        public override string RenderName => "Location";
        public override float Width { get; set; } = 100.0f;
        public override string HelpText { get; set; } = "Shows the location of the item in your inventory.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override FilterType DefaultIn => Logic.FilterType.SearchFilter | Logic.FilterType.SortingFilter | Logic.FilterType.CraftFilter | Logic.FilterType.HistoryFilter;
    }
}
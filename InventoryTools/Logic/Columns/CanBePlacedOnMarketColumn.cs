using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class CanBePlacedOnMarketColumn : CheckboxColumn
    {
        public CanBePlacedOnMarketColumn(ILogger<CanBePlacedOnMarketColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return searchResult.Item.CanBePlacedOnMarket;
        }
        public override string Name { get; set; } = "Can be Placed on Market?";
        public override float Width { get; set; } = 90.0f;
        public override string HelpText { get; set; } = "Can the item be placed on the marketboard?";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}
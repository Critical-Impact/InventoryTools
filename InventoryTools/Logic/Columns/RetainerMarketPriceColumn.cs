using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class RetainerMarketPriceColumn : GilColumn
    {
        public RetainerMarketPriceColumn(ILogger<RetainerMarketPriceColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Inventory;
        public override int? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            if (searchResult.InventoryItem != null)
            {
                return (int)searchResult.InventoryItem.RetainerMarketPrice;
            }

            return null;
        }
        public override string Name { get; set; } = "Retainer Selling Unit Price";
        public override string RenderName => "Retainer Unit Price";
        public override float Width { get; set; } = 100;

        public override string HelpText { get; set; } =
            "If the item is selling on the market, this is the unit price it has been put up for.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}
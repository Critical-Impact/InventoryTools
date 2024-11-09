using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class CanBeTradedColumn : CheckboxColumn
    {
        public CanBeTradedColumn(ILogger<CanBeTradedColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return searchResult.Item.CanBeTraded;
        }
        public override string Name { get; set; } = "Is Tradable?";
        public override float Width { get; set; } = 90.0f;
        public override string HelpText { get; set; } = "Can the item be traded?";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}
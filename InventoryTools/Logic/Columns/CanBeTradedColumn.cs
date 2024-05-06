using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
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
        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return item.CanBeTraded;
        }

        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return item.CanBeTraded;
        }

        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item.InventoryItem);
        }

        public override string Name { get; set; } = "Is Tradable?";
        public override float Width { get; set; } = 90.0f;
        public override string HelpText { get; set; } = "Can the item be traded?";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}
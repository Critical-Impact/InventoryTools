using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class ItemIdColumn : IntegerColumn
    {
        public ItemIdColumn(ILogger<ItemIdColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override int? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return (int)item.ItemId;
        }

        public override int? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return (int)item.RowId;
        }

        public override int? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item.InventoryItem);
        }

        public override string Name { get; set; } = "Item ID";
        public override float Width { get; set; } = 100.0f;
        public override string HelpText { get; set; } = "Shows the item's internal ID.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}
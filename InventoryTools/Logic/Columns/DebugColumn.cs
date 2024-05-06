using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class DebugColumn : TextColumn
    {
        public DebugColumn(ILogger<DebugColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Debug;
        public override string? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return CurrentValue(columnConfiguration, item.Item);
        }

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return "Item Search: " + item.ItemSearchCategory.Row + " - Ui Category: " + item.ItemUICategory.Row + " - Sort Category: " + item.ItemSortCategory.Row + " - Equip Slot Category: " + item.EquipSlotCategory.Row + " - Class Job Category: " + item.ClassJobCategory.Row + " - Buy: " + item.PriceMid + " - Unknown: " + item.Unknown19;
        }

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item.InventoryItem);
        }

        public override string Name { get; set; } = "Debug - General Information";
        public override float Width { get; set; } = 200;
        public override string HelpText { get; set; } = "Shows basic debug information";
        public override bool HasFilter { get; set; } = true;
        public override bool IsDebug { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}
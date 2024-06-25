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

        public override string? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return "Item Search: " + searchResult.Item.ItemSearchCategory.Row + " - Ui Category: " + searchResult.Item.ItemUICategory.Row + " - Sort Category: " + searchResult.Item.ItemSortCategory.Row + " - Equip Slot Category: " + searchResult.Item.EquipSlotCategory.Row + " - Class Job Category: " + searchResult.Item.ClassJobCategory.Row + " - Buy: " + searchResult.Item.PriceMid + " - Unknown: " + searchResult.Item.Unknown19;
        }
        public override string Name { get; set; } = "Debug - General Information";
        public override float Width { get; set; } = 200;
        public override string HelpText { get; set; } = "Shows basic debug information";
        public override bool HasFilter { get; set; } = true;
        public override bool IsDebug { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}
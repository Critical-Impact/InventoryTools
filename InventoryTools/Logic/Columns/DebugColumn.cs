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
            return "Item Search: " + searchResult.Item.Base.ItemSearchCategory.RowId + " - Ui Category: " + searchResult.Item.Base.ItemUICategory.RowId + " - Sort Category: " + searchResult.Item.Base.ItemSortCategory.RowId + " - Equip Slot Category: " + searchResult.Item.Base.EquipSlotCategory.RowId + " - Class Job Category: " + searchResult.Item.Base.ClassJobCategory.RowId + " - Buy: " + searchResult.Item.Base.PriceMid;
        }
        public override string Name { get; set; } = "Debug - General Information";
        public override float Width { get; set; } = 200;
        public override string HelpText { get; set; } = "Shows basic debug information";
        public override bool HasFilter { get; set; } = true;
        public override bool IsDebug { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}
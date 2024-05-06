using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class LeveIsCraftLevelColumn : CheckboxColumn
    {
        private readonly ExcelCache _excelCache;

        public LeveIsCraftLevelColumn(ILogger<LeveIsCraftLevelColumn> logger, ImGuiService imGuiService, ExcelCache excelCache) : base(logger, imGuiService)
        {
            _excelCache = excelCache;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return _excelCache.IsItemCraftLeve(item.ItemId);
        }

        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return _excelCache.IsItemCraftLeve(item.RowId);
        }

        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item.InventoryItem);
        }

        public override string Name { get; set; } = "Is Leve(Craft) Item?";
        public override string RenderName => "Leve (Craft)";
        public override float Width { get; set; } = 100.0f;
        public override string HelpText { get; set; } = "Is this item used in a craft leve?";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
    }
}
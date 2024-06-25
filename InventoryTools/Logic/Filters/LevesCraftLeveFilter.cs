using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class LeveIsCraftLeveFilter : BooleanFilter
    {
        private readonly ExcelCache _excelCache;
        public override string Key { get; set; } = "LeveIsCraftLeve";
        public override string Name { get; set; } = "Is Craft Leve Item?";
        public override string HelpText { get; set; } = "Is this item craftable and a hand-in for a leve?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Crafting;


        
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            var currentValue = this.CurrentValue(configuration);
            return currentValue switch
            {
                null => true,
                true => _excelCache.IsItemCraftLeve(item.ItemId),
                _ => !_excelCache.IsItemCraftLeve(item.ItemId)
            };
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            var currentValue = this.CurrentValue(configuration);
            return currentValue switch
            {
                null => true,
                true => _excelCache.IsItemCraftLeve(item.RowId),
                _ => !_excelCache.IsItemCraftLeve(item.RowId)
            };
        }

        public LeveIsCraftLeveFilter(ILogger<LeveIsCraftLeveFilter> logger, ImGuiService imGuiService, ExcelCache excelCache) : base(logger, imGuiService)
        {
            _excelCache = excelCache;
        }
    }
}
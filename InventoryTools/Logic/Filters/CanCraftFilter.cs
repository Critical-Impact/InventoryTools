using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class CanCraftFilter : BooleanFilter
    {
        private readonly ExcelCache _excelCache;
        public override string Key { get; set; } = "CanCraft";
        public override string Name { get; set; } = "Can Craft?";
        public override string HelpText { get; set; } = "Can this be crafted?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Crafting;



        public override bool? FilterItem(FilterConfiguration configuration,InventoryItem item)
        {

            return FilterItem(configuration, item.Item);
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            var currentValue = CurrentValue(configuration);
            var canCraft = _excelCache.CanCraftItem(item.RowId);
            return currentValue == null || currentValue.Value && canCraft || !currentValue.Value && !canCraft;
        }

        public CanCraftFilter(ILogger<CanCraftFilter> logger, ImGuiService imGuiService, ExcelCache excelCache) : base(logger, imGuiService)
        {
            _excelCache = excelCache;
        }
    }
}
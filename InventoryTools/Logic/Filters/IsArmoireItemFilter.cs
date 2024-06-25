using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class IsArmoireItemFilter : BooleanFilter
    {
        private readonly ExcelCache _excelCache;
        public override string Key { get; set; } = "IsArmoire";
        public override string Name { get; set; } = "Is Armoire Item?";
        public override string HelpText { get; set; } = "Only show items that can be put into the armoire.";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Searching;



        
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            var currentValue = this.CurrentValue(configuration);
            return currentValue switch
            {
                null => true,
                true => _excelCache.IsArmoireItem(item.Item.RowId),
                _ => !_excelCache.IsArmoireItem(item.Item.RowId)
            };
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            var currentValue = this.CurrentValue(configuration);
            return currentValue switch
            {
                null => true,
                true => _excelCache.IsArmoireItem(item.RowId),
                _ => !_excelCache.IsArmoireItem(item.RowId)
            };
        }

        public IsArmoireItemFilter(ILogger<IsArmoireItemFilter> logger, ImGuiService imGuiService, ExcelCache excelCache) : base(logger, imGuiService)
        {
            _excelCache = excelCache;
        }
    }
}
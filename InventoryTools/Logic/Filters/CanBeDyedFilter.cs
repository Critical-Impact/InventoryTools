using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class CanBeDyedFilter : BooleanFilter
    {
        public override string Key { get; set; } = "CanBeDyed";
        public override string Name { get; set; } = "Can be Dyed?";
        public override string HelpText { get; set; } = "Can this be item be dyed?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;



        public override bool? FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            return FilterItem(configuration, item.Item);
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            var currentValue = CurrentValue(configuration);
            var canByDyed = item.DyeCount != 0;
            return currentValue == null || currentValue.Value && canByDyed || !currentValue.Value && !canByDyed;
        }

        public CanBeDyedFilter(ILogger<CanBeDyedFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}
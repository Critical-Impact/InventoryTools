using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class CanBeDesynthedFilter : BooleanFilter
    {
        public override string Key { get; set; } = "desynth";
        public override string Name { get; set; } = "Can be Desynthed?";
        public override string HelpText { get; set; } = "Can this item be desynthesised?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;

        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return FilterItem(configuration, item.Item);
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue == null)
            {
                return null;
            }
            return currentValue.Value && item.CanBeDesynthed || !currentValue.Value && !item.CanBeDesynthed;
        }

        public CanBeDesynthedFilter(ILogger<CanBeDesynthedFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}
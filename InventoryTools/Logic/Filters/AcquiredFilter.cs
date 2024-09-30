using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class AcquiredFilter : BooleanFilter
    {
        private readonly IGameInterface _gameInterface;

        public AcquiredFilter(ILogger<AcquiredFilter> logger, ImGuiService imGuiService, IGameInterface gameInterface) : base(logger, imGuiService)
        {
            _gameInterface = gameInterface;
        }
        public override string Key { get; set; } = "Acquired";
        public override string Name { get; set; } = "Is Acquired?";
        public override string HelpText { get; set; } = "Has this item be acquired?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Acquisition;

        public override bool? FilterItem(FilterConfiguration configuration,InventoryItem item)
        {

            return FilterItem(configuration, item.Item);
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue == null)
            {
                return true;
            }
            var action = item.ItemAction?.Value;
            if (!ActionTypeExt.IsValidAction(action)) {
                return false;
            }
            return currentValue.Value && _gameInterface.HasAcquired(item) || !currentValue.Value && !_gameInterface.HasAcquired(item);
        }
    }
}
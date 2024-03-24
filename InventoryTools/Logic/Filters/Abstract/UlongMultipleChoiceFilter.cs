using System.Collections.Generic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters.Abstract
{
    public abstract class UlongMultipleChoiceFilter : MultipleChoiceFilter<ulong>
    {
        public override List<ulong> DefaultValue { get; set; } = new();
        public override List<ulong> CurrentValue(FilterConfiguration configuration)
        {
            return configuration.GetUlongChoiceFilter(Key);
        }
        
        public override void UpdateFilterConfiguration(FilterConfiguration configuration, List<ulong> newValue)
        {
            configuration.UpdateUlongChoiceFilter(Key, newValue);
        }
        
        public override void ResetFilter(FilterConfiguration configuration)
        {
            UpdateFilterConfiguration(configuration, DefaultValue);
        }

        protected UlongMultipleChoiceFilter(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}
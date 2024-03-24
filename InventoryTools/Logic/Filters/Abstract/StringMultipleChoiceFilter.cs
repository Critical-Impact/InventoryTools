using System.Collections.Generic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters.Abstract
{
    public abstract class StringMultipleChoiceFilter : MultipleChoiceFilter<string>
    {
        public override List<string> DefaultValue { get; set; } = new();

        public override List<string> CurrentValue(FilterConfiguration configuration)
        {
            return configuration.GetStringChoiceFilter(Key);
        }
        
        public override void UpdateFilterConfiguration(FilterConfiguration configuration, List<string> newValue)
        {
            configuration.UpdateStringChoiceFilter(Key, newValue);
        }
        
        public override void ResetFilter(FilterConfiguration configuration)
        {
            UpdateFilterConfiguration(configuration, DefaultValue);
        }

        protected StringMultipleChoiceFilter(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}
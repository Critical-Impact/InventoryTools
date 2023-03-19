using System.Collections.Generic;

namespace InventoryTools.Logic.Filters.Abstract
{
    public abstract class UlongMultipleChoiceFilter : MultipleChoiceFilter<ulong>
    {
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
            UpdateFilterConfiguration(configuration, new List<ulong>());
        }
    }
}
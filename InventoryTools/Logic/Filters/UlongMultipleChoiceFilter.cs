using System.Collections.Generic;

namespace InventoryTools.Logic.Filters
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
    }
}
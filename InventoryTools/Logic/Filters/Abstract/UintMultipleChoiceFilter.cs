using System.Collections.Generic;
using System.Linq;

namespace InventoryTools.Logic.Filters.Abstract
{
    public abstract class UintMultipleChoiceFilter : MultipleChoiceFilter<uint>
    {
        public override List<uint> CurrentValue(FilterConfiguration configuration)
        {
            return configuration.GetUintChoiceFilter(Key);
        }
        
        public override void UpdateFilterConfiguration(FilterConfiguration configuration, List<uint> newValue)
        {
            configuration.UpdateUintChoiceFilter(Key, newValue.ToList());
        }
        
        public override void ResetFilter(FilterConfiguration configuration)
        {
            UpdateFilterConfiguration(configuration, DefaultValue);
        }
    }
}
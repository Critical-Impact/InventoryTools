using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class IsTimedNodeFilter : BooleanFilter
    {
        public override string Key { get; set; } = "TimedNode";
        public override string Name { get; set; } = "Is Timed Node?";
        public override string HelpText { get; set; } = "Is the item available in timed nodes?";
        public override FilterType AvailableIn { get; set; }  = FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter;
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Gathering;

        public override bool FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue == null) return true;
            
            if(currentValue.Value && item.IsItemAvailableAtTimedNode)
            {
                return true;
            }
                
            return !currentValue.Value && !item.IsItemAvailableAtTimedNode;

        }

        public override bool FilterItem(FilterConfiguration configuration, Item item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue == null) return true;
            
            if(currentValue.Value && item.IsCollectable)
            {
                return true;
            }
                
            return !currentValue.Value && !item.IsCollectable;
        }
    }
}
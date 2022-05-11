using CriticalCommonLib.Models;
using InventoryTools.Extensions;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class SpiritBondFilter : StringFilter
    {
        public override string Key { get; set; } = "SB";
        public override string Name { get; set; } = "Spirit Bond";
        public override string HelpText { get; set; } = "The spirit bond of the item.";
        public override FilterType AvailableIn { get; set; }  = FilterType.SearchFilter | FilterType.SortingFilter;
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;

        public override bool FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            var currentValue = CurrentValue(configuration);
            if (!string.IsNullOrEmpty(currentValue))
            {
                if (item.IsCollectible)
                {
                    return false;
                }
                var spiritBond = item.Spiritbond/100;
                if (!spiritBond.PassesFilter(currentValue.ToLower()))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool FilterItem(FilterConfiguration configuration, Item item)
        {
            return true;
        }
    }
}
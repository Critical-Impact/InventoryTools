using CriticalCommonLib.Models;
using InventoryTools.Extensions;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class RequiredLevelFilter : StringFilter
    {
        public override string Key { get; set; } = "ItemLvl";
        public override string Name { get; set; } = "Required Level";
        public override string HelpText { get; set; } = "The required level to equip the item.";
        public override FilterType AvailableIn { get; set; }  = FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter;
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;

        public override bool FilterItem(FilterConfiguration configuration,InventoryItem item)
        {
            if (item.Item == null)
            {
                return false;
            }
            return FilterItem(configuration, item.Item);
        }

        public override bool FilterItem(FilterConfiguration configuration, Item item)
        {
            var currentValue = CurrentValue(configuration);
            if (!string.IsNullOrEmpty(currentValue))
            {
                if (((int)item.LevelEquip).PassesFilter(currentValue.ToLower()))
                {
                    return true;
                }

                return false;
            }
            return true;
        }
    }
}
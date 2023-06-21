using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;

namespace InventoryTools.Logic.Filters.Abstract
{
    public abstract class Filter<T> : IFilter
    {
        public virtual int LabelSize { get; set; } = 200;
        public virtual int InputSize { get; set; } = 250;
        public abstract T CurrentValue(FilterConfiguration configuration);
        public abstract void Draw(FilterConfiguration configuration);
        public abstract void ResetFilter(FilterConfiguration configuration);

        public abstract void UpdateFilterConfiguration(FilterConfiguration configuration, T newValue);

        public abstract string Key { get; set; }
        public abstract string Name { get; set; }
        public abstract string HelpText { get; set; }
        
        public abstract FilterCategory FilterCategory { get; set; }

        public virtual int Order { get; set; } = 0;
        public virtual bool ShowReset { get; set; } = true;

        public abstract bool HasValueSet(FilterConfiguration configuration);
        public abstract FilterType AvailableIn { get; set; }
        public abstract bool? FilterItem(FilterConfiguration configuration,InventoryItem item);
        public abstract bool? FilterItem(FilterConfiguration configuration, ItemEx item);

        public virtual bool? FilterItem(FilterConfiguration configuration, InventoryChange item)
        {
            return FilterItem(configuration,item.InventoryItem);
        }

    }
}
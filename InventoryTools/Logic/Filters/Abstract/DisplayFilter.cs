using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;

namespace InventoryTools.Logic.Filters.Abstract
{
    public abstract class DisplayFilter : IFilter
    {
        public virtual int Order { get; set; } = 0;
        public virtual int LabelSize { get; set; } = 240;
        public virtual int InputSize { get; set; } = 200;
        public abstract string Key { get; set; }
        public abstract string Name { get; set; }
        public abstract string HelpText { get; set; }
        public abstract FilterCategory FilterCategory { get; set; }
        public abstract bool HasValueSet(FilterConfiguration configuration);
        public bool ShowReset { get; set; } = false;
        public abstract FilterType AvailableIn { get; set; }

        public abstract void Draw(FilterConfiguration configuration);

        public void ResetFilter(FilterConfiguration configuration)
        {
            
        }
        
        public virtual bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return null;
        }

        public virtual bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            return null;
        }

        public virtual bool? FilterItem(FilterConfiguration configuration, InventoryChange item)
        {
            return null;
        }

    }
}
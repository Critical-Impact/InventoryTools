using CriticalCommonLib.Models;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public abstract class Filter<T> : IFilter
    {
        public abstract T CurrentValue(FilterConfiguration configuration);
        public abstract void Draw(FilterConfiguration configuration);
        public abstract void UpdateFilterConfiguration(FilterConfiguration configuration, T newValue);

        public abstract string Key { get; set; }
        public abstract string Name { get; set; }
        public abstract string HelpText { get; set; }
        
        public abstract FilterCategory FilterCategory { get; set; }

        public abstract bool HasValueSet(FilterConfiguration configuration);
        public abstract FilterType AvailableIn { get; set; }
        public abstract bool FilterItem(FilterConfiguration configuration,InventoryItem item);
        public abstract bool FilterItem(FilterConfiguration configuration,Item item);
    }
}
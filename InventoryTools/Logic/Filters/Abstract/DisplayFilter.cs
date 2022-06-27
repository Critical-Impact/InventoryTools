using CriticalCommonLib.Models;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters.Abstract
{
    public abstract class DisplayFilter : IFilter
    {
        public virtual int Order { get; set; } = 0;
        public virtual int LabelSize { get; set; } = 240;
        public abstract string Key { get; set; }
        public abstract string Name { get; set; }
        public abstract string HelpText { get; set; }
        public abstract FilterCategory FilterCategory { get; set; }
        public abstract bool HasValueSet(FilterConfiguration configuration);

        public abstract FilterType AvailableIn { get; set; }
        public abstract bool? FilterItem(FilterConfiguration configuration, InventoryItem item);

        public abstract bool? FilterItem(FilterConfiguration configuration, Item item);

        public abstract void Draw(FilterConfiguration configuration);
    }
}
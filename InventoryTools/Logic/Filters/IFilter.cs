using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;


namespace InventoryTools.Logic.Filters
{
    public interface IFilter
    {
        public int LabelSize { get; set; }
        public int InputSize { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string HelpText { get; set; }
        public bool ShowReset { get; set; }
        public bool ShowOperatorTooltip { get; set; }

        public FilterCategory FilterCategory { get; set; }

        public int Order { get; set; }

        public bool HasValueSet(FilterConfiguration configuration);

        public FilterType AvailableIn { get; set; }
        public bool? FilterItem(FilterConfiguration configuration, InventoryItem item);
        public bool? FilterItem(FilterConfiguration configuration, ItemRow item);
        public bool? FilterItem(FilterConfiguration configuration, InventoryChange item);
        public void Draw(FilterConfiguration configuration);

        public void ResetFilter(FilterConfiguration configuration);
        public void ResetFilter(FilterConfiguration fromConfiguration, FilterConfiguration toConfiguration);

        public void InvalidateSearchCache();

    }
}
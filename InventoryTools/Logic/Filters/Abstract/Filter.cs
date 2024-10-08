using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters.Abstract
{
    public abstract class Filter<T> : IFilter
    {
        public ILogger Logger { get; }
        public ImGuiService ImGuiService { get; }

        public Filter(ILogger logger, ImGuiService imGuiService)
        {
            Logger = logger;
            ImGuiService = imGuiService;
        }
        public virtual int LabelSize { get; set; } = 220;
        public virtual int InputSize { get; set; } = 250;
        public abstract T CurrentValue(FilterConfiguration configuration);
        public abstract void Draw(FilterConfiguration configuration);
        public abstract void ResetFilter(FilterConfiguration configuration);
        public void ResetFilter(FilterConfiguration fromConfiguration, FilterConfiguration toConfiguration)
        {
            var currentValue = CurrentValue(fromConfiguration);
            UpdateFilterConfiguration(toConfiguration, currentValue);
        }

        public virtual void InvalidateSearchCache()
        {

        }

        public abstract void UpdateFilterConfiguration(FilterConfiguration configuration, T newValue);

        public abstract string Key { get; set; }
        public abstract string Name { get; set; }
        public abstract string HelpText { get; set; }

        public bool ShowOperatorTooltip { get; set; } = false;
        public abstract FilterCategory FilterCategory { get; set; }

        public virtual int Order { get; set; } = 0;
        public virtual bool ShowReset { get; set; } = true;
        public abstract T DefaultValue { get; set; }

        public abstract bool HasValueSet(FilterConfiguration configuration);
        public virtual FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter | FilterType.HistoryFilter | FilterType.CuratedList;
        public abstract bool? FilterItem(FilterConfiguration configuration,InventoryItem item);
        public abstract bool? FilterItem(FilterConfiguration configuration, ItemEx item);

        public virtual bool? FilterItem(FilterConfiguration configuration, InventoryChange item)
        {
            return FilterItem(configuration,item.InventoryItem);
        }

    }
}
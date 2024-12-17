using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class DisplayFilterInRetainersFilter : ChoiceFilter<FilterItemsRetainerEnum>
    {
        public override string Key { get; set; } = "FilterInRetainers";
        public override string Name { get; set; } = "Filter Items when in Retainer?";

        public override string HelpText { get; set; } =
            "When talking with a retainer should the filter adjust itself to only show items that should be put inside the retainer from your inventory? If set to only, highlighting will only occur when at the retainer bell and when within a retainer.";

        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Display;
        public override void ResetFilter(FilterConfiguration configuration)
        {
            UpdateFilterConfiguration(configuration, DefaultValue);
        }

        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return null;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            return null;
        }

        public override FilterItemsRetainerEnum CurrentValue(FilterConfiguration configuration)
        {
            return configuration.FilterItemsInRetainersEnum;
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, FilterItemsRetainerEnum newValue)
        {
            configuration.FilterItemsInRetainersEnum = newValue;
        }

        public override FilterItemsRetainerEnum DefaultValue { get; set; } = FilterItemsRetainerEnum.No;

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter | FilterType.HistoryFilter | FilterType.CuratedList | FilterType.CraftFilter;

        public override List<FilterItemsRetainerEnum> GetChoices(FilterConfiguration configuration)
        {
            return Enum.GetValues<FilterItemsRetainerEnum>().ToList();
        }

        public override string GetFormattedChoice(FilterConfiguration filterConfiguration,
            FilterItemsRetainerEnum choice)
        {
            if (choice == FilterItemsRetainerEnum.No)
            {
                return "No";
            }

            if (choice == FilterItemsRetainerEnum.Yes)
            {
                return "Yes";
            }

            if (choice == FilterItemsRetainerEnum.Only)
            {
                return "Only";
            }

            return choice.ToString();
        }

        public DisplayFilterInRetainersFilter(ILogger<DisplayFilterInRetainersFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}
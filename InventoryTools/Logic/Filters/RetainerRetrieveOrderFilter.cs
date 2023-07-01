using System.Collections.Generic;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;

namespace InventoryTools.Logic.Filters;

public class RetainerRetrieveOrderFilter : ChoiceFilter<RetainerRetrieveOrder>
{
    public override RetainerRetrieveOrder CurrentValue(FilterConfiguration configuration)
    {
        if (configuration.FilterType == FilterType.CraftFilter)
        {
            return configuration.CraftList.RetainerRetrieveOrder;
        }

        return RetainerRetrieveOrder.RetrieveFirst;
    }

    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
    {
        return null;
    }

    public override void ResetFilter(FilterConfiguration configuration)
    {
        UpdateFilterConfiguration(configuration, RetainerRetrieveOrder.RetrieveFirst);
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, RetainerRetrieveOrder newValue)
    {
        if (configuration.FilterType == FilterType.CraftFilter)
        {
            configuration.CraftList.RetainerRetrieveOrder = newValue;
            configuration.NotifyConfigurationChange();
        }
    }

    public override string Key { get; set; } = "RetainerRetrieveOrder";
    public override string Name { get; set; } = "Retainer Retrieve Order";
    public override string HelpText { get; set; } = "When displaying the items for a craft, if there are items to be retrieved should we display this before or after the shortfall is made up. If first is selected, it will make you retrieve items first, if last is selected, any missing items you'll need will have to be collected/purchased before the remainder will be shown for retrieval.";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;
    public override RetainerRetrieveOrder DefaultValue { get; set; } = RetainerRetrieveOrder.RetrieveFirst;
    public override List<RetainerRetrieveOrder> GetChoices(FilterConfiguration configuration)
    {
        return new List<RetainerRetrieveOrder>()
        {
            RetainerRetrieveOrder.RetrieveFirst,
            RetainerRetrieveOrder.RetrieveLast
        };
    }

    public override string GetFormattedChoice(RetainerRetrieveOrder choice)
    {
        switch (choice)
        {
            case RetainerRetrieveOrder.RetrieveFirst:
                return "Retrieve First";
                break;
            case RetainerRetrieveOrder.RetrieveLast:
                return "Retrieve Last";
        }
        return "Unknown";
    }
}
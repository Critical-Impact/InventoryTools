using System.Collections.Generic;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;

namespace InventoryTools.Logic.Filters;

public class CraftDefaultRetrieveFromRetainerFilter : ChoiceFilter<CraftRetainerRetrieval>
{
    public override CraftRetainerRetrieval CurrentValue(FilterConfiguration configuration)
    {
        return configuration.CraftList.CraftRetainerRetrieval;
    }

    public override void ResetFilter(FilterConfiguration configuration)
    {
        UpdateFilterConfiguration(configuration, DefaultValue);
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, CraftRetainerRetrieval newValue)
    {
        configuration.CraftList.CraftRetainerRetrieval = newValue;
        configuration.NotifyConfigurationChange();
    }

    public override string Key { get; set; } = "CraftDefaultRetrieveFromRetainerOutput";
    public override string Name { get; set; } = "Retainer Retrieval";

    public override string HelpText { get; set; } =
        "What should the default 'Retrieve from Retainer' setting be for 'Non-output' items?";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;
    public override CraftRetainerRetrieval DefaultValue { get; set; } = CraftRetainerRetrieval.Yes;
    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
    {
        return null;
    }

    public override List<CraftRetainerRetrieval> GetChoices(FilterConfiguration configuration)
    {
        return new List<CraftRetainerRetrieval>()
        {
            CraftRetainerRetrieval.No,
            CraftRetainerRetrieval.Yes,
            CraftRetainerRetrieval.HQOnly
        };
    }

    public override string GetFormattedChoice(CraftRetainerRetrieval choice)
    {
        return choice.FormattedName();
    }
}
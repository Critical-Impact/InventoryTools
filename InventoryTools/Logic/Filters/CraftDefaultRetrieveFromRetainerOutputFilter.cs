using System.Collections.Generic;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftDefaultRetrieveFromRetainerOutputFilter : ChoiceFilter<CraftRetainerRetrieval>
{
    public override CraftRetainerRetrieval CurrentValue(FilterConfiguration configuration)
    {
        return configuration.CraftList.CraftRetainerRetrievalOutput;
    }

    public override void ResetFilter(FilterConfiguration configuration)
    {
        UpdateFilterConfiguration(configuration, DefaultValue);
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, CraftRetainerRetrieval newValue)
    {
        configuration.CraftList.CraftRetainerRetrievalOutput = newValue;
        configuration.NotifyConfigurationChange();
    }

    public override string Key { get; set; } = "CraftDefaultRetrieveFromRetainer";
    public override string Name { get; set; } = "Retainer Retrieval (Output)";

    public override string HelpText { get; set; } =
        "What should the default 'Retrieve from Retainer' setting be for 'Output' items?";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Settings;
    public override CraftRetainerRetrieval DefaultValue { get; set; } = CraftRetainerRetrieval.No;
    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
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

    public override string GetFormattedChoice(FilterConfiguration filterConfiguration, CraftRetainerRetrieval choice)
    {
        return choice.FormattedName();
    }

    public CraftDefaultRetrieveFromRetainerOutputFilter(ILogger<CraftDefaultRetrieveFromRetainerOutputFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}
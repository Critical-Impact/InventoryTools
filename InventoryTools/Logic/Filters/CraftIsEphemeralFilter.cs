using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftIsEphemeralFilter : BooleanFilter
{
    public override string Key { get; set; } = "CraftIsEphemeral";
    public override string Name { get; set; } = "Ephemeral?";

    public override string HelpText { get; set; } =
        "Is this craft list ephemeral? If checked, once all the items in the craft list are deleted, the list will delete itself. This is only checked as each craft is completed.";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Settings;
    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        return null;
    }

    public override bool? CurrentValue(FilterConfiguration configuration)
    {
        return configuration.IsEphemeralCraftList;
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, bool? newValue)
    {
        configuration.IsEphemeralCraftList = newValue ?? false;
    }

    private readonly string[] _choices = new []{"Yes", "No"};

    public override string[] GetChoices()
    {
        return _choices;
    }

    public override bool? DefaultValue { get; set; } = false;

    public CraftIsEphemeralFilter(ILogger<CraftIsEphemeralFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}
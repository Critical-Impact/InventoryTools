using System.Collections.Generic;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftListModeFilter : ChoiceFilter<CraftListMode>
{
    public CraftListModeFilter(ILogger<CraftListMode> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override CraftListMode CurrentValue(FilterConfiguration configuration)
    {
        return configuration.CraftList.CraftListMode;
    }

    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        return null;
    }

    public override void ResetFilter(FilterConfiguration configuration)
    {
        this.UpdateFilterConfiguration(configuration, DefaultValue);
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, CraftListMode newValue)
    {
        configuration.CraftList.CraftListMode = newValue;
    }

    public override string Key { get; set; } = "CraftListMode";
    public override string Name { get; set; } = "Craft List Mode";

    public override string HelpText { get; set; } =
        "Should the craft list operate normally or in stocking mode. In normal mode, a quantity is entered and as you craft that number goes down. In stock mode, a quantity is entered and the number goes up based on the items within your characters inventory.";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Settings;

    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;
    public override CraftListMode DefaultValue { get; set; } = CraftListMode.Normal;
    public override List<CraftListMode> GetChoices(FilterConfiguration configuration)
    {
        return new List<CraftListMode>()
        {
            CraftListMode.Normal,
            CraftListMode.Stock,
        };
    }

    public override string GetFormattedChoice(FilterConfiguration filterConfiguration, CraftListMode choice)
    {
        return choice.ToString();
    }
}
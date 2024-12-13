using System.Collections.Generic;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftCraftCompletionModeFilter : ChoiceFilter<CraftCompletionMode>
{
    public override CraftCompletionMode CurrentValue(FilterConfiguration configuration)
    {
        return configuration.CraftList.CraftCompletionMode;
    }

    public override void ResetFilter(FilterConfiguration configuration)
    {
        configuration.CraftList.CraftCompletionMode = DefaultValue;
    }

    public override void UpdateFilterConfiguration(FilterConfiguration configuration, CraftCompletionMode newValue)
    {
        configuration.CraftList.CraftCompletionMode = newValue;
    }

    public override string Key { get; set; } = "HideCompletedMode";
    public override string Name { get; set; } = "Craft Completion Mode";

    public override string HelpText { get; set; } =
        "When an output reaches 0, should it be deleted or just be hidden(when Hide Completed is checked).";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Settings;
    public override CraftCompletionMode DefaultValue { get; set; } = CraftCompletionMode.Delete;
    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        return null;
    }

    public override List<CraftCompletionMode> GetChoices(FilterConfiguration configuration)
    {
        return new List<CraftCompletionMode>()
        {
            CraftCompletionMode.Delete,
            CraftCompletionMode.DoNothing
        };
    }

    public override string GetFormattedChoice(FilterConfiguration filterConfiguration, CraftCompletionMode choice)
    {
        switch (choice)
        {
            case(CraftCompletionMode.Delete):
                return "Delete";
            case(CraftCompletionMode.DoNothing):
                return "Do Nothing";
        }

        return choice.ToString();
    }

    public CraftCraftCompletionModeFilter(ILogger<CraftCraftCompletionModeFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}
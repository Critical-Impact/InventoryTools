using System.Linq;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class IsRecipeCompletedFilter : BooleanFilter
{
    private readonly IQuestManagerService _questManagerService;
    public override string Key { get; set; } = "IsRecipeCompleted";
    public override string Name { get; set; } = "Are Recipes Completed?";
    public override string HelpText { get; set; } = "Have the recipes that make this item been completed?";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Crafting;

    public IsRecipeCompletedFilter(ILogger<IsRecipeCompletedFilter> logger, ImGuiService imGuiService, IQuestManagerService questManagerService) : base(logger, imGuiService)
    {
        _questManagerService = questManagerService;
    }

    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return FilterItem(configuration, item.Item);
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        var currentValue = CurrentValue(configuration);
        if (currentValue == null)
        {
            return null;
        }

        if (!item.CanBeCrafted)
        {
            return false;
        }

        switch (currentValue.Value)
        {
            case false:
                return !item.Recipes.All(c => _questManagerService.IsRecipeComplete(c.RowId));
            case true:
                return item.Recipes.All(c => _questManagerService.IsRecipeComplete(c.RowId));
        }
    }
}
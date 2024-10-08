using System.Linq;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class IsRecipeCompletedColumn : CheckboxColumn
{
    private readonly IQuestManagerService _questManagerService;

    public IsRecipeCompletedColumn(ILogger<IsRecipeCompletedColumn> logger, IQuestManagerService questManagerService, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        _questManagerService = questManagerService;
    }

    public override ColumnCategory ColumnCategory => ColumnCategory.Crafting;
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;

    public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        if (!searchResult.Item.CanBeCrafted)
        {
            return null;
        }

        return searchResult.Item.RecipesAsResult.All(c => _questManagerService.IsRecipeComplete(c.RowId));
    }

    public override string Name { get; set; } = "Are Recipes Completed?";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "Have the recipes that make this item been completed?";
}
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class RecipeTotalColumn : IntegerColumn
{
    public RecipeTotalColumn(ILogger<RecipeTotalColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
    public override ColumnCategory ColumnCategory { get; } = ColumnCategory.Crafting;
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    public override int? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        return searchResult.Item.RecipesAsRequirement.Count;
    }

    public override string Name { get; set; } = "Recipe Total Count";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "The number of recipes the item is a component of.";
}
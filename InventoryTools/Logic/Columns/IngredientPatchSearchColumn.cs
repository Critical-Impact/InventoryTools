using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Sheets.Rows;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class IngredientPatchSearchColumn : DecimalColumn
{
    private readonly IngredientPatchService _ingredientPatchService;

    public IngredientPatchSearchColumn(IngredientPatchService ingredientPatchService, ILogger<IngredientPatchSearchColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        _ingredientPatchService = ingredientPatchService;
    }

    public override ColumnCategory ColumnCategory { get; } = ColumnCategory.Tools;
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

    public override decimal? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        if (!searchResult.Item.HasUsesByType(ItemInfoType.CraftRecipe))
        {
            return null;
        }

        return _ingredientPatchService.IngredientPatches.GetValueOrDefault(searchResult.ItemId);
    }



    public override string Name { get; set; } = "Ingredient Patch Search";
    public override float Width { get; set; } = 100;

    public override string HelpText { get; set; } =
        "Shows a number indicating the highest patch a craft material is used in.";
}
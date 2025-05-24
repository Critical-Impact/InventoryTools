using System;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.GenericFilters;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class IngredientPatchSearchFilter : GenericDecimalFilter
{
    public
        IngredientPatchSearchFilter(IngredientPatchService ingredientPatchService,
            ILogger<IngredientPatchSearchFilter> logger, ImGuiService imGuiService) : base(
        "IngredientPatchSearch",
        "Ingredient Patch Search",
        "Shows a number indicating the highest patch a craft material is used in.",
        FilterCategory.Crafting,
        item => ingredientPatchService.IngredientPatches.TryGetValue(item.Item.RowId, out var value) ? value : null,
        item => ingredientPatchService.IngredientPatches.TryGetValue(item.RowId, out var value) ? value : null,
        logger,
        imGuiService)
    {
    }
}
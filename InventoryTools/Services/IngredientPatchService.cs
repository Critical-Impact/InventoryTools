using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;

namespace InventoryTools.Services;

public class IngredientPatchService(ItemSheet itemSheet)
{
    private Dictionary<uint, decimal>? _ingredientPatches;

    public Dictionary<uint, decimal> IngredientPatches => _ingredientPatches ??= CalculateHighestPatch();

    private Dictionary<uint, decimal> CalculateHighestPatch()
    {
        Dictionary<uint, decimal> ingredientPatches = new();
        foreach (var item in itemSheet)
        {
            if (!item.HasUsesByType(ItemInfoType.CraftRecipe))
            {
                continue;
            }

            AddToIngredientList(ingredientPatches, item, null);
        }
        return ingredientPatches;
    }
    private void AddToIngredientList(Dictionary<uint, decimal> patchDictionary, ItemRow baseItem, ItemRow? itemRow)
    {
        var actualItem = itemRow ?? baseItem;
        if (!actualItem.HasUsesByType(ItemInfoType.CraftRecipe, ItemInfoType.FreeCompanyCraftRecipe))
        {
            return;
        }

        foreach (var requirement in actualItem.RecipesAsRequirement.Select(c => c.ItemResult))
        {
            if (requirement == null)
            {
                continue;
            }

            if (!patchDictionary.TryGetValue(baseItem.RowId, out var value) || value < requirement.Patch)
            {
                value = requirement.Patch;
                patchDictionary[baseItem.RowId] = value;
            }
            AddToIngredientList(patchDictionary, baseItem, requirement);
        }
    }
}
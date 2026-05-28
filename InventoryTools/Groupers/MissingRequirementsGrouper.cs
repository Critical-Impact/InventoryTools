using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Services;

namespace InventoryTools.Groupers;

public class MissingRequirementsGrouper
{
    private readonly IItemObtainabilityService _obtainabilityService;
    private readonly RecipeSheet _recipeSheet;

    public MissingRequirementsGrouper(IItemObtainabilityService obtainabilityService, RecipeSheet recipeSheet)
    {
        _obtainabilityService = obtainabilityService;
        _recipeSheet = recipeSheet;
    }

    public IReadOnlyList<MissingRequirementGroup> GetMissingRequirements(CraftList craftList)
    {
        var groups = new Dictionary<(string, uint), (global::Lumina.Excel.RowRef? RowRef, ObtainabilityRequirementType Type, HashSet<string> Names)>();

        foreach (var craftItem in craftList.GetFlattenedMaterials())
        {
            var preferenceType = craftItem.IngredientPreference.Type;

            if (preferenceType != IngredientPreferenceType.Crafting
                && preferenceType != IngredientPreferenceType.Mining
                && preferenceType != IngredientPreferenceType.Botany
                && preferenceType != IngredientPreferenceType.Fishing
                && preferenceType != IngredientPreferenceType.SpearFishing)
            {
                continue;
            }

            RecipeRow? recipe = null;
            if (preferenceType == IngredientPreferenceType.Crafting)
            {
                var recipeId = craftList.CraftRecipePreferences.TryGetValue(craftItem.ItemId, out var prefId)
                    ? prefId
                    : _recipeSheet.GetRecipesByItemId(craftItem.ItemId)?.FirstOrDefault()?.RowId;
                if (recipeId.HasValue)
                {
                    recipe = _recipeSheet.GetRowOrDefault(recipeId.Value);
                }
            }

            var requirements = _obtainabilityService.GetRequirements(craftItem.Item, preferenceType, recipe);
            foreach (var req in requirements)
            {
                if (req.IsMet) continue;
                var rowId = req.RowRef?.RowId ?? 0;
                var key = (req.Description, rowId);
                if (!groups.TryGetValue(key, out var group))
                {
                    group = (req.RowRef, req.Type, new HashSet<string>());
                    groups[key] = group;
                }
                group.Names.Add(craftItem.Name);
            }
        }

        return groups.Select(kv => new MissingRequirementGroup(
            kv.Value.Type,
            kv.Key.Item1,
            kv.Value.RowRef,
            kv.Value.Names.ToList()
        )).OrderBy(c => c.Type).ThenBy(c => c.RowRef?.RowType ?? null).ToList();
    }
}

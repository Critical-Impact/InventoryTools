using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;

namespace InventoryTools.Localizers;

public class CraftItemLocalizer
{
    private readonly IngredientPreferenceLocalizer _ingredientPreferenceLocalizer;

    public CraftItemLocalizer(IngredientPreferenceLocalizer ingredientPreferenceLocalizer)
    {
        _ingredientPreferenceLocalizer = ingredientPreferenceLocalizer;
    }

    public int SourceIcon(CraftItem craftItem)
    {
        return craftItem.IngredientPreference.Type switch
        {
            IngredientPreferenceType.Crafting => craftItem.Recipe?.CraftType?.Icon ?? Icons.CraftIcon,
            IngredientPreferenceType.None => craftItem.Item.Icon,
            _ => _ingredientPreferenceLocalizer.SourceIcon(craftItem.IngredientPreference)!.Value
        };
    }

    public string SourceName(CraftItem craftItem)
    {
        return craftItem.IngredientPreference.Type switch
        {
            IngredientPreferenceType.Crafting => craftItem.Recipe?.CraftType?.FormattedName ?? (craftItem.Item.CompanyCraftSequence != null ? "Company Craft" : "Unknown"),
            IngredientPreferenceType.None => "N/A",
            _ => _ingredientPreferenceLocalizer.FormattedName(craftItem.IngredientPreference)
        };
    }

}
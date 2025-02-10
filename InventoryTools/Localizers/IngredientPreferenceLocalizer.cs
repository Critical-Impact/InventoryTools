using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;

namespace InventoryTools.Localizers;

public class IngredientPreferenceLocalizer
{
    private readonly ItemSheet _itemSheet;
    private readonly CraftTypeSheet _craftTypeSheet;

    public IngredientPreferenceLocalizer(ItemSheet itemSheet, CraftTypeSheet craftTypeSheet)
    {
        _itemSheet = itemSheet;
        _craftTypeSheet = craftTypeSheet;
    }

    public string FormattedName(IngredientPreference ingredientPreference)
    {
        switch (ingredientPreference.Type)
        {
            case IngredientPreferenceType.Item:
                if (ingredientPreference.LinkedItemId != null && ingredientPreference.LinkedItemQuantity != null)
                {
                    string? itemName2 = null;
                    string? itemName3 = null;
                    if (ingredientPreference.LinkedItem2Id != null && ingredientPreference.LinkedItem2Quantity != null)
                    {
                        if (ingredientPreference.LinkedItem3Id != null &&
                            ingredientPreference.LinkedItem3Quantity != null)
                        {
                            itemName3 =
                                (_itemSheet.GetRow(ingredientPreference.LinkedItem3Id.Value)
                                    ?.NameString ?? "Unknown Item") + " - " +
                                ingredientPreference.LinkedItem3Quantity.Value;
                        }

                        itemName2 =
                            (_itemSheet.GetRow(ingredientPreference.LinkedItem2Id.Value)
                                ?.NameString ?? "Unknown Item") + " - " +
                            ingredientPreference.LinkedItem2Quantity.Value;
                    }

                    var itemName =
                        _itemSheet.GetRow(ingredientPreference.LinkedItemId.Value)?.NameString ??
                        "Unknown Item";
                    if (itemName3 != null)
                    {
                        itemName = itemName + "," + itemName2 + "," + itemName3;
                    }
                    else if (itemName2 != null)
                    {
                        itemName = itemName + "," + itemName2;
                    }

                    return itemName + " - " + ingredientPreference.LinkedItemQuantity.Value;
                }

                return "No item selected";
            case IngredientPreferenceType.Reduction:
                if (ingredientPreference.LinkedItemId != null && ingredientPreference.LinkedItemQuantity != null)
                {
                    var itemName =
                        _itemSheet.GetRow(ingredientPreference.LinkedItemId.Value)?.NameString ??
                        "Unknown Item";
                    return "Reduction (" + itemName + " - " + ingredientPreference.LinkedItemQuantity.Value + ")";
                }

                return "No item selected";
        }

        return ingredientPreference.Type.FormattedName();
    }

    public int? SourceIcon(IngredientPreference ingredientPreference)
    {
        return ingredientPreference.Type switch
        {
            IngredientPreferenceType.Buy => Icons.BuyIcon,
            IngredientPreferenceType.HouseVendor => Icons.BuyIcon,
            IngredientPreferenceType.Botany => Icons.BotanyIcon,
            IngredientPreferenceType.Crafting => _craftTypeSheet
                .GetRow(ingredientPreference.RecipeCraftTypeId ?? 0)?.Icon ?? Icons.CraftIcon,
            IngredientPreferenceType.Desynthesis => Icons.DesynthesisIcon,
            IngredientPreferenceType.Fishing => Icons.FishingIcon,
            IngredientPreferenceType.Item => ingredientPreference.LinkedItemId != null
                ? _itemSheet.GetRowOrDefault(ingredientPreference.LinkedItemId.Value)?.Icon ??
                  Icons.SpecialItemIcon
                : Icons.SpecialItemIcon,
            IngredientPreferenceType.Marketboard => Icons.MarketboardIcon,
            IngredientPreferenceType.Mining => Icons.MiningIcon,
            IngredientPreferenceType.Mobs => Icons.MobIcon,
            IngredientPreferenceType.None => null,
            IngredientPreferenceType.Reduction => Icons.ReductionIcon,
            IngredientPreferenceType.Venture => Icons.VentureIcon,
            IngredientPreferenceType.ExplorationVenture => Icons.VentureIcon,
            IngredientPreferenceType.Empty => Icons.RedXIcon,
            IngredientPreferenceType.ResourceInspection => Icons.SkybuildersScripIcon,
            IngredientPreferenceType.Gardening => Icons.SproutIcon,
            _ => Icons.QuestionMarkIcon
        };
    }
}
using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns;

public class NameIconColumn : TextIconColumn
{
    public override (string, ushort, bool)? CurrentValue(InventoryItem item)
    {
        return (item.Item.Name, item.Icon, item.IsHQ);
    }

    public override (string, ushort, bool)? CurrentValue(ItemEx item)
    {
        return (item.Name, item.Icon, false);
    }

    public override (string, ushort, bool)? CurrentValue(SortingResult item)
    {
        return (item.InventoryItem.Item.Name, item.InventoryItem.Item.Icon, item.InventoryItem.IsHQ);
    }
    
    public override void Draw(FilterConfiguration configuration, CraftItem item, int rowIndex)
    {
        base.Draw(configuration, item, rowIndex);
        if (item.IsOutputItem)
        {
            if (Service.ExcelCache.ItemRecipes.ContainsKey(item.ItemId))
            {
                var itemRecipes = Service.ExcelCache.ItemRecipes[item.ItemId];
                if (itemRecipes.Count != 1)
                {
                    var actualRecipes = itemRecipes.Select(c => Service.ExcelCache.GetRecipeExSheet().GetRow(c)!)
                        .OrderBy(c => c.CraftType.Value?.Name ?? "").ToList();
                    var value = item.Recipe?.CraftType.Value?.Name ?? "";
                    ImGui.SameLine();
                    if (ImGui.BeginCombo("##SetRecipe" + rowIndex, value))
                    {
                        foreach (var recipe in actualRecipes)
                        {
                            if (ImGui.Selectable(recipe.CraftType.Value?.Name ?? "",
                                    value == (recipe.CraftType.Value?.Name ?? "")))
                            {
                                configuration.CraftList.SetCraftRecipe(item.ItemId, recipe.RowId);
                                configuration.StartRefresh();
                            }
                        }

                        ImGui.EndCombo();
                    }
                }
            }
        }
    }

    public override string Name { get; set; } = "Name & Icon";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "The name of the item with the icon next to it.";
    public override string FilterText { get; set; } = "";
    public override bool HasFilter { get; set; } = false;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
}
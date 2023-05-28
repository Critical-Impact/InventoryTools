using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using OtterGui.Raii;

namespace InventoryTools.Logic.Columns;

public class NameIconColumn : TextIconColumn
{
    public override (string, ushort, bool)? CurrentValue(InventoryItem item)
    {
        return (item.Item.NameString, item.Icon, item.IsHQ);
    }

    public override (string, ushort, bool)? CurrentValue(ItemEx item)
    {
        return (item.NameString, item.Icon, false);
    }

    public override (string, ushort, bool)? CurrentValue(SortingResult item)
    {
        return (item.InventoryItem.Item.NameString, item.InventoryItem.Item.Icon, item.InventoryItem.IsHQ);
    }
    
    public override dynamic? JsonExport(InventoryItem item)
    {
        return CurrentValue(item)?.Item1 ?? "";
    }

    public override dynamic? JsonExport(ItemEx item)
    {
        return CurrentValue(item)?.Item1 ?? "";
    }

    public override dynamic? JsonExport(SortingResult item)
    {
        return CurrentValue(item)?.Item1 ?? "";
    }

    public override dynamic? JsonExport(CraftItem item)
    {
        return CurrentValue(item)?.Item1 ?? "";
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
                    using (var combo = ImRaii.Combo("##SetRecipe" + rowIndex, value))
                    {
                        if (combo.Success)
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
                        }
                    }
                }
            }
        }
    }

    public override string Name { get; set; } = "Name & Icon";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "The name of the item with the icon next to it.";
    public override bool HasFilter { get; set; } = false;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
}
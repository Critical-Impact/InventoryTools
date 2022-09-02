using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns
{
    public class NameColumn : ColoredTextColumn
    {
        public override (string, Vector4)? CurrentValue(InventoryItem item)
        {
            return (item.FormattedName, item.ItemColour);
        }

        public override (string, Vector4)? CurrentValue(ItemEx item)
        {
            return (item.NameString, ImGuiColors.DalamudWhite);
        }

        public override (string, Vector4)? CurrentValue(SortingResult item)
        {
            return CurrentValue(item.InventoryItem);
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

        public override (string, Vector4)? CurrentValue(CraftItem currentValue)
        {
            return (currentValue.FormattedName, ImGuiColors.DalamudWhite);
        }

        public override string Name { get; set; } = "Name";
        public override float Width { get; set; } = 250.0f;
        public override string HelpText { get; set; } = "The name of the item.";
        public override string FilterText { get; set; } = "";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}
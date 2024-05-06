using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class NameIconColumn : TextIconColumn
{
    private readonly ExcelCache _excelCache;

    public NameIconColumn(ILogger<NameIconColumn> logger, ImGuiService imGuiService, ExcelCache excelCache) : base(logger, imGuiService)
    {
        _excelCache = excelCache;
    }
    public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
    public override (string, ushort, bool)? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
    {
        return (item.Item.NameString, item.Icon, item.IsHQ);
    }

    public override (string, ushort, bool)? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
    {
        return (item.NameString, item.Icon, false);
    }

    public override (string, ushort, bool)? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
    {
        return (item.InventoryItem.Item.NameString, item.InventoryItem.Item.Icon, item.InventoryItem.IsHQ);
    }
    
    public override dynamic? JsonExport(ColumnConfiguration columnConfiguration, InventoryItem item)
    {
        return CurrentValue(columnConfiguration, item)?.Item1 ?? "";
    }

    public override dynamic? JsonExport(ColumnConfiguration columnConfiguration, ItemEx item)
    {
        return CurrentValue(columnConfiguration, item)?.Item1 ?? "";
    }

    public override dynamic? JsonExport(ColumnConfiguration columnConfiguration, SortingResult item)
    {
        return CurrentValue(columnConfiguration, item)?.Item1 ?? "";
    }

    public override dynamic? JsonExport(ColumnConfiguration columnConfiguration, CraftItem item)
    {
        return CurrentValue(columnConfiguration, item)?.Item1 ?? "";
    }
    
    public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
        CraftItem item, int rowIndex)
    {
        base.Draw(configuration, columnConfiguration, item, rowIndex);
        if (item.IsOutputItem)
        {
            if (_excelCache.ItemRecipes.ContainsKey(item.ItemId))
            {
                var itemRecipes = _excelCache.ItemRecipes[item.ItemId];
                if (itemRecipes.Count != 1)
                {
                    var actualRecipes = itemRecipes.Select(c => _excelCache.GetRecipeExSheet().GetRow(c)!)
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
                                    configuration.NeedsRefresh = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        return null;
    }

    public override string Name { get; set; } = "Name & Icon";
    public override string RenderName => "Name";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "The name of the item with the icon next to it.";
    public override bool HasFilter { get; set; } = false;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
}
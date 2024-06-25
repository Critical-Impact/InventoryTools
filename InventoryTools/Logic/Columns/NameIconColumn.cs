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
    public override (string, ushort, bool)? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        return (
            searchResult.CraftItem?.FormattedName ??
            searchResult.InventoryItem?.FormattedName ?? searchResult.Item.NameString,
            searchResult.InventoryItem?.Icon ?? searchResult.Item.Icon,
            searchResult.InventoryItem?.IsHQ ?? false);
    }

    public override dynamic? JsonExport(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        return CurrentValue(columnConfiguration, searchResult)?.Item1 ?? "";
    }
    
    public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
        SearchResult searchResult, int rowIndex, int columnIndex)
    {
        base.Draw(configuration, columnConfiguration, searchResult, rowIndex, columnIndex);
        if (searchResult.CraftItem != null && searchResult.CraftItem.IsOutputItem)
        {
            if (_excelCache.ItemRecipes.ContainsKey(searchResult.CraftItem.ItemId))
            {
                var itemRecipes = _excelCache.ItemRecipes[searchResult.CraftItem.ItemId];
                if (itemRecipes.Count != 1)
                {
                    var actualRecipes = itemRecipes.Select(c => _excelCache.GetRecipeExSheet().GetRow(c)!)
                        .OrderBy(c => c.CraftType.Value?.Name ?? "").ToList();
                    var value = searchResult.CraftItem.Recipe?.CraftType.Value?.Name ?? "";
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
                                    configuration.CraftList.SetCraftRecipe(searchResult.CraftItem.ItemId, recipe.RowId);
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
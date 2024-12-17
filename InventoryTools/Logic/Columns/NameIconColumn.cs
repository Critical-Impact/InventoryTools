using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Logic.Settings;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class NameIconColumn : TextIconColumn
{
    private readonly ImGuiTooltipService _tooltipService;
    private readonly ImGuiTooltipModeSetting _tooltipModeSetting;
    private readonly InventoryToolsConfiguration _configuration;

    public NameIconColumn(ILogger<NameIconColumn> logger, ImGuiTooltipService tooltipService, ImGuiTooltipModeSetting tooltipModeSetting, ImGuiService imGuiService, InventoryToolsConfiguration configuration) : base(logger, imGuiService)
    {
        _tooltipService = tooltipService;
        _tooltipModeSetting = tooltipModeSetting;
        _configuration = configuration;
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
        _tooltipService.DrawItemTooltip(searchResult);
        if (_tooltipModeSetting.CurrentValue(_configuration) == ImGuiTooltipMode.Icons)
        {
            if (ImGui.IsItemHovered())
            {
                _tooltipService.DrawItemTooltip(searchResult);
            }
        }
        if (searchResult.CraftItem != null && searchResult.CraftItem.IsOutputItem)
        {
            var itemRecipes = searchResult.Item.Recipes
                .OrderBy(c => c.CraftType?.FormattedName ?? "").ToList();

            if (itemRecipes.Count != 1)
            {
                var value = searchResult.CraftItem.Recipe?.CraftType?.FormattedName ?? "";
                ImGui.SameLine();
                using (var combo = ImRaii.Combo("##SetRecipe" + rowIndex, value))
                {
                    if (combo.Success)
                    {
                        foreach (var recipe in itemRecipes)
                        {
                            if (ImGui.Selectable(recipe.CraftType?.FormattedName ?? "",
                                    value == (recipe.CraftType?.FormattedName ?? "")))
                            {
                                configuration.CraftList.SetCraftRecipe(searchResult.CraftItem.ItemId, recipe.RowId);
                                configuration.NeedsRefresh = true;
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
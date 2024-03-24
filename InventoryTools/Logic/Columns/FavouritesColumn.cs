using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using Dalamud.Plugin.Services;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui;

namespace InventoryTools.Logic.Columns;

public class FavouritesColumn : CheckboxColumn
{
    private readonly InventoryToolsConfiguration _configuration;

    public FavouritesColumn(ILogger<FavouritesColumn> logger, ImGuiService imGuiService, InventoryToolsConfiguration configuration) : base(logger, imGuiService)
    {
        _configuration = configuration;
    }
    public override ColumnCategory ColumnCategory { get; } = ColumnCategory.Basic;
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;

    public override bool? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
    {
        return _configuration.FavouriteItemsList.Contains(item.ItemId);
    }

    public override bool? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
    {
        return _configuration.FavouriteItemsList.Contains(item.RowId);
    }

    public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
    {
        return _configuration.FavouriteItemsList.Contains(item.InventoryItem.ItemId);
    }

    public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
        ItemEx item,
        int rowIndex)
    {
        base.Draw(configuration, columnConfiguration, item, rowIndex);
        return PostDraw(item.RowId);
    }

    public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
        CraftItem item, int rowIndex)
    {
        base.Draw(configuration, columnConfiguration, item, rowIndex);
        return PostDraw(item.ItemId);
    }

    public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
        InventoryChange item, int rowIndex)
    {
        base.Draw(configuration, columnConfiguration, item, rowIndex);
        return PostDraw(item.InventoryItem.ItemId);
    }

    public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
        InventoryItem item, int rowIndex)
    {
        base.Draw(configuration, columnConfiguration, item, rowIndex);
        return PostDraw(item.ItemId);
    }

    public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
        SortingResult item, int rowIndex)
    {
        base.Draw(configuration, columnConfiguration, item, rowIndex);
        return PostDraw(item.InventoryItem.ItemId);
    }

    public List<MessageBase>? PostDraw(uint itemId)
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
        {
            _configuration.ToggleFavouriteItem(itemId);
        }
        ImGuiUtil.HoverTooltip("Click to favourite/unfavourite.");
        return null;
    }

    public override string Name { get; set; } = "Favourite?";
    public override float Width { get; set; } = 80;
    public override string HelpText { get; set; } = "Is this item in your list of favourites?";
}
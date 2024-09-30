using System.Collections.Generic;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
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
    
    public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        return _configuration.FavouriteItemsList.Contains(searchResult.Item.RowId);
    }

    public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
        SearchResult searchResult,
        int rowIndex, int columnIndex)
    {
        base.Draw(configuration, columnConfiguration, searchResult, rowIndex, columnIndex);
        return PostDraw(searchResult.Item.RowId);
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

    public override FilterType DefaultIn => Logic.FilterType.SearchFilter | Logic.FilterType.SortingFilter | Logic.FilterType.GameItemFilter;
}
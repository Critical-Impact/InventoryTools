using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class FavouritesFilter : BooleanFilter
{
    private readonly InventoryToolsConfiguration _configuration;
    public override string Key { get; set; } = "favourites";
    public override string Name { get; set; } = "Is Favourite?";
    public override string HelpText { get; set; } = "Is this item a favourite?";
    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Searching;

    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        var currentValue = CurrentValue(configuration);
        if (currentValue != null)
        {
            return currentValue.Value && _configuration.IsFavouriteItem(item.ItemId) || !currentValue.Value && !_configuration.IsFavouriteItem(item.ItemId);
        }

        return null;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        var currentValue = CurrentValue(configuration);
        if (currentValue != null)
        {
            return currentValue.Value && _configuration.IsFavouriteItem(item.RowId) || !currentValue.Value && !_configuration.IsFavouriteItem(item.RowId);
        }

        return null;
    }

    public FavouritesFilter(ILogger<FavouritesFilter> logger, ImGuiService imGuiService, InventoryToolsConfiguration configuration) : base(logger, imGuiService)
    {
        _configuration = configuration;
    }
}
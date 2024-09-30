using System.Collections.Generic;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class CraftZoneColumn : TextColumn
{
    private readonly ExcelCache _excelCache;

    public CraftZoneColumn(ILogger<CraftZoneColumn> logger, ExcelCache excelCache, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        _excelCache = excelCache;
    }

    public override string Name { get; set; } = "Zone";
    public override float Width { get; set; } = 100;

    public override bool? CraftOnly { get; } = true;

    public override string HelpText { get; set; } =
        "The zone in which this item should be gathered from. This is only relevant to craft lists.";

    public override ColumnCategory ColumnCategory { get; } = ColumnCategory.Crafting;
    public override string? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        return "";
    }

    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

    public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration, SearchResult searchResult, int rowIndex,
        int columnIndex)
    {
        if (searchResult.CraftItem == null)
        {
            return null;
        }

        var item = searchResult.CraftItem;

        var mapIds = item.Item.GetSourceMaps(item.IngredientPreference.Type, item.IngredientPreference.LinkedItemId)
            .OrderBySequence(configuration.CraftList.ZonePreferenceOrder, location => location);
        MapEx? selectedLocation = null;
        uint? mapPreference;
        if (item.IngredientPreference.Type == IngredientPreferenceType.Buy)
        {
            mapPreference = configuration.CraftList.ZoneBuyPreferences.TryGetValue(item.ItemId, out var preference)
                ? preference
                : null;
        }
        else if (item.IngredientPreference.Type == IngredientPreferenceType.Item)
        {
            mapPreference = configuration.CraftList.ZoneItemPreferences.TryGetValue(item.ItemId, out var preference)
                ? preference
                : null;
        }
        else if (item.IngredientPreference.Type == IngredientPreferenceType.Botany)
        {
            mapPreference = configuration.CraftList.ZoneBotanyPreferences.TryGetValue(item.ItemId, out var preference)
                ? preference
                : null;
        }
        else if (item.IngredientPreference.Type == IngredientPreferenceType.Mining)
        {
            mapPreference = configuration.CraftList.ZoneMiningPreferences.TryGetValue(item.ItemId, out var preference)
                ? preference
                : null;
        }
        else if (item.IngredientPreference.Type == IngredientPreferenceType.Mobs)
        {
            mapPreference = configuration.CraftList.ZoneMobPreferences.TryGetValue(item.ItemId, out var preference)
                ? preference
                : null;
        }
        else
        {
            mapPreference = null;
        }

        foreach (var mapId in mapIds)
        {
            if (selectedLocation == null)
            {
                selectedLocation = _excelCache.GetMapSheet().GetRow(mapId);
            }

            if (mapPreference != null && mapPreference == mapId)
            {
                selectedLocation = _excelCache.GetMapSheet().GetRow(mapId);
                break;
            }
        }

        string currentValue = "";
        if (selectedLocation != null)
        {
            currentValue = selectedLocation.FormattedName;
        }

        return DoDraw(searchResult, currentValue, rowIndex, configuration, columnConfiguration);
    }
}
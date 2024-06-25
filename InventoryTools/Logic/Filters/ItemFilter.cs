using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class ItemFilter : UintMultipleChoiceFilter
{
    private readonly ExcelCache _excelCache;
    public override string Key { get; set; } = "ItemFilter";
    public override string Name { get; set; } = "Name (Selector)";

    public override string HelpText { get; set; } =
        "Select a list of items and the filter will only display these items. You are better served using a Curated List but this filter will still work.";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;
    public override List<uint> DefaultValue { get; set; } = new();

    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return FilterItem(configuration, item.Item);
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
    {
        var searchItems = CurrentValue(configuration).ToList();
        if (searchItems.Count == 0)
        {
            return null;
        }

        if (searchItems.Contains(item.RowId))
        {
            return true;
        }

        return false;
    }

    public override Dictionary<uint, string> GetChoices(FilterConfiguration configuration)
    {
        return _excelCache.ItemNamesById;
    }

    public override bool HideAlreadyPicked { get; set; }

    public ItemFilter(ILogger<ItemFilter> logger, ImGuiService imGuiService, ExcelCache excelCache) : base(logger, imGuiService)
    {
        _excelCache = excelCache;
    }
}
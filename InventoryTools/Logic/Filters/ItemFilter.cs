using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class ItemFilter : UintMultipleChoiceFilter
{
    private readonly ItemSheet _itemSheet;

    public ItemFilter(ILogger<ItemFilter> logger, ImGuiService imGuiService, ItemSheet itemSheet) : base(logger, imGuiService)
    {
        _itemSheet = itemSheet;
    }

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

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
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
        return _itemSheet.ItemsSearchStringsById;
    }

    public override bool HideAlreadyPicked { get; set; }
}
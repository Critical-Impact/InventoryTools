using System;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.GenericFilters;

public class GenericBooleanFilter : BooleanFilter, IGenericFilter
{
    private readonly Func<InventoryItem, bool>? _inventoryItemFunc;
    private readonly Func<ItemRow, bool>? _itemFunc;

    public delegate GenericBooleanFilter Factory(string key, string name, string helpText, FilterCategory filterCategory, Func<InventoryItem, bool>? inventoryItemFunc, Func<ItemRow, bool>? itemFunc);


    public GenericBooleanFilter(string key, string name, string helpText, FilterCategory filterCategory, Func<InventoryItem, bool>? inventoryItemFunc, Func<ItemRow, bool>? itemFunc, ILogger<GenericBooleanFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        _inventoryItemFunc = inventoryItemFunc;
        _itemFunc = itemFunc;
        Key = key;
        Name = name;
        HelpText = helpText;
        FilterCategory = filterCategory;
    }

    public sealed override string Key { get; set; }
    public sealed override string Name { get; set; }
    public sealed override string HelpText { get; set; }
    public sealed override FilterCategory FilterCategory { get; set; }
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        if (_inventoryItemFunc == null)
        {
            return FilterItem(configuration, item.Item);
        }
        var currentValue = CurrentValue(configuration);

        if (currentValue == null)
        {
            return null;
        }

        var result = _inventoryItemFunc?.Invoke(item);
        if (result == null)
        {
            return null;
        }
        return currentValue.Value && result.Value || !currentValue.Value && !result.Value;
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        var currentValue = CurrentValue(configuration);

        if (currentValue == null)
        {
            return null;
        }

        var result = _itemFunc?.Invoke(item);
        if (result == null)
        {
            return null;
        }
        return currentValue.Value && result.Value || !currentValue.Value && !result.Value;
    }
}
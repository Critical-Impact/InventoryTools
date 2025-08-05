using System;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Models;
using Dalamud.Interface.Colors;
using Dalamud.Bindings.ImGui;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui.Raii;

namespace InventoryTools.Logic.GenericFilters
{
    public class GenericIntegerFilter : StringFilter, IGenericFilter
    {
        private readonly Func<InventoryItem, int?>? _inventoryItemFunc;
        private readonly Func<ItemRow, int?>? _itemFunc;

        public delegate GenericIntegerFilter Factory(string key, string name, string helpText, FilterCategory filterCategory, Func<InventoryItem, int?>? inventoryItemFunc, Func<ItemRow, int?>? itemFunc);


        public GenericIntegerFilter(string key, string name, string helpText, FilterCategory filterCategory, Func<InventoryItem, int?>? inventoryItemFunc, Func<ItemRow, int?>? itemFunc, ILogger<GenericIntegerFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
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

            if (currentValue == string.Empty)
            {
                return null;
            }

            var result = _inventoryItemFunc?.Invoke(item);
            if (result == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(currentValue))
            {
                if (((int)result).PassesFilter(currentValue.ToLower()))
                {
                    return true;
                }

                return false;
            }
            return true;
        }



        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            var currentValue = CurrentValue(configuration);

            if (currentValue == string.Empty)
            {
                return null;
            }

            var result = _itemFunc?.Invoke(item);
            if (result == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(currentValue))
            {
                if (((int)result).PassesFilter(currentValue.ToLower()))
                {
                    return true;
                }

                return false;
            }
            return true;
        }
    }
}
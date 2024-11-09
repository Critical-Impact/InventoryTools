using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class ItemUiCategoryFilter : UintMultipleChoiceFilter
    {
        private readonly ExcelSheet<ItemUICategory> _itemUiCategorySheet;

        public ItemUiCategoryFilter(ILogger<ItemUiCategoryFilter> logger, ImGuiService imGuiService, ExcelSheet<ItemUICategory> itemUiCategorySheet) : base(logger, imGuiService)
        {
            _itemUiCategorySheet = itemUiCategorySheet;
        }

        public override string Key { get; set; } = "UiCategory";

        public override string Name { get; set; } = "Categories";

        public override string HelpText { get; set; } = "Filter by the categories the game gives items when you scroll over them.";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Searching;

        private Dictionary<uint, string> _choices = new();
        private bool _choicesLoaded = false;

        public override List<uint> DefaultValue { get; set; } = new();



        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return FilterItem(configuration, item.Item);
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            var currentValue = CurrentValue(configuration);
            if (currentValue.Count != 0 && !currentValue.Contains(item.Base.ItemUICategory.RowId))
            {
                return false;
            }

            return true;
        }

        public override Dictionary<uint, string> GetChoices(FilterConfiguration configuration)
        {
            if (!_choicesLoaded)
            {
                _choices = _itemUiCategorySheet.OrderBy(c => c.Name.ToString())
                    .ToDictionary(c => c.RowId, c => c.Name.ExtractText().ToString());
                _choicesLoaded = true;
            }

            return _choices;
        }

        public override Dictionary<uint, string> GetActiveChoices(FilterConfiguration configuration)
        {
            var choices = GetChoices(configuration);
            if (HideAlreadyPicked)
            {
                var currentChoices = CurrentValue(configuration);
                return choices.Where(c => !currentChoices.Contains(c.Key)).ToDictionary(c => c.Key, c => c.Value);
            }

            return choices;
        }

        public override bool HideAlreadyPicked { get; set; } = true;
    }
}
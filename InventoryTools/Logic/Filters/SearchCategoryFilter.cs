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
    public class SearchCategoryFilter : UintMultipleChoiceFilter
    {
        private readonly ExcelSheet<ItemSearchCategory> _itemSearchCategorySheet;

        public SearchCategoryFilter(ILogger<SearchCategoryFilter> logger, ImGuiService imGuiService, ExcelSheet<ItemSearchCategory> itemSearchCategorySheet) : base(logger, imGuiService)
        {
            _itemSearchCategorySheet = itemSearchCategorySheet;
        }

        public override string Key { get; set; } = "SchCategory";

        public override string Name { get; set; } = "Market Board Categories";

        public override string HelpText { get; set; } = "Filter by the categories available on the market board.";
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
            if (currentValue.Count != 0 && !currentValue.Contains(item.Base.ItemSearchCategory.RowId))
            {
                return false;
            }

            return true;
        }

        public override Dictionary<uint, string> GetChoices(FilterConfiguration configuration)
        {
            if (!_choicesLoaded)
            {
                _choices = _itemSearchCategorySheet.ToDictionary(c => c.RowId, c => c.Name.ExtractText());
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
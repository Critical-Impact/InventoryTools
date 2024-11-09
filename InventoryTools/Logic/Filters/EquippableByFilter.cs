using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Extensions;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class EquippableByFilter : UintMultipleChoiceFilter
    {
        private readonly ExcelSheet<ClassJob> _classJobSheet;

        public EquippableByFilter(ILogger<EquippableByFilter> logger, ImGuiService imGuiService, ExcelSheet<ClassJob> classJobSheet) : base(logger, imGuiService)
        {
            _classJobSheet = classJobSheet;
        }
        public override string Key { get; set; } = "EquippableBy";
        public override string Name { get; set; } = "Equippable By";
        public override string HelpText { get; set; } = "Which classes can this equipment be equipped by?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;

        public override List<uint> DefaultValue { get; set; } = new();


        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return FilterItem(configuration, item.Item) == true;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            var currentValue = this.CurrentValue(configuration);
            if (currentValue.Count == 0)
            {
                return true;
            }
            if (item.ClassJobCategory != null)
            {

                if (item.ClassJobCategory.ClassJobIds.Any(c => currentValue.Contains(c)))
                {
                    return true;
                }
            }

            return false;
        }

        public override Dictionary<uint, string> GetChoices(FilterConfiguration configuration)
        {
            var choices = new Dictionary<uint, string>();
            foreach (var classJob in _classJobSheet)
            {
                choices.Add(classJob.RowId, classJob.Name.ToString().ToTitleCase());
            }

            return choices;
        }

        public override bool HideAlreadyPicked { get; set; } = true;
    }
}
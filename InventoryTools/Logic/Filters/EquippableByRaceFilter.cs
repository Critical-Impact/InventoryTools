using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class EquippableByRaceFilter : UintMultipleChoiceFilter
    {
        private readonly ExcelCache _excelCache;
        public override string Key { get; set; } = "EquippableByRace";
        public override string Name { get; set; } = "Equippable By Race";
        public override string HelpText { get; set; } = "Which races can this equipment be equipped by?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;

        public override List<uint> DefaultValue { get; set; } = new List<uint>();


        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return FilterItem(configuration, item.Item) == true;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            var currentValue = this.CurrentValue(configuration);
            if (currentValue.Count == 0)
            {
                return true;
            }
            return currentValue.Any(race => item.CanBeEquippedByRaceGender((CharacterRace)race, CharacterSex.Either));
        }

        public override Dictionary<uint, string> GetChoices(FilterConfiguration configuration)
        {
            var choices = new Dictionary<uint, string>();
            var sheet = _excelCache.GetRaceSheet();
            foreach (var race in sheet)
            {
                choices.Add(race.RowId, race.Masculine);
            }

            return choices;
        }

        public override bool HideAlreadyPicked { get; set; } = true;

        public EquippableByRaceFilter(ILogger<EquippableByRaceFilter> logger, ImGuiService imGuiService, ExcelCache excelCache) : base(logger, imGuiService)
        {
            _excelCache = excelCache;
        }
    }
}
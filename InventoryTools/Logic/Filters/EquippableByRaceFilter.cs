using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class EquippableByRaceFilter : UintMultipleChoiceFilter
    {
        private readonly ExcelSheet<Race> _raceSheet;

        public EquippableByRaceFilter(ILogger<EquippableByRaceFilter> logger, ImGuiService imGuiService, ExcelSheet<Race> raceSheet) : base(logger, imGuiService)
        {
            _raceSheet = raceSheet;
        }
        public override string Key { get; set; } = "EquippableByRace";
        public override string Name { get; set; } = "Equippable By Race";
        public override string HelpText { get; set; } = "Which races can this equipment be equipped by?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;

        public override List<uint> DefaultValue { get; set; } = new List<uint>();


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
            return currentValue.Any(race => item.CanBeEquippedByRaceGender((CharacterRace)race, CharacterSex.Either));
        }

        public override Dictionary<uint, string> GetChoices(FilterConfiguration configuration)
        {
            return _raceSheet.ToDictionary(race => race.RowId, race => race.Masculine.ExtractText());
        }

        public override bool HideAlreadyPicked { get; set; } = true;
    }
}
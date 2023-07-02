using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Filters.Abstract;

namespace InventoryTools.Logic.Filters
{
    public class EquippableByRaceFilter : UintMultipleChoiceFilter
    {
        public override string Key { get; set; } = "EquippableByRace";
        public override string Name { get; set; } = "Equippable By Race";
        public override string HelpText { get; set; } = "Which races can this equipment be equipped by?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;

        public override List<uint> DefaultValue { get; set; } = new List<uint>();

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter | FilterType.HistoryFilter;
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
            var sheet = Service.ExcelCache.GetRaceSheet();
            foreach (var race in sheet)
            {
                choices.Add(race.RowId, race.Masculine);
            }

            return choices;
        }

        public override bool HideAlreadyPicked { get; set; } = true;
    }
}
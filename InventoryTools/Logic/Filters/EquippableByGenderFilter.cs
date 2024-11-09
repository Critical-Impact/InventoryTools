using System.Collections.Generic;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class EquippableByGenderFilter : ChoiceFilter<uint?>
    {
        public override uint? CurrentValue(FilterConfiguration configuration)
        {
            return configuration.GetUintFilter(Key);
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, uint? newValue)
        {
            configuration.UpdateUintFilter(Key,newValue);
        }

        public override void ResetFilter(FilterConfiguration configuration)
        {
            UpdateFilterConfiguration(configuration, null);
        }


        public override string Key { get; set; } = "EquippableByGender";
        public override string Name { get; set; } = "Equippable By Gender";
        public override string HelpText { get; set; } = "Which genders can this equipment be equipped by?";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Basic;



        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return FilterItem(configuration, item.Item) == true;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            var currentValue = this.CurrentValue(configuration);
            if (currentValue == null)
            {
                return true;
            }

            CharacterSex sex = (CharacterSex) currentValue;
            return item.CanBeEquippedByRaceGender(CharacterRace.Any, sex);
        }

        public override uint? DefaultValue { get; set; } = null;

        public override List<uint?> GetChoices(FilterConfiguration configuration)
        {
            var choices = new List<uint?>
            {
                null,
                (uint) CharacterSex.Both,
                (uint) CharacterSex.Female,
                (uint) CharacterSex.Male,
                (uint) CharacterSex.Either,
                (uint) CharacterSex.FemaleOnly,
                (uint) CharacterSex.MaleOnly
            };
            return choices;
        }

        public override string GetFormattedChoice(FilterConfiguration filterConfiguration, uint? choice)
        {
            if (choice == null)
            {
                return "";
            }

            return ((CharacterSex) choice).FormattedName();
        }

        public EquippableByGenderFilter(ILogger<EquippableByGenderFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}
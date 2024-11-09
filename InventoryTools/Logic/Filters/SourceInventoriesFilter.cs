using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class SourceInventoriesFilter : MultipleChoiceFilter<(ulong, InventoryCategory)>
    {
        private readonly ICharacterMonitor _characterMonitor;
        private readonly InventoryToolsConfiguration _configuration;

        public SourceInventoriesFilter(ILogger<SourceInventoriesFilter> logger, ImGuiService imGuiService, ICharacterMonitor characterMonitor, InventoryToolsConfiguration configuration) : base(logger, imGuiService)
        {
            _characterMonitor = characterMonitor;
            _configuration = configuration;
        }
        public override int LabelSize { get; set; } = 240;
        public override string Key { get; set; } = "SourceInventories";
        public override string Name { get; set; } = "Source - Inventories";
        public override string HelpText { get; set; } =
            "This is a list of source inventories to sort items from based on the filter configuration";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Inventories;
        public override List<(ulong, InventoryCategory)> DefaultValue { get; set; } = new();
        public override FilterType AvailableIn { get; set; } = FilterType.SearchFilter | FilterType.SortingFilter | FilterType.CraftFilter | FilterType.HistoryFilter;

        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return null;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            return null;
        }

        public override void ResetFilter(FilterConfiguration configuration)
        {
            UpdateFilterConfiguration(configuration, new List<(ulong, InventoryCategory)>());
        }

        public override Dictionary<(ulong, InventoryCategory), string> GetChoices(FilterConfiguration configuration)
        {
            var allCharacters = _characterMonitor.AllCharacters();
            if (!_configuration.DisplayCrossCharacter)
            {
                allCharacters = allCharacters.Where(c =>
                    _characterMonitor.BelongsToActiveCharacter(c.Key)).ToArray();
            }

            var dict = new Dictionary<(ulong, InventoryCategory), string>();
            foreach (var character in allCharacters)
            {
                if (_characterMonitor.IsRetainer(character.Key))
                {
                    dict.Add((character.Key, InventoryCategory.RetainerBags), character.Value.FormattedName + " - " + InventoryCategory.RetainerBags.FormattedName());
                    dict.Add((character.Key, InventoryCategory.RetainerMarket), character.Value.FormattedName + " - " + InventoryCategory.RetainerMarket.FormattedName());
                    dict.Add((character.Key, InventoryCategory.RetainerEquipped), character.Value.FormattedName + " - " + InventoryCategory.RetainerEquipped.FormattedName());
                    dict.Add((character.Key, InventoryCategory.Currency), character.Value.FormattedName + " - " + InventoryCategory.Currency.FormattedName());
                    dict.Add((character.Key, InventoryCategory.Crystals), character.Value.FormattedName + " - " + InventoryCategory.Crystals.FormattedName());
                }
                else if (_characterMonitor.IsFreeCompany(character.Key))
                {
                    dict.Add((character.Key, InventoryCategory.FreeCompanyBags), character.Value.FormattedName + " - " + InventoryCategory.FreeCompanyBags.FormattedName());
                    dict.Add((character.Key, InventoryCategory.Currency), character.Value.FormattedName + " - " + InventoryCategory.Currency.FormattedName());
                    dict.Add((character.Key, InventoryCategory.Crystals), character.Value.FormattedName + " - " + InventoryCategory.Crystals.FormattedName());
                }
                else if (_characterMonitor.IsHousing(character.Key))
                {
                    dict.Add((character.Key, InventoryCategory.HousingInteriorAppearance), character.Value.FormattedName + " - " + InventoryCategory.HousingInteriorAppearance.FormattedName());
                    dict.Add((character.Key, InventoryCategory.HousingInteriorItems), character.Value.FormattedName + " - " + InventoryCategory.HousingInteriorItems.FormattedName());
                    dict.Add((character.Key, InventoryCategory.HousingInteriorStoreroom), character.Value.FormattedName + " - " + InventoryCategory.HousingInteriorStoreroom.FormattedName());
                    dict.Add((character.Key, InventoryCategory.HousingExteriorAppearance), character.Value.FormattedName + " - " + InventoryCategory.HousingExteriorAppearance.FormattedName());
                    dict.Add((character.Key, InventoryCategory.HousingExteriorItems), character.Value.FormattedName + " - " + InventoryCategory.HousingExteriorItems.FormattedName());
                    dict.Add((character.Key, InventoryCategory.HousingExteriorStoreroom), character.Value.FormattedName + " - " + InventoryCategory.HousingExteriorStoreroom.FormattedName());
                }
                else
                {
                    dict.Add((character.Key, InventoryCategory.CharacterBags), character.Value.FormattedName + " - " + InventoryCategory.CharacterBags.FormattedName());
                    dict.Add((character.Key, InventoryCategory.CharacterSaddleBags), character.Value.FormattedName + " - " + InventoryCategory.CharacterSaddleBags.FormattedName());
                    dict.Add((character.Key, InventoryCategory.CharacterPremiumSaddleBags), character.Value.FormattedName + " - " + InventoryCategory.CharacterPremiumSaddleBags.FormattedName());
                    dict.Add((character.Key, InventoryCategory.FreeCompanyBags), character.Value.FormattedName + " - " + InventoryCategory.FreeCompanyBags.FormattedName());
                    dict.Add((character.Key, InventoryCategory.CharacterArmoryChest), character.Value.FormattedName + " - " + InventoryCategory.CharacterArmoryChest.FormattedName());
                    dict.Add((character.Key, InventoryCategory.GlamourChest), character.Value.FormattedName + " - " + InventoryCategory.GlamourChest.FormattedName());
                    dict.Add((character.Key, InventoryCategory.Armoire), character.Value.FormattedName + " - " + InventoryCategory.Armoire.FormattedName());
                    dict.Add((character.Key, InventoryCategory.Crystals), character.Value.FormattedName + " - " + InventoryCategory.Crystals.FormattedName());
                    dict.Add((character.Key, InventoryCategory.Currency), character.Value.FormattedName + " - " + InventoryCategory.Currency.FormattedName());
                    dict.Add((character.Key, InventoryCategory.CharacterEquipped), character.Value.FormattedName + " - " + InventoryCategory.CharacterEquipped.FormattedName());

                }
            }
            return dict;
        }

        public override List<(ulong, InventoryCategory)> CurrentValue(FilterConfiguration configuration)
        {
            return configuration.SourceInventories.ToList();
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, List<(ulong, InventoryCategory)> newValue)
        {
            List<(ulong, InventoryCategory)> newSourceInventories = new();
            foreach (var item in newValue)
            {
                newSourceInventories.Add(item);
            }

            configuration.SourceInventories = newSourceInventories;
        }

        public override bool HideAlreadyPicked { get; set; } = true;
    }
}
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;

using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class DestinationInventoryCategoriesFilter : MultipleChoiceFilter<InventoryCategory>
    {
        public override List<InventoryCategory> CurrentValue(FilterConfiguration configuration)
        {
            return configuration.DestinationCategories?.ToList() ?? new List<InventoryCategory>();
        }

        public override void ResetFilter(FilterConfiguration configuration)
        {
            UpdateFilterConfiguration(configuration, new List<InventoryCategory>());
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, List<InventoryCategory> newValue)
        {
            configuration.DestinationCategories = newValue.Count == 0 ? null : newValue.Distinct().ToHashSet();
        }
        public override int LabelSize { get; set; } = 240;
        public override string Key { get; set; } = "DestinationInventoryCategories";
        public override string Name { get; set; } = "Destination - Inventory Categories";
        public override string HelpText { get; set; } =
            "This is a list of destinations categories to sort items into. It will attempt to sort items into any bag of the given category.";

        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Inventories;
        public override List<InventoryCategory> DefaultValue { get; set; } = new();
        public override FilterType AvailableIn { get; set; } = FilterType.SortingFilter;

        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return null;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
        {
            return null;
        }

        public override Dictionary<InventoryCategory, string> GetChoices(FilterConfiguration configuration)
        {

            var dict = new Dictionary<InventoryCategory, string>();
            dict.Add(InventoryCategory.RetainerBags, "Retainer " +InventoryCategory.RetainerBags.FormattedName());
            dict.Add(InventoryCategory.RetainerMarket, "Retainer " +InventoryCategory.RetainerMarket.FormattedName());
            dict.Add(InventoryCategory.CharacterEquipped, InventoryCategory.CharacterEquipped.FormattedName());
            dict.Add(InventoryCategory.RetainerEquipped, "Retainer " +InventoryCategory.RetainerEquipped.FormattedName());
            dict.Add(InventoryCategory.CharacterBags, InventoryCategory.CharacterBags.FormattedName());
            dict.Add(InventoryCategory.CharacterSaddleBags, InventoryCategory.CharacterSaddleBags.FormattedName());
            dict.Add(InventoryCategory.CharacterPremiumSaddleBags, InventoryCategory.CharacterPremiumSaddleBags.FormattedName());
            dict.Add(InventoryCategory.FreeCompanyBags, InventoryCategory.FreeCompanyBags.FormattedName());
            dict.Add(InventoryCategory.CharacterArmoryChest, InventoryCategory.CharacterArmoryChest.FormattedName());
            dict.Add(InventoryCategory.GlamourChest, InventoryCategory.GlamourChest.FormattedName());
            dict.Add(InventoryCategory.Armoire, InventoryCategory.Armoire.FormattedName());
            dict.Add(InventoryCategory.Currency, InventoryCategory.Currency.FormattedName());
            dict.Add(InventoryCategory.Crystals, InventoryCategory.Crystals.FormattedName());
            dict.Add(InventoryCategory.HousingInteriorAppearance, InventoryCategory.HousingInteriorAppearance.FormattedName());
            dict.Add(InventoryCategory.HousingInteriorItems, InventoryCategory.HousingInteriorItems.FormattedName());
            dict.Add(InventoryCategory.HousingInteriorStoreroom, InventoryCategory.HousingInteriorStoreroom.FormattedName());
            dict.Add(InventoryCategory.HousingExteriorAppearance, InventoryCategory.HousingExteriorAppearance.FormattedName());
            dict.Add(InventoryCategory.HousingExteriorItems, InventoryCategory.HousingExteriorItems.FormattedName());
            dict.Add(InventoryCategory.HousingExteriorStoreroom, InventoryCategory.HousingExteriorStoreroom.FormattedName());

            return dict;
        }

        public override bool HideAlreadyPicked { get; set; } = true;

        public DestinationInventoryCategoriesFilter(ILogger<DestinationInventoryCategoriesFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}
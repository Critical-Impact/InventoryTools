using System.Collections.Generic;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Editors;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftSourceInventoriesFilter : InventoryScopeFilter
{
    public CraftSourceInventoriesFilter(InventoryScopePicker scopePicker, ILogger<CraftSourceInventoriesFilter> logger, ImGuiService imGuiService) : base(scopePicker, logger, imGuiService)
    {
    }

    public override string Key { get; set; } = "CraftSourceInventories";
    public override string Name { get; set; } = "Inventories to Retrieve From";

    public override string HelpText { get; set; } =
        "Which inventories should the crafting list check for materials to withdraw? Items found in the selected inventories will appear in the 'Items in Retainers/Bags' list and you will need to retrieve them either before gathering or after gathering depending on your craft list's configuration.";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Inventories;

    public override List<InventorySearchScope>? DefaultValue { get; set; } = null;

    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;
    public override List<InventorySearchScope>? GenerateDefaultScope()
    {
        return new List<InventorySearchScope>()
        {
            new InventorySearchScope() { ActiveCharacter = true, Categories = [InventoryCategory.RetainerBags, InventoryCategory.FreeCompanyBags, InventoryCategory.CharacterSaddleBags, InventoryCategory.CharacterPremiumSaddleBags] }
        };
    }

    public override int Order { get; set; } = -3;
}
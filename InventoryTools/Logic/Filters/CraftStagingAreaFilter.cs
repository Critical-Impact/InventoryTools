using System.Collections.Generic;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Editors;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class CraftStagingAreaFilter : InventoryScopeFilter
{
    public CraftStagingAreaFilter(InventoryScopePicker scopePicker, ILogger<CraftStagingAreaFilter> logger, ImGuiService imGuiService) : base(scopePicker, logger, imGuiService)
    {
    }

    public override string Key { get; set; } = "CraftStagingArea";
    public override string Name { get; set; } = "Staging Area";

    public override string HelpText { get; set; } =
        "When crafting, what inventories should be considered the staging area? Any items in the staging area are considered in the users inventories. By default the current character's bags, crystals and currency are the staging area but if you wanted to include the saddlebag you could.";

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Inventories;

    public override List<InventorySearchScope>? DefaultValue { get; set; } = null;

    public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;
    public override List<InventorySearchScope>? GenerateDefaultScope()
    {
        return new List<InventorySearchScope>()
        {
            new InventorySearchScope() { ActiveCharacter = true, Categories = [InventoryCategory.CharacterBags, InventoryCategory.Currency, InventoryCategory.Crystals] }
        };
    }

    public override int Order { get; set; } = -1;
}
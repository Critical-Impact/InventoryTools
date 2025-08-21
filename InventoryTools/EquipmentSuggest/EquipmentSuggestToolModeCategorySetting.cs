using System;
using System.Collections.Generic;
using AllaganLib.Interface.FormFields;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.Settings.Abstract.Generic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.EquipmentSuggest;

public enum EquipmentSuggestToolModeCategory
{
    Crafting,
    Gathering,
    Combat,
    CombatTank,
    CombatHealer,
    CombatMelee,
    CombatRanged,
}

public class EquipmentSuggestToolModeCategorySetting : EnumFormField<EquipmentSuggestToolModeCategory, EquipmentSuggestConfig>
{
    public EquipmentSuggestToolModeCategorySetting(ImGuiService imGuiService) : base(imGuiService)
    {
    }

    public override Enum DefaultValue { get; set; } = EquipmentSuggestToolModeCategory.Crafting;
    public override string Key { get; set; } = "ToolModeCategory";
    public override string Name { get; set; } = "Category";
    public override string HelpText { get; set; } = "The category to use when in tool mode";
    public override string Version { get; } = "12.0.10";

    public override Dictionary<Enum, string> Choices { get; } = new()
    {
        { EquipmentSuggestToolModeCategory.Crafting, "Crafting" },
        { EquipmentSuggestToolModeCategory.Gathering, "Gathering" },
        { EquipmentSuggestToolModeCategory.Combat, "Combat" },
        { EquipmentSuggestToolModeCategory.CombatTank, "Combat (Tank)" },
        { EquipmentSuggestToolModeCategory.CombatHealer, "Combat (Healer)" },
        { EquipmentSuggestToolModeCategory.CombatMelee, "Combat (Melee)" },
        { EquipmentSuggestToolModeCategory.CombatRanged, "Combat (Ranged)" },

    };

    public override bool Equal(Enum item1, Enum item2)
    {
        return item1.Equals(item2);
    }
}
using AllaganLib.Interface.FormFields;
using InventoryTools.Services;

namespace InventoryTools.EquipmentSuggest;

public class EquipmentSuggestOwnedGearOnlyFormField : BooleanFormField<EquipmentSuggestConfig>
{
    public EquipmentSuggestOwnedGearOnlyFormField(ImGuiService imGuiService) : base(imGuiService)
    {
    }

    public override bool DefaultValue { get; set; } = false;
    public override string Key { get; set; } = "OwnedGearOnly";
    public override string Name { get; set; } = "Owned Gear Only?";
    public override string HelpText { get; set; } = "Should only gear that you own be shown?";
    public override string Version { get; } = "13.1.1";
}
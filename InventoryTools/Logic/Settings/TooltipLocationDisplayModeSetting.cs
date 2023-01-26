using System.Collections.Generic;
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Settings;

public class TooltipLocationDisplayModeSetting : ChoiceSetting<TooltipLocationDisplayMode>
{
    public override TooltipLocationDisplayMode DefaultValue { get; set; } =
        TooltipLocationDisplayMode.CharacterCategoryQuantityQuality;
    public override TooltipLocationDisplayMode CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.TooltipLocationDisplayMode;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, TooltipLocationDisplayMode newValue)
    {
        configuration.TooltipLocationDisplayMode = newValue;
    }

    public override string Key { get; set; } = "TooltipLocationDisplayMode";
    public override string Name { get; set; } = "Tooltip Location Display Mode";

    public override string HelpText { get; set; } =
        "How the locations of items should be presented in the tooltip. This requires 'Display Amount Owned?' to be on.";

    public override SettingCategory SettingCategory { get; set; } = SettingCategory.Visuals;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.Tooltips;

    public override Dictionary<TooltipLocationDisplayMode, string> Choices
    {
        get
        {
            return new Dictionary<TooltipLocationDisplayMode, string>()
            {
                { TooltipLocationDisplayMode.CharacterQuantityQuality, "Character/Retainer - Quantity - Quality" },
                { TooltipLocationDisplayMode.CharacterBagSlotQuality, "Character/Retainer - Bag - Slot - Quality" },
                {
                    TooltipLocationDisplayMode.CharacterCategoryQuantityQuality,
                    "Character/Retainer - Category - Quantity - Quality"
                },
            };
        }
    }
}
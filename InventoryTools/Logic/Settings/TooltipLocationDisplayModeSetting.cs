using System.Collections.Generic;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

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
    public override string Name { get; set; } = "Add Item Locations (Display Mode)";

    public override string WizardName { get; } = "Display Mode";

    public override string HelpText { get; set; } =
        "How the locations of items should be presented in the tooltip. This requires 'Add Item Locations?' to be on.";

    public override SettingCategory SettingCategory { get; set; } = SettingCategory.ToolTips;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.AddItemLocations;

    public override Dictionary<TooltipLocationDisplayMode, string> Choices
    {
        get
        {
            return new Dictionary<TooltipLocationDisplayMode, string>()
            {
                { TooltipLocationDisplayMode.CharacterQuantityQuality, "Character/Retainer - Quantity - Quality" },
                { TooltipLocationDisplayMode.CharacterBagSlotQuality, "Character/Retainer - Bag - Slot - Quality" },
                { TooltipLocationDisplayMode.CharacterBagSlotQuantity, "Character/Retainer - Bag - Slot - Quantity" },
                {
                    TooltipLocationDisplayMode.CharacterCategoryQuantityQuality,
                    "Character/Retainer - Category - Quantity - Quality"
                },
            };
        }
    }
    public override string Version => "1.7.0.0";

    public TooltipLocationDisplayModeSetting(ILogger<TooltipLocationDisplayModeSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}
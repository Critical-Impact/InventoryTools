using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings
{
    public class TooltipDisplayAmountOwnedSetting : BooleanSetting
    {
        public override bool DefaultValue { get; set; } = true;

        public override bool CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.TooltipDisplayAmountOwned;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
        {
            configuration.TooltipDisplayAmountOwned = newValue;
        }

        public override string Key { get; set; } = "TooltipDisplayOwned";
        public override string Name { get; set; } = "Add Item Locations";

        public override string WizardName { get; } = "Add Item Locations";

        public override string HelpText { get; set; } =
            "When hovering an item, should the tooltip show the locations of any copies of the item you currently own?";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.ToolTips;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.AddItemLocations;
        public override string Version => "1.7.0.0";

        public TooltipDisplayAmountOwnedSetting(ILogger<TooltipDisplayAmountOwnedSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings
{
    public class ShowTooltipsSetting : BooleanSetting
    {
        public override bool DefaultValue { get; set; } = true;
        public override bool CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.DisplayTooltip;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
        {
            configuration.DisplayTooltip = newValue;
        }

        public override string Key { get; set; } = "ShowTooltips";
        public override string Name { get; set; } = "Enable Tooltip Tweaks?";

        public override string HelpText { get; set; } =
            "When hovering an item in game, show additional information about the item including it's location in inventories and market price(if available).";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.ToolTips;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.General;
        public override string Version => "1.6.2.5";

        public ShowTooltipsSetting(ILogger<ShowTooltipsSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}
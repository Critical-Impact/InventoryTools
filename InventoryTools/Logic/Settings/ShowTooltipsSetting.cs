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
            "Disable/enable the entire tooltip modification system for the plugin. If this is off, no changes will be made to your item's tooltips.";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.ToolTips;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.General;
        public override string Version => "1.7.0.0";

        public ShowTooltipsSetting(ILogger<ShowTooltipsSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings
{
    public class ShowItemNumberRetainerListSetting : BooleanSetting
    {
        public override bool DefaultValue { get; set; } = true;

        public override bool CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.ShowItemNumberRetainerList;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
        {
            configuration.ShowItemNumberRetainerList = newValue;
        }

        public override string Key { get; set; } = "ShowItemNumberRetainerList";
        public override string Name { get; set; } = "Show item number in retainer list?";

        public override string HelpText { get; set; } =
            "Should the name of the retainer in the summoning bell list have the number of items to be sorted or are available in their inventory?";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.Highlighting;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.General;
        public override string Version => "1.7.0.0";

        public ShowItemNumberRetainerListSetting(ILogger<ShowItemNumberRetainerListSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}
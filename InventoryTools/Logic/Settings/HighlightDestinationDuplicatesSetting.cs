using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings
{
    public class HighlightDestinationSetting : BooleanSetting
    {
        public override bool DefaultValue { get; set; } = true;
        public override bool CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.HighlightDestination;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
        {
            configuration.HighlightDestination = newValue;
        }

        public override string Key { get; set; } = "HighlightDestination";
        public override string Name { get; set; } = "Highlight Destination?";

        public override string HelpText { get; set; } =
            "Should the destination for items be highlighted? This can be overridden in the filter configuration.";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.Highlighting;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.General;
        public override string Version => "1.7.0.0";

        public HighlightDestinationSetting(ILogger<HighlightDestinationSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}
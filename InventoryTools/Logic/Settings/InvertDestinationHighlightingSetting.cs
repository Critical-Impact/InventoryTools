using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings
{
    public class InvertDestinationHighlightingSetting : BooleanSetting
    {
        public override bool DefaultValue { get; set; } = false;
        public override bool CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.InvertDestinationHighlighting;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
        {
            configuration.InvertDestinationHighlighting = newValue;
        }

        public override string Key { get; set; } = "InvertDestinationHighlighting";
        public override string Name { get; set; } = "Invert Destination Highlighting?";

        public override string HelpText { get; set; } =
            "When highlighting destination items should the colour of the items be inverted?";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.Highlighting;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.General;
        public override string Version => "1.7.0.0";

        public InvertDestinationHighlightingSetting(ILogger<InvertDestinationHighlightingSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}
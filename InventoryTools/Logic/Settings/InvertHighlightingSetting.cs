using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings
{
    public class InvertHighlightingSetting : BooleanSetting
    {
        public override bool DefaultValue { get; set; } = true;
        public override bool CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.InvertHighlighting;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
        {
            configuration.InvertHighlighting = newValue;
        }

        public override string Key { get; set; } = "InvertHighlighting";
        public override string Name { get; set; } = "Invert Highlighting?";

        public override string HelpText { get; set; } =
            "Should all the items not matching a list be highlighted instead? This can be overridden in the list configuration.";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.Lists;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.Highlighting;
        public override string Version => "1.7.0.0";

        public InvertHighlightingSetting(ILogger<InvertHighlightingSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}
using System.Collections.Generic;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings
{
    public class HighlightWhenSetting : ChoiceSetting<string>
    {
        public Dictionary<string, string> StaticChoices = new Dictionary<string, string>()
        {
            {"Always", "Always"}, {"When Searching", "When Searching"}
        };

        public override string DefaultValue { get; set; } = "When Searching";

        public override string CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.HighlightWhen;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, string newValue)
        {
            configuration.HighlightWhen = newValue;
        }

        public override string Key { get; set; } = "HighlightWhen";
        public override string Name { get; set; } = "Highlight When?";
        public override string HelpText { get; set; } = "When highlighting is turned on for a list, should it always be active or should it only be active when a column is being searched in";
        public override SettingCategory SettingCategory { get; set; } = SettingCategory.Lists;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.Highlighting;

        public override Dictionary<string, string> Choices
        {
            get
            {
                return StaticChoices;
            }
        }
        public override string Version => "1.7.0.0";

        public HighlightWhenSetting(ILogger<HighlightWhenSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}
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
        public override string HelpText { get; set; } = "When highlighting is turned on for a filter, should it show always or should it only show when searching in the item table?";
        public override SettingCategory SettingCategory { get; set; } = SettingCategory.Visuals;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.Highlighting;

        public override Dictionary<string, string> Choices
        {
            get
            {
                return StaticChoices;
            }
        }
        public override string Version => "1.6.2.5";

        public HighlightWhenSetting(ILogger<HighlightWhenSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}
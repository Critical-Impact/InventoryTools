using System.Numerics;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings
{
    public class HighlightDestinationColourSetting : ColorSetting
    {
        public override Vector4 DefaultValue { get; set; } = new Vector4(0.321f, 0.239f, 0.03f, 1f);
        public override Vector4 CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.DestinationHighlightColor;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, Vector4 newValue)
        {
            configuration.DestinationHighlightColor = newValue;
        }

        public override string Key { get; set; } = "DestinationHighlightColour";
        public override string Name { get; set; } = "Destination Highlight Colour";
        public override string HelpText { get; set; } = "The color to set any items in the destination that match your source filter(assuming highlight destination duplicates is on).";
        public override SettingCategory SettingCategory { get; set; } = SettingCategory.Highlighting;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.Colours;
        public override string Version => "1.7.0.0";

        public HighlightDestinationColourSetting(ILogger<HighlightDestinationColourSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}
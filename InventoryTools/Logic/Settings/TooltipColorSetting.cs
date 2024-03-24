using CriticalCommonLib.Services;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings
{
    public class TooltipColorSetting : GameColorSetting
    {
        public override uint? DefaultValue { get; set; } = null;
        public override uint? CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.TooltipColor;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, uint? newValue)
        {
            configuration.TooltipColor = newValue;
        }

        public override string Key { get; set; } = "TooltipColor";
        public override string Name { get; set; } = "Text Colour";
        public override string HelpText { get; set; } = "This is the colour of the text within the tooltip";
        public override SettingCategory SettingCategory { get; set; } = SettingCategory.ToolTips;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.Visuals;
        public override string Version => "1.6.2.5";

        public TooltipColorSetting(ILogger<TooltipColorSetting> logger, ImGuiService imGuiService, ExcelCache excelCache) : base(logger, imGuiService, excelCache)
        {
        }
    }
}
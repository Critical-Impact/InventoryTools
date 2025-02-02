using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings
{
    public class TooltipCurrentCharacterSetting : BooleanSetting
    {
        public override bool DefaultValue { get; set; } = false;

        public override bool CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.TooltipCurrentCharacter;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
        {
            configuration.TooltipCurrentCharacter = newValue;
        }

        public override string Key { get; set; } = "TooltipCurrentCharacter";
        public override string Name { get; set; } = "Limit to items on the current character?";

        public override string HelpText { get; set; } =
            "Limits the information displayed on the tooltip to inventories belonging to the currently logged in character.";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.ToolTips;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.AddItemLocations;
        public override string Version => "1.7.0.0";

        public TooltipCurrentCharacterSetting(ILogger<TooltipCurrentCharacterSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}
using System.Numerics;
using Dalamud.Interface.Colors;
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Settings
{
    public class RetainerListColourSetting : ColorSetting
    {
        public override Vector4 DefaultValue { get; set; } = ImGuiColors.HealerGreen;
        public override Vector4 CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.RetainerListColor;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, Vector4 newValue)
        {
            configuration.RetainerListColor = newValue;
        }

        public override string Key { get; set; } = "RetainerListColour";
        public override string Name { get; set; } = "Retainer List Colour";

        public override string HelpText { get; set; } =
            "The color to set the retainer(when the retainer contains filtered items) list to.";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.Visuals;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.Highlighting;

    }
}
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Settings
{
    public class MoreInfoContextMenuSetting : BooleanSetting
    {
        public override bool DefaultValue { get; set; } = false;
        public override bool CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.AddMoreInformationContextMenu;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
        {
            configuration.AddMoreInformationContextMenu = newValue;
        }

        public override string Key { get; set; } = "moreInfoContextMenu";
        public override string Name { get; set; } = "Context Menu - More Information";

        public override string HelpText { get; set; } =
            "Add the more information item to the right click/context menu for items?";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.Visuals;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.ContextMenus;
    }
}
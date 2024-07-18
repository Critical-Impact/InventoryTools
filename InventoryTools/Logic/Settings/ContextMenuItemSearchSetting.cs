using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings
{
    public class ContextMenuItemSearchSetting : BooleanSetting
    {
        public ContextMenuItemSearchSetting(ILogger<ContextMenuItemSearchSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        
        public override bool DefaultValue { get; set; } = false;
        public override bool CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.ItemSearchContextMenu;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
        {
            configuration.ItemSearchContextMenu = newValue;
        }

        public override string Key { get; set; } = "ItemSearchContextMenu";
        public override string Name { get; set; } = "Context Menu - Search";

        public override string WizardName { get; } = "Search";

        public override string HelpText { get; set; } =
            "Performs a search covering either all inventories or the scope defined below?";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.ContextMenu;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.General;
        public override string Version => "1.7.0.13";
    }
}
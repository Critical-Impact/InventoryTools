using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings
{
    public class ContextMenuAddToCraftListSetting : BooleanSetting
    {
        public ContextMenuAddToCraftListSetting(ILogger<ContextMenuMoreInformationSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        
        public override bool DefaultValue { get; set; } = false;
        public override bool CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.AddToCraftListContextMenu;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
        {
            configuration.AddToCraftListContextMenu = newValue;
        }

        public override string Key { get; set; } = "addToCraftListContextMenu";
        public override string Name { get; set; } = "Context Menu - Add to Craft List";

        public override string WizardName { get; } = "Add to Craft List";

        public override string HelpText { get; set; } =
            "Add a submenu to add the item to a craft list?";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.ContextMenu;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.General;
        public override string Version => "1.6.2.5";
    }
}
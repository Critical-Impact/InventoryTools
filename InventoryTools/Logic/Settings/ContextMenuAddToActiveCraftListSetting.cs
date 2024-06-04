using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings
{
    public class ContextMenuAddToActiveCraftListSetting : BooleanSetting
    {
        public ContextMenuAddToActiveCraftListSetting(ILogger<ContextMenuMoreInformationSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }

        public override bool DefaultValue { get; set; } = false;
        public override bool CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.AddToActiveCraftListContextMenu;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
        {
            configuration.AddToActiveCraftListContextMenu = newValue;
        }

        public override string Key { get; set; } = "addToActiveCraftListContextMenu";
        public override string Name { get; set; } = "Context Menu - Add to Active Craft List";

        public override string WizardName { get; } = "Add to Active Craft List";

        public override string HelpText { get; set; } =
            "Add a submenu to add the item to a active craft list?";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.ContextMenu;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.General;
        public override string Version => "1.7.0.5";
    }
}
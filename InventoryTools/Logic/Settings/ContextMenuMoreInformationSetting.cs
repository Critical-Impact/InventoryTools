using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings
{
    public class ContextMenuMoreInformationSetting : BooleanSetting
    {
        public override bool DefaultValue { get; set; } = true;
        public override bool CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.AddMoreInformationContextMenu;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
        {
            configuration.AddMoreInformationContextMenu = newValue;
        }

        public override string Key { get; set; } = "moreInfoContextMenu";
        public override string Name { get; set; } = "Context Menu - More Information (Items)";

        public override string WizardName { get; } = "More Information";

        public override string HelpText { get; set; } =
            "Add the more information menu item to the right click/context menu for items?";

        public override string Version => "1.7.0.0";

        public ContextMenuMoreInformationSetting(ILogger<ContextMenuMoreInformationSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
    }
}
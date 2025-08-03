using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.Settings.Abstract.Generic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings
{
    public class TooltipGlobalSeparatorSetting : GenericSeparatorSetting
    {
        public TooltipGlobalSeparatorSetting(ILogger<TooltipGlobalSeparatorSetting> logger, ImGuiService imGuiService) : base(
            "TooltipSeparatorGlobal",
            "Enable separator ?",
            "Add separator in header and footer",
            false,
            SettingCategory.ToolTips,
            SettingSubCategory.General,
            "1.7.0.21",
            logger,
            imGuiService
        )
        {
        }
        
        public override bool CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.TooltipSeparatorGlobalEnable;
        }
        
        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
        {
            configuration.TooltipSeparatorGlobalEnable = newValue;
        }
    }
}

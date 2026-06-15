using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.Settings.Abstract.Generic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.WizardSettings;

public class CraftNotificationsReportToChatSetting : GenericBooleanSetting
{
    private bool _shouldAdd;

    public CraftNotificationsReportToChatSetting(ILogger<CraftNotificationsReportToChatSetting> logger,
        ImGuiService imGuiService) : base("CraftNotificationsReportToChat",
        "Report acquisition to chat",
        "When acquiring items that are part of a craft list, print progress to chat. The craft list must be active for notifications to occur.",
        true,
        SettingCategory.None,
        SettingSubCategory.None,
        "15.0.8",
        logger,
        imGuiService)
    {
    }

    public override bool CurrentValue(InventoryToolsConfiguration configuration)
    {
        return _shouldAdd;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
    {
        _shouldAdd = newValue;
    }
}
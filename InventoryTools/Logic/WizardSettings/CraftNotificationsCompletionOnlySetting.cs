using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.Settings.Abstract.Generic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.WizardSettings;

public class CraftNotificationsCompletionOnlySetting : GenericBooleanSetting
{
    private bool _shouldAdd;

    public CraftNotificationsCompletionOnlySetting(ILogger<CraftNotificationsCompletionOnlySetting> logger,
        ImGuiService imGuiService) : base("CraftNotificationsCompletionOnly",
        "Only report when an item is complete?",
        "Instead of reporting every acquisition, only print a message when an item reaches its required quantity. The craft list must be active for notifications to occur.",
        false,
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
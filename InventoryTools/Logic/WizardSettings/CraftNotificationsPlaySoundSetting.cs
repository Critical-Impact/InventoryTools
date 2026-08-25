using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.Settings.Abstract.Generic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.WizardSettings;

public class CraftNotificationsPlaySoundSetting : GenericBooleanSetting
{
    public override bool AppearsInConfigWindow => false;

    private bool _shouldAdd;

    public CraftNotificationsPlaySoundSetting(ILogger<CraftNotificationsPlaySoundSetting> logger,
        ImGuiService imGuiService) : base("CraftNotificationsPlaySound",
        "Play sound when an item is complete",
        "Play a sound effect when an item in a craft list reaches its required quantity. The craft list must be active for notifications to occur.",
        false,
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
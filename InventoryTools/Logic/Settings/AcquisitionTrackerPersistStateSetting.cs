using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.Settings.Abstract.Generic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class AcquisitionTrackerPersistStateSetting : GenericIntegerSetting
{
    public AcquisitionTrackerPersistStateSetting(ILogger<AcquisitionTrackerPersistStateSetting> logger, ImGuiService imGuiService) : base("AcquisitionTrackerPersistState", "Persist State Time (seconds)", "The acquisition tracker will track your current state(crafting, gathering, etc). It will keep this state by default for 2 seconds after it changes back to doing nothing. This is to catch any delays in the tracker, set this number higher if you find the tracker is not picking up items you craft/gather/etc", 2, SettingCategory.Troubleshooting, SettingSubCategory.AcquisitionTracker,"1.12.0.8", logger, imGuiService)
    {
    }
}
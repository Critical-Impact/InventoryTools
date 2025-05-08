using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.Settings.Abstract.Generic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class AcquisitionTrackerLoginDelaySetting : GenericIntegerSetting
{
    public AcquisitionTrackerLoginDelaySetting(ILogger<AcquisitionTrackerLoginDelaySetting> logger, ImGuiService imGuiService) : base("AcquisitionTrackerLoginDelay", "Login Delay", "The acquisition tracker perform an initial scan of your characters bags when you login. As the game receives multiple inventory updates, the scanner must wait a set amount of time before it's done. Adjust this to be higher if you are seeing items in your craft list randomly complete when you first login.", 5, SettingCategory.Troubleshooting, SettingSubCategory.AcquisitionTracker,"1.12.0.8", logger, imGuiService)
    {
    }
}
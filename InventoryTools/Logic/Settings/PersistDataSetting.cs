using InventoryTools.Boot;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Logic.Settings.Abstract.Generic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class PersistDataSetting : GenericBooleanSetting
{
    private readonly BootConfiguration _bootConfiguration;

    public PersistDataSetting(ILogger<PersistDataSetting> logger, BootConfiguration bootConfiguration, ImGuiService imGuiService) : base("PersistData", "Persist Cached Data", "Allagan Tools has to calculate information when it first boots that can take upwards of 5-10 seconds depending on your computer. If this is on, that data is persisted between updates speeding up the boot time of the plugin.", true, SettingCategory.Troubleshooting, SettingSubCategory.General, "14.0.5", logger, imGuiService)
    {
        _bootConfiguration = bootConfiguration;
    }

    public override bool CurrentValue(InventoryToolsConfiguration configuration)
    {
        return _bootConfiguration.PersistLuminaCache;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
    {
        _bootConfiguration.PersistLuminaCache = newValue;
    }
}
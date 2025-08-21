using AllaganLib.Monitors.Interfaces;
using InventoryTools.Logic.Settings;

namespace InventoryTools.ServiceConfigurations;

public class AcquisitionMonitorConfiguration : IAcquisitionMonitorConfiguration
{
    private readonly AcquisitionTrackerPersistStateSetting _persistStateSetting;
    private readonly AcquisitionTrackerLoginDelaySetting _loginDelaySetting;
    private readonly InventoryToolsConfiguration _configuration;

    public AcquisitionMonitorConfiguration(AcquisitionTrackerPersistStateSetting persistStateSetting, AcquisitionTrackerLoginDelaySetting loginDelaySetting, InventoryToolsConfiguration configuration)
    {
        _persistStateSetting = persistStateSetting;
        _loginDelaySetting = loginDelaySetting;
        _configuration = configuration;
    }

    public int PersistStateTime
    {
        get => _persistStateSetting.CurrentValue(_configuration);
        set => _persistStateSetting.UpdateFilterConfiguration(_configuration, value);
    }

    public int LoginDelay
    {
        get => _loginDelaySetting.CurrentValue(_configuration);
        set => _loginDelaySetting.UpdateFilterConfiguration(_configuration, value);
    }
}
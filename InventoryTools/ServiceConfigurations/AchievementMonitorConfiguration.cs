using AllaganLib.Monitors.Interfaces;

namespace InventoryTools.ServiceConfigurations;

public class AchievementMonitorConfiguration : IAchievementMonitorConfiguration
{
    public int PollIntervalSeconds { get; set; } = 5;
}
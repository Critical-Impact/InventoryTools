using System.Collections.Generic;
using AllaganLib.Monitors.Interfaces;
using InventoryTools.ServiceConfigurations;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace InventoryToolsMock;

public class MockAchievementMonitorService : IAchievementMonitorService
{
    public List<uint> GetCompletedAchievementIds()
    {
        return [];
    }

    public List<RowRef<Achievement>> GetCompletedAchievements()
    {
        return [];
    }

    public bool IsCompleted(uint achievementId)
    {
        return achievementId % 2 == 0;
    }

    public bool IsLoaded { get;  } = false;
    public IAchievementMonitorConfiguration Configuration { get; } = new AchievementMonitorConfiguration();
}
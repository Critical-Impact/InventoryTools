using CriticalCommonLib.MarketBoard;

namespace InventoryToolsMock;

public class MockMarketCache : IMarketCache
{
    public void Dispose()
    {
    }

    public int AutomaticCheckTime { get; set; } = 24;
    public int AutomaticSaveTime { get; set; } = 24;
    public int CacheTimeHours { get; set; } = 500;
    public bool CacheAutoRetrieve { get; set; } = false;
    
    public void StartAutomaticCheckTimer()
    {
        
    }

    public void RestartAutomaticCheckTimer()
    {
    }

    public void StopAutomaticCheckTimer()
    {
    }

    public void LoadExistingCache()
    {
    }

    public void ClearCache()
    {
    }

    public void SaveCache(bool forceSave = false)
    {
    }

    public PricingResponse? GetPricing(uint itemID, bool forceCheck)
    {
        return null;
    }

    public void RequestCheck(uint itemID)
    {
    }
}
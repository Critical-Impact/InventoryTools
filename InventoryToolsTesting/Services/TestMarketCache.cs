using System.Collections.Concurrent;
using System.Collections.Generic;
using CriticalCommonLib.MarketBoard;

namespace InventoryToolsTesting.Services;

public class TestMarketCache : IMarketCache
{
    public void Dispose()
    {

    }

    public int AutomaticCheckTime { get; set; }
    public int AutomaticSaveTime { get; set; }
    public int CacheTimeHours { get; set; }
    public bool CacheAutoRetrieve { get; set; }
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

    public MarketPricing? GetPricing(uint itemId, uint worldId, bool forceCheck)
    {
        return null;
    }

    public MarketCachePricingResult GetPricing(uint itemId, uint worldId, bool ignoreCache, bool forceCheck,
        out MarketPricing? pricing)
    {
        pricing = null;
        return MarketCachePricingResult.Disabled;
    }

    public List<MarketPricing> GetPricing(uint itemId, List<uint> worldIds, bool forceCheck)
    {
        return new();
    }

    public List<MarketPricing> GetPricing(uint itemId, bool forceCheck)
    {
        return new();
    }

    public ConcurrentDictionary<(uint, uint), MarketPricing> CachedPricing { get; set; }

    public bool RequestCheck(uint itemId, uint worldId, bool forceCheck)
    {
        return false;
    }

    public void RequestCheck(List<uint> itemIds, List<uint> worldIds, bool forceCheck)
    {
    }

    public void RequestCheck(List<uint> itemIds, uint worldId, bool forceCheck)
    {
    }

    public void RequestCheck(uint itemId, List<uint> worldIDs, bool forceCheck)
    {
    }
}
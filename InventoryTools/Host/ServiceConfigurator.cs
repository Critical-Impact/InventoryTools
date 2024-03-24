using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Services;
using InventoryTools.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventoryTools;

/// <summary>
/// After the plugin's configuration is loaded, preloads any data that is required by other services before the rest of the plugin runs
/// </summary>
public class ServiceConfigurator : IHostedService
{
    private readonly ILogger<ServiceConfigurator> _logger;
    private readonly ConfigurationManager _configurationManager;
    private readonly InventoryToolsConfiguration _configuration;
    private readonly IMarketCache _marketCache;
    private readonly IUniversalis _universalis;
    private readonly ICharacterMonitor _characterMonitor;
    private readonly IInventoryMonitor _inventoryMonitor;
    private readonly InventoryHistory _inventoryHistory;
    private readonly IMobTracker _mobTracker;

    public ServiceConfigurator(ILogger<ServiceConfigurator> logger, ConfigurationManager configurationManager, InventoryToolsConfiguration configuration, IMarketCache marketCache, IUniversalis universalis, ICharacterMonitor characterMonitor, IInventoryMonitor inventoryMonitor, InventoryHistory inventoryHistory, IMobTracker mobTracker)
    {
        _logger = logger;
        _configurationManager = configurationManager;
        _configuration = configuration;
        _marketCache = marketCache;
        _universalis = universalis;
        _characterMonitor = characterMonitor;
        _inventoryMonitor = inventoryMonitor;
        _inventoryHistory = inventoryHistory;
        _mobTracker = mobTracker;
    }

    public void ConfigureServices()
    {
        _characterMonitor.LoadExistingRetainers(_configuration.GetSavedRetainers());
        _inventoryMonitor.LoadExistingData(_configurationManager.LoadInventory());
        _inventoryHistory.LoadExistingHistory(_configurationManager.LoadHistoryFromCsv(out _));
        var entries = _mobTracker.LoadCsv(_configurationManager.MobSpawnFile, out var success);
        if(success)
        {
            _mobTracker.SetEntries(entries);
        }
        
        _marketCache.CacheAutoRetrieve = _configuration.AutomaticallyDownloadMarketPrices;
        _marketCache.CacheTimeHours = _configuration.MarketRefreshTimeHours;
        _universalis.SetSaleHistoryLimit(_configuration.MarketRefreshTimeHours);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Starting service {type} ({this})", GetType().Name, this);
        ConfigureServices();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
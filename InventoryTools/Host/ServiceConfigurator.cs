using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Services;
using InventoryTools.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Host;

/// <summary>
/// After the plugin's configuration is loaded, preloads any data that is required by other services before the rest of the plugin runs
/// </summary>
public class ServiceConfigurator : IHostedService
{
    private readonly ILogger<ServiceConfigurator> _logger;
    private readonly ConfigurationManagerService _configurationManagerService;
    private readonly InventoryToolsConfiguration _configuration;
    private readonly IMarketCache _marketCache;
    private readonly ICharacterMonitor _characterMonitor;
    private readonly IInventoryMonitor _inventoryMonitor;
    private readonly InventoryHistory _inventoryHistory;
    private readonly IMobTracker _mobTracker;
    private readonly IHostedUniversalisConfiguration _hostedUniversalisConfiguration;

    public ServiceConfigurator(ILogger<ServiceConfigurator> logger, ConfigurationManagerService configurationManagerService, InventoryToolsConfiguration configuration, IMarketCache marketCache, ICharacterMonitor characterMonitor, IInventoryMonitor inventoryMonitor, InventoryHistory inventoryHistory, IMobTracker mobTracker, IHostedUniversalisConfiguration hostedUniversalisConfiguration)
    {
        _logger = logger;
        _configurationManagerService = configurationManagerService;
        _configuration = configuration;
        _marketCache = marketCache;
        _characterMonitor = characterMonitor;
        _inventoryMonitor = inventoryMonitor;
        _inventoryHistory = inventoryHistory;
        _mobTracker = mobTracker;
        _hostedUniversalisConfiguration = hostedUniversalisConfiguration;
    }

    public void ConfigureServices()
    {
        _characterMonitor.LoadExistingRetainers(_configuration.GetSavedRetainers());
        _inventoryMonitor.LoadExistingData(_configurationManagerService.LoadInventory());
        _inventoryHistory.LoadExistingHistory(_configurationManagerService.LoadHistoryFromCsv(out _));
        var entries = _mobTracker.LoadCsv(_configurationManagerService.MobSpawnFile, out var success);
        if(success)
        {
            _mobTracker.SetEntries(entries);
        }
        
        _marketCache.CacheAutoRetrieve = _configuration.AutomaticallyDownloadMarketPrices;
        _marketCache.CacheTimeHours = _configuration.MarketRefreshTimeHours;
        _hostedUniversalisConfiguration.SaleHistoryLimit = _configuration.MarketSaleHistoryLimit;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Starting service {type} ({this})", GetType().Name, this);
        ConfigureServices();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Stopped service {type} ({this})", GetType().Name, this);
        return Task.CompletedTask;
    }
}
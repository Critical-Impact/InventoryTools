using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using AllaganLib.GameSheets.Caches;
using Autofac;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using Dalamud.Interface.Colors;
using Dalamud.Plugin;
using InventoryTools.Logic;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.ItemRenderers;
using InventoryTools.Logic.Settings;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Services;

public class MigrationManagerService : IHostedService
{
    private readonly ILogger<MigrationManagerService> _logger;
    private readonly InventoryToolsConfiguration _configuration;
    private readonly IMarketCache _marketCache;
    private readonly IFilterService _filterService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IComponentContext _componentContext;
    private readonly IListService _listService;
    private readonly IDalamudPluginInterface _pluginInterfaceService;

    public MigrationManagerService(ILogger<MigrationManagerService> logger, InventoryToolsConfiguration configuration, IMarketCache marketCache, IFilterService filterService, IServiceProvider serviceProvider, IComponentContext componentContext, IListService listService, IDalamudPluginInterface pluginInterfaceService)
    {
        _logger = logger;
        _configuration = configuration;
        _marketCache = marketCache;
        _filterService = filterService;
        _serviceProvider = serviceProvider;
        _componentContext = componentContext;
        _listService = listService;
        _pluginInterfaceService = pluginInterfaceService;
    }

    public void RunMigrations()
    {
        var config = _configuration;
        if (config.InternalVersion == 0)
        {
            _logger.LogInformation("Migrating to version 1");
            var highlight = config.HighlightColor;
            if (highlight.W == 0.0f)
            {
                highlight.W = 1;
                config.HighlightColor = highlight;
            }
            config.TabHighlightColor = new(0.007f, 0.008f, 0.007f, 1.0f);

            foreach (var filterConfig in config.FilterConfigurations)
            {
                if (filterConfig.HighlightColor != null)
                {
                    if (filterConfig.HighlightColor.Value.X == 0.0f && filterConfig.HighlightColor.Value.Y == 0.0f &&
                        filterConfig.HighlightColor.Value.Z == 0.0f && filterConfig.HighlightColor.Value.W == 0.0f)
                    {
                        filterConfig.HighlightColor = null;
                        filterConfig.TabHighlightColor = null;
                    }
                    else
                    {
                        var highlightColor = filterConfig.HighlightColor.Value;
                        highlightColor.W = 1;
                        filterConfig.TabHighlightColor = highlightColor;
                    }
                }
            }

            config.InternalVersion++;
        }
        if (config.InternalVersion == 1)
        {
            _logger.LogInformation("Migrating to version 2");
            config.InvertTabHighlighting = config.InvertHighlighting;

            foreach (var filterConfig in config.FilterConfigurations)
            {
                if (filterConfig.InvertHighlighting != null)
                {
                    filterConfig.InvertTabHighlighting = filterConfig.InvertHighlighting;
                }
            }

            config.InternalVersion++;
        }
        if (config.InternalVersion == 2)
        {
            _logger.LogInformation("Migrating to version 3");
            config.InternalVersion++;
        }
        if (config.InternalVersion == 3)
        {
            _logger.LogInformation("Migrating to version 4");



            foreach (var filterConfig in config.FilterConfigurations)
            {
                _serviceProvider.GetRequiredService<IsHqFilter>().UpdateFilterConfiguration(filterConfig, filterConfig.IsHq);
                _serviceProvider.GetRequiredService<IsCollectibleFilter>().UpdateFilterConfiguration(filterConfig, filterConfig.IsCollectible);
                _serviceProvider.GetRequiredService<NameFilter>().UpdateFilterConfiguration(filterConfig, filterConfig.NameFilter);
                _serviceProvider.GetRequiredService<QuantityFilter>().UpdateFilterConfiguration(filterConfig, filterConfig.Quantity);
                _serviceProvider.GetRequiredService<ItemLevelFilter>().UpdateFilterConfiguration(filterConfig, filterConfig.ILevel);
                _serviceProvider.GetRequiredService<SpiritBondFilter>().UpdateFilterConfiguration(filterConfig, filterConfig.Spiritbond);
                _serviceProvider.GetRequiredService<SellToVendorPriceFilter>().UpdateFilterConfiguration(filterConfig, filterConfig.ShopSellingPrice);
                _serviceProvider.GetRequiredService<BuyFromVendorPriceFilter>().UpdateFilterConfiguration(filterConfig, filterConfig.ShopBuyingPrice);
                _serviceProvider.GetRequiredService<MarketBoardPriceFilter>().UpdateFilterConfiguration(filterConfig, filterConfig.MarketAveragePrice);
                _serviceProvider.GetRequiredService<MarketBoardTotalPriceFilter>().UpdateFilterConfiguration(filterConfig, filterConfig.MarketTotalAveragePrice);
                _serviceProvider.GetRequiredService<ItemUiCategoryFilter>().UpdateFilterConfiguration(filterConfig, filterConfig.ItemUiCategoryId);
                _serviceProvider.GetRequiredService<SearchCategoryFilter>().UpdateFilterConfiguration(filterConfig, filterConfig.ItemSearchCategoryId);
            }
            config.InternalVersion++;
        }

        if (config.InternalVersion == 4)
        {
            _logger.LogInformation("Migrating to version 5");
            config.RetainerListColor = ImGuiColors.HealerGreen;
            config.InternalVersion++;
        }

        if (config.InternalVersion == 5)
        {
            _logger.LogInformation("Migrating to version 6");
            config.TooltipDisplayAmountOwned = true;
            config.TooltipDisplayMarketAveragePrice = true;
            config.InternalVersion++;
        }

        if (config.InternalVersion == 6)
        {
            _logger.LogInformation("Migrating to version 7");
            config.HighlightDestination = true;
            config.DestinationHighlightColor = new Vector4(0.321f, 0.239f, 0.03f, 1f);
            config.InternalVersion++;
        }

        if (config.InternalVersion == 7)
        {
            config.InternalVersion++;
        }

        if (config.InternalVersion == 8)
        {
            _logger.LogInformation("Migrating to version 9");
            var order = 0u;
            foreach (var configuration in config.FilterConfigurations)
            {
                if (configuration.FilterType != FilterType.CraftFilter)
                {
                    configuration.Order = order;
                    order++;
                }
            }
            order = 0u;
            foreach (var configuration in config.FilterConfigurations)
            {
                if (configuration.FilterType == FilterType.CraftFilter)
                {
                    configuration.Order = order;
                    order++;
                }
            }
            config.InternalVersion++;
        }

        if (config.InternalVersion == 9)
        {
            _logger.LogInformation("Migrating to version 10");
            foreach (var configuration in config.FilterConfigurations)
            {
#pragma warning disable CS0612
                if (configuration.FilterItemsInRetainers.HasValue && configuration.FilterItemsInRetainers == true)
#pragma warning restore CS0612
                {
                    configuration.FilterItemsInRetainersEnum = FilterItemsRetainerEnum.Yes;
                }
                else
                {
                    configuration.FilterItemsInRetainersEnum = FilterItemsRetainerEnum.No;
                }
            }
            config.InternalVersion++;
        }

        if (config.InternalVersion == 10)
        {
            _logger.LogInformation("Migrating to version 11");
            config.InternalVersion++;
        }

        if (config.InternalVersion == 11)
        {
            _logger.LogInformation("Migrating to version 12");
            config.TooltipLocationLimit = 10;
            config.TooltipLocationDisplayMode =
                TooltipLocationDisplayMode.CharacterCategoryQuantityQuality;
            config.InternalVersion++;
        }

        if (config.InternalVersion == 12)
        {
            _logger.LogInformation("Migrating to version 13");
            config.FiltersLayout = WindowLayout.Tabs;
            config.CraftWindowLayout = WindowLayout.Tabs;
            config.InternalVersion++;
        }

        if (config.InternalVersion == 13)
        {
            _logger.LogInformation("Migrating to version 14");
            var toReset = _filterService.AvailableFilters.Where(c =>
                c is CraftCrystalGroupFilter or CraftCurrencyGroupFilter or CraftPrecraftGroupFilter
                    or CraftRetrieveGroupFilter or CraftEverythingElseGroupFilter or CraftIngredientPreferenceFilter
                    or CraftDefaultHQRequiredFilter or CraftDefaultRetrieveFromRetainerFilter
                    or CraftDefaultRetrieveFromRetainerOutputFilter).ToList();
            var hadDefaultCraftList = config.HasDefaultCraftList();

            foreach (var filterConfig in config.FilterConfigurations)
            {
                if (filterConfig.FilterType == FilterType.CraftFilter)
                {
                    if (hadDefaultCraftList && filterConfig.CraftListDefault)
                    {
                        _listService.AddDefaultColumns(filterConfig);
                    }

                    foreach (var filter in toReset)
                    {
                        filter.ResetFilter(filterConfig);
                    }
                }
            }
            config.InternalVersion++;
        }

        if (config.InternalVersion == 14)
        {
            _logger.LogInformation("Migrating to version 15");
            config.HistoryTrackReasons = new()
            {
                InventoryChangeReason.Added,
                InventoryChangeReason.Moved,
                InventoryChangeReason.Removed,
                InventoryChangeReason.Transferred,
                InventoryChangeReason.MarketPriceChanged,
                InventoryChangeReason.QuantityChanged
            };
            config.InternalVersion++;
        }

        if (config.InternalVersion == 15)
        {
            _logger.LogInformation("Migrating to version 16");
            var hasExistingHistoryList = config.HasList("History");
            if (!hasExistingHistoryList && !config.FirstRun)
            {
                var historyFilter = new FilterConfiguration("History",  FilterType.HistoryFilter);
                historyFilter.DisplayInTabs = true;
                historyFilter.SourceAllCharacters = true;
                historyFilter.SourceAllRetainers = true;
                historyFilter.SourceAllFreeCompanies = true;
                historyFilter.SourceAllHouses = true;
                config.FilterConfigurations.Add(historyFilter);
            }
            config.InternalVersion++;
        }

        if (config.InternalVersion == 16)
        {
            _logger.LogInformation("Migrating to version 17");
            foreach (var filterConfig in config.FilterConfigurations)
            {
                if (filterConfig.FilterType == FilterType.CraftFilter)
                {
                    filterConfig.UpdateBooleanFilter("CraftWorldPriceUseActiveWorld", true);
                    filterConfig.UpdateBooleanFilter("CraftWorldPriceUseDefaults", true);
                    filterConfig.UpdateBooleanFilter("CraftWorldPriceUseHomeWorld", true);
                }
            }

            config.InternalVersion++;
        }
        if (config.InternalVersion == 17)
        {
            _logger.LogInformation("Migrating to version 18");
            AddStain2ToInventories();

            config.InternalVersion++;
        }

        if (config.InternalVersion == 18)
        {
            _logger.LogInformation("Migrating to version 19");

            foreach (var filterConfig in config.FilterConfigurations)
            {
                _componentContext.Resolve<GenericHasUseFilter>(new NamedParameter("itemType", ItemInfoType.Desynthesis)).UpdateFilterConfiguration(filterConfig, filterConfig.GetBooleanFilter("desynth"));
                _componentContext.Resolve<GenericHasUseFilter>(new NamedParameter("itemType", ItemInfoType.Stain)).UpdateFilterConfiguration(filterConfig, filterConfig.GetBooleanFilter("CanBeDyed"));
                _componentContext.Resolve<GenericHasUseFilter>(new NamedParameter("itemType", ItemInfoType.Aquarium)).UpdateFilterConfiguration(filterConfig, filterConfig.GetBooleanFilter("IsAquarium"));
                _componentContext.Resolve<GenericHasUseFilter>(new NamedParameter("itemType", ItemInfoType.Armoire)).UpdateFilterConfiguration(filterConfig, filterConfig.GetBooleanFilter("IsArmoire"));
                _componentContext.Resolve<GenericHasUseFilter>(new NamedParameter("itemType", ItemInfoType.SpecialShop)).UpdateFilterConfiguration(filterConfig, filterConfig.GetBooleanFilter("IsCurrency"));
                _componentContext.Resolve<GenericHasUseFilter>(new NamedParameter("itemType", ItemInfoType.CustomDelivery)).UpdateFilterConfiguration(filterConfig, filterConfig.GetBooleanFilter("IsCustomDeliveryItem"));
                _componentContext.Resolve<GenericHasUseFilter>(new NamedParameter("itemType", ItemInfoType.GCDailySupply)).UpdateFilterConfiguration(filterConfig, filterConfig.GetBooleanFilter("IsGCSupplyItem"));
                _componentContext.Resolve<GenericHasUseFilter>(new NamedParameter("itemType", ItemInfoType.SkybuilderHandIn)).UpdateFilterConfiguration(filterConfig, filterConfig.GetBooleanFilter("IsIshgardCraft"));
                _componentContext.Resolve<GenericHasSourceFilter>(new NamedParameter("itemType", ItemInfoType.GilShop)).UpdateFilterConfiguration(filterConfig, filterConfig.GetBooleanFilter("Purchasable"));
                _componentContext.Resolve<GenericHasSourceFilter>(new NamedParameter("itemType", ItemInfoType.CraftRecipe)).UpdateFilterConfiguration(filterConfig, filterConfig.GetBooleanFilter("CanCraft"));
                _componentContext.Resolve<GenericHasSourceFilter>(new NamedParameter("itemType", ItemInfoType.CalamitySalvagerShop)).UpdateFilterConfiguration(filterConfig, filterConfig.GetBooleanFilter("FromCalamitySalvager"));
                _componentContext.Resolve<GenericHasSourceFilter>(new NamedParameter("itemType", ItemInfoType.Fate)).UpdateFilterConfiguration(filterConfig, filterConfig.GetBooleanFilter("IsFromFate"));
                _componentContext.Resolve<GenericHasSourceFilter>(new NamedParameter("itemType", ItemInfoType.Monster)).UpdateFilterConfiguration(filterConfig, filterConfig.GetBooleanFilter("IsMobDrop"));
                _componentContext.Resolve<GenericHasSourceFilter>(new NamedParameter("itemType", ItemInfoType.CraftLeve)).UpdateFilterConfiguration(filterConfig, filterConfig.GetBooleanFilter("LeveIsCraftLeve"));
                _componentContext.Resolve<GenericHasSourceFilter>(new NamedParameter("itemType", ItemInfoType.CashShop)).UpdateFilterConfiguration(filterConfig, filterConfig.GetBooleanFilter("StoreFilter"));
                _componentContext.Resolve<GenericHasSourceCategoryFilter>(new NamedParameter("renderCategory", ItemInfoRenderCategory.Gathering)).UpdateFilterConfiguration(filterConfig, filterConfig.GetBooleanFilter("Gatherable"));
                _componentContext.Resolve<GenericHasSourceCategoryFilter>(new NamedParameter("renderCategory", ItemInfoRenderCategory.EphemeralGathering)).UpdateFilterConfiguration(filterConfig, filterConfig.GetBooleanFilter("EphemeralNode"));
                _componentContext.Resolve<GenericHasSourceCategoryFilter>(new NamedParameter("renderCategory", ItemInfoRenderCategory.HiddenGathering)).UpdateFilterConfiguration(filterConfig, filterConfig.GetBooleanFilter("HiddenNode"));
                _componentContext.Resolve<GenericHasSourceCategoryFilter>(new NamedParameter("renderCategory", ItemInfoRenderCategory.TimedGathering)).UpdateFilterConfiguration(filterConfig, filterConfig.GetBooleanFilter("TimedNode"));
                _componentContext.Resolve<GenericHasUseCategoryFilter>(new NamedParameter("renderCategory", ItemInfoRenderCategory.Crafting)).UpdateFilterConfiguration(filterConfig, filterConfig.GetBooleanFilter("IsCrafting"));
                _componentContext.Resolve<GenericHasUseCategoryFilter>(new NamedParameter("renderCategory", ItemInfoRenderCategory.House)).UpdateFilterConfiguration(filterConfig, filterConfig.GetBooleanFilter("IsHousing"));
            }
            config.InternalVersion++;

        }
    }

    private string GetNewFileName(string fileName, string extension)
    {
        var path = Path.Join(_pluginInterfaceService.ConfigDirectory.FullName, fileName + "." + extension);
        var fileIndex = 0;
        while (File.Exists(path))
        {
            fileIndex++;
            path = Path.Join(_pluginInterfaceService.ConfigDirectory.FullName, fileName + fileIndex + "." + extension);
        }

        return path;
    }
    private void AddStain2ToInventories()
    {
        string inputFile  = Path.Join(_pluginInterfaceService.ConfigDirectory.FullName, "inventories.csv");
        string outputFile = Path.Join(_pluginInterfaceService.ConfigDirectory.FullName, "inventories_migration.csv");

        if (!File.Exists(inputFile))
        {
            return;
        }

        if (File.Exists(outputFile))
        {
            File.Delete(outputFile);
        }

        try
        {
            using (var reader = new StreamReader(inputFile))
            using (var writer = new StreamWriter(outputFile))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = new List<string>(line.Split(','));

                    // Insert a new column at position 18
                    values.Insert(18, "0");

                    var newLine = string.Join(",", values);
                    writer.WriteLine(newLine);
                }
            }

            var backupFile = GetNewFileName("inventories_backup_M17", "csv");
            File.Move(inputFile, backupFile);
            File.Move(outputFile, inputFile);
        }
        catch (Exception e)
        {
            var backupFile = GetNewFileName("inventories_backup_M17", "csv");
            _logger.LogError($"Failed to migrate inventories, has been saved to {backupFile}");
            File.Copy(inputFile, backupFile);
        }
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Starting service {type} ({this})", GetType().Name, this);
        RunMigrations();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Stopped service {type} ({this})", GetType().Name, this);
        return Task.CompletedTask;
    }
}
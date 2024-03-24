using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using Dalamud.Interface.Colors;
using Dalamud.Plugin.Services;
using InventoryTools.Logic;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Services;

public class MigrationManager : IHostedService
{
    private readonly ILogger _log;
    private readonly ILogger<MigrationManager> _logger;
    private readonly InventoryToolsConfiguration _configuration;
    private readonly IMarketCache _marketCache;
    private readonly IFilterService _filterService;
    private readonly IServiceProvider _serviceProvider;

    public MigrationManager(ILogger<MigrationManager> logger, InventoryToolsConfiguration configuration, IMarketCache marketCache, IFilterService filterService, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _marketCache = marketCache;
        _filterService = filterService;
        _serviceProvider = serviceProvider;
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
            foreach (var filterConfig in config.FilterConfigurations)
            {
                filterConfig.GenerateNewTableId();
                filterConfig.Columns = new List<ColumnConfiguration>();
                filterConfig.AddColumn("IconColumn");
                filterConfig.AddColumn("NameColumn");
                filterConfig.AddColumn("TypeColumn");
                filterConfig.AddColumn("SourceColumn");
                filterConfig.AddColumn("LocationColumn");
                if (filterConfig.FilterType == FilterType.SortingFilter)
                {
                    filterConfig.AddColumn("DestinationColumn");
                }
                filterConfig.AddColumn("QuantityColumn");
                filterConfig.AddColumn("ItemILevelColumn");
                filterConfig.AddColumn("SearchCategoryColumn");
                filterConfig.AddColumn("MarketBoardPriceColumn");
            }
            _marketCache.ClearCache();
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
                _serviceProvider.GetRequiredService<CanBePurchasedFilter>().UpdateFilterConfiguration(filterConfig, filterConfig.CanBeBought);
                _serviceProvider.GetRequiredService<MarketBoardPriceFilter>().UpdateFilterConfiguration(filterConfig, filterConfig.MarketAveragePrice);
                _serviceProvider.GetRequiredService<MarketBoardTotalPriceFilter>().UpdateFilterConfiguration(filterConfig, filterConfig.MarketTotalAveragePrice);
                _serviceProvider.GetRequiredService<IsTimedNodeFilter>().UpdateFilterConfiguration(filterConfig, filterConfig.IsAvailableAtTimedNode);
                _serviceProvider.GetRequiredService<ItemUiCategoryFilter>().UpdateFilterConfiguration(filterConfig, filterConfig.ItemUiCategoryId);
                _serviceProvider.GetRequiredService<SearchCategoryFilter>().UpdateFilterConfiguration(filterConfig, filterConfig.ItemSearchCategoryId);
                filterConfig.FilterType++;
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
            foreach (var configuration in config.FilterConfigurations)
            {
                foreach (var filterConfig in config.FilterConfigurations)
                {
                    filterConfig.TableHeight = 32;
                    filterConfig.CraftTableHeight = 32;
                    if (filterConfig.FilterType == FilterType.CraftFilter)
                    {
                        filterConfig.FreezeCraftColumns = 2;
                        filterConfig.GenerateNewCraftTableId();
                        filterConfig.CraftColumns = new();
                        filterConfig.AddCraftColumn("IconColumn");
                        filterConfig.AddCraftColumn("NameColumn");
                        if (filterConfig.SimpleCraftingMode == true)
                        {
                            filterConfig.AddCraftColumn("CraftAmountRequiredColumn");
                            filterConfig.AddCraftColumn("CraftSimpleColumn");
                        }
                        else
                        {
                            filterConfig.AddCraftColumn("QuantityAvailableColumn");
                            filterConfig.AddCraftColumn("CraftAmountRequiredColumn");
                            filterConfig.AddCraftColumn("CraftAmountReadyColumn");
                            filterConfig.AddCraftColumn("CraftAmountAvailableColumn");
                            filterConfig.AddCraftColumn("CraftAmountUnavailableColumn");
                            filterConfig.AddCraftColumn("CraftAmountCanCraftColumn");
                        }
                        filterConfig.AddCraftColumn("MarketBoardMinPriceColumn");
                        filterConfig.AddCraftColumn("MarketBoardMinTotalPriceColumn");
                        filterConfig.AddCraftColumn("AcquisitionSourceIconsColumn");
                        filterConfig.AddCraftColumn("CraftGatherColumn");
                    }
                }
            }
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
                        filterConfig.AddDefaultColumns();
                    }

                    foreach (var filter in toReset)
                    {
                        filter.ResetFilter(filterConfig);
                    }

                    if (hadDefaultCraftList || !filterConfig.CraftListDefault)
                    {
                        filterConfig.AddCraftColumn("CraftSettingsColumn", 2);
                        filterConfig.AddCraftColumn("CraftSimpleColumn", 3);
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
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Starting service {type} ({this})", GetType().Name, this);
        RunMigrations();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
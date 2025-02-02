using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;

using Dalamud.Plugin.Services;
using InventoryTools.Hotkeys;
using InventoryTools.Logic;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.ItemRenderers;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using InventoryTools.Tooltips;
using InventoryTools.Ui;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InventoryTools
{
    public partial class PluginLogic : DisposableMediatorSubscriberBase, IHostedService
    {
        private readonly ConfigurationManagerService _configurationManagerService;
        private readonly IChatUtilities _chatUtilities;
        private readonly IListService _listService;
        private readonly ILogger<PluginLogic> _logger;
        private readonly IFramework _framework;
        private readonly HostedInventoryHistory _hostedInventoryHistory;
        private readonly IInventoryMonitor _inventoryMonitor;
        private readonly IInventoryScanner _inventoryScanner;
        private readonly ICharacterMonitor _characterMonitor;
        private readonly InventoryToolsConfiguration _configuration;
        private readonly IMobTracker _mobTracker;
        private readonly IHotkeyService _hotkeyService;
        private readonly ICraftMonitor _craftMonitor;
        private readonly IUnlockTrackerService _unlockTrackerService;
        private readonly IEnumerable<BaseTooltip> tooltips;
        private readonly IEnumerable<IHotkey> hotkeys;
        private readonly ITooltipService _tooltipService;
        private readonly Func<ItemInfoRenderCategory, GenericHasSourceCategoryFilter> _sourceCategoryFilterFactory;
        private readonly BuyFromVendorPriceFilter _buyFromVendorPriceFilter;
        private readonly Func<Type, IFilter> _filterFactory;
        private readonly IMarketCache _marketCache;
        private Dictionary<uint, InventoryMonitor.ItemChangesItem> _recentlyAddedSeen = new();

        public bool WasRecentlySeen(uint itemId)
        {
            if (_recentlyAddedSeen.ContainsKey(itemId))
            {
                return true;
            }
            return false;
        }

        public TimeSpan? GetLastSeenTime(uint itemId)
        {
            if (WasRecentlySeen(itemId))
            {
                return DateTime.Now - _recentlyAddedSeen[itemId].Date;
            }
            return null;
        }

        private DateTime? _nextSaveTime = null;

        public PluginLogic(ConfigurationManagerService configurationManagerService, IChatUtilities chatUtilities, IListService listService, ILogger<PluginLogic> logger, IFramework framework, MediatorService mediatorService, HostedInventoryHistory hostedInventoryHistory, IInventoryMonitor inventoryMonitor, IInventoryScanner inventoryScanner, ICharacterMonitor characterMonitor, InventoryToolsConfiguration configuration, IMobTracker mobTracker, IHotkeyService hotkeyService, ICraftMonitor craftMonitor, IUnlockTrackerService unlockTrackerService,IEnumerable<BaseTooltip> tooltips, IEnumerable<IHotkey> hotkeys, Func<Type,IFilter> filterFactory, IMarketCache marketCache, ITooltipService tooltipService, Func<ItemInfoRenderCategory, GenericHasSourceCategoryFilter> sourceCategoryFilterFactory, BuyFromVendorPriceFilter buyFromVendorPriceFilter) : base(logger, mediatorService)
        {
            _configurationManagerService = configurationManagerService;
            _chatUtilities = chatUtilities;
            _listService = listService;
            _logger = logger;
            _framework = framework;
            _hostedInventoryHistory = hostedInventoryHistory;
            _inventoryMonitor = inventoryMonitor;
            _inventoryScanner = inventoryScanner;
            _characterMonitor = characterMonitor;
            _configuration = configuration;
            _mobTracker = mobTracker;
            _hotkeyService = hotkeyService;
            _craftMonitor = craftMonitor;
            _unlockTrackerService = unlockTrackerService;
            this.tooltips = tooltips;
            this.hotkeys = hotkeys;
            _tooltipService = tooltipService;
            _sourceCategoryFilterFactory = sourceCategoryFilterFactory;
            _buyFromVendorPriceFilter = buyFromVendorPriceFilter;
            _filterFactory = filterFactory;
            _marketCache = marketCache;
            this.MediatorService.Subscribe<PluginLoadedMessage>(this, PluginLoaded);

        }

        private void PluginLoaded(PluginLoadedMessage obj)
        {
            _inventoryMonitor.Start();
            _inventoryScanner.Enable();
        }

        private void CraftMonitorOnCraftCompleted(uint itemid, FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags flags, uint quantity)
        {
            _logger.LogTrace("Craft completed for item " + itemid);

            var activeCraftList = _listService.GetActiveCraftList();
            if (activeCraftList != null && activeCraftList.FilterType == FilterType.CraftFilter && activeCraftList.CraftList.CraftListMode == CraftListMode.Normal)
            {
                activeCraftList.CraftList.MarkCrafted(itemid, flags, quantity);
                if (activeCraftList is { IsEphemeralCraftList: true, CraftList.IsCompleted: true })
                {
                    _chatUtilities.Print("Ephemeral craft list '" + activeCraftList.Name + "' completed. List has been removed.");
                    _listService.RemoveList(activeCraftList);
                }
                else
                {
                    activeCraftList.NeedsRefresh = true;
                }
            }
        }

        private void CraftMonitorOnCraftFailed(uint itemid)
        {
            _logger.LogTrace("Craft failed for item " + itemid);
        }

        private void CraftMonitorOnCraftStarted(uint itemid)
        {
            _logger.LogTrace("Craft started for item " + itemid);
        }

        private void UnlockTrackerServiceOnAcquiredItemsUpdated()
        {
            var activeCharacter = _characterMonitor.ActiveCharacterId;
            if (activeCharacter != 0)
            {
                _configuration.AcquiredItems[activeCharacter] = _unlockTrackerService.UnlockedItems;
            }
        }

        public void ClearOrphans()
        {
            var keys = _inventoryMonitor.Inventories.Keys;
            foreach (var key in keys)
            {
                var character = _characterMonitor.GetCharacterById(key);
                if (character == null)
                {
                    _logger.LogInformation("Removing inventories for " + key + " from inventory cache as there is no character associated with this inventory.");
                    _inventoryMonitor.ClearCharacterInventories(key);
                }
            }
        }

        private void FrameworkOnUpdate(IFramework framework)
        {
            if (_configuration.AutoSave)
            {
                if (NextSaveTime == null && _configuration.AutoSaveMinutes != 0)
                {
                    _nextSaveTime = DateTime.Now.AddMinutes(_configuration.AutoSaveMinutes);
                }
                else
                {
                    if (DateTime.Now >= NextSaveTime)
                    {
                        _nextSaveTime = null;
                        _configuration.IsDirty = true;
                    }
                }
            }
        }

        private void ConfigOnConfigurationChanged()
        {
            SyncConfigurationChanges();
        }

        private void SyncConfigurationChanges(bool save = true)
        {
            if (_mobTracker.Enabled != _configuration.TrackMobSpawns)
            {
                if (_configuration.TrackMobSpawns)
                {
                    _mobTracker.Enable();
                }
                else
                {
                    _mobTracker.Disable();
                }
            }

            if (_hostedInventoryHistory.Enabled != _configuration.HistoryEnabled)
            {
                if (_configuration.HistoryEnabled)
                {
                    _hostedInventoryHistory.Enable();
                }
                else
                {
                    _hostedInventoryHistory.Disable();
                }
            }

            if (_configuration.HistoryTrackReasons != null)
            {
                if (_hostedInventoryHistory.ReasonsToLog.ToList() !=
                    _configuration.HistoryTrackReasons)
                {
                    _hostedInventoryHistory.SetChangeReasonsToLog(
                        _configuration.HistoryTrackReasons.Distinct().ToHashSet());
                }
            }
        }

        public void LoadDefaultData()
        {
            _listService.GetDefaultCraftList();

            AddAllFilter();

            AddRetainerFilter();

            AddPlayerFilter();

            AddFreeCompanyFilter();

            AddHousingFilter();

            AddAllGameItemsFilter();

            AddFavouritesFilter();

            AddCraftFilter();

            AddHistoryFilter();
        }

        public void AddAllFilter(string newName = "All")
        {
            var allItemsFilter = new FilterConfiguration(newName, FilterType.SearchFilter);
            allItemsFilter.DisplayInTabs = true;
            allItemsFilter.SourceAllCharacters = true;
            allItemsFilter.SourceAllRetainers = true;
            allItemsFilter.SourceAllFreeCompanies = true;
            _listService.AddDefaultColumns(allItemsFilter);
            _listService.AddList(allItemsFilter);
        }

        public void AddRetainerFilter(string newName = "Retainers")
        {
            var retainerItemsFilter = new FilterConfiguration(newName, FilterType.SearchFilter);
            retainerItemsFilter.DisplayInTabs = true;
            retainerItemsFilter.SourceAllRetainers = true;
            _listService.AddDefaultColumns(retainerItemsFilter);
            _listService.AddList(retainerItemsFilter);
        }

        public void AddPlayerFilter(string newName = "Player")
        {
            var playerItemsFilter = new FilterConfiguration(newName,  FilterType.SearchFilter);
            playerItemsFilter.DisplayInTabs = true;
            playerItemsFilter.SourceAllCharacters = true;
            _listService.AddDefaultColumns(playerItemsFilter);
            _listService.AddList(playerItemsFilter);
        }

        public void AddHistoryFilter(string newName = "History")
        {
            var historyFilter = new FilterConfiguration(newName,  FilterType.HistoryFilter);
            historyFilter.DisplayInTabs = true;
            historyFilter.SourceAllCharacters = true;
            historyFilter.SourceAllRetainers = true;
            historyFilter.SourceAllFreeCompanies = true;
            historyFilter.SourceAllHouses = true;
            _listService.AddDefaultColumns(historyFilter);
            _listService.AddList(historyFilter);
        }

        public void AddFreeCompanyFilter(string newName = "Free Company")
        {
            var newFilter = new FilterConfiguration(newName,  FilterType.SearchFilter);
            newFilter.DisplayInTabs = true;
            newFilter.SourceAllFreeCompanies = true;
            _listService.AddDefaultColumns(newFilter);
            _listService.AddList(newFilter);
        }

        public void AddHousingFilter(string newName = "Housing")
        {
            var newFilter = new FilterConfiguration(newName,  FilterType.SearchFilter);
            newFilter.DisplayInTabs = true;
            newFilter.SourceAllHouses = true;
            _listService.AddDefaultColumns(newFilter);
            _listService.AddList(newFilter);
        }

        public void AddAllGameItemsFilter(string newName = "All Game Items")
        {
            var allGameItemsFilter = new FilterConfiguration(newName, FilterType.GameItemFilter);
            allGameItemsFilter.DisplayInTabs = true;
            _listService.AddDefaultColumns(allGameItemsFilter);
            _listService.AddList(allGameItemsFilter);
        }

        public void AddFavouritesFilter(string newName = "Favourites")
        {
            var newFilter = new FilterConfiguration(newName, FilterType.GameItemFilter);
            var favouritesFilter = (FavouritesFilter)_filterFactory.Invoke(typeof(FavouritesFilter));
            favouritesFilter.UpdateFilterConfiguration(newFilter, true);
            newFilter.DisplayInTabs = true;
            _listService.AddDefaultColumns(newFilter);
            _listService.AddList(newFilter);
        }

        public void AddCraftFilter(string newName = "Craft List")
        {
            var newFilter = _listService.AddNewCraftList(newName);
            newFilter.DisplayInTabs = true;
        }

        public void AddNewCraftFilter()
        {
            var filterConfiguration = _listService.AddNewCraftList();
            MediatorService.Publish(new FocusListMessage(typeof(CraftsWindow), filterConfiguration));
        }

        public void AddFilter(FilterConfiguration filterConfiguration)
        {
            filterConfiguration.DestinationInventories.Clear();
            filterConfiguration.SourceInventories.Clear();
            _listService.AddList(filterConfiguration);
        }

        public void AddSampleFilter100Gil(string newName = "100 gil or less")
        {
            var sampleFilter = new FilterConfiguration(newName, FilterType.SearchFilter);
            sampleFilter.DisplayInTabs = true;
            sampleFilter.SourceAllCharacters = true;
            sampleFilter.SourceAllRetainers = true;
            sampleFilter.SourceAllFreeCompanies = true;
            _sourceCategoryFilterFactory.Invoke(ItemInfoRenderCategory.Shop).UpdateFilterConfiguration(sampleFilter, true);
            _buyFromVendorPriceFilter.UpdateFilterConfiguration(sampleFilter, "<=100");
            _listService.AddList(sampleFilter);
            _listService.AddDefaultColumns(sampleFilter);
        }

        public void AddSampleFilterMaterials(string newName = "Put away materials")
        {
            var sampleFilter = new FilterConfiguration(newName, FilterType.SortingFilter);
            sampleFilter.DisplayInTabs = true;
            sampleFilter.SourceCategories = new HashSet<InventoryCategory>() {InventoryCategory.CharacterBags};
            sampleFilter.DestinationCategories =  new HashSet<InventoryCategory>() {InventoryCategory.RetainerBags};
            sampleFilter.FilterItemsInRetainersEnum = FilterItemsRetainerEnum.Yes;
            sampleFilter.HighlightWhen = "Always";
            var gatherFilter = _sourceCategoryFilterFactory.Invoke(ItemInfoRenderCategory.Gathering);
            gatherFilter.UpdateFilterConfiguration(sampleFilter, true);
            _listService.AddList(sampleFilter);
            _listService.AddDefaultColumns(sampleFilter);
        }

        public void AddSampleFilterDuplicatedItems(string newName = "Duplicated SortItems")
        {
            var sampleFilter = new FilterConfiguration(newName, FilterType.SortingFilter);
            sampleFilter.DisplayInTabs = true;
            sampleFilter.SourceCategories = new HashSet<InventoryCategory>() {InventoryCategory.CharacterBags,InventoryCategory.RetainerBags};
            sampleFilter.DestinationCategories =  new HashSet<InventoryCategory>() {InventoryCategory.RetainerBags};
            sampleFilter.FilterItemsInRetainersEnum = FilterItemsRetainerEnum.Yes;
            sampleFilter.DuplicatesOnly = true;
            sampleFilter.HighlightWhen = "Always";
            _listService.AddList(sampleFilter);
            _listService.AddDefaultColumns(sampleFilter);
        }


        public DateTime? NextSaveTime => _nextSaveTime;

        public void ClearAutoSave()
        {
            _nextSaveTime = null;
        }

        private void InventoryMonitorOnOnInventoryChanged(List<InventoryChange> inventoryChanges, InventoryMonitor.ItemChanges? itemChanges)
        {
            _logger.LogTrace("PluginLogic: Inventory changed, saving to config.");
            var allItems = _inventoryMonitor.AllItems.ToList();
            _configurationManagerService.SaveInventoriesAsync(allItems);
            if (_configuration.AutomaticallyDownloadMarketPrices)
            {
                var activeCharacter = _characterMonitor.ActiveCharacter;
                if (activeCharacter != null)
                {
                    foreach (var inventory in allItems)
                    {
                        _marketCache.RequestCheck(inventory.ItemId, activeCharacter.WorldId, false);
                    }
                }
            }

            if (itemChanges != null)
            {
                foreach (var item in itemChanges.NewItems)
                {
                    if (_recentlyAddedSeen.ContainsKey(item.ItemId))
                    {
                        _recentlyAddedSeen.Remove(item.ItemId);
                    }

                    _recentlyAddedSeen.Add(item.ItemId, item);
                }
            }
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogTrace("Starting service {type} ({this})", GetType().Name, this);
            _inventoryMonitor.OnInventoryChanged += InventoryMonitorOnOnInventoryChanged;
            _framework.Update += FrameworkOnUpdate;
            _configurationManagerService.ConfigurationChanged += ConfigOnConfigurationChanged;

            _craftMonitor.CraftStarted += CraftMonitorOnCraftStarted;
            _craftMonitor.CraftFailed += CraftMonitorOnCraftFailed ;
            _craftMonitor.CraftCompleted += CraftMonitorOnCraftCompleted ;
            _unlockTrackerService.ItemUnlockStatusChanged += UnlockTrackerServiceOnAcquiredItemsUpdated;

            foreach (var tooltip in tooltips.OrderBy(c => c.Order))
            {
                _tooltipService.AddTooltipTweak(tooltip);
            }

            if (_configuration.FirstRun)
            {
                LoadDefaultData();
                _configuration.FirstRun = false;
            }
            SyncConfigurationChanges(false);
            ClearOrphans();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogTrace("Stopping service {type} ({this})", GetType().Name, this);
            _unlockTrackerService.ItemUnlockStatusChanged -= UnlockTrackerServiceOnAcquiredItemsUpdated;
            _configuration.SavedCharacters = _characterMonitor.Characters;
            _framework.Update -= FrameworkOnUpdate;
            _inventoryMonitor.OnInventoryChanged -= InventoryMonitorOnOnInventoryChanged;
            _craftMonitor.CraftStarted -= CraftMonitorOnCraftStarted;
            _craftMonitor.CraftFailed -= CraftMonitorOnCraftFailed ;
            _craftMonitor.CraftCompleted -= CraftMonitorOnCraftCompleted ;
            _configurationManagerService.ConfigurationChanged -= ConfigOnConfigurationChanged;
            _configurationManagerService.Save();
            _configurationManagerService.SaveInventoriesAsync(_inventoryMonitor.AllItems.ToList()).Wait(TimeSpan.FromSeconds(2));
            _configurationManagerService.SaveHistory(_hostedInventoryHistory.GetHistory());
            if (_configuration.TrackMobSpawns)
            {
                _mobTracker.SaveCsv(_configurationManagerService.MobSpawnFile,
                    _mobTracker.GetEntries());
            }
            return Task.CompletedTask;
        }
    }
}

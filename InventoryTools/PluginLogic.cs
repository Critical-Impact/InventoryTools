using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Monitors.Enums;
using AllaganLib.Monitors.Interfaces;
using AllaganLib.Monitors.Services;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Plugin.Services;
using InventoryTools.Hotkeys;
using InventoryTools.Logic;
using InventoryTools.Logic.Features;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.ItemRenderers;
using InventoryTools.Logic.Settings;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using InventoryTools.Tooltips;
using InventoryTools.Ui;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using InventoryItem = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem;

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
        private readonly ICraftMonitor _craftMonitor;
        private readonly IUnlockTrackerService _unlockTrackerService;
        private readonly IEnumerable<BaseTooltip> tooltips;
        private readonly ITooltipService _tooltipService;
        private readonly FilterConfiguration.Factory _filterConfigFactory;
        private readonly IAcquisitionMonitorService _acquisitionMonitorService;
        private readonly CraftTrackerTrackCraftsFilter _trackCraftsFilter;
        private readonly CraftTrackerTrackGatheringFilter _trackGatheringFilter;
        private readonly CraftTrackerTrackShoppingFilter _trackShoppingFilter;
        private readonly CraftTrackerTrackCombatDropFilter _trackCombatDropFilter;
        private readonly CraftTrackerTrackOtherFilter _trackOtherFilter;
        private readonly CraftTrackerTrackMarketBoardFilter _trackMarketBoardFilter;
        private readonly UseOldCraftTrackerSetting _useOldCraftTrackerSetting;
        private readonly IMarketCache _marketCache;
        private readonly IEnumerable<ISampleFilter> _sampleFilters;
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

        public PluginLogic(ConfigurationManagerService configurationManagerService, IChatUtilities chatUtilities,
            IListService listService, ILogger<PluginLogic> logger, IFramework framework,
            MediatorService mediatorService, HostedInventoryHistory hostedInventoryHistory,
            IInventoryMonitor inventoryMonitor, IInventoryScanner inventoryScanner, ICharacterMonitor characterMonitor,
            InventoryToolsConfiguration configuration, IMobTracker mobTracker,
            ICraftMonitor craftMonitor, IUnlockTrackerService unlockTrackerService, IEnumerable<BaseTooltip> tooltips,
            IMarketCache marketCache, IEnumerable<ISampleFilter> sampleFilters,
            ITooltipService tooltipService, FilterConfiguration.Factory filterConfigFactory,
            IAcquisitionMonitorService acquisitionMonitorService,
            CraftTrackerTrackCraftsFilter trackCraftsFilter, CraftTrackerTrackGatheringFilter trackGatheringFilter,
            CraftTrackerTrackShoppingFilter trackShoppingFilter, CraftTrackerTrackCombatDropFilter trackCombatDropFilter,
            CraftTrackerTrackOtherFilter trackOtherFilter, UseOldCraftTrackerSetting useOldCraftTrackerSetting,
            CraftTrackerTrackMarketBoardFilter trackMarketBoardFilter) : base(logger, mediatorService)
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
            _craftMonitor = craftMonitor;
            _unlockTrackerService = unlockTrackerService;
            this.tooltips = tooltips;
            _tooltipService = tooltipService;
            _filterConfigFactory = filterConfigFactory;
            _acquisitionMonitorService = acquisitionMonitorService;
            _trackCraftsFilter = trackCraftsFilter;
            _trackGatheringFilter = trackGatheringFilter;
            _trackShoppingFilter = trackShoppingFilter;
            _trackCombatDropFilter = trackCombatDropFilter;
            _trackOtherFilter = trackOtherFilter;
            _useOldCraftTrackerSetting = useOldCraftTrackerSetting;
            _trackMarketBoardFilter = trackMarketBoardFilter;
            _marketCache = marketCache;
            _sampleFilters = sampleFilters;
            MediatorService.Subscribe<PluginLoadedMessage>(this, PluginLoaded);
        }

        private void PluginLoaded(PluginLoadedMessage obj)
        {
            _inventoryMonitor.Start();
            _inventoryScanner.Enable();
        }

        private void CraftMonitorOnCraftCompleted(uint itemid, FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags flags, uint quantity)
        {
            if (!_useOldCraftTrackerSetting.CurrentValue(_configuration))
            {
                _logger.LogTrace("Craft monitor event ignored as the acquisition tracker currently has precedence.");
                return;
            }
            _logger.LogTrace("Craft completed for {Quantity} qty of item {ItemId}", quantity, itemid);

            var activeCraftList = _listService.GetActiveCraftList();
            if (activeCraftList != null && activeCraftList.FilterType == FilterType.CraftFilter && activeCraftList.CraftList.CraftListMode == CraftListMode.Normal)
            {
                _logger.LogTrace("Marking {Quantity} qty for item {ItemId} ({HqFlag}) as crafted.", quantity, itemid, flags == InventoryItem.ItemFlags.None ? "NQ" : "HQ");
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
            else
            {
                _logger.LogTrace("Active craft list is either inactive or in stock mode.");
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
            AddCraftFilter();
            foreach (var sampleFilter in _sampleFilters.OrderBy(c => c.Name))
            {
                if (sampleFilter.SampleFilterType == SampleFilterType.Default)
                {
                    sampleFilter.AddFilter();
                }
            }
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
            _listService.AddList(filterConfiguration);
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

            _acquisitionMonitorService.ItemAcquired += AcquisitionMonitorServiceOnItemAcquired;
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

        private void AcquisitionMonitorServiceOnItemAcquired(uint itemId, InventoryItem.ItemFlags itemFlags, int qtyIncrease, AcquisitionReason reason)
        {
            if (_useOldCraftTrackerSetting.CurrentValue(_configuration))
            {
                _logger.LogTrace("Acquisition tracker event ignored as the craft monitor currently has precedence.");
                return;
            }
            _logger.LogTrace("Item acquired through {Reason}, qty of {QtyIncrease}, item ID: {ItemId}", reason, qtyIncrease, itemId);

            var activeCraftList = _listService.GetActiveCraftList();
            if (activeCraftList != null && activeCraftList.FilterType == FilterType.CraftFilter && activeCraftList.CraftList.CraftListMode == CraftListMode.Normal)
            {
                if ((reason == AcquisitionReason.Crafting && _trackCraftsFilter.CurrentValue(activeCraftList) == false) ||
                    (reason == AcquisitionReason.Gathering && _trackGatheringFilter.CurrentValue(activeCraftList) == false) ||
                    (reason == AcquisitionReason.Shopping && _trackShoppingFilter.CurrentValue(activeCraftList) == false) ||
                    (reason == AcquisitionReason.CombatDrop && _trackCombatDropFilter.CurrentValue(activeCraftList) == false) ||
                    (reason == AcquisitionReason.Other && _trackOtherFilter.CurrentValue(activeCraftList) == false) ||
                    (reason == AcquisitionReason.Marketboard && _trackMarketBoardFilter.CurrentValue(activeCraftList) == false)
                    )
                {
                    _logger.LogTrace("Craft list configured to not track {Reason}, not altering required item counts.", reason);
                    return;
                }

                _logger.LogTrace("Marking {Quantity} qty for item {ItemId} ({HqFlag}) as crafted.", qtyIncrease, itemId, itemFlags.ToString());
                activeCraftList.CraftList.MarkCrafted(itemId, itemFlags, (uint)qtyIncrease);
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
            else
            {
                _logger.LogTrace("Active craft list is either inactive or in stock mode.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogTrace("Stopping service {Type} ({This})", GetType().Name, this);
            _unlockTrackerService.ItemUnlockStatusChanged -= UnlockTrackerServiceOnAcquiredItemsUpdated;
            _configuration.SavedCharacters = _characterMonitor.Characters;
            _framework.Update -= FrameworkOnUpdate;
            _inventoryMonitor.OnInventoryChanged -= InventoryMonitorOnOnInventoryChanged;
            _acquisitionMonitorService.ItemAcquired -= AcquisitionMonitorServiceOnItemAcquired;
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
            Logger.LogTrace("Stopped service {Type} ({This})", GetType().Name, this);
            return Task.CompletedTask;
        }
    }
}

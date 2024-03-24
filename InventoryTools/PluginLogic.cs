using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Services.Ui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Internal;
using Dalamud.Plugin.Services;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Hotkeys;
using InventoryTools.Images;
using InventoryTools.Logic;
using InventoryTools.Logic.Columns;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.Settings;
using InventoryTools.Logic.Settings.Abstract;
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
        private readonly ConfigurationManager _configurationManager;
        private readonly IChatUtilities _chatUtilities;
        private readonly IListService _listService;
        private readonly ILogger<PluginLogic> _logger;
        private readonly IFramework _framework;
        private readonly InventoryHistory _history;
        private readonly IInventoryMonitor _inventoryMonitor;
        private readonly ICharacterMonitor _characterMonitor;
        private readonly InventoryToolsConfiguration _configuration;
        private readonly IMobTracker _mobTracker;
        private readonly IHotkeyService _hotkeyService;
        private readonly ICraftMonitor _craftMonitor;
        private readonly IGameInterface _gameInterface;
        private readonly ITooltipService _tooltipService;
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
        
        public PluginLogic(ConfigurationManager configurationManager, IChatUtilities chatUtilities, IListService listService, ILogger<PluginLogic> logger, IFramework framework, MediatorService mediatorService, InventoryHistory history, IInventoryMonitor inventoryMonitor, ICharacterMonitor characterMonitor, InventoryToolsConfiguration configuration, IMobTracker mobTracker, IHotkeyService hotkeyService, ICraftMonitor craftMonitor, IGameInterface gameInterface, ITooltipService tooltipService, IEnumerable<BaseTooltip> tooltips, IEnumerable<IHotkey> hotkeys, Func<Type,IFilter> filterFactory, IMarketCache marketCache) : base(logger, mediatorService)
        {
            _configurationManager = configurationManager;
            _chatUtilities = chatUtilities;
            _listService = listService;
            _logger = logger;
            _framework = framework;
            _history = history;
            _inventoryMonitor = inventoryMonitor;
            _characterMonitor = characterMonitor;
            _configuration = configuration;
            _mobTracker = mobTracker;
            _hotkeyService = hotkeyService;
            _craftMonitor = craftMonitor;
            _gameInterface = gameInterface;
            _tooltipService = tooltipService;
            _filterFactory = filterFactory;
            _marketCache = marketCache;

            //Events we need to track, inventory updates, active retainer changes, player changes, 
            _inventoryMonitor.OnInventoryChanged += InventoryMonitorOnOnInventoryChanged;
            _characterMonitor.OnCharacterUpdated += CharacterMonitorOnOnCharacterUpdated;
            _framework.Update += FrameworkOnUpdate;


            _craftMonitor.CraftStarted += CraftMonitorOnCraftStarted;
            _craftMonitor.CraftFailed += CraftMonitorOnCraftFailed ;
            _craftMonitor.CraftCompleted += CraftMonitorOnCraftCompleted ;
            _gameInterface.AcquiredItemsUpdated += GameInterfaceOnAcquiredItemsUpdated;
            
            foreach (var hotkey in hotkeys)
            {
                _hotkeyService.AddHotkey(hotkey);
            }
            
            foreach (var tooltip in tooltips)
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
        }

        private void PluginServiceOnOnPluginLoaded()
        {
            _inventoryMonitor.Start();
        }

        private void CraftMonitorOnCraftCompleted(uint itemid, FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags flags, uint quantity)
        {
            var activeCraftList = _listService.GetActiveCraftList();
            if (activeCraftList != null && activeCraftList.FilterType == FilterType.CraftFilter)
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
        }

        private void CraftMonitorOnCraftStarted(uint itemid)
        {
        }


        private void GameInterfaceOnAcquiredItemsUpdated()
        {
            var activeCharacter = _characterMonitor.ActiveCharacterId;
            if (activeCharacter != 0)
            {
                _configuration.AcquiredItems[activeCharacter] = _gameInterface.AcquiredItems;
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

            if (_history.Enabled != _configuration.HistoryEnabled)
            {
                if (_configuration.HistoryEnabled)
                {
                    _history.Enable();
                }
                else
                {
                    _history.Disable();
                }
            }

            if (_configuration.HistoryTrackReasons != null)
            {
                if (_history.ReasonsToLog.ToList() !=
                    _configuration.HistoryTrackReasons)
                {
                    _history.SetChangeReasonsToLog(
                        _configuration.HistoryTrackReasons.Distinct().ToHashSet());
                }
            }
        }

        private void CharacterMonitorOnOnCharacterUpdated(Character? character)
        {
            if (character != null)
            {
                _configuration.IsDirty = true;
                if (_configuration.AcquiredItems.ContainsKey(character.CharacterId))
                {
                    _gameInterface.AcquiredItems = _configuration.AcquiredItems[character.CharacterId];
                }
            }
            else
            {
                _gameInterface.AcquiredItems = new HashSet<uint>();
            }
        }

        public void LoadDefaultData()
        {
            _listService.GetDefaultCraftList();
            
            AddAllFilter();

            AddRetainerFilter();

            AddPlayerFilter();
            
            AddFreeCompanyFilter();

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
            _listService.AddList(allItemsFilter);
        }

        public void AddRetainerFilter(string newName = "Retainers")
        {
            var retainerItemsFilter = new FilterConfiguration(newName, FilterType.SearchFilter);
            retainerItemsFilter.DisplayInTabs = true;
            retainerItemsFilter.SourceAllRetainers = true;
            _listService.AddList(retainerItemsFilter);
        }

        public void AddPlayerFilter(string newName = "Player")
        {
            var playerItemsFilter = new FilterConfiguration(newName,  FilterType.SearchFilter);
            playerItemsFilter.DisplayInTabs = true;
            playerItemsFilter.SourceAllCharacters = true;
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
            _listService.AddList(historyFilter);
        }

        public void AddFreeCompanyFilter(string newName = "Free Company")
        {
            var newFilter = new FilterConfiguration(newName,  FilterType.SearchFilter);
            newFilter.DisplayInTabs = true;
            newFilter.SourceAllFreeCompanies = true;
            _listService.AddList(newFilter);
        }

        public void AddAllGameItemsFilter(string newName = "All Game Items")
        {
            var allGameItemsFilter = new FilterConfiguration(newName, FilterType.GameItemFilter);
            allGameItemsFilter.DisplayInTabs = true;            
            _listService.AddList(allGameItemsFilter);
        }

        public void AddFavouritesFilter(string newName = "Favourites")
        {
            var newFilter = new FilterConfiguration(newName, FilterType.GameItemFilter);
            var favouritesFilter = (FavouritesFilter)_filterFactory.Invoke(typeof(FavouritesFilter));
            favouritesFilter.UpdateFilterConfiguration(newFilter, true);
            newFilter.DisplayInTabs = true;   
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
            sampleFilter.CanBeBought = true;
            sampleFilter.ShopBuyingPrice = "<=100";
            _listService.AddList(sampleFilter);
        }

        public void AddSampleFilterMaterials(string newName = "Put away materials")
        {
            var sampleFilter = new FilterConfiguration(newName, FilterType.SortingFilter);
            sampleFilter.DisplayInTabs = true;
            sampleFilter.SourceCategories = new HashSet<InventoryCategory>() {InventoryCategory.CharacterBags};
            sampleFilter.DestinationCategories =  new HashSet<InventoryCategory>() {InventoryCategory.RetainerBags};
            sampleFilter.FilterItemsInRetainersEnum = FilterItemsRetainerEnum.Yes;
            sampleFilter.HighlightWhen = "Always";
            var gatherFilter = (CanBeGatheredFilter)_filterFactory.Invoke(typeof(CanBeGatheredFilter));
            gatherFilter.UpdateFilterConfiguration(sampleFilter, true);
            _listService.AddList(sampleFilter);
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
            _configurationManager.SaveInventories(allItems);
            if (_configuration.AutomaticallyDownloadMarketPrices)
            {
                var activeCharacter = _characterMonitor.ActiveCharacter;
                if (activeCharacter != null)
                {
                    foreach (var inventory in allItems)
                    {
                        _marketCache.RequestCheck(inventory.ItemId, activeCharacter.WorldId);
                    }
                }
            }
        }

        private bool _disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if(!_disposed && disposing)
            {
                _gameInterface.AcquiredItemsUpdated -= GameInterfaceOnAcquiredItemsUpdated;
                _configuration.SavedCharacters = _characterMonitor.Characters;
                _framework.Update -= FrameworkOnUpdate;
                _inventoryMonitor.OnInventoryChanged -= InventoryMonitorOnOnInventoryChanged;
                _characterMonitor.OnCharacterUpdated -= CharacterMonitorOnOnCharacterUpdated;
                _craftMonitor.CraftStarted -= CraftMonitorOnCraftStarted;
                _craftMonitor.CraftFailed -= CraftMonitorOnCraftFailed ;
                _craftMonitor.CraftCompleted -= CraftMonitorOnCraftCompleted ;
                _configurationManager.ConfigurationChanged -= ConfigOnConfigurationChanged;
                _configurationManager.Save();
                _configurationManager.SaveInventories(_inventoryMonitor.AllItems.ToList());
                _configurationManager.SaveHistory(_history.GetHistory());
                if (_configuration.TrackMobSpawns)
                {
                    _mobTracker.SaveCsv(_configurationManager.MobSpawnFile,
                        _mobTracker.GetEntries());
                }
            }
            _disposed = true;         
        }
        
            
        ~PluginLogic()
        {
#if DEBUG
            // In debug-builds, make sure that a warning is displayed when the Disposable object hasn't been
            // disposed by the programmer.

            if( _disposed == false )
            {
                _logger.LogError("There is a disposable object which hasn't been disposed before the finalizer call: " + (this.GetType ().Name));
            }
#endif
            Dispose (true);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogTrace("Starting service {type} ({this})", GetType().Name, this);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

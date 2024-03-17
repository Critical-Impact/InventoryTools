using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using CriticalCommonLib;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
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
using InventoryTools.Tooltips;
using InventoryTools.Ui;

namespace InventoryTools
{
    public partial class PluginLogic : IDisposable
    {
        private Dictionary<string, RenderTableBase> _filterTables = new();
        private List<IFilter>? _availableFilters = null;
        private List<ISetting>? _availableSettings = null;
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

        private RightClickColumn _rightClickColumn = new();
        
        public readonly ConcurrentDictionary<ushort, IDalamudTextureWrap> TextureDictionary = new ConcurrentDictionary<ushort, IDalamudTextureWrap>();
        public readonly ConcurrentDictionary<ushort, IDalamudTextureWrap> HQTextureDictionary = new ConcurrentDictionary<ushort, IDalamudTextureWrap>();
        public readonly ConcurrentDictionary<string, IDalamudTextureWrap> UldTextureDictionary = new ConcurrentDictionary<string, IDalamudTextureWrap>();
        
        public PluginLogic()
        {
            //Events we need to track, inventory updates, active retainer changes, player changes, 
            PluginService.InventoryMonitor.OnInventoryChanged += InventoryMonitorOnOnInventoryChanged;
            PluginService.CharacterMonitor.OnCharacterUpdated += CharacterMonitorOnOnCharacterUpdated;
            ConfigurationManager.Config.ConfigurationChanged += ConfigOnConfigurationChanged;
            Service.Framework.Update += FrameworkOnUpdate;

            PluginService.CharacterMonitor.LoadExistingRetainers(ConfigurationManager.Config.GetSavedRetainers());
            PluginService.InventoryMonitor.LoadExistingData(ConfigurationManager.LoadInventory());
            PluginService.InventoryHistory.LoadExistingHistory(ConfigurationManager.LoadHistoryFromCsv(out _));
            var entries = PluginService.MobTracker.LoadCsv(ConfigurationManager.MobSpawnFile, out var success);
            if(success)
            {
                PluginService.MobTracker.SetEntries(entries);
            }

            PluginService.HotkeyService.AddHotkey(new AirshipsWindowHotkey());
            PluginService.HotkeyService.AddHotkey(new ConfigurationWindowHotkey());
            PluginService.HotkeyService.AddHotkey(new CraftWindowHotkey());
            PluginService.HotkeyService.AddHotkey(new DutiesWindowHotkey());
            PluginService.HotkeyService.AddHotkey(new MobWindowHotkey());
            PluginService.HotkeyService.AddHotkey(new MoreInfoWindowHotkey());
            PluginService.HotkeyService.AddHotkey(new SubmarinesWindowHotkey());
            PluginService.CraftMonitor.CraftStarted += CraftMonitorOnCraftStarted;
            PluginService.CraftMonitor.CraftFailed += CraftMonitorOnCraftFailed ;
            PluginService.CraftMonitor.CraftCompleted += CraftMonitorOnCraftCompleted ;
            PluginService.OnPluginLoaded += PluginServiceOnOnPluginLoaded;
            PluginService.GameInterface.AcquiredItemsUpdated += GameInterfaceOnAcquiredItemsUpdated;
            PluginService.TooltipService.AddTooltipTweak(new HeaderTextTooltip());
            PluginService.TooltipService.AddTooltipTweak(new LocationDisplayTooltip());
            PluginService.TooltipService.AddTooltipTweak(new AmountOwnedTooltip());
            PluginService.TooltipService.AddTooltipTweak(new DisplayMarketPriceTooltip());
            PluginService.TooltipService.AddTooltipTweak(new FooterTextTooltip());
            RunMigrations();
            
            if (ConfigurationManager.Config.FirstRun)
            {
                LoadDefaultData();
                ConfigurationManager.Config.FirstRun = false;
            }
            SyncConfigurationChanges(false);
            ClearOrphans();
        }

#pragma warning disable CS8618
        public PluginLogic(bool noExternals = false)
#pragma warning restore CS8618
        {

        }

        private void PluginServiceOnOnPluginLoaded()
        {
            if (!ConfigurationManager.Config.IntroShown)
            {
                PluginService.WindowService.OpenWindow<IntroWindow>(IntroWindow.AsKey);
                ConfigurationManager.Config.IntroShown = true;
            }

            PluginService.InventoryMonitor.Start();
        }

        private void CraftMonitorOnCraftCompleted(uint itemid, FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags flags, uint quantity)
        {
            var activeCraftList = PluginService.FilterService.GetActiveCraftList();
            if (activeCraftList != null && activeCraftList.FilterType == FilterType.CraftFilter)
            {
                activeCraftList.CraftList.MarkCrafted(itemid, flags, quantity);
                if (activeCraftList is { IsEphemeralCraftList: true, CraftList.IsCompleted: true })
                {
                    PluginService.ChatService.Print("Ephemeral craft list '" + activeCraftList.Name + "' completed. List has been removed.");
                    PluginService.FilterService.RemoveFilter(activeCraftList);
                }
                else
                {
                    activeCraftList.StartRefresh();
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
            var activeCharacter = PluginService.CharacterMonitor.ActiveCharacterId;
            if (activeCharacter != 0)
            {
                ConfigurationManager.Config.AcquiredItems[activeCharacter] = PluginService.GameInterface.AcquiredItems;
            }
        }

        public void ClearOrphans()
        {
            var keys = PluginService.InventoryMonitor.Inventories.Keys;
            foreach (var key in keys)
            {
                var character = PluginService.CharacterMonitor.GetCharacterById(key);
                if (character == null)
                {
                    Service.Log.Info("Removing inventories for " + key + " from inventory cache as there is no character associated with this inventory.");
                    PluginService.InventoryMonitor.ClearCharacterInventories(key);
                }
            }
        }

        public void RunMigrations()
        {
            
            if (ConfigurationManager.Config.InternalVersion == 0)
            {
                Service.Log.Info("Migrating to version 1");
                var highlight = ConfigurationManager.Config.HighlightColor;
                if (highlight.W == 0.0f)
                {
                    highlight.W = 1;
                    ConfigurationManager.Config.HighlightColor = highlight;
                }
                ConfigurationManager.Config.TabHighlightColor = new(0.007f, 0.008f, 0.007f, 1.0f);

                foreach (var filterConfig in PluginService.FilterService.FiltersList)
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

                ConfigurationManager.Config.InternalVersion++;
            }
            if (ConfigurationManager.Config.InternalVersion == 1)
            {
                Service.Log.Info("Migrating to version 2");
                ConfigurationManager.Config.InvertTabHighlighting = ConfigurationManager.Config.InvertHighlighting;

                foreach (var filterConfig in PluginService.FilterService.FiltersList)
                {
                    if (filterConfig.InvertHighlighting != null)
                    {
                        filterConfig.InvertTabHighlighting = filterConfig.InvertHighlighting;
                    }
                }

                ConfigurationManager.Config.InternalVersion++;
            }
            if (ConfigurationManager.Config.InternalVersion == 2)
            {
                Service.Log.Info("Migrating to version 3");
                foreach (var filterConfig in PluginService.FilterService.FiltersList)
                {
                    filterConfig.GenerateNewTableId();
                    filterConfig.Columns = new List<string>();
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
                PluginService.MarketCache.ClearCache();
                ConfigurationManager.Config.InternalVersion++;
            }
            if (ConfigurationManager.Config.InternalVersion == 3)
            {
                Service.Log.Info("Migrating to version 4");
                
                foreach (var filterConfig in PluginService.FilterService.FiltersList)
                {
                    new IsHqFilter().UpdateFilterConfiguration(filterConfig, filterConfig.IsHq);
                    new IsCollectibleFilter().UpdateFilterConfiguration(filterConfig, filterConfig.IsCollectible);
                    new NameFilter().UpdateFilterConfiguration(filterConfig, filterConfig.NameFilter);
                    new QuantityFilter().UpdateFilterConfiguration(filterConfig, filterConfig.Quantity);
                    new ItemLevelFilter().UpdateFilterConfiguration(filterConfig, filterConfig.ILevel);
                    new SpiritBondFilter().UpdateFilterConfiguration(filterConfig, filterConfig.Spiritbond);
                    new SellToVendorPriceFilter().UpdateFilterConfiguration(filterConfig, filterConfig.ShopSellingPrice);
                    new BuyFromVendorPriceFilter().UpdateFilterConfiguration(filterConfig, filterConfig.ShopBuyingPrice);
                    new CanBePurchasedFilter().UpdateFilterConfiguration(filterConfig, filterConfig.CanBeBought);
                    new MarketBoardPriceFilter().UpdateFilterConfiguration(filterConfig, filterConfig.MarketAveragePrice);
                    new MarketBoardTotalPriceFilter().UpdateFilterConfiguration(filterConfig, filterConfig.MarketTotalAveragePrice);
                    new IsTimedNodeFilter().UpdateFilterConfiguration(filterConfig, filterConfig.IsAvailableAtTimedNode);
                    new ItemUiCategoryFilter().UpdateFilterConfiguration(filterConfig, filterConfig.ItemUiCategoryId);
                    new SearchCategoryFilter().UpdateFilterConfiguration(filterConfig, filterConfig.ItemSearchCategoryId);
                    filterConfig.FilterType++;
                }
                ConfigurationManager.Config.InternalVersion++;
            }

            if (ConfigurationManager.Config.InternalVersion == 4)
            {
                Service.Log.Info("Migrating to version 5");
                ConfigurationManager.Config.RetainerListColor = ImGuiColors.HealerGreen;
                ConfigurationManager.Config.InternalVersion++;
            }

            if (ConfigurationManager.Config.InternalVersion == 5)
            {
                Service.Log.Info("Migrating to version 6");
                ConfigurationManager.Config.TooltipDisplayAmountOwned = true;
                ConfigurationManager.Config.TooltipDisplayMarketAveragePrice = true;
                ConfigurationManager.Config.InternalVersion++;
            }

            if (ConfigurationManager.Config.InternalVersion == 6)
            {
                Service.Log.Info("Migrating to version 7");
                ConfigurationManager.Config.HighlightDestination = true;
                ConfigurationManager.Config.DestinationHighlightColor = new Vector4(0.321f, 0.239f, 0.03f, 1f);
                ConfigurationManager.Config.InternalVersion++;
            }

            if (ConfigurationManager.Config.InternalVersion == 7)
            {
                ConfigurationManager.Config.InternalVersion++;
            }

            if (ConfigurationManager.Config.InternalVersion == 8)
            {
                Service.Log.Info("Migrating to version 9");
                var order = 0u;
                foreach (var configuration in PluginService.FilterService.Filters)
                {
                    if (configuration.Value.FilterType != FilterType.CraftFilter)
                    {
                        configuration.Value.Order = order;
                        order++;
                    }
                }
                order = 0u;
                foreach (var configuration in PluginService.FilterService.Filters)
                {
                    if (configuration.Value.FilterType == FilterType.CraftFilter)
                    {
                        configuration.Value.Order = order;
                        order++;
                    }
                }
                ConfigurationManager.Config.InternalVersion++;
            }

            if (ConfigurationManager.Config.InternalVersion == 9)
            {
                Service.Log.Info("Migrating to version 10");
                foreach (var configuration in PluginService.FilterService.Filters)
                {
#pragma warning disable CS0612
                    if (configuration.Value.FilterItemsInRetainers.HasValue && configuration.Value.FilterItemsInRetainers == true)
#pragma warning restore CS0612
                    {
                        configuration.Value.FilterItemsInRetainersEnum = FilterItemsRetainerEnum.Yes;
                    }
                    else
                    {
                        configuration.Value.FilterItemsInRetainersEnum = FilterItemsRetainerEnum.No;
                    }
                }
                ConfigurationManager.Config.InternalVersion++;
            }

            if (ConfigurationManager.Config.InternalVersion == 10)
            {
                Service.Log.Info("Migrating to version 11");
                foreach (var configuration in PluginService.FilterService.Filters)
                {
                    foreach (var filterConfig in PluginService.FilterService.FiltersList)
                    {
                        filterConfig.TableHeight = 32;
                        filterConfig.CraftTableHeight = 32;
                        if (filterConfig.FilterType == FilterType.CraftFilter)
                        {
                            filterConfig.FreezeCraftColumns = 2;
                            filterConfig.GenerateNewCraftTableId();
                            filterConfig.CraftColumns = new List<string>();
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
                ConfigurationManager.Config.InternalVersion++;
            }

            if (ConfigurationManager.Config.InternalVersion == 11)
            {
                Service.Log.Info("Migrating to version 12");
                ConfigurationManager.Config.TooltipLocationLimit = 10;                
                ConfigurationManager.Config.TooltipLocationDisplayMode =
                    TooltipLocationDisplayMode.CharacterCategoryQuantityQuality;
                ConfigurationManager.Config.InternalVersion++;
            }

            if (ConfigurationManager.Config.InternalVersion == 12)
            {
                Service.Log.Info("Migrating to version 13");
                ConfigurationManager.Config.FiltersLayout = WindowLayout.Tabs;
                ConfigurationManager.Config.CraftWindowLayout = WindowLayout.Tabs;
                ConfigurationManager.Config.InternalVersion++;
            }

            if (ConfigurationManager.Config.InternalVersion == 13)
            {
                Service.Log.Info("Migrating to version 14");
                var toReset = AvailableFilters.Where(c =>
                    c is CraftCrystalGroupFilter or CraftCurrencyGroupFilter or CraftPrecraftGroupFilter
                        or CraftRetrieveGroupFilter or CraftEverythingElseGroupFilter or CraftIngredientPreferenceFilter
                        or CraftDefaultHQRequiredFilter or CraftDefaultRetrieveFromRetainerFilter
                        or CraftDefaultRetrieveFromRetainerOutputFilter).ToList();
                PluginService.FilterService.GetDefaultCraftList();
                foreach (var filterConfig in PluginService.FilterService.FiltersList)
                {
                    if (filterConfig.FilterType == FilterType.CraftFilter)
                    {
                        if (filterConfig.CraftListDefault)
                        {
                            filterConfig.AddDefaultColumns();
                        }

                        foreach (var filter in toReset)
                        {
                            filter.ResetFilter(filterConfig);
                        }
                        
                        filterConfig.AddCraftColumn("CraftSettingsColumn",2);
                        filterConfig.AddCraftColumn("CraftSimpleColumn",3);
                    }
                }
                ConfigurationManager.Config.InternalVersion++;
            }

            if (ConfigurationManager.Config.InternalVersion == 14)
            {
                Service.Log.Info("Migrating to version 15");
                ConfigurationManager.Config.HistoryTrackReasons = new()
                {
                    InventoryChangeReason.Added,
                    InventoryChangeReason.Moved,
                    InventoryChangeReason.Removed,
                    InventoryChangeReason.Transferred,
                    InventoryChangeReason.MarketPriceChanged,
                    InventoryChangeReason.QuantityChanged
                };
                ConfigurationManager.Config.InternalVersion++;
            }

            if (ConfigurationManager.Config.InternalVersion == 15)
            {
                Service.Log.Info("Migrating to version 16");
                var historyFilter = PluginService.FilterService.GetFilter("History");
                if (historyFilter == null && !ConfigurationManager.Config.FirstRun)
                {
                    AddHistoryFilter();
                }
                ConfigurationManager.Config.InternalVersion++;
            }
        }

        private void FrameworkOnUpdate(IFramework framework)
        {
            if (ConfigurationManager.Config.AutoSave)
            {
                if (NextSaveTime == null && ConfigurationManager.Config.AutoSaveMinutes != 0)
                {
                    _nextSaveTime = DateTime.Now.AddMinutes(ConfigurationManager.Config.AutoSaveMinutes);
                }
                else
                {
                    if (DateTime.Now >= NextSaveTime)
                    {
                        _nextSaveTime = null;
                        ConfigurationManager.SaveAsync();
                    }
                }
            }
        }

        private void ConfigOnConfigurationChanged()
        {
            SyncConfigurationChanges();
        }

        private static void SyncConfigurationChanges(bool save = true)
        {
            if (save)
            {
                ConfigurationManager.SaveAsync();
            }

            if (PluginService.MobTracker.Enabled != ConfigurationManager.Config.TrackMobSpawns)
            {
                if (ConfigurationManager.Config.TrackMobSpawns)
                {
                    PluginService.MobTracker.Enable();
                }
                else
                {
                    PluginService.MobTracker.Disable();
                }
            }

            if (PluginService.InventoryHistory.Enabled != ConfigurationManager.Config.HistoryEnabled)
            {
                if (ConfigurationManager.Config.HistoryEnabled)
                {
                    PluginService.InventoryHistory.Enable();
                }
                else
                {
                    PluginService.InventoryHistory.Disable();
                }
            }

            if (ConfigurationManager.Config.HistoryTrackReasons != null)
            {
                if (PluginService.InventoryHistory.ReasonsToLog.ToList() !=
                    ConfigurationManager.Config.HistoryTrackReasons)
                {
                    PluginService.InventoryHistory.SetChangeReasonsToLog(
                        ConfigurationManager.Config.HistoryTrackReasons.Distinct().ToHashSet());
                }
            }
        }

        private void CharacterMonitorOnOnCharacterUpdated(Character? character)
        {
            if (character != null)
            {
                ConfigurationManager.SaveAsync();
                if (ConfigurationManager.Config.AcquiredItems.ContainsKey(character.CharacterId))
                {
                    PluginService.GameInterface.AcquiredItems = ConfigurationManager.Config.AcquiredItems[character.CharacterId];
                }
            }
            else
            {
                PluginService.GameInterface.AcquiredItems = new HashSet<uint>();
            }
        }

        public void LoadDefaultData()
        {
            AddAllFilter();

            AddRetainerFilter();

            AddPlayerFilter();
            
            AddFreeCompanyFilter();

            AddAllGameItemsFilter();
            
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
            PluginService.FilterService.AddFilter(allItemsFilter);
        }

        public void AddRetainerFilter(string newName = "Retainers")
        {
            var retainerItemsFilter = new FilterConfiguration(newName, FilterType.SearchFilter);
            retainerItemsFilter.DisplayInTabs = true;
            retainerItemsFilter.SourceAllRetainers = true;
            PluginService.FilterService.AddFilter(retainerItemsFilter);
        }

        public void AddPlayerFilter(string newName = "Player")
        {
            var playerItemsFilter = new FilterConfiguration(newName,  FilterType.SearchFilter);
            playerItemsFilter.DisplayInTabs = true;
            playerItemsFilter.SourceAllCharacters = true;
            PluginService.FilterService.AddFilter(playerItemsFilter);
        }

        public void AddHistoryFilter(string newName = "History")
        {
            var historyFilter = new FilterConfiguration(newName,  FilterType.HistoryFilter);
            historyFilter.DisplayInTabs = true;
            historyFilter.SourceAllCharacters = true;
            historyFilter.SourceAllRetainers = true;
            historyFilter.SourceAllFreeCompanies = true;
            historyFilter.SourceAllHouses = true;
            PluginService.FilterService.AddFilter(historyFilter);
        }

        public void AddFreeCompanyFilter(string newName = "Free Company")
        {
            var newFilter = new FilterConfiguration(newName,  FilterType.SearchFilter);
            newFilter.DisplayInTabs = true;
            newFilter.SourceAllFreeCompanies = true;
            PluginService.FilterService.AddFilter(newFilter);
        }

        public void AddAllGameItemsFilter(string newName = "All Game Items")
        {
            var allGameItemsFilter = new FilterConfiguration(newName, FilterType.GameItemFilter);
            allGameItemsFilter.DisplayInTabs = true;            
            PluginService.FilterService.AddFilter(allGameItemsFilter);
        }

        public void AddCraftFilter(string newName = "Craft List")
        {
            var newFilter = PluginService.FilterService.AddNewCraftFilter(newName);
            newFilter.DisplayInTabs = true;            
        }

        public void AddNewCraftFilter()
        {
            var filterConfiguration = PluginService.FilterService.AddNewCraftFilter();
            var craftsWindow = PluginService.WindowService.GetCraftsWindow();
            craftsWindow.FocusFilter(filterConfiguration, true);
        }

        public void AddFilter(FilterConfiguration filterConfiguration)
        {
            filterConfiguration.DestinationInventories.Clear();
            filterConfiguration.SourceInventories.Clear();
            PluginService.FilterService.AddFilter(filterConfiguration);
        }

        public void AddSampleFilter100Gil(string newName = "100 gill or less")
        {
            var sampleFilter = new FilterConfiguration(newName, FilterType.SearchFilter);
            sampleFilter.DisplayInTabs = true;
            sampleFilter.SourceAllCharacters = true;
            sampleFilter.SourceAllRetainers = true;
            sampleFilter.SourceAllFreeCompanies = true;
            sampleFilter.CanBeBought = true;
            sampleFilter.ShopBuyingPrice = "<=100";
            PluginService.FilterService.AddFilter(sampleFilter);
        }

        public void AddSampleFilterMaterials(string newName = "Put away materials")
        {
            var sampleFilter = new FilterConfiguration(newName, FilterType.SortingFilter);
            sampleFilter.DisplayInTabs = true;
            sampleFilter.SourceCategories = new HashSet<InventoryCategory>() {InventoryCategory.CharacterBags};
            sampleFilter.DestinationCategories =  new HashSet<InventoryCategory>() {InventoryCategory.RetainerBags};
            sampleFilter.FilterItemsInRetainersEnum = FilterItemsRetainerEnum.Yes;
            sampleFilter.HighlightWhen = "Always";
            var gatherFilter = new CanBeGatheredFilter();
            gatherFilter.UpdateFilterConfiguration(sampleFilter, true);
            PluginService.FilterService.AddFilter(sampleFilter);
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
            PluginService.FilterService.AddFilter(sampleFilter);
        }


        public DateTime? NextSaveTime => _nextSaveTime;

        public void ClearAutoSave()
        {
            _nextSaveTime = null;
        }

        private void InventoryMonitorOnOnInventoryChanged(List<InventoryChange> inventoryChanges, InventoryMonitor.ItemChanges? itemChanges)
        {
            Service.Log.Verbose("PluginLogic: Inventory changed, saving to config.");
            var allItems = PluginService.InventoryMonitor.AllItems.ToList();
            ConfigurationManager.SaveInventories(allItems);
            if (ConfigurationManager.Config.AutomaticallyDownloadMarketPrices)
            {
                foreach (var inventory in allItems)
                {
                    PluginService.MarketCache.RequestCheck(inventory.ItemId);
                }
            }
        }
        
        private Dictionary<string,IColumn>? _gridColumns;
        public Dictionary<string,IColumn> GridColumns
        {
            get
            {
                if (_gridColumns == null)
                {
                    _gridColumns = new Dictionary<string, IColumn>();
                    var columnType = typeof(IColumn);
                    var types = Assembly.GetExecutingAssembly().GetLoadableTypes().Where(columnType.IsAssignableFrom).ToList();
                    foreach (var type in types)
                    {
                        if (type.IsClass && type.Name != "RightClickColumn" && !type.IsAbstract)
                        {
                            IColumn? instance = (IColumn?)Activator.CreateInstance(type);
                            if (instance != null)
                            {
                                #if !DEBUG
                                if (instance.IsDebug)
                                {
                                    continue;
                                }
                                #endif
                                _gridColumns.Add(type.Name, instance);
                            }
                        }
                    }
                }

                return _gridColumns;
            }
        }
        
        public List<ISetting> AvailableSettings
        {
            get
            {
                if (_availableSettings == null)
                {
                    _availableSettings = new List<ISetting>();
                    var columnType = typeof(ISetting);
                    var types = Assembly.GetExecutingAssembly().GetLoadableTypes().Where(columnType.IsAssignableFrom).ToList();
                    foreach (var type in types)
                    {
                        if (type.IsClass && !type.IsAbstract && !type.ContainsGenericParameters)
                        {
                            ISetting? instance = (ISetting?)Activator.CreateInstance(type);
                            if (instance != null)
                            {
                                _availableSettings.Add(instance);
                            }
                        }
                    }
                }

                return _availableSettings;
            }
        }
        
        public List<IFilter> AvailableFilters
        {
            get
            {
                if (_availableFilters == null)
                {
                    _availableFilters = new List<IFilter>();
                    var columnType = typeof(IFilter);
                    var types = Assembly.GetExecutingAssembly().GetLoadableTypes().Where(columnType.IsAssignableFrom).ToList();
                    foreach (var type in types)
                    {
                        if (type.IsClass && !type.IsAbstract)
                        {
                            IFilter? instance = (IFilter?)Activator.CreateInstance(type);
                            if (instance != null)
                            {
                                _availableFilters.Add(instance);
                            }
                        }
                    }
                }

                return _availableFilters;
            }
        }

        private Dictionary<FilterCategory, List<IFilter>>? _groupedFilters;
        public Dictionary<FilterCategory, List<IFilter>> GroupedFilters
        {
            get
            {
                if (_groupedFilters == null)
                {
                    _groupedFilters = AvailableFilters.OrderBy(c => c.Order).ThenBy(c => c.Name).GroupBy(c => c.FilterCategory).OrderBy(c => IFilter.FilterCategoryOrder.IndexOf(c.Key)).ToDictionary(c => c.Key, c => c.ToList());
                }

                return _groupedFilters;
            }
        }

        public RightClickColumn RightClickColumn => _rightClickColumn;


        public static Type? GetType(string strFullyQualifiedName)
        {
            Type? type = Type.GetType(strFullyQualifiedName);
            if (type != null)
                return type;
            var asm = Assembly.GetExecutingAssembly();
            type = asm.GetType(strFullyQualifiedName);
            if (type != null)
                return type;
            return null;
        }
        
        public static dynamic? GetClassFromString(string className)
        {
            var classAddress = $"InventoryTools.Logic.Columns.{className}";
            Type? type = GetType(classAddress);

            // Check whether the class is existed?
            if (type == null)
                return null;

            // Then create an instance
            object? instance = Activator.CreateInstance(type);
            return instance;
        }

        internal void DrawIcon(ushort icon, Vector2 size, bool hqIcon = false) {
            if (icon <= 65103)
            {
                var textureDictionary = hqIcon ? HQTextureDictionary : TextureDictionary;
                
                if (textureDictionary.ContainsKey(icon)) {
                    var tex = textureDictionary[icon];
                    if (tex.ImGuiHandle == IntPtr.Zero) {

                    } else {
                        ImGui.Image(textureDictionary[icon].ImGuiHandle, size);
                    }
                } else {
                    ImGui.BeginChild("WaitingTexture", size, true);
                    ImGui.EndChild();

                    try {
                        var iconTex = hqIcon ?  Service.TextureProvider.GetIcon(icon, ITextureProvider.IconFlags.ItemHighQuality) : Service.TextureProvider.GetIcon(icon);
                        if (iconTex != null)
                        {
                            if (iconTex.ImGuiHandle != IntPtr.Zero)
                            {
                                textureDictionary[icon] = iconTex;
                            }
                        }
                    } 
                    catch
                    {
                        Service.Log.Error("Failed to load icon correctly - " + icon + (hqIcon ? "hq" : "nq"));
                    }
                }
            } else {
                ImGui.BeginChild("NoIcon", size, true);
                ImGui.EndChild();
            }
        }


        internal void DrawUldIcon(GameIcon gameIcon,  Vector2? size = null)
        {
            DrawUldIcon(gameIcon.Name, size ?? gameIcon.Size, gameIcon.Uv0, gameIcon.Uv1);
        }

        internal void DrawUldIcon(string name, Vector2 size, Vector2? uvStart = null, Vector2? uvEnd = null)
        {
            if (UldTextureDictionary.ContainsKey(name))
            {
                var tex = UldTextureDictionary[name];
                if (tex.ImGuiHandle == IntPtr.Zero)
                {
                    if (ImGui.BeginChild("FailedTexture", size, true))
                    {
                        ImGui.Text(name);
                    }

                    ImGui.EndChild();
                }
                else
                {
                    if (uvStart.HasValue && uvEnd.HasValue)
                    {
                        ImGui.Image(UldTextureDictionary[name].ImGuiHandle, size, uvStart.Value,
                            uvEnd.Value);
                    }
                    else if (uvStart.HasValue)
                    {
                        ImGui.Image(UldTextureDictionary[name].ImGuiHandle, size, uvStart.Value);
                    }
                    else
                    {
                        ImGui.Image(UldTextureDictionary[name].ImGuiHandle, size);
                    }
                }
            }
            else
            {
                ImGui.BeginChild("WaitingTexture", size, true);
                ImGui.EndChild();

                try
                {
                    var iconTex = Service.TextureProvider.GetUldIcon(name);
                    if (iconTex != null)
                    {
                        if (iconTex.ImGuiHandle != IntPtr.Zero)
                        {
                            UldTextureDictionary[name] = iconTex;
                        }
                    }
                }
                catch
                {
                    Service.Log.Error("Failed to load icon correctly - " + name);
                }
            }
        }
        
        internal bool DrawUldIconButton(GameIcon gameIcon, Vector2? size = null)
        {
            return DrawUldIconButton(gameIcon.Name, size ?? gameIcon.Size, gameIcon.Uv0, gameIcon.Uv1);
        }

        internal bool DrawUldIconButton(string name, Vector2 size, Vector2? uvStart = null, Vector2? uvEnd = null) {
            if (UldTextureDictionary.ContainsKey(name)) {
                var tex = UldTextureDictionary[name];
                if (tex.ImGuiHandle == IntPtr.Zero) {
                    ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(1, 0, 0, 1));
                    if (ImGui.BeginChild("FailedTexture", size, true))
                    {
                        ImGui.Text(name);
                    }
                    ImGui.EndChild();
                    ImGui.PopStyleColor();
                } else {
                    if (uvStart.HasValue && uvEnd.HasValue)
                    {
                        return ImGui.ImageButton(UldTextureDictionary[name].ImGuiHandle, size, uvStart.Value,
                            uvEnd.Value);
                    }
                    else if (uvStart.HasValue)
                    {
                        return ImGui.ImageButton(UldTextureDictionary[name].ImGuiHandle, size, uvStart.Value);
                    }
                    else
                    {
                        return ImGui.ImageButton(UldTextureDictionary[name].ImGuiHandle, size);
                    }
                }
            } else {
                ImGui.BeginChild("WaitingTexture", size, true);
                ImGui.EndChild();

                try {
                    var iconTex = Service.TextureProvider.GetUldIcon(name);
                    if (iconTex != null)
                    {
                        if (iconTex.ImGuiHandle != IntPtr.Zero)
                        {
                            UldTextureDictionary[name] = iconTex;
                        }
                    }
                } catch {
                    Service.Log.Error("Failed to load icon correctly - " + name);
                }
            }

            return false;
        }
        
        public bool ToggleWindowFilterByName(string filterName)
        {
            var filterConfigurations = PluginService.FilterService.FiltersList;
            if (filterConfigurations.Any(c => c.Name == filterName))
            {
                var filter = filterConfigurations.First(c => c.Name == filterName);
                PluginService.WindowService.ToggleFilterWindow(filter.Key);
                return true;
            }
            PluginService.ChatService.Print("Failed to find filter with name: " + filterName);
            return false;
        }

        public IDalamudTextureWrap LoadImage(string imageName)
        {
            var assemblyLocation = Service.Interface!.AssemblyLocation.DirectoryName!;
            var imagePath = Path.Combine(assemblyLocation, $@"Images\{imageName}.png");
            return Service.TextureProvider.GetTextureFromFile(new FileInfo(imagePath));
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
                foreach (var filterTables in _filterTables)
                {
                    filterTables.Value.Dispose();
                }

                foreach (var textureWrap in TextureDictionary)
                {
                    textureWrap.Value.Dispose();
                }

                foreach (var textureWrap in UldTextureDictionary)
                {
                    textureWrap.Value.Dispose();
                }

                foreach (var textureWrap in HQTextureDictionary)
                {
                    textureWrap.Value.Dispose();
                }

                if (_gridColumns != null)
                {
                    foreach (var gridColumn in _gridColumns)
                    {
                        gridColumn.Value.Dispose();
                    }
                }

                PluginService.OnPluginLoaded -= PluginServiceOnOnPluginLoaded;
                PluginService.GameInterface.AcquiredItemsUpdated -= GameInterfaceOnAcquiredItemsUpdated;
                ConfigurationManager.Config.SavedCharacters = PluginService.CharacterMonitor.Characters;
                Service.Framework.Update -= FrameworkOnUpdate;
                PluginService.InventoryMonitor.OnInventoryChanged -= InventoryMonitorOnOnInventoryChanged;
                PluginService.CharacterMonitor.OnCharacterUpdated -= CharacterMonitorOnOnCharacterUpdated;
                PluginService.CraftMonitor.CraftStarted -= CraftMonitorOnCraftStarted;
                PluginService.CraftMonitor.CraftFailed -= CraftMonitorOnCraftFailed ;
                PluginService.CraftMonitor.CraftCompleted -= CraftMonitorOnCraftCompleted ;
                ConfigurationManager.Config.ConfigurationChanged -= ConfigOnConfigurationChanged;
                ConfigurationManager.Save();
                ConfigurationManager.SaveInventories(PluginService.InventoryMonitor.AllItems.ToList());
                ConfigurationManager.SaveHistory(PluginService.InventoryHistory.GetHistory());
                if (ConfigurationManager.Config.TrackMobSpawns)
                {
                    PluginService.MobTracker.SaveCsv(ConfigurationManager.MobSpawnFile,
                        PluginService.MobTracker.GetEntries());
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
                Service.Log.Error("There is a disposable object which hasn't been disposed before the finalizer call: " + (this.GetType ().Name));
            }
#endif
            Dispose (true);
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using Dalamud.Utility;
using ImGuiNET;
using ImGuiScene;
using InventoryTools.Images;
using InventoryTools.Logic;
using InventoryTools.Logic.Columns;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.Settings;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Tooltips;
using InventoryTools.Ui;
using InventoryItem = CriticalCommonLib.Models.InventoryItem;

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
        public static InventoryToolsConfiguration PluginConfiguration => ConfigurationManager.Config;

        private DateTime? _nextSaveTime = null;

        private RightClickColumn _rightClickColumn = new();
        
        public readonly ConcurrentDictionary<ushort, TextureWrap> TextureDictionary = new ConcurrentDictionary<ushort, TextureWrap>();
        public readonly ConcurrentDictionary<ushort, TextureWrap> HQTextureDictionary = new ConcurrentDictionary<ushort, TextureWrap>();
        public readonly ConcurrentDictionary<string, TextureWrap> UldTextureDictionary = new ConcurrentDictionary<string, TextureWrap>();
        
        public PluginLogic()
        {
            //Events we need to track, inventory updates, active retainer changes, player changes, 
            PluginService.InventoryMonitor.OnInventoryChanged += InventoryMonitorOnOnInventoryChanged;
            PluginService.CharacterMonitor.OnCharacterUpdated += CharacterMonitorOnOnCharacterUpdated;
            PluginConfiguration.ConfigurationChanged += ConfigOnConfigurationChanged;
            PluginService.FrameworkService.Update += FrameworkOnUpdate;

            PluginService.InventoryMonitor.LoadExistingData(PluginConfiguration.GetSavedInventory());
            PluginService.CharacterMonitor.LoadExistingRetainers(PluginConfiguration.GetSavedRetainers());
            var entries = PluginService.MobTracker.LoadCsv(ConfigurationManager.MobSpawnFile, out var success);
            if(success)
            {
                PluginService.MobTracker.SetEntries(entries);
            }
            if (ConfigurationManager.Config.TrackMobSpawns)
            {
                PluginService.MobTracker.Enable();
            }

            PluginService.GameUi.WatchWindowState(WindowName.RetainerGrid0);
            PluginService.GameUi.WatchWindowState(WindowName.InventoryGrid0E);
            PluginService.GameUi.WatchWindowState(WindowName.RetainerList);
            PluginService.GameUi.WatchWindowState(WindowName.Inventory);
            PluginService.GameUi.WatchWindowState(WindowName.InventoryLarge);
            PluginService.GameUi.WatchWindowState(WindowName.InventoryRetainerLarge);
            PluginService.GameUi.WatchWindowState(WindowName.InventoryRetainer);
            PluginService.GameUi.WatchWindowState(WindowName.InventoryBuddy);
            PluginService.GameUi.WatchWindowState(WindowName.InventoryBuddy2);
            PluginService.CraftMonitor.CraftStarted += CraftMonitorOnCraftStarted;
            PluginService.CraftMonitor.CraftFailed += CraftMonitorOnCraftFailed ;
            PluginService.CraftMonitor.CraftCompleted += CraftMonitorOnCraftCompleted ;
            PluginService.OnPluginLoaded += PluginServiceOnOnPluginLoaded;
            PluginService.GameInterface.AcquiredItemsUpdated += GameInterfaceOnAcquiredItemsUpdated;
            PluginService.TooltipService.AddTooltipTweak(new LocationDisplayTooltip());
            PluginService.TooltipService.AddTooltipTweak(new AmountOwnedTooltip());
            PluginService.TooltipService.AddTooltipTweak(new DisplayMarketPriceTooltip());
            RunMigrations();
            
            if (PluginConfiguration.FirstRun)
            {
                LoadDefaultData();
                PluginConfiguration.FirstRun = false;
            }
        }

#pragma warning disable CS8618
        public PluginLogic(bool noExternals = false)
#pragma warning restore CS8618
        {

        }

        private void PluginServiceOnOnPluginLoaded()
        {
            if (!PluginConfiguration.IntroShown)
            {
                PluginService.WindowService.OpenWindow<IntroWindow>(IntroWindow.AsKey);
                PluginConfiguration.IntroShown = true;
            }
        }

        private void CraftMonitorOnCraftCompleted(uint itemid, FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags flags, uint quantity)
        {
            var activeFilter = PluginService.FilterService.GetActiveFilter();
            if (activeFilter != null && activeFilter.FilterType == FilterType.CraftFilter)
            {
                activeFilter.CraftList.MarkCrafted(itemid, FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.None, quantity);
                activeFilter.StartRefresh();
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
                PluginConfiguration.AcquiredItems[activeCharacter] = PluginService.GameInterface.AcquiredItems;
            }
        }

        public void RunMigrations()
        {
            
            if (PluginConfiguration.InternalVersion == 0)
            {
                PluginLog.Log("Migrating to version 1");
                var highlight = PluginConfiguration.HighlightColor;
                if (highlight.W == 0.0f)
                {
                    highlight.W = 1;
                    PluginConfiguration.HighlightColor = highlight;
                }
                PluginConfiguration.TabHighlightColor = PluginConfiguration.HighlightColor;

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

                PluginConfiguration.InternalVersion++;
            }
            if (PluginConfiguration.InternalVersion == 1)
            {
                PluginLog.Log("Migrating to version 2");
                PluginConfiguration.InvertTabHighlighting = PluginConfiguration.InvertHighlighting;

                foreach (var filterConfig in PluginService.FilterService.FiltersList)
                {
                    if (filterConfig.InvertHighlighting != null)
                    {
                        filterConfig.InvertTabHighlighting = filterConfig.InvertHighlighting;
                    }
                }

                PluginConfiguration.InternalVersion++;
            }
            if (PluginConfiguration.InternalVersion == 2)
            {
                PluginLog.Log("Migrating to version 3");
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
                PluginConfiguration.InternalVersion++;
            }
            if (PluginConfiguration.InternalVersion == 3)
            {
                PluginLog.Log("Migrating to version 4");
                
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
                PluginConfiguration.InternalVersion++;
            }

            if (PluginConfiguration.InternalVersion == 4)
            {
                PluginLog.Log("Migrating to version 5");
                PluginConfiguration.RetainerListColor = ImGuiColors.HealerGreen;
                PluginConfiguration.InternalVersion++;
            }

            if (PluginConfiguration.InternalVersion == 5)
            {
                PluginLog.Log("Migrating to version 6");
                PluginConfiguration.TooltipDisplayAmountOwned = true;
                PluginConfiguration.TooltipDisplayMarketAveragePrice = true;
                PluginConfiguration.InternalVersion++;
            }

            if (PluginConfiguration.InternalVersion == 6)
            {
                PluginLog.Log("Migrating to version 7");
                PluginConfiguration.HighlightDestination = true;
                PluginConfiguration.DestinationHighlightColor = new Vector4(0.321f, 0.239f, 0.03f, 1f);
                PluginConfiguration.InternalVersion++;
            }

            if (PluginConfiguration.InternalVersion == 7)
            {
                PluginConfiguration.InternalVersion++;
            }

            if (PluginConfiguration.InternalVersion == 8)
            {
                PluginLog.Log("Migrating to version 9");
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
                PluginConfiguration.InternalVersion++;
            }

            if (PluginConfiguration.InternalVersion == 9)
            {
                PluginLog.Log("Migrating to version 10");
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
                PluginConfiguration.InternalVersion++;
            }

            if (PluginConfiguration.InternalVersion == 10)
            {
                PluginLog.Log("Migrating to version 11");
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
                PluginConfiguration.InternalVersion++;
            }

            if (PluginConfiguration.InternalVersion == 11)
            {
                PluginLog.Log("Migrating to version 12");
                PluginConfiguration.TooltipLocationLimit = 10;                
                PluginConfiguration.TooltipLocationDisplayMode =
                    TooltipLocationDisplayMode.CharacterCategoryQuantityQuality;
                PluginConfiguration.InternalVersion++;
            }
        }
        
        private bool HotkeyPressed(VirtualKey[] keys) {
            if (keys.Length == 1 && keys[0] == VirtualKey.NO_KEY)
            {
                return false;
            }
            foreach (var vk in PluginService.KeyStateService.GetValidVirtualKeys()) {
                if (keys.Contains(vk)) {
                    if (!PluginService.KeyStateService[vk]) return false;
                } else {
                    if (PluginService.KeyStateService[vk]) return false;
                }
            }
            return true;
        }

        private void FrameworkOnUpdate(IFrameworkService framework)
        {
            if (PluginConfiguration.AutoSave)
            {
                if (NextSaveTime == null && PluginConfiguration.AutoSaveMinutes != 0)
                {
                    _nextSaveTime = DateTime.Now.AddMinutes(PluginConfiguration.AutoSaveMinutes);
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
            //Hotkeys - move to own file at some point
            if (PluginConfiguration.MoreInformationHotKey != null)
            {
                var virtualKeys = PluginConfiguration.MoreInformationKeys;
                if (virtualKeys != null && virtualKeys.Length != 0 && HotkeyPressed(virtualKeys))
                {
                        var id = Service.Gui.HoveredItem;
                        if (id >= 2000000 || id == 0) return;
                        id %= 500000;
                        var item = Service.ExcelCache.GetItemExSheet().GetRow((uint) id);
                        if (item == null) return;
                        PluginService.WindowService.OpenItemWindow(item.RowId);
                        foreach (var k in virtualKeys) {
                            PluginService.KeyStateService[(int) k] = false;
                        }
                }
            }
        }

        private void ConfigOnConfigurationChanged()
        {
            ConfigurationManager.SaveAsync();
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
        }

        private void CharacterMonitorOnOnCharacterUpdated(Character? character)
        {
            if (character != null)
            {
                if (PluginConfiguration.AcquiredItems.ContainsKey(character.CharacterId))
                {
                    PluginService.GameInterface.AcquiredItems = PluginConfiguration.AcquiredItems[character.CharacterId];
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

            AddAllGameItemsFilter();
        }

        public void AddAllFilter()
        {
            var allItemsFilter = new FilterConfiguration("All", FilterType.SearchFilter);
            allItemsFilter.DisplayInTabs = true;
            allItemsFilter.SourceAllCharacters = true;
            allItemsFilter.SourceAllRetainers = true;
            PluginService.FilterService.AddFilter(allItemsFilter);
        }

        public void AddRetainerFilter()
        {
            var retainerItemsFilter = new FilterConfiguration("Retainers", FilterType.SearchFilter);
            retainerItemsFilter.DisplayInTabs = true;
            retainerItemsFilter.SourceAllRetainers = true;
            PluginService.FilterService.AddFilter(retainerItemsFilter);
        }

        public void AddPlayerFilter()
        {
            var playerItemsFilter = new FilterConfiguration("Player",  FilterType.SearchFilter);
            playerItemsFilter.DisplayInTabs = true;
            playerItemsFilter.SourceAllCharacters = true;
            PluginService.FilterService.AddFilter(playerItemsFilter);
        }

        public void AddAllGameItemsFilter()
        {
            var allGameItemsFilter = new FilterConfiguration("All Game Items", FilterType.GameItemFilter);
            allGameItemsFilter.DisplayInTabs = true;            
            PluginService.FilterService.AddFilter(allGameItemsFilter);

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
            var itemUiCategories = Service.ExcelCache.GetAllItemUICategories();
            //I'm making assumptions about the names of these and one day I will try to support more than english
            var categories = new HashSet<string>() { "Bone", "Cloth", "Catalyst", "Crystal", "Ingredient", "Leather", "Lumber", "Metal", "Part", "Stone" };
            sampleFilter.ItemUiCategoryId = new List<uint>();
            foreach (var itemUiCategory in itemUiCategories)
            {
                if (categories.Contains(itemUiCategory.Value.Name))
                {
                    sampleFilter.ItemUiCategoryId.Add(itemUiCategory.Key);
                }
            }
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

        private void InventoryMonitorOnOnInventoryChanged(Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>> inventories, InventoryMonitor.ItemChanges itemChanges)
        {
            PluginLog.Verbose("PluginLogic: Inventory changed, saving to config.");
            PluginConfiguration.SavedInventories = inventories;
            if (PluginConfiguration.AutomaticallyDownloadMarketPrices)
            {
                foreach (var inventory in PluginService.InventoryMonitor.AllItems)
                {
                    PluginService.MarketCache.RequestCheck(inventory.ItemId);
                }
            }

            foreach (var item in itemChanges.NewItems)
            {
                if (_recentlyAddedSeen.ContainsKey(item.ItemId))
                {
                    _recentlyAddedSeen.Remove(item.ItemId);
                }
                _recentlyAddedSeen.Add(item.ItemId, item);
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
                        if (type.IsClass && !type.IsAbstract)
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

        internal TextureWrap? GetIcon(ushort icon, bool hqIcon = false)
        {
            if (icon < 65100)
            {
                var textureDictionary = hqIcon ? HQTextureDictionary : TextureDictionary;
                
                if (textureDictionary.ContainsKey(icon)) {
                    var tex = textureDictionary[icon];
                    if (tex.ImGuiHandle != IntPtr.Zero)
                    {
                        return tex;
                    }
                } else {
                    try {
                        var iconTex = hqIcon ?  PluginService.DataService.GetHqIcon(icon) : PluginService.DataService.GetIcon(icon);
                        if (iconTex != null)
                        {
                            var tex = PluginService.PluginInterfaceService.LoadImageRaw(iconTex.GetRgbaImageData(),
                                iconTex.Header.Width, iconTex.Header.Height, 4);
                            if (tex.ImGuiHandle != IntPtr.Zero)
                            {
                                textureDictionary[icon] = tex;
                                return tex;
                            }
                        }
                    } catch {
                        PluginLog.Error("Failed to load icon correctly - " + icon + (hqIcon ? "hq" : "nq"));
                        return null;
                    }
                }
            }

            return null;
        }
        
        internal void DrawIcon(ushort icon, Vector2 size, bool hqIcon = false) {
            if (icon < 65100)
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
                        var iconTex = hqIcon ?  PluginService.DataService.GetHqIcon(icon) : PluginService.DataService.GetIcon(icon);
                        if (iconTex != null)
                        {
                            var tex = PluginService.PluginInterfaceService.LoadImageRaw(iconTex.GetRgbaImageData(),
                                iconTex.Header.Width, iconTex.Header.Height, 4);
                            if (tex.ImGuiHandle != IntPtr.Zero)
                            {
                                textureDictionary[icon] = tex;
                            }
                        }
                    } 
                    catch
                    {
                        PluginLog.Error("Failed to load icon correctly - " + icon + (hqIcon ? "hq" : "nq"));
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
                    ImGui.BeginChild("FailedTexture", size, true);
                    ImGui.Text(name);
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
                    var iconTex = PluginService.DataService.GetUldIcon(name);
                    if (iconTex != null)
                    {
                        var tex = PluginService.PluginInterfaceService.LoadImageRaw(iconTex.GetRgbaImageData(),
                            iconTex.Header.Width, iconTex.Header.Height, 4);
                        if (tex.ImGuiHandle != IntPtr.Zero)
                        {
                            UldTextureDictionary[name] = tex;
                        }
                    }
                }
                catch
                {
                    PluginLog.Error("Failed to load icon correctly - " + name);
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
                    ImGui.BeginChild("FailedTexture", size, true);
                    ImGui.Text(name);
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
                    var iconTex = PluginService.DataService.GetUldIcon(name);
                    if (iconTex != null)
                    {
                        var tex = PluginService.PluginInterfaceService.LoadImageRaw(iconTex.GetRgbaImageData(),
                            iconTex.Header.Width, iconTex.Header.Height, 4);
                        if (tex.ImGuiHandle != IntPtr.Zero)
                        {
                            UldTextureDictionary[name] = tex;
                        }
                    }
                } catch {
                    PluginLog.Error("Failed to load icon correctly - " + name);
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

        public TextureWrap LoadImage(string imageName)
        {
            var assemblyLocation = PluginService.PluginInterfaceService!.AssemblyLocation.DirectoryName!;
            var imagePath = Path.Combine(assemblyLocation, $@"Images\{imageName}.png");

            return  PluginService.PluginInterfaceService!.LoadImage(imagePath);
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
                PluginService.OnPluginLoaded -= PluginServiceOnOnPluginLoaded;
                PluginService.GameInterface.AcquiredItemsUpdated -= GameInterfaceOnAcquiredItemsUpdated;
                PluginConfiguration.SavedCharacters = PluginService.CharacterMonitor.Characters;
                PluginService.FrameworkService.Update -= FrameworkOnUpdate;
                PluginService.InventoryMonitor.OnInventoryChanged -= InventoryMonitorOnOnInventoryChanged;
                PluginService.CharacterMonitor.OnCharacterUpdated -= CharacterMonitorOnOnCharacterUpdated;
                PluginService.CraftMonitor.CraftStarted -= CraftMonitorOnCraftStarted;
                PluginService.CraftMonitor.CraftFailed -= CraftMonitorOnCraftFailed ;
                PluginService.CraftMonitor.CraftCompleted -= CraftMonitorOnCraftCompleted ;
                PluginConfiguration.ConfigurationChanged -= ConfigOnConfigurationChanged;
                if (PluginConfiguration.TrackMobSpawns)
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
                PluginLog.Error("There is a disposable object which hasn't been disposed before the finalizer call: " + (this.GetType ().Name));
            }
#endif
            Dispose (true);
        }
    }
}
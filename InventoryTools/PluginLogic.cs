using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using CriticalCommonLib;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using Dalamud.Game;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using Dalamud.Utility;
using ImGuiNET;
using ImGuiScene;
using InventoryTools.Extensions;
using InventoryTools.Images;
using InventoryTools.Logic;
using InventoryTools.Logic.Columns;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.Settings;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Ui;
using XivCommon;
using XivCommon.Functions.Tooltips;
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
        private XivCommonBase CommonBase { get; set; }

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
            Service.Framework.Update += FrameworkOnUpdate;

            PluginService.InventoryMonitor.LoadExistingData(PluginConfiguration.GetSavedInventory());
            PluginService.CharacterMonitor.LoadExistingRetainers(PluginConfiguration.GetSavedRetainers());

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

            RunMigrations();
            
            if (PluginConfiguration.FirstRun)
            {
                LoadDefaultData();
                PluginConfiguration.FirstRun = false;
            }

            this.CommonBase = new XivCommonBase(Hooks.Tooltips);
            this.CommonBase.Functions.Tooltips.OnItemTooltip += this.OnItemTooltip;
            this.CommonBase.Functions.Tooltips.OnItemTooltip += this.AddHotKeyTooltip;

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

        private void AddHotKeyTooltip(ItemTooltip tooltip, ulong itemId)
        {
            if(Service.KeyState[VirtualKey.CONTROL])
            {
                return;
            }
            ItemTooltipString itemTooltipString = ItemTooltipString.ControllerControls;
            if (itemId > 2000000 || itemId == 0)
            {
                return;
            }

            var description = tooltip[itemTooltipString];
            if (PluginConfiguration.MoreInformationHotKey != null)
            {
                description.Payloads.Add(new TextPayload($"\n{PluginConfiguration.MoreInformationHotKey.Value.FormattedName()}  More info"));
            }
            tooltip[itemTooltipString] = description;

        }

        private void CraftMonitorOnCraftCompleted(uint itemid, FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags flags, uint quantity)
        {
            var activeFilter = PluginService.FilterService.GetActiveFilter();
            if (activeFilter != null && activeFilter.FilterType == FilterType.CraftFilter)
            {
                activeFilter.CraftList.MarkCrafted(itemid, FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.None, 1);
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
            var activeCharacter = PluginService.CharacterMonitor.ActiveCharacter;
            if (activeCharacter != 0)
            {
                PluginConfiguration.AcquiredItems[activeCharacter] = PluginService.GameInterface.AcquiredItems;
            }
        }

        private void RunMigrations()
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
            foreach (var vk in Service.KeyState.GetValidVirtualKeys()) {
                if (keys.Contains(vk)) {
                    if (!Service.KeyState[vk]) return false;
                } else {
                    if (Service.KeyState[vk]) return false;
                }
            }
            return true;
        }

        private void FrameworkOnUpdate(Framework framework)
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
                            Service.KeyState[(int) k] = false;
                        }
                }
            }
        }

        private void ConfigOnConfigurationChanged()
        {
            ConfigurationManager.SaveAsync();
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

        public void AddSampleFilter100Gil()
        {
            var sampleFilter = new FilterConfiguration("100 gill or less", FilterType.SearchFilter);
            sampleFilter.DisplayInTabs = true;
            sampleFilter.SourceAllCharacters = true;
            sampleFilter.SourceAllRetainers = true;
            sampleFilter.CanBeBought = true;
            sampleFilter.ShopBuyingPrice = "<=100";
            PluginService.FilterService.AddFilter(sampleFilter);
        }

        public void AddSampleFilterMaterials()
        {
            var sampleFilter = new FilterConfiguration("Put away materials", FilterType.SortingFilter);
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

        public void AddSampleFilterDuplicatedItems()
        {
            var sampleFilter = new FilterConfiguration("Duplicated SortedItems", FilterType.SortingFilter);
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
            _clearCachedLines = true;
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

        private Dictionary<ulong, List<Payload>> _cachedTooltipLines = new();
        private bool _clearCachedLines = false;

        private void OnItemTooltip(ItemTooltip tooltip, ulong itemId)
        {
            if (!PluginConfiguration.DisplayTooltip || Service.KeyState[VirtualKey.CONTROL])
            {
                return;
            }

            if (_clearCachedLines)
            {
                _cachedTooltipLines = new Dictionary<ulong, List<Payload>>();
            }

            ItemTooltipString itemTooltipString;
            if (tooltip.Fields.HasFlag(ItemTooltipFields.Description))
            {
                itemTooltipString = ItemTooltipString.Description;
            }
            else if (tooltip.Fields.HasFlag(ItemTooltipFields.Levels))
            {
                itemTooltipString = ItemTooltipString.EquipLevel;
            }
            else if (tooltip.Fields.HasFlag(ItemTooltipFields.Effects))
            {
                itemTooltipString = ItemTooltipString.Effects;
            }
            else
            {
                return;
            }

            if (itemId > 2000000 || itemId == 0)
            {
                return;
            }

            var isHq = false;
            var isCollectible = false;
            if (itemId > 1000000)
            {
                itemId -= 1000000;
                isHq = true;
            }
            if (itemId > 500000)
            {
                itemId -= 500000;
                isCollectible = true;
            }
            var lines = new List<Payload>();
            var textLines = new List<string>();
            
            if (_cachedTooltipLines.ContainsKey(itemId))
            {
                lines = _cachedTooltipLines[itemId];
            }
            else
            {
                const string indentation = "      ";

                if (PluginConfiguration.TooltipDisplayRetrieveAmount)
                {
                    var filterConfiguration = PluginService.FilterService.GetActiveFilter();
                    if (filterConfiguration != null)
                    {
                        if (filterConfiguration.FilterType == FilterType.CraftFilter)
                        {
                            var filterResult = filterConfiguration.FilterResult;
                            if (filterResult != null)
                            {
                                var sortedItems = filterResult.Value.SortedItems.Where(c =>
                                    c.InventoryItem.ItemId == itemId && c.InventoryItem.IsHQ == isHq).ToList();
                                if (sortedItems.Any())
                                {
                                    var sortedItem = sortedItems.First();
                                    if (sortedItem.Quantity != 0)
                                    {
                                        textLines.Add("Retrieve: " + sortedItem.Quantity + "\n");
                                    }
                                }
                            }
                        }
                    }
                }

                if (PluginConfiguration.TooltipDisplayAmountOwned)
                {
                    var ownedItems = PluginService.InventoryMonitor.AllItems.Where(item => 
                            item.ItemId == itemId && 
                            PluginService.CharacterMonitor.Characters.ContainsKey(item.RetainerId) &&
                            ((PluginConfiguration.TooltipCurrentCharacter && PluginService.CharacterMonitor.BelongsToActiveCharacter(item.RetainerId)) ||  !PluginConfiguration.TooltipCurrentCharacter)
                            )
                        .ToList();
                        
                    uint storageCount = 0;
                    List<string> locations = new List<string>();
                    
                    if (PluginConfiguration.TooltipLocationDisplayMode ==
                        TooltipLocationDisplayMode.CharacterBagSlotQuality)
                    {
                        var maxOwnedItems = ownedItems.Take(PluginConfiguration.TooltipLocationLimit).ToList();
                        foreach (var oItem in maxOwnedItems)
                        {
                            storageCount += oItem.Quantity;

                            var characterMonitorCharacter = PluginService.CharacterMonitor.Characters[oItem.RetainerId];
                            var name = characterMonitorCharacter?.FormattedName ?? "Unknown";
                            name = name.Trim().Length == 0 ? "Unknown" : name.Trim();
                            if (characterMonitorCharacter != null && characterMonitorCharacter.OwnerId != 0 &&
                                PluginConfiguration.TooltipAddCharacterNameOwned &&
                                PluginService.CharacterMonitor.Characters.ContainsKey(characterMonitorCharacter
                                    .OwnerId))
                            {
                                var owner = PluginService.CharacterMonitor.Characters[
                                    characterMonitorCharacter.OwnerId];
                                name += " (" + owner.FormattedName + ")";
                            }

                            var typeIcon = "";
                            if (oItem.IsHQ)
                            {
                                typeIcon = "\uE03c";
                            }
                            else if (oItem.IsCollectible)
                            {
                                typeIcon = "\uE03d";
                            }

                            locations.Add($"{name} - {oItem.FormattedBagLocation} " + typeIcon);
                        }
                        if (ownedItems.Count > maxOwnedItems.Count)
                        {
                            locations.Add(ownedItems.Count - maxOwnedItems.Count + " other locations.");
                        }                        
                    }
                    else if (PluginConfiguration.TooltipLocationDisplayMode == TooltipLocationDisplayMode.CharacterCategoryQuantityQuality)
                    {
                        var groupedItems = ownedItems.GroupBy(c => (c.RetainerId, c.SortedCategory, c.Flags)).ToList();
                        var maxGroupedItems = groupedItems.Take(PluginConfiguration.TooltipLocationLimit).ToList();
                        foreach (var oGroup in maxGroupedItems)
                        {
                            var quantity = oGroup.Sum(c => c.Quantity);
                            storageCount += (uint)quantity;

                            var characterMonitorCharacter = PluginService.CharacterMonitor.Characters[oGroup.Key.RetainerId];
                            var name = characterMonitorCharacter?.FormattedName ?? "Unknown";
                            name = name.Trim().Length == 0 ? "Unknown" : name.Trim();
                            if (characterMonitorCharacter != null && characterMonitorCharacter.OwnerId != 0 &&
                                PluginConfiguration.TooltipAddCharacterNameOwned &&
                                PluginService.CharacterMonitor.Characters.ContainsKey(characterMonitorCharacter
                                    .OwnerId))
                            {
                                var owner = PluginService.CharacterMonitor.Characters[
                                    characterMonitorCharacter.OwnerId];
                                name += " (" + owner.FormattedName + ")";
                            }

                            var typeIcon = "";
                            if ((oGroup.Key.Flags & FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.HQ) != 0)
                            {
                                typeIcon = "\uE03c";
                            }
                            else if ((oGroup.Key.Flags & FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.Collectable) != 0)
                            {
                                typeIcon = "\uE03d";
                            }

                            locations.Add($"{name} - {oGroup.Key.SortedCategory.FormattedName()} - " + quantity + " " + typeIcon);
                        }
                        if (groupedItems.Count > maxGroupedItems.Count)
                        {
                            locations.Add(groupedItems.Count - maxGroupedItems.Count + " other locations.");
                        }  
                    }
                    else if (PluginConfiguration.TooltipLocationDisplayMode == TooltipLocationDisplayMode.CharacterQuantityQuality)
                    {
                        var groupedItems = ownedItems.GroupBy(c => (c.RetainerId, c.Flags)).ToList();
                        var maxGroupedItems = groupedItems.Take(PluginConfiguration.TooltipLocationLimit).ToList();
                        foreach (var oGroup in maxGroupedItems)
                        {
                            var quantity = oGroup.Sum(c => c.Quantity);
                            storageCount += (uint)quantity;

                            var characterMonitorCharacter = PluginService.CharacterMonitor.Characters[oGroup.Key.RetainerId];
                            var name = characterMonitorCharacter?.FormattedName ?? "Unknown";
                            name = name.Trim().Length == 0 ? "Unknown" : name.Trim();
                            if (characterMonitorCharacter != null && characterMonitorCharacter.OwnerId != 0 &&
                                PluginConfiguration.TooltipAddCharacterNameOwned &&
                                PluginService.CharacterMonitor.Characters.ContainsKey(characterMonitorCharacter
                                    .OwnerId))
                            {
                                var owner = PluginService.CharacterMonitor.Characters[
                                    characterMonitorCharacter.OwnerId];
                                name += " (" + owner.FormattedName + ")";
                            }

                            var typeIcon = "";
                            if ((oGroup.Key.Flags & FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.HQ) != 0)
                            {
                                typeIcon = "\uE03c";
                            }
                            else if ((oGroup.Key.Flags & FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.Collectable) != 0)
                            {
                                typeIcon = "\uE03d";
                            }

                            locations.Add($"{name} - " + quantity + " " + typeIcon);
                        }
                        if (groupedItems.Count > maxGroupedItems.Count)
                        {
                            locations.Add(groupedItems.Count - maxGroupedItems.Count + " other locations.");
                        }  
                    }

                    if (storageCount > 0)
                    {
                        textLines.Add($"Owned: {storageCount}\n");
                        textLines.Add($"Locations:\n");
                        foreach (var location in locations)
                        {
                            textLines.Add($"{indentation}{location}\n");
                        }
                    }
                }

                if (PluginConfiguration.TooltipDisplayMarketAveragePrice ||
                    PluginConfiguration.TooltipDisplayMarketLowestPrice)
                {
                    if (!(Service.ExcelCache.GetItemExSheet().GetRow((uint)itemId)?.IsUntradable ?? true))
                    {
                        var marketData = PluginService.MarketCache.GetPricing((uint)itemId, false);
                        if (marketData != null)
                        {
                            textLines.Add("Market Board Data:\n");
                            if (PluginConfiguration.TooltipDisplayMarketAveragePrice)
                            {
                                textLines.Add($"{indentation}Average Price: {Math.Round(marketData.averagePriceNQ, 0)}\n");
                                textLines.Add(
                                    $"{indentation}Average Price (HQ): {Math.Round(marketData.averagePriceHQ, 0)}\n");
                            }

                            if (PluginConfiguration.TooltipDisplayMarketLowestPrice)
                            {
                                textLines.Add($"{indentation}Minimum Price: {Math.Round(marketData.minPriceNQ, 0)}\n");
                                textLines.Add($"{indentation}Minimum Price (HQ): {Math.Round(marketData.minPriceHQ, 0)}\n");
                            }
                        }
                    }
                }


                
                var newText = "";

                if (textLines.Count != 0)
                {
                    newText += "\n\n";
                    newText += "[InventoryTools]\n";
                    foreach (var line in textLines)
                    {
                        newText += line;
                    }
                }

                if (newText != "")
                {
                    lines = new List<Payload>()
                    {
                        new UIForegroundPayload((ushort)(PluginConfiguration.TooltipColor ?? 1)),
                        new UIGlowPayload(0),                    
                        new TextPayload(newText),                    
                        new UIForegroundPayload(0),
                        new UIGlowPayload(0), 
                    };
                }
                else
                {
                    lines = new List<Payload>();
                }
                _cachedTooltipLines[itemId] = lines;

            }
            var description = tooltip[itemTooltipString];
            description = description.Append(lines);
            tooltip[itemTooltipString] = description;

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

                    Task.Run(() => {
                        try {
                            var iconTex = hqIcon ?  Service.Data.GetHqIcon(icon) : Service.Data.GetIcon(icon);
                            if (iconTex != null)
                            {
                                var tex = Service.Interface.UiBuilder.LoadImageRaw(iconTex.GetRgbaImageData(),
                                    iconTex.Header.Width, iconTex.Header.Height, 4);
                                if (tex.ImGuiHandle != IntPtr.Zero)
                                {
                                    textureDictionary[icon] = tex;
                                }
                            }
                        } catch {
                        }
                    });
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

                    Task.Run(() => {
                        try {
                            var iconTex = hqIcon ?  Service.Data.GetHqIcon(icon) : Service.Data.GetIcon(icon);
                            if (iconTex != null)
                            {
                                var tex = Service.Interface.UiBuilder.LoadImageRaw(iconTex.GetRgbaImageData(),
                                    iconTex.Header.Width, iconTex.Header.Height, 4);
                                if (tex.ImGuiHandle != IntPtr.Zero)
                                {
                                    textureDictionary[icon] = tex;
                                }
                            }
                        } catch {
                        }
                    });
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

                Task.Run(() =>
                {
                    try
                    {
                        var iconTex = Service.Data.GetUldIcon(name);
                        if (iconTex != null)
                        {
                            var tex = Service.Interface.UiBuilder.LoadImageRaw(iconTex.GetRgbaImageData(),
                                iconTex.Header.Width, iconTex.Header.Height, 4);
                            if (tex.ImGuiHandle != IntPtr.Zero)
                            {
                                UldTextureDictionary[name] = tex;
                            }
                        }
                    }
                    catch
                    {
                    }
                });
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

                Task.Run(() => {
                    try {
                        var iconTex = Service.Data.GetUldIcon(name);
                        if (iconTex != null)
                        {
                            var tex = Service.Interface.UiBuilder.LoadImageRaw(iconTex.GetRgbaImageData(),
                                iconTex.Header.Width, iconTex.Header.Height, 4);
                            if (tex.ImGuiHandle != IntPtr.Zero)
                            {
                                UldTextureDictionary[name] = tex;
                            }
                        }
                    } catch {
                    }
                });
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
            Service.Chat.Print("Failed to find filter with name: " + filterName);
            return false;
        }

        public TextureWrap LoadImage(string imageName)
        {
            var assemblyLocation = PluginService.PluginInterface!.AssemblyLocation.DirectoryName!;
            var imagePath = Path.Combine(assemblyLocation, $@"Images\{imageName}.png");

            return  PluginService.PluginInterface!.UiBuilder.LoadImage(imagePath);
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
                PluginService.OnPluginLoaded -= PluginServiceOnOnPluginLoaded;
                PluginService.GameInterface.AcquiredItemsUpdated -= GameInterfaceOnAcquiredItemsUpdated;
                PluginConfiguration.SavedCharacters = PluginService.CharacterMonitor.Characters;
                Service.Framework.Update -= FrameworkOnUpdate;
                PluginService.InventoryMonitor.OnInventoryChanged -= InventoryMonitorOnOnInventoryChanged;
                PluginService.CharacterMonitor.OnCharacterUpdated -= CharacterMonitorOnOnCharacterUpdated;
                PluginService.CraftMonitor.CraftStarted -= CraftMonitorOnCraftStarted;
                PluginService.CraftMonitor.CraftFailed -= CraftMonitorOnCraftFailed ;
                PluginService.CraftMonitor.CraftCompleted -= CraftMonitorOnCraftCompleted ;
                PluginConfiguration.ConfigurationChanged -= ConfigOnConfigurationChanged;
                CommonBase.Functions.Tooltips.OnItemTooltip -= this.OnItemTooltip;
                CommonBase.Functions.Tooltips.OnItemTooltip -= this.AddHotKeyTooltip;
                CommonBase.Dispose();
            }
            _disposed = true;         
        }
    }
}
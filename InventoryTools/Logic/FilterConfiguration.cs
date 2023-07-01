using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using Dalamud.Logging;
using InventoryTools.Extensions;
using Newtonsoft.Json;

namespace InventoryTools.Logic
{
    public class FilterConfiguration
    {
        public delegate void ConfigurationChangedDelegate(FilterConfiguration filterConfiguration, bool filterInvalidated = false);
        public delegate void TableConfigurationChangedDelegate(FilterConfiguration filterConfiguration);
        public delegate void ListUpdatedDelegate(FilterConfiguration filterConfiguration);

        private List<(ulong, InventoryCategory)> _destinationInventories = new();
        private bool _displayInTabs = false;
        private bool? _duplicatesOnly;
        private List<uint> _equipSlotCategoryId = new();
        private bool? _isCollectible;
        private bool? _isHq;
        private List<uint> _itemSearchCategoryId = new();
        private List<uint> _itemSortCategoryId = new();
        private List<uint> _itemUiCategoryId = new();
        private Dictionary<string, bool>? _booleanFilters = new();
        private Dictionary<string, string>? _stringFilters = new();
        private Dictionary<string, int>? _integerFilters = new();
        private Dictionary<string, decimal>? _decimalFilters = new();
        private Dictionary<string, uint>? _uintFilters = new();
        private Dictionary<string, List<uint>>? _uintChoiceFilters = new();
        private Dictionary<string, List<ulong>>? _ulongChoiceFilters = new();
        private Dictionary<string, List<string>>? _stringChoiceFilters = new();
        private Dictionary<string, Vector4>? _colorFilters = new();
        private string? _name = "";
        private string _key = "";
        private ulong _ownerId = 0;
        private uint _order = 0;
        private bool? _filterItemsInRetainers;
        private FilterItemsRetainerEnum? _filterItemsInRetainersEnum;
        private bool? _sourceAllRetainers;
        private bool? _sourceAllHouses;
        private bool? _sourceAllCharacters;
        private bool? _sourceAllFreeCompanies;
        private bool? _destinationAllRetainers;
        private bool? _destinationAllCharacters;
        private bool? _destinationAllFreeCompanies;
        private bool? _destinationAllHouses;
        private bool? _sourceIncludeCrossCharacter;
        private bool? _destinationIncludeCrossCharacter;
        private int? _freezeColumns;
        private int? _freezeCraftColumns;
        private HashSet<InventoryCategory>? _destinationCategories;
        private HashSet<InventoryCategory>? _sourceCategories;
        private string _quantity = "";
        private string _spiritbond = "";
        private string _nameFilter = "";
        private string _iLevel = "";
        private string _shopSellingPrice = "";
        private string _shopBuyingPrice = "";
        private string _marketAveragePrice = "";
        private string _marketTotalAveragePrice = "";
        private bool _openAsWindow = false;
        private bool? _canBeBought;
        private bool? _isAvailableAtTimedNode;
        private List<(ulong, InventoryCategory)> _sourceInventories = new();
        private FilterType _filterType;
        private Vector4? _highlightColor;
        private Vector4? _tabHighlightColor;
        private Vector4? _retainerListColor;
        private Vector4? _destinationHighlightColor;
        private bool? _invertHighlighting = null;
        private bool? _invertDestinationHighlighting = null;
        private bool? _invertTabHighlighting = null;
        private bool? _highlightDestination = null;
        private bool? _highlightDestinationEmpty = null;
        private bool? _ignoreHQWhenSorting = null;
        private bool _craftListDefault = false;
        private string? _highlightWhen = null;
        private int _tableHeight = 24;
        private int _craftTableHeight = 24;
        private List<string>? _columns;
        private List<string>? _craftColumns;
        private string? _icon;
        private static readonly byte CurrentVersion = 1;
        private HashSet<uint>? _sourceWorlds;
        
        //Crafting
        private CraftList? _craftList = null;
        private bool? _simpleCraftingMode = null;
        private bool? _useORFiltering = null;
        public string ExportBase64()
        {
            var toExport = (FilterConfiguration)this.MemberwiseClone();
            toExport.DestinationInventories = new List<(ulong, InventoryCategory)>();
            toExport.SourceInventories = new List<(ulong, InventoryCategory)>();
            var json  = JsonConvert.SerializeObject(toExport);
            var bytes = Encoding.UTF8.GetBytes(json).Prepend(CurrentVersion).ToArray();
            return bytes.ToCompressedBase64();
        }
        public static bool FromBase64(string data, out FilterConfiguration filterConfiguration)
        {
            filterConfiguration = new FilterConfiguration();
            try
            {
                var bytes = data.FromCompressedBase64();
                if (bytes.Length == 0 || bytes[0] != CurrentVersion)
                {
                    return false;
                }

                var json = Encoding.UTF8.GetString(bytes.AsSpan()[1..]);
                var deserializeObject = JsonConvert.DeserializeObject<FilterConfiguration>(json);
                if (deserializeObject == null)
                {
                    return false;
                }

                deserializeObject.Key = Guid.NewGuid().ToString("N");
                filterConfiguration = deserializeObject;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string TableId
        {
            get
            {
                if (_tableId == null)
                {
                    var newTableId = GenerateNewTableId();
                    return newTableId;
                }
                return _tableId;
            }
            set => _tableId = value;
        }

        public string GenerateNewTableId()
        {
            TableId = Guid.NewGuid().ToString("N");
            return TableId;
        }

        public string CraftTableId
        {
            get
            {
                if (_craftTableId == null)
                {
                    var newCraftTableId = GenerateNewCraftTableId();
                    return newCraftTableId;
                }
                return _craftTableId;
            }
            set => _craftTableId = value;
        }

        public string GenerateNewCraftTableId()
        {
            CraftTableId = Guid.NewGuid().ToString("N");
            return CraftTableId;
        }
        
        [JsonIgnore]
        private FilterResult? _filterResult = null;

        private string? _tableId = null;
        private string? _craftTableId = null;
        public bool NeedsRefresh { get; set; } = true;
        public HighlightMode HighlightMode { get; set; } = HighlightMode.Never;

        [JsonIgnore]
        public FilterResult? FilterResult
        {
            get
            {
                if ((_filterResult == null || NeedsRefresh) && !_refreshing)
                {
                    StartRefresh();
                }
                return _filterResult;
            }
            set => _filterResult = value;
        }

        private Queue _refreshQueue = new Queue();

        [JsonIgnore]
        private bool _refreshing;

        public async void StartRefresh()
        {
            if (_refreshing)
            {
                _refreshQueue?.Enqueue(null);
                PluginLog.Debug("Not refreshing filter: " + this.Name);
                return;
            }

            PluginLog.Debug("Refreshing filter: " + this.Name);
            _refreshing = true;
            CraftList.BeenUpdated = false;

            if (this.FilterType == FilterType.CraftFilter)
            {
                //Clean up this sloppy function
                var playerBags = PluginService.InventoryMonitor.GetSpecificInventory(PluginService.CharacterMonitor.ActiveCharacterId,
                   InventoryCategory.CharacterBags);
                var crystalBags = PluginService.InventoryMonitor.GetSpecificInventory(PluginService.CharacterMonitor.ActiveCharacterId,
                   InventoryCategory.Crystals);
                var currencyBags = PluginService.InventoryMonitor.GetSpecificInventory(PluginService.CharacterMonitor.ActiveCharacterId,
                   InventoryCategory.Currency);
                var retainerSetting = this.FilterItemsInRetainersEnum;
                _filterItemsInRetainersEnum = FilterItemsRetainerEnum.No;
                var tempFilterResult = await GenerateFilteredList(PluginService.InventoryMonitor.Inventories.Select(c => c.Value).ToList());
                _filterItemsInRetainersEnum = retainerSetting;
                var externalBags = tempFilterResult.SortedItems.Select(c => c.InventoryItem).ToList();
                var characterSources = new Dictionary<uint, List<CraftItemSource>>();
                var externalSources = new Dictionary<uint, List<CraftItemSource>>();
                foreach (var item in playerBags)
                {
                    if (!characterSources.ContainsKey(item.ItemId))
                    {
                        characterSources.Add(item.ItemId,new List<CraftItemSource>());
                    }
                    characterSources[item.ItemId].Add(new CraftItemSource(item.ItemId, item.Quantity, item.IsHQ));
                }
                foreach (var item in crystalBags)
                {
                    if (!characterSources.ContainsKey(item.ItemId))
                    {
                        characterSources.Add(item.ItemId,new List<CraftItemSource>());
                    }
                    characterSources[item.ItemId].Add(new CraftItemSource(item.ItemId, item.Quantity, item.IsHQ));
                }
                foreach (var item in currencyBags)
                {
                    if (!characterSources.ContainsKey(item.ItemId))
                    {
                        characterSources.Add(item.ItemId,new List<CraftItemSource>());
                    }
                    characterSources[item.ItemId].Add(new CraftItemSource(item.ItemId, item.Quantity, item.IsHQ));
                }
                
                foreach (var item in externalBags)
                {
                    if (!externalSources.ContainsKey(item.ItemId))
                    {
                        externalSources.Add(item.ItemId,new List<CraftItemSource>());
                    }
                    externalSources[item.ItemId].Add(new CraftItemSource(item.ItemId, item.Quantity, item.IsHQ));
                }
                CraftList.Update(characterSources, externalSources);
                CraftList.CalculateCosts(PluginService.MarketCache);
                PluginLog.Debug("Generating filtered list for filter: " + this.Name);
                _filterResult = await GenerateFilteredList(PluginService.InventoryMonitor.Inventories.Select(c => c.Value).ToList());
                PluginLog.Debug("Finished generating filtered list for filter: " + this.Name);
            }
            else
            {
                PluginLog.Debug("Generating filtered list for filter: " + this.Name);
                _filterResult = await GenerateFilteredList(PluginService.InventoryMonitor.Inventories.Select(c => c.Value).ToList());
                PluginLog.Debug("Finished generating filtered list for filter: " + this.Name);
            }
            
            NeedsRefresh = false;
            await PluginService.FrameworkService.RunOnFrameworkThread(() => { ListUpdated?.Invoke(this); });
            PluginLog.Debug("Finished refreshing filter: " + this.Name);
            _refreshing = false;
            if (_refreshQueue.Contains(null))
            {
                _refreshQueue.Clear();
                StartRefresh();
            }
        }
        
        public FilterConfiguration(string name, string key, FilterType filterType)
        {
            FilterType = filterType;
            Name = name;
            Key = key;
            AddDefaultColumns();
        }

        public FilterConfiguration(string name, FilterType filterType)
        {
            FilterType = filterType;
            Name = name;
            Key = Guid.NewGuid().ToString("N");
            AddDefaultColumns();
        }

        public static FilterConfiguration GenerateDefaultFilterConfiguration()
        {
            var defaultFilter = new FilterConfiguration("Default Craft List", FilterType.CraftFilter);
            defaultFilter.ApplyDefaultCraftFilterConfiguration();
            return defaultFilter;
        }

        public void ApplyDefaultCraftFilterConfiguration()
        {
            CraftListDefault = true;
            DestinationAllCharacters = true;
            DestinationIncludeCrossCharacter = false;
            SourceAllCharacters = false;
            SourceAllRetainers = true;
            SourceAllFreeCompanies = true;
            SourceIncludeCrossCharacter = false;
            HighlightWhen = "Always";
            SourceCategories = new HashSet<InventoryCategory>()
            {
                InventoryCategory.FreeCompanyBags,
                InventoryCategory.CharacterSaddleBags,
                InventoryCategory.CharacterPremiumSaddleBags,
            };
        }

        public void AddDefaultColumns()
        {
            if (FilterType == FilterType.SearchFilter)
            {
                Columns = new List<string>();
                Columns.Add("IconColumn");
                Columns.Add("NameColumn");
                Columns.Add("TypeColumn");
                Columns.Add("QuantityColumn");
                Columns.Add("SourceColumn");
                Columns.Add("LocationColumn");
            }
            else if (FilterType == FilterType.SortingFilter)
            {
                Columns = new List<string>();
                Columns.Add("IconColumn");
                Columns.Add("NameColumn");
                Columns.Add("TypeColumn");
                Columns.Add("QuantityColumn");
                Columns.Add("SourceColumn");
                Columns.Add("LocationColumn");
                Columns.Add("DestinationColumn");
            }
            else if (FilterType == FilterType.GameItemFilter)
            {
                Columns = new List<string>();
                Columns.Add("IconColumn");
                Columns.Add("NameColumn");
                Columns.Add("UiCategoryColumn");
                Columns.Add("SearchCategoryColumn");
                Columns.Add("ItemILevelColumn");
                Columns.Add("ItemLevelColumn");
                Columns.Add("RarityColumn");
                Columns.Add("CraftColumn");
                Columns.Add("IsCraftingItemColumn");
                Columns.Add("CanBeGatheredColumn");
                Columns.Add("CanBePurchasedColumn");
                Columns.Add("AcquiredColumn");
                Columns.Add("SellToVendorPriceColumn");
                Columns.Add("BuyFromVendorPriceColumn");
                Columns.Add("AcquisitionSourceIconsColumn");
            }
            else if (FilterType == FilterType.CraftFilter)
            {
                Columns = new List<string>();
                Columns.Add("IconColumn");
                Columns.Add("NameColumn");
                Columns.Add("CraftAmountAvailableColumn");
                Columns.Add("QuantityColumn");
                Columns.Add("SourceColumn");
                Columns.Add("LocationColumn");
                
                CraftColumns = new List<string>();
                AddCraftColumn("IconColumn");
                AddCraftColumn("NameColumn");
                AddCraftColumn("CraftAmountRequiredColumn");
                AddCraftColumn("CraftSettingsColumn");
                AddCraftColumn("CraftSimpleColumn");
                AddCraftColumn("MarketBoardMinPriceColumn");
                AddCraftColumn("MarketBoardMinTotalPriceColumn");
                AddCraftColumn("AcquisitionSourceIconsColumn");
                AddCraftColumn("CraftGatherColumn");
            }
            else if (FilterType == FilterType.HistoryFilter)
            {
                Columns = new List<string>();
                Columns.Add("IconColumn");
                Columns.Add("NameColumn");
                Columns.Add("HistoryChangeAmountColumn");
                Columns.Add("HistoryChangeReasonColumn");
                Columns.Add("HistoryChangeDateColumn");
                Columns.Add("TypeColumn");
                Columns.Add("QuantityColumn");
                Columns.Add("SourceColumn");
                Columns.Add("LocationColumn");
            }
        }

        public FilterConfiguration()
        {
        }

        public void Refresh()
        {
            
        }

        public void ResetDefaultCraftFilter()
        {
            CraftColumns = new List<string>();
            Columns = new List<string>();
            foreach (var filter in PluginService.PluginLogic.AvailableFilters)
            {
                if (filter.AvailableIn.HasFlag(FilterType.CraftFilter))
                {
                    filter.ResetFilter(this);
                }
            }
            AddDefaultColumns();
            ApplyDefaultCraftFilterConfiguration();
        }

        public void ResetCraftFilter()
        {
            var defaultConfiguration = PluginService.FilterService.GetDefaultCraftList();
            if (defaultConfiguration != null)
            {
                if (this == defaultConfiguration)
                {
                    ResetDefaultCraftFilter();
                    return;
                }
                CraftColumns = new List<string>();
                Columns = new List<string>();
                foreach (var filter in PluginService.PluginLogic.AvailableFilters)
                {
                    if (filter.AvailableIn.HasFlag(FilterType.CraftFilter))
                    {
                        filter.ResetFilter(defaultConfiguration, this);
                    }
                }
            }
        }

        public FilterTable GenerateTable()
        {
            FilterTable table = new FilterTable(this);
            table.RefreshColumns();
            table.ShowFilterRow = true;
            return table;
        }

        public CraftItemTable GenerateCraftTable()
        {
            CraftItemTable table = new CraftItemTable(this);
            table.RefreshColumns();
            table.ShowFilterRow = false;
            return table;
        }


        public List<(ulong, InventoryCategory)> SourceInventories
        {
            get => _sourceInventories;
            set { _sourceInventories = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public List<uint> ItemUiCategoryId
        {
            get => _itemUiCategoryId;
            set { _itemUiCategoryId = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public List<uint> ItemSearchCategoryId
        {
            get => _itemSearchCategoryId;
            set { _itemSearchCategoryId = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public List<uint> EquipSlotCategoryId
        {
            get => _equipSlotCategoryId;
            set { _equipSlotCategoryId = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public int TableHeight
        {
            get => _tableHeight;
            set { _tableHeight = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { TableConfigurationChanged?.Invoke(this); });
            }
        }


        public int CraftTableHeight
        {
            get => _craftTableHeight;
            set { _craftTableHeight = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public List<uint> ItemSortCategoryId
        {
            get => _itemSortCategoryId;
            set { _itemSortCategoryId = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public List<(ulong, InventoryCategory)> DestinationInventories
        {
            get => _destinationInventories;
            set { _destinationInventories = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public bool? IsHq
        {
            get => _isHq;
            set { _isHq = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public bool? IsCollectible
        {
            get => _isCollectible;
            set { _isCollectible = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public string Name
        {
            get => _name == null ? "" :  _name;
            set {
                unsafe
                {
                    _name = value;
                    _nameAsBytes = null;
                    PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
                }
            }
        }

        [JsonIgnore]
        public byte[] NameAsBytes
        {
            get
            {
                if (_nameAsBytes == null)
                {
                    _nameAsBytes = System.Text.Encoding.UTF8.GetBytes(Name == "" ? "Untitled" : Name);
                }

                return _nameAsBytes;
            }
        }

        private byte[]? _nameAsBytes;

        public bool? DuplicatesOnly
        {
            get => _duplicatesOnly;
            set { _duplicatesOnly = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        [Obsolete]
        public bool? FilterItemsInRetainers
        {
            get => _filterItemsInRetainers;
            set { _filterItemsInRetainers = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public FilterItemsRetainerEnum FilterItemsInRetainersEnum
        {
            get
            {
                if (_filterItemsInRetainersEnum == null)
                {
                    _filterItemsInRetainersEnum = FilterItemsRetainerEnum.No;
                }
                return _filterItemsInRetainersEnum.Value;
            }
            set { _filterItemsInRetainersEnum = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public bool DisplayInTabs
        {
            get => _displayInTabs;
            set { _displayInTabs = value;
                ConfigurationChanged?.Invoke(this, true);
            }
        }

        public bool OpenAsWindow
        {
            get => _openAsWindow;
            set { _openAsWindow = value;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public string Quantity
        {
            get => _quantity;
            set { _quantity = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public string ILevel
        {
            get => _iLevel;
            set { _iLevel = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public string Spiritbond
        {
            get => _spiritbond;
            set { _spiritbond = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public string NameFilter
        {
            get => _nameFilter;
            set { _nameFilter = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public FilterType FilterType
        {
            get => _filterType;
            set => _filterType = value;
        }

        public string Key
        {
            get => _key;
            set => _key = value;
        }

        public bool? SourceAllRetainers
        {
            get => _sourceAllRetainers;
            set { _sourceAllRetainers = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }            
        }

        public bool? SourceAllHouses
        {
            get => _sourceAllHouses;
            set { _sourceAllHouses = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }            
        }

        public bool? SourceAllFreeCompanies
        {
            get => _sourceAllFreeCompanies;
            set { _sourceAllFreeCompanies = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }            
        }
        
        public string? HighlightWhen
        {
            get => _highlightWhen;
            set
            {
                _highlightWhen = value;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        
        public bool? UseORFiltering
        {
            get => _useORFiltering;
            set
            {
                _useORFiltering = value;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public bool? SourceAllCharacters
        {
            get => _sourceAllCharacters;
            set { _sourceAllCharacters = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }            
        }

        public bool? DestinationAllRetainers
        {
            get => _destinationAllRetainers;
            set { _destinationAllRetainers = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }            
        }

        public bool? DestinationAllFreeCompanies
        {
            get => _destinationAllFreeCompanies;
            set { _destinationAllFreeCompanies = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }            
        }

        public bool? DestinationAllHouses
        {
            get => _destinationAllHouses;
            set { _destinationAllHouses = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }            
        }

        public bool? SourceIncludeCrossCharacter
        {
            get => _sourceIncludeCrossCharacter;
            set { _sourceIncludeCrossCharacter = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }            
        }

        public bool? DestinationIncludeCrossCharacter
        {
            get => _destinationIncludeCrossCharacter;
            set { _destinationIncludeCrossCharacter = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }            
        }

        public int? FreezeColumns
        {
            get => _freezeColumns;
            set { _freezeColumns = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { TableConfigurationChanged?.Invoke(this); });
            }            
        }

        public int? FreezeCraftColumns
        {
            get => _freezeCraftColumns;
            set { _freezeCraftColumns = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { TableConfigurationChanged?.Invoke(this); });
            }            
        }

        public HashSet<InventoryCategory>? DestinationCategories
        {
            get => _destinationCategories;
            set { _destinationCategories = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }            
        }

        public HashSet<InventoryCategory>? SourceCategories
        {
            get => _sourceCategories;
            set { _sourceCategories = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }            
        }

        public bool? DestinationAllCharacters
        {
            get => _destinationAllCharacters;
            set { _destinationAllCharacters = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }            
        }
        
        public string ShopSellingPrice
        {
            get => _shopSellingPrice;
            set
            {
                _shopSellingPrice = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });                
            }
        }

        public string ShopBuyingPrice
        {
            get => _shopBuyingPrice;
            set
            {
                _shopBuyingPrice = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public string MarketAveragePrice
        {
            get => _marketAveragePrice;
            set
            {
                _marketAveragePrice = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public string MarketTotalAveragePrice
        {
            get => _marketTotalAveragePrice;
            set
            {
                _marketTotalAveragePrice = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public bool? CanBeBought
        {
            get => _canBeBought;
            set
            {
                _canBeBought = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }
        
        public bool? IsAvailableAtTimedNode
        {
            get => _isAvailableAtTimedNode;
            set
            {
                _isAvailableAtTimedNode = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }            
        }

        public ulong OwnerId
        {
            get => _ownerId;
            set => _ownerId = value;
        }

        public string? Icon
        {
            get => _icon;
            set => _icon = value;
        }
        
        public Vector4? HighlightColor
        {
            get => _highlightColor;
            set
            {
                _highlightColor = value;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }
        
        public Vector4? DestinationHighlightColor
        {
            get => _destinationHighlightColor;
            set
            {
                _destinationHighlightColor = value;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }
        
        public Vector4? RetainerListColor
        {
            get => _retainerListColor;
            set
            {
                _retainerListColor = value;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }
        
        public Vector4? TabHighlightColor
        {
            get => _tabHighlightColor;
            set
            {
                _tabHighlightColor = value;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public bool? InvertHighlighting
        {
            get => _invertHighlighting;
            set
            {
                _invertHighlighting = value;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public bool? InvertDestinationHighlighting
        {
            get => _invertDestinationHighlighting;
            set
            {
                _invertDestinationHighlighting = value;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public bool? InvertTabHighlighting
        {
            get => _invertTabHighlighting;
            set
            {
                _invertTabHighlighting = value;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public bool? HighlightDestination
        {
            get => _highlightDestination;
            set
            {
                _highlightDestination = value;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public bool? HighlightDestinationEmpty
        {
            get => _highlightDestinationEmpty;
            set
            {
                _highlightDestinationEmpty = value;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public bool? IgnoreHQWhenSorting
        {
            get => _ignoreHQWhenSorting;
            set
            {
                _ignoreHQWhenSorting = value;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public bool CraftListDefault
        {
            get => _craftListDefault;
            set
            {
                _craftListDefault = value;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public bool? SimpleCraftingMode
        {
            get => _simpleCraftingMode;
            set
            {
                _simpleCraftingMode = value;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { TableConfigurationChanged?.Invoke(this); });
            }
        }
        
        public HashSet<uint>? SourceWorlds
        {
            get => _sourceWorlds;
            set { _sourceWorlds = value;
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }            
        }

        public event ConfigurationChangedDelegate? ConfigurationChanged;
        public event TableConfigurationChangedDelegate? TableConfigurationChanged;
        public event ListUpdatedDelegate? ListUpdated;

        public bool FilterItem(ItemEx item)
        {
            if (FilterType == FilterType.CraftFilter)
            {
                var requiredMaterialsList = CraftList.BeenUpdated ? CraftList.GetAvailableMaterialsList().Where(c => c.Value != 0).ToDictionary(c => c.Key, c => c.Value) : CraftList.GetRequiredMaterialsList();
                if (!requiredMaterialsList.ContainsKey(item.RowId))
                {
                    return false;
                }
            }

            var matchesAny = false;
            for (var index = 0; index < PluginService.PluginLogic.AvailableFilters.Count; index++)
            {
                var filter = PluginService.PluginLogic.AvailableFilters[index];
                if (UseORFiltering != null && UseORFiltering == true)
                {
                    if (filter.FilterItem(this, (ItemEx)item) == true)
                    {
                        matchesAny = true;
                    }
                }
                else
                {
                    if (filter.FilterItem(this, (ItemEx)item) == false)
                    {
                        return false;
                    }
                }
            }

            if (matchesAny)
            {
                return true;
            }

            return true;
        }

        public bool FilterItem(InventoryChange item)
        {
            var matchesAny = false;
            for (var index = 0; index < PluginService.PluginLogic.AvailableFilters.Count; index++)
            {
                var filter = PluginService.PluginLogic.AvailableFilters[index];
                if (UseORFiltering != null && UseORFiltering == true)
                {
                    if (filter.FilterItem(this, item) == true)
                    {
                        matchesAny = true;
                    }
                }
                else
                {
                    if (filter.FilterItem(this, item) == false)
                    {
                        return false;
                    }
                }
            }

            if (matchesAny)
            {
                return true;
            }

            return true;
        }
        
        public FilteredItem? FilterItem(InventoryItem item)
        {
            //TODO: Make sure this doesn't break shit
            if (item.ItemId == 0)
            {
                return null;
            }
            uint? requiredAmount = null;
            if (FilterType == FilterType.CraftFilter)
            {
                var requiredMaterial = CraftList.GetItemById(item.ItemId, item.IsHQ);
                if (requiredMaterial == null)
                {
                    return null;
                }

                if (CraftList.BeenUpdated)
                {
                    requiredAmount = requiredMaterial.QuantityWillRetrieve;
                }
            }

            var matchesAny = false;
            for (var index = 0; index < PluginService.PluginLogic.AvailableFilters.Count; index++)
            {
                var filter = PluginService.PluginLogic.AvailableFilters[index];
                if (!filter.HasValueSet(this))
                {
                    continue;
                }

                if (UseORFiltering != null && UseORFiltering == true)
                {
                    if (filter.FilterItem(this, item) == true)
                    {
                        matchesAny = true;
                    }
                }
                else
                {
                    if (filter.FilterItem(this, item) == false)
                    {
                        return null;
                    }
                }
            }

            if (UseORFiltering != null && UseORFiltering == true && matchesAny)
            {
                return new FilteredItem(item, requiredAmount);
            }
            else if(UseORFiltering != null && UseORFiltering == true)
            {
                return null;
            }
            
            return new FilteredItem(item, requiredAmount);
        }

        public void AddColumn(string columnName)
        {
            if (_columns == null)
            {
                _columns = new List<string>();
            }

            if (!_columns.Contains(columnName))
            {
                _columns.Add(columnName);
                GenerateNewTableId();
                PluginService.FrameworkService.RunOnFrameworkThread(() => { TableConfigurationChanged?.Invoke(this); });
            }
        }

        public void AddCraftColumn(string columnName, int? index = null)
        {
            if (_craftColumns == null)
            {
                _craftColumns = new List<string>();
            }

            if (!_craftColumns.Contains(columnName))
            {
                if (index != null)
                {
                    _craftColumns.Insert(Math.Min(index.Value,_craftColumns.Count), columnName);
                }
                else
                {
                    _craftColumns.Add(columnName);
                }
                GenerateNewCraftTableId();
                PluginService.FrameworkService.RunOnFrameworkThread(() => { TableConfigurationChanged?.Invoke(this); });
            }
        }
        
        public void UpColumn(string columnName)
        {
            if (this._columns == null)
            {
                return;
            }
            _columns = _columns.MoveUp(columnName);
            GenerateNewTableId();
            PluginService.FrameworkService.RunOnFrameworkThread(() => { TableConfigurationChanged?.Invoke(this); });
        }
        
        public void DownColumn(string columnName)
        {
            if (this._columns == null)
            {
                return;
            }
            _columns = _columns.MoveDown(columnName);
            GenerateNewTableId();
            PluginService.FrameworkService.RunOnFrameworkThread(() => { TableConfigurationChanged?.Invoke(this); });
        }

        public void RemoveColumn(string columnName)
        {
            if (this._columns == null)
            {
                return;
            }
            _columns = _columns.Where(c => c != columnName).ToList();
            GenerateNewTableId();
            PluginService.FrameworkService.RunOnFrameworkThread(() => { TableConfigurationChanged?.Invoke(this); });
        }
        
        public void AddDestinationInventory((ulong, InventoryCategory) inventory)
        {
            if (!DestinationInventories.Contains(inventory))
            {
                DestinationInventories.Add(inventory);
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public void RemoveDestinationInventory((ulong, InventoryCategory) inventory)
        {
            if (DestinationInventories.Contains(inventory))
            {
                DestinationInventories.Remove(inventory);
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public void AddSourceInventory((ulong, InventoryCategory) inventory)
        {
            if (!SourceInventories.Contains(inventory))
            {
                SourceInventories.Add(inventory);
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public void RemoveSourceInventory((ulong, InventoryCategory) inventory)
        {
            if (SourceInventories.Contains(inventory))
            {
                SourceInventories.Remove(inventory);
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public void AddItemUiCategory(uint itemUiCategoryId)
        {
            if (!ItemUiCategoryId.Contains(itemUiCategoryId))
            {
                ItemUiCategoryId.Add(itemUiCategoryId);
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public void RemoveItemUiCategory(uint itemUiCategoryId)
        {
            if (ItemUiCategoryId.Contains(itemUiCategoryId))
            {
                ItemUiCategoryId.Remove(itemUiCategoryId);
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public void AddItemSearchCategory(uint itemSearchCategoryId)
        {
            if (!ItemSearchCategoryId.Contains(itemSearchCategoryId))
            {
                ItemSearchCategoryId.Add(itemSearchCategoryId);
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public void RemoveItemSearchCategory(uint itemSearchCategoryId)
        {
            if (ItemSearchCategoryId.Contains(itemSearchCategoryId))
            {
                ItemSearchCategoryId.Remove(itemSearchCategoryId);
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        [JsonIgnore]
        public string FormattedFilterType
        {
            get
            {
                if (FilterType.HasFlag(FilterType.SearchFilter))
                {
                    return "Search Filter";
                }
                else if (FilterType.HasFlag(FilterType.SortingFilter))
                {
                    return "Sort Filter";
                }
                else if (FilterType.HasFlag(FilterType.GameItemFilter))
                {
                    return "Game Item Filter";
                }
                else if (FilterType.HasFlag(FilterType.CraftFilter))
                {
                    return "Craft List";
                }
                return "";
            }
        }

        public List<string>? Columns
        {
            get => _columns;
            set
            {
                _columns = value;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { TableConfigurationChanged?.Invoke(this); });
            }
        }

        public List<string>? CraftColumns
        {
            get => _craftColumns;
            set
            {
                _craftColumns = value;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { TableConfigurationChanged?.Invoke(this); });
            }
        }

        public bool? GetBooleanFilter(string key)
        {
            if (BooleanFilters.ContainsKey(key))
            {
                return BooleanFilters[key];
            }

            return null;
        }

        public Vector4? GetColorFilter(string key)
        {
            if (ColorFilters.ContainsKey(key))
            {
                return ColorFilters[key];
            }

            return null;
        }

        public string GetStringFilter(string key)
        {
            if (StringFilters.ContainsKey(key))
            {
                return StringFilters[key];
            }

            return "";
        }

        public int? GetIntegerFilter(string key)
        {
            if (IntegerFilters.ContainsKey(key))
            {
                return (int?)IntegerFilters[key];
            }

            return null;
        }

        public int? GetDecimalFilter(string key)
        {
            if (DecimalFilters.ContainsKey(key))
            {
                return (int?)DecimalFilters[key];
            }

            return null;
        }

        public List<uint> GetUintChoiceFilter(string key)
        {
            if (UintChoiceFilters.ContainsKey(key))
            {
                return UintChoiceFilters[key];
            }

            return new List<uint>();
        }

        public uint? GetUintFilter(string key)
        {
            if (UintFilters.ContainsKey(key))
            {
                return UintFilters[key];
            }

            return null;
        }

        public List<ulong> GetUlongChoiceFilter(string key)
        {
            if (UlongChoiceFilters.ContainsKey(key))
            {
                return UlongChoiceFilters[key];
            }

            return new List<ulong>();
        }

        public List<string> GetStringChoiceFilter(string key)
        {
            if (StringChoiceFilters.ContainsKey(key))
            {
                return StringChoiceFilters[key];
            }

            return new List<string>();
        }

        public void UpdateBooleanFilter(string key, bool value)
        {
            if (BooleanFilters.ContainsKey(key) && BooleanFilters[key] == value)
            {
                return;
            }

            BooleanFilters[key] = value;
            NeedsRefresh = true;
            PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
        }

        public void UpdateColorFilter(string key, Vector4 value)
        {
            if (ColorFilters.ContainsKey(key) && ColorFilters[key] == value)
            {
                return;
            }

            ColorFilters[key] = value;
            NeedsRefresh = true;
            PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
        }

        public void RemoveBooleanFilter(string key)
        {
            if (BooleanFilters.ContainsKey(key))
            {
                BooleanFilters.Remove(key);
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public void RemoveColorFilter(string key)
        {
            if (ColorFilters.ContainsKey(key))
            {
                ColorFilters.Remove(key);
                NeedsRefresh = true;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public void UpdateStringFilter(string key, string value)
        {
            if (StringFilters.ContainsKey(key) && StringFilters[key] == value)
            {
                return;
            }

            StringFilters[key] = value;
            NeedsRefresh = true;
            PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
        }

        public void UpdateIntegerFilter(string key, int? value)
        {
            if (IntegerFilters.ContainsKey(key) && IntegerFilters[key] == value)
            {
                return;
            }
            if (IntegerFilters.ContainsKey(key) && value == null)
            {
                IntegerFilters.Remove(key);
            }
            else if (value != null)
            {
                IntegerFilters[key] = value.Value;
            }

            NeedsRefresh = true;
            PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
        }

        public void UpdateDecimalFilter(string key, decimal? value)
        {
            if (DecimalFilters.ContainsKey(key) && DecimalFilters[key] == value)
            {
                return;
            }
            if (DecimalFilters.ContainsKey(key) && value == null)
            {
                DecimalFilters.Remove(key);
            }
            else if (value != null)
            {
                DecimalFilters[key] = value.Value;
            }

            NeedsRefresh = true;
            PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
        }

        public void UpdateUintChoiceFilter(string key, List<uint> value)
        {
            UintChoiceFilters[key] = value;
            NeedsRefresh = true;
            PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
        }

        public void UpdateUintFilter(string key, uint? value)
        {
            if (value == null && UintFilters.ContainsKey(key))
            {
                UintFilters.Remove(key);
            }
            else if(value != null)
            {
                UintFilters[key] = value.Value;
            }
            NeedsRefresh = true;
            PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
        }

        public void UpdateUlongChoiceFilter(string key, List<ulong> value)
        {
            UlongChoiceFilters[key] = value;
            NeedsRefresh = true;
            PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
        }

        public void UpdateStringChoiceFilter(string key, List<string> value)
        {
            StringChoiceFilters[key] = value;
            NeedsRefresh = true;
            PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
        }

        public Dictionary<string, bool> BooleanFilters
        {
            get
            {
                if (_booleanFilters == null)
                {
                    _booleanFilters = new();
                }
                return _booleanFilters;
            }
            set => _booleanFilters = value;
        }

        public Dictionary<string, Vector4> ColorFilters
        {
            get
            {
                if (_colorFilters == null)
                {
                    _colorFilters = new();
                }
                return _colorFilters;
            }
            set => _colorFilters = value;
        }


        public Dictionary<string, string> StringFilters
        {
            get
            {
                if (_stringFilters == null)
                {
                    _stringFilters = new();
                }
                return _stringFilters;
            }
            set => _stringFilters = value;
        }


        public Dictionary<string, int> IntegerFilters
        {
            get
            {
                if (_integerFilters == null)
                {
                    _integerFilters = new();
                }
                return _integerFilters;
            }
            set => _integerFilters = value;
        }


        public Dictionary<string, decimal> DecimalFilters
        {
            get
            {
                if (_decimalFilters == null)
                {
                    _decimalFilters = new();
                }
                return _decimalFilters;
            }
            set => _decimalFilters = value;
        }


        public Dictionary<string, uint> UintFilters
        {
            get
            {
                if (_uintFilters == null)
                {
                    _uintFilters = new();
                }
                return _uintFilters;
            }
            set => _uintFilters = value;
        }

        public Dictionary<string, List<uint>> UintChoiceFilters
        {
            get
            {
                if (_uintChoiceFilters == null)
                {
                    _uintChoiceFilters = new Dictionary<string, List<uint>>();
                }
                return _uintChoiceFilters;
            }
            set => _uintChoiceFilters = value;
        }

        public Dictionary<string, List<ulong>> UlongChoiceFilters
        {
            get
            {
                if (_ulongChoiceFilters == null)
                {
                    _ulongChoiceFilters = new Dictionary<string, List<ulong>>();
                }
                return _ulongChoiceFilters;
            }
            set => _ulongChoiceFilters = value;
        }

        public Dictionary<string, List<string>> StringChoiceFilters
        {
            get
            {
                if (_stringChoiceFilters == null)
                {
                    _stringChoiceFilters = new Dictionary<string, List<string>>();
                }
                return _stringChoiceFilters;
            }
            set => _stringChoiceFilters = value;
        }

        public bool InActiveInventories(ulong activeCharacterId, ulong activeRetainerId, ulong sourceCharacterId,
            ulong destinationCharacterId)
        {
            if (FilterItemsInRetainersEnum is FilterItemsRetainerEnum.Yes or FilterItemsRetainerEnum.Only && activeRetainerId != 0)
            {
                //When the active character is the source and the active retainer is the destination
                if (activeCharacterId == sourceCharacterId && activeRetainerId == destinationCharacterId)
                {
                    return true;
                }
                //When the active retainer is the source and the destination is elsewhere
                if (activeRetainerId == sourceCharacterId && activeRetainerId != destinationCharacterId)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        [JsonIgnore]
        public Dictionary<ulong, HashSet<InventoryCategory>> DestinationRetainerCategories
        {
            get
            {
                var categoryValues = Enum.GetValues<InventoryCategory>();
                
                Dictionary<ulong, HashSet<InventoryCategory>> categories = new();
                var allRetainers = PluginService.CharacterMonitor.GetRetainerCharacters().Where(c =>
                {
                    var destinationIncludeCrossCharacter = DestinationIncludeCrossCharacter ?? ConfigurationManager.Config.DisplayCrossCharacter;
                    return PluginService.CharacterMonitor.BelongsToActiveCharacter(c.Key) || destinationIncludeCrossCharacter;
                }).ToDictionary(c => c.Key, c => c.Value);
                if (DestinationAllRetainers == true)
                {
                    foreach (var retainer in allRetainers)
                    {
                        foreach (var categoryValue in categoryValues)
                        {
                            if (categoryValue.IsRetainerCategory())
                            {
                                if (!categories.ContainsKey(retainer.Key))
                                {
                                    categories.Add(retainer.Key, new HashSet<InventoryCategory>());
                                }

                                if (!categories[retainer.Key].Contains(categoryValue))
                                {
                                    categories[retainer.Key].Add(categoryValue);
                                }
                            }
                        }
                    }
                }

                if (DestinationCategories != null)
                {
                    foreach (var categoryValue in DestinationCategories)
                    {
                        foreach (var retainer in allRetainers)
                        {
                            if (categoryValue.IsRetainerCategory())
                            {
                                if (!categories.ContainsKey(retainer.Key))
                                {
                                    categories.Add(retainer.Key, new HashSet<InventoryCategory>());
                                }
                                if (!categories[retainer.Key].Contains(categoryValue))
                                {
                                    categories[retainer.Key].Add(categoryValue);
                                }
                            }
                        }
                    }
                }

                foreach (var category in DestinationInventories)
                {
                    if (category.Item2.IsRetainerCategory())
                    {
                        if (!categories.ContainsKey(category.Item1))
                        {
                            categories.Add(category.Item1, new HashSet<InventoryCategory>());
                        }
                        
                        if (!categories[category.Item1].Contains( category.Item2))
                        {
                            categories[category.Item1].Add( category.Item2);
                        }
                    }
                }

                return categories;
            }
        }
        
        
        [JsonIgnore]
        public Dictionary<ulong, HashSet<InventoryCategory>> DestinationFreeCompanyCategories
        {
            get
            {
                var categoryValues = Enum.GetValues<InventoryCategory>();
                
                Dictionary<ulong, HashSet<InventoryCategory>> categories = new();
                var allFreeCompanies = PluginService.CharacterMonitor.GetFreeCompanies().Where(c =>
                {
                    var destinationIncludeCrossCharacter = DestinationIncludeCrossCharacter ?? ConfigurationManager.Config.DisplayCrossCharacter;
                    return PluginService.CharacterMonitor.BelongsToActiveCharacter(c.Key) || destinationIncludeCrossCharacter;
                }).ToDictionary(c => c.Key, c => c.Value);
                
                if (DestinationAllRetainers == true)
                {
                    foreach (var retainer in allFreeCompanies)
                    {
                        foreach (var categoryValue in categoryValues)
                        {
                            if (categoryValue.IsFreeCompanyCategory())
                            {
                                if (!categories.ContainsKey(retainer.Key))
                                {
                                    categories.Add(retainer.Key, new HashSet<InventoryCategory>());
                                }

                                if (!categories[retainer.Key].Contains(categoryValue))
                                {
                                    categories[retainer.Key].Add(categoryValue);
                                }
                            }
                        }
                    }
                }
                
                if (DestinationCategories != null)
                {
                    foreach (var categoryValue in DestinationCategories)
                    {
                        foreach (var freeCompany in allFreeCompanies)
                        {
                            if (categoryValue.IsFreeCompanyCategory())
                            {
                                if (!categories.ContainsKey(freeCompany.Key))
                                {
                                    categories.Add(freeCompany.Key, new HashSet<InventoryCategory>());
                                }
                                if (!categories[freeCompany.Key].Contains(categoryValue))
                                {
                                    categories[freeCompany.Key].Add(categoryValue);
                                }
                            }
                        }
                    }
                }

                foreach (var category in DestinationInventories)
                {
                    if (category.Item2.IsFreeCompanyCategory())
                    {
                        if (!categories.ContainsKey(category.Item1))
                        {
                            categories.Add(category.Item1, new HashSet<InventoryCategory>());
                        }
                        
                        if (!categories[category.Item1].Contains( category.Item2))
                        {
                            categories[category.Item1].Add( category.Item2);
                        }
                    }
                }

                return categories;
            }
        }
        [JsonIgnore]
        public Dictionary<ulong, HashSet<InventoryCategory>> DestinationHouseCategories
        {
            get
            {
                var categoryValues = Enum.GetValues<InventoryCategory>();
                
                Dictionary<ulong, HashSet<InventoryCategory>> categories = new();
                var allHouses = PluginService.CharacterMonitor.GetHouses().Where(c =>
                {
                    var destinationIncludeCrossCharacter = DestinationIncludeCrossCharacter ?? ConfigurationManager.Config.DisplayCrossCharacter;
                    return PluginService.CharacterMonitor.BelongsToActiveCharacter(c.Key) || destinationIncludeCrossCharacter;
                }).ToDictionary(c => c.Key, c => c.Value);
                
                if (DestinationAllHouses == true)
                {
                    foreach (var house in allHouses)
                    {
                        foreach (var categoryValue in categoryValues)
                        {
                            if (categoryValue.IsHousingCategory())
                            {
                                if (!categories.ContainsKey(house.Key))
                                {
                                    categories.Add(house.Key, new HashSet<InventoryCategory>());
                                }

                                if (!categories[house.Key].Contains(categoryValue))
                                {
                                    categories[house.Key].Add(categoryValue);
                                }
                            }
                        }
                    }
                }
                
                if (DestinationCategories != null)
                {
                    foreach (var categoryValue in DestinationCategories)
                    {
                        foreach (var house in allHouses)
                        {
                            if (categoryValue.IsHousingCategory())
                            {
                                if (!categories.ContainsKey(house.Key))
                                {
                                    categories.Add(house.Key, new HashSet<InventoryCategory>());
                                }
                                if (!categories[house.Key].Contains(categoryValue))
                                {
                                    categories[house.Key].Add(categoryValue);
                                }
                            }
                        }
                    }
                }

                foreach (var category in DestinationInventories)
                {
                    if (category.Item2.IsHousingCategory())
                    {
                        if (!categories.ContainsKey(category.Item1))
                        {
                            categories.Add(category.Item1, new HashSet<InventoryCategory>());
                        }
                        
                        if (!categories[category.Item1].Contains( category.Item2))
                        {
                            categories[category.Item1].Add( category.Item2);
                        }
                    }
                }

                return categories;
            }
        }

        [JsonIgnore]
        public Dictionary<ulong, HashSet<InventoryCategory>> DestinationCharacterCategories
        {
            get
            {
                var categoryValues = Enum.GetValues<InventoryCategory>();
                
                Dictionary<ulong, HashSet<InventoryCategory>> categories = new();
                var allCharacters = PluginService.CharacterMonitor.GetPlayerCharacters().Where(c =>
                {
                    var destinationIncludeCrossCharacter = DestinationIncludeCrossCharacter ?? ConfigurationManager.Config.DisplayCrossCharacter;
                    return PluginService.CharacterMonitor.BelongsToActiveCharacter(c.Key) || destinationIncludeCrossCharacter;
                }).ToDictionary(c => c.Key, c => c.Value);
                if (DestinationAllCharacters == true)
                {
                    foreach (var character in allCharacters)
                    {
                        foreach (var categoryValue in categoryValues)
                        {
                            if (categoryValue.IsCharacterCategory())
                            {
                                if (!categories.ContainsKey(character.Key))
                                {
                                    categories.Add(character.Key, new HashSet<InventoryCategory>());
                                }
                                if (!categories[character.Key].Contains(categoryValue))
                                {
                                    categories[character.Key].Add(categoryValue);
                                }
                            }
                        }
                    }
                }

                if (DestinationCategories != null)
                {
                    foreach (var categoryValue in DestinationCategories)
                    {
                        foreach (var character in allCharacters)
                        {
                            if (categoryValue.IsCharacterCategory())
                            {
                                if (!categories.ContainsKey(character.Key))
                                {
                                    categories.Add(character.Key, new HashSet<InventoryCategory>());
                                }
                                if (!categories[character.Key].Contains(categoryValue))
                                {
                                    categories[character.Key].Add(categoryValue);
                                }
                            }
                        }
                    }
                }

                foreach (var category in DestinationInventories)
                {
                    if (category.Item2.IsCharacterCategory())
                    {
                        if (!categories.ContainsKey(category.Item1))
                        {
                            categories.Add(category.Item1, new HashSet<InventoryCategory>());
                        }
                        if (!categories[category.Item1].Contains( category.Item2))
                        {
                            categories[category.Item1].Add( category.Item2);
                        }
                    }
                }

                return categories;
            }
        }
        
        [JsonIgnore]
        public Dictionary<ulong, HashSet<InventoryCategory>> SourceRetainerCategories
        {
            get
            {
                var categoryValues = Enum.GetValues<InventoryCategory>();
                
                Dictionary<ulong, HashSet<InventoryCategory>> categories = new();
                var allRetainers = PluginService.CharacterMonitor.GetRetainerCharacters().Where(c =>
                {
                    var sourceIncludeCrossCharacter = SourceIncludeCrossCharacter ?? ConfigurationManager.Config.DisplayCrossCharacter;
                    return PluginService.CharacterMonitor.BelongsToActiveCharacter(c.Key) || sourceIncludeCrossCharacter;
                }).ToDictionary(c => c.Key, c => c.Value);
                if (SourceAllRetainers == true)
                {
                    foreach (var retainer in allRetainers)
                    {
                        foreach (var categoryValue in categoryValues)
                        {
                            if (categoryValue.IsRetainerCategory())
                            {
                                if (!categories.ContainsKey(retainer.Key))
                                {
                                    categories.Add(retainer.Key, new HashSet<InventoryCategory>());
                                }
                                if (!categories[retainer.Key].Contains(categoryValue))
                                {
                                    categories[retainer.Key].Add(categoryValue);
                                }
                            }
                        }
                    }
                }

                if (SourceCategories != null)
                {
                    foreach (var categoryValue in SourceCategories)
                    {
                        foreach (var retainer in allRetainers)
                        {
                            if (categoryValue.IsRetainerCategory())
                            {
                                if (!categories.ContainsKey(retainer.Key))
                                {
                                    categories.Add(retainer.Key, new HashSet<InventoryCategory>());
                                }
                                if (!categories[retainer.Key].Contains(categoryValue))
                                {
                                    categories[retainer.Key].Add(categoryValue);
                                }
                            }
                        }
                    }
                }

                foreach (var category in SourceInventories)
                {
                    if (category.Item2.IsRetainerCategory())
                    {
                        if (!categories.ContainsKey(category.Item1))
                        {
                            categories.Add(category.Item1, new HashSet<InventoryCategory>());
                        }
                        if (!categories[category.Item1].Contains( category.Item2))
                        {
                            categories[category.Item1].Add( category.Item2);
                        }
                    }
                }
                
                
                if (SourceWorlds != null)
                {
                    foreach (var retainer in allRetainers)
                    {
                        if(SourceWorlds.Contains(retainer.Value.WorldId))
                        {
                            foreach (var categoryValue in categoryValues)
                            {
                                if (categoryValue.IsRetainerCategory())
                                {
                                    if (!categories.ContainsKey(retainer.Key))
                                    {
                                        categories.Add(retainer.Key, new HashSet<InventoryCategory>());
                                    }

                                    if (!categories[retainer.Key].Contains(categoryValue))
                                    {
                                        categories[retainer.Key].Add(categoryValue);
                                    }
                                }
                            }
                        }
                    }
                }

                return categories;
            }
        }
        
        
        [JsonIgnore]
        public Dictionary<ulong, HashSet<InventoryCategory>> SourceFreeCompanyCategories
        {
            get
            {
                var categoryValues = Enum.GetValues<InventoryCategory>();
                
                Dictionary<ulong, HashSet<InventoryCategory>> categories = new();

                var allFreeCompanies = PluginService.CharacterMonitor.GetFreeCompanies().Where(c =>
                {
                    var sourceIncludeCrossCharacter = SourceIncludeCrossCharacter ?? ConfigurationManager.Config.DisplayCrossCharacter;
                    return PluginService.CharacterMonitor.BelongsToActiveCharacter(c.Key) || sourceIncludeCrossCharacter;
                    
                }).ToDictionary(c => c.Key, c => c.Value);

                if (SourceAllFreeCompanies == true)
                {
                    foreach (var retainer in allFreeCompanies)
                    {
                        foreach (var categoryValue in categoryValues)
                        {
                            if (categoryValue.IsFreeCompanyCategory())
                            {
                                if (!categories.ContainsKey(retainer.Key))
                                {
                                    categories.Add(retainer.Key, new HashSet<InventoryCategory>());
                                }
                                if (!categories[retainer.Key].Contains(categoryValue))
                                {
                                    categories[retainer.Key].Add(categoryValue);
                                }
                            }
                        }
                    }
                }
                
                if (SourceCategories != null)
                {
                    foreach (var categoryValue in SourceCategories)
                    {
                        foreach (var freeCompany in allFreeCompanies)
                        {
                            if (categoryValue.IsFreeCompanyCategory())
                            {
                                if (!categories.ContainsKey(freeCompany.Key))
                                {
                                    categories.Add(freeCompany.Key, new HashSet<InventoryCategory>());
                                }
                                if (!categories[freeCompany.Key].Contains(categoryValue))
                                {
                                    categories[freeCompany.Key].Add(categoryValue);
                                }
                            }
                        }
                    }
                }

                foreach (var category in SourceInventories)
                {
                    if (category.Item2.IsFreeCompanyCategory())
                    {
                        if (!categories.ContainsKey(category.Item1))
                        {
                            categories.Add(category.Item1, new HashSet<InventoryCategory>());
                        }
                        if (!categories[category.Item1].Contains( category.Item2))
                        {
                            categories[category.Item1].Add( category.Item2);
                        }
                    }
                }
                
                
                if (SourceWorlds != null)
                {
                    foreach (var freeCompany in allFreeCompanies)
                    {
                        if(SourceWorlds.Contains(freeCompany.Value.WorldId))
                        {
                            foreach (var categoryValue in categoryValues)
                            {
                                if (categoryValue.IsFreeCompanyCategory())
                                {
                                    if (!categories.ContainsKey(freeCompany.Key))
                                    {
                                        categories.Add(freeCompany.Key, new HashSet<InventoryCategory>());
                                    }

                                    if (!categories[freeCompany.Key].Contains(categoryValue))
                                    {
                                        categories[freeCompany.Key].Add(categoryValue);
                                    }
                                }
                            }
                        }
                    }
                }

                return categories;
            }
        }
        
        [JsonIgnore]
        public Dictionary<ulong, HashSet<InventoryCategory>> SourceHouseCategories
        {
            get
            {
                var categoryValues = Enum.GetValues<InventoryCategory>();
                
                Dictionary<ulong, HashSet<InventoryCategory>> categories = new();

                var allHouses = PluginService.CharacterMonitor.GetHouses().Where(c =>
                {
                    var sourceIncludeCrossCharacter = SourceIncludeCrossCharacter ?? ConfigurationManager.Config.DisplayCrossCharacter;
                    return PluginService.CharacterMonitor.BelongsToActiveCharacter(c.Key) || sourceIncludeCrossCharacter;
                    
                }).ToDictionary(c => c.Key, c => c.Value);

                if (SourceAllHouses == true)
                {
                    foreach (var house in allHouses)
                    {
                        foreach (var categoryValue in categoryValues)
                        {
                            if (categoryValue.IsHousingCategory())
                            {
                                if (!categories.ContainsKey(house.Key))
                                {
                                    categories.Add(house.Key, new HashSet<InventoryCategory>());
                                }
                                if (!categories[house.Key].Contains(categoryValue))
                                {
                                    categories[house.Key].Add(categoryValue);
                                }
                            }
                        }
                    }
                }
                
                if (SourceCategories != null)
                {
                    foreach (var categoryValue in SourceCategories)
                    {
                        foreach (var categoryId in allHouses)
                        {
                            if (categoryValue.IsHousingCategory())
                            {
                                if (!categories.ContainsKey(categoryId.Key))
                                {
                                    categories.Add(categoryId.Key, new HashSet<InventoryCategory>());
                                }
                                if (!categories[categoryId.Key].Contains(categoryValue))
                                {
                                    categories[categoryId.Key].Add(categoryValue);
                                }
                            }
                        }
                    }
                }

                foreach (var category in SourceInventories)
                {
                    if (category.Item2.IsHousingCategory())
                    {
                        if (!categories.ContainsKey(category.Item1))
                        {
                            categories.Add(category.Item1, new HashSet<InventoryCategory>());
                        }
                        if (!categories[category.Item1].Contains( category.Item2))
                        {
                            categories[category.Item1].Add( category.Item2);
                        }
                    }
                }
                
                
                if (SourceWorlds != null)
                {
                    foreach (var house in allHouses)
                    {
                        if(SourceWorlds.Contains(house.Value.WorldId))
                        {
                            foreach (var categoryValue in categoryValues)
                            {
                                if (categoryValue.IsHousingCategory())
                                {
                                    if (!categories.ContainsKey(house.Key))
                                    {
                                        categories.Add(house.Key, new HashSet<InventoryCategory>());
                                    }

                                    if (!categories[house.Key].Contains(categoryValue))
                                    {
                                        categories[house.Key].Add(categoryValue);
                                    }
                                }
                            }
                        }
                    }
                }

                return categories;
            }
        }

        [JsonIgnore]
        public Dictionary<ulong, HashSet<InventoryCategory>> SourceCharacterCategories
        {
            get
            {
                var categoryValues = Enum.GetValues<InventoryCategory>();
                
                Dictionary<ulong, HashSet<InventoryCategory>> categories = new();
                var allCharacters = PluginService.CharacterMonitor.GetPlayerCharacters().Where(c =>
                {
                    var sourceIncludeCrossCharacter = SourceIncludeCrossCharacter ?? ConfigurationManager.Config.DisplayCrossCharacter;
                    return PluginService.CharacterMonitor.BelongsToActiveCharacter(c.Key) || sourceIncludeCrossCharacter;
                }).ToDictionary(c => c.Key, c => c.Value);
                if (SourceAllCharacters == true)
                {
                    foreach (var character in allCharacters)
                    {
                        foreach (var categoryValue in categoryValues)
                        {
                            if (categoryValue.IsCharacterCategory())
                            {
                                if (!categories.ContainsKey(character.Key))
                                {
                                    categories.Add(character.Key, new HashSet<InventoryCategory>());
                                }
                                if (!categories[character.Key].Contains(categoryValue))
                                {
                                    categories[character.Key].Add(categoryValue);
                                }
                            }
                        }
                    }
                }

                if (SourceCategories != null)
                {
                    foreach (var categoryValue in SourceCategories)
                    {
                        foreach (var character in allCharacters)
                        {
                            if (categoryValue.IsCharacterCategory())
                            {
                                if (!categories.ContainsKey(character.Key))
                                {
                                    categories.Add(character.Key, new HashSet<InventoryCategory>());
                                }
                                if (!categories[character.Key].Contains(categoryValue))
                                {
                                    categories[character.Key].Add(categoryValue);
                                }
                            }
                        }
                    }
                }

                foreach (var category in SourceInventories)
                {
                    if (category.Item2.IsCharacterCategory())
                    {
                        if (!categories.ContainsKey(category.Item1))
                        {
                            categories.Add(category.Item1, new HashSet<InventoryCategory>());
                        }
                        if (!categories[category.Item1].Contains( category.Item2))
                        {
                            categories[category.Item1].Add( category.Item2);
                        }
                    }
                }
                
                if (SourceWorlds != null)
                {
                    foreach (var retainer in allCharacters)
                    {
                        if(SourceWorlds.Contains(retainer.Value.WorldId))
                        {
                            foreach (var categoryValue in categoryValues)
                            {
                                if (categoryValue.IsRetainerCategory())
                                {
                                    if (!categories.ContainsKey(retainer.Key))
                                    {
                                        categories.Add(retainer.Key, new HashSet<InventoryCategory>());
                                    }

                                    if (!categories[retainer.Key].Contains(categoryValue))
                                    {
                                        categories[retainer.Key].Add(categoryValue);
                                    }
                                }
                            }
                        }
                    }
                }

                return categories;
            }
            
        }

        public CraftList CraftList
        {
            get
            {
                if (_craftList == null)
                {
                    _craftList = new CraftList();
                }
                return _craftList;
            }
        }

        public uint Order
        {
            get => _order;
            set { _order = value;
                PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
            }
        }

        public void SetOrder(uint order)
        {
            _order = order;
        }

        public void NotifyConfigurationChange()
        {
            PluginService.FrameworkService.RunOnFrameworkThread(() => { ConfigurationChanged?.Invoke(this); });
        }


        #region Filter Generation
        public FilterResult GenerateFilteredListInternal(FilterConfiguration filter, List<Inventory> inventories)
        {
            var sortedItems = new List<SortingResult>();
            var unsortableItems = new List<InventoryItem>();
            var items = new List<ItemEx>();
            var inventoryHistory = new List<InventoryChange>();
            var characterMonitor = PluginService.CharacterMonitor;
            var activeCharacter = characterMonitor.ActiveCharacterId;
            var activeRetainer = characterMonitor.ActiveRetainer;
            var displaySourceCrossCharacter = filter.SourceIncludeCrossCharacter ?? ConfigurationManager.Config.DisplayCrossCharacter;
            var displayDestinationCrossCharacter = filter.DestinationIncludeCrossCharacter ?? ConfigurationManager.Config.DisplayCrossCharacter;
            
            PluginLog.Verbose("Filter Information:");
            PluginLog.Verbose("Filter Type: " + filter.FilterType);

            if (filter.FilterType == FilterType.SortingFilter || filter.FilterType == FilterType.CraftFilter)
            {
                //Determine which source and destination inventories we actually need to examine
                Dictionary<(ulong, InventoryType), List<InventoryItem>> sourceInventories = new();
                Dictionary<(ulong, InventoryType), List<InventoryItem>> destinationInventories = new();
                foreach (var character in inventories)
                {
                    foreach (var inventory in character.GetAllInventoriesByType())
                    {
                        var type = inventory.Key;
                        var inventoryKey = (character.CharacterId, type);
                        if (filter.SourceAllRetainers.HasValue && filter.SourceAllRetainers.Value &&
                            characterMonitor.IsRetainer(character.CharacterId) && (displaySourceCrossCharacter ||
                                                                           characterMonitor
                                                                               .BelongsToActiveCharacter(
                                                                                   character.CharacterId)))
                        {
                            if (!sourceInventories.ContainsKey(inventoryKey))
                            {
                                sourceInventories.Add(inventoryKey, inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
                            }
                        }

                        if (filter.SourceAllCharacters.HasValue && filter.SourceAllCharacters.Value &&
                            characterMonitor.IsCharacter(character.CharacterId) &&
                            characterMonitor.ActiveCharacterId == character.CharacterId && (displaySourceCrossCharacter ||
                                characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                        {
                            if (inventoryKey.Item2.ToInventoryCategory() is not InventoryCategory.FreeCompanyBags &&
                                !sourceInventories.ContainsKey(inventoryKey))
                            {
                                sourceInventories.Add(inventoryKey, inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
                            }
                        }
                        
                        if (filter.SourceAllFreeCompanies.HasValue && filter.SourceAllFreeCompanies.Value &&
                            characterMonitor.IsFreeCompany(character.CharacterId) && (displaySourceCrossCharacter ||
                                characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                        {
                            if (!sourceInventories.ContainsKey(inventoryKey))
                            {
                                sourceInventories.Add(inventoryKey, inventory.Value.Where(c => c != null &&  c.SortedContainer == type).ToList()!);
                            }
                        }
                        
                        if (filter.SourceAllHouses.HasValue && filter.SourceAllHouses.Value &&
                            characterMonitor.IsHousing(character.CharacterId) && (displaySourceCrossCharacter ||
                                characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                        {
                            if (!sourceInventories.ContainsKey(inventoryKey))
                            {
                                sourceInventories.Add(inventoryKey, inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
                            }
                        }

                        if (filter.SourceInventories.Contains((character.CharacterId, inventoryKey.type.ToInventoryCategory())) && (displaySourceCrossCharacter ||
                            characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                        {

                            if (!sourceInventories.ContainsKey(inventoryKey))
                            {
                                sourceInventories.Add(inventoryKey, inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
                            }
                        }

                        if (filter.SourceCategories != null &&
                            filter.SourceCategories.Contains(inventoryKey.Item2.ToInventoryCategory()) && (displaySourceCrossCharacter ||
                                characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                        {
                            if (!sourceInventories.ContainsKey(inventoryKey))
                            {
                                sourceInventories.Add(inventoryKey, inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
                            }
                        }

                        if (filter.SourceWorlds != null && filter.SourceWorlds.Contains(characterMonitor.GetCharacterById(character.CharacterId)?.WorldId ?? 0))
                        {
                            if (!sourceInventories.ContainsKey(inventoryKey))
                            {
                                sourceInventories.Add(inventoryKey, inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
                            }
                        }

                        if (inventoryKey.Item2.ToInventoryCategory() is InventoryCategory.CharacterEquipped or InventoryCategory
                                .RetainerEquipped or InventoryCategory.RetainerMarket or InventoryCategory.Currency
                            or
                            InventoryCategory.Crystals)
                        {
                            continue;
                        }

                        if (filter.DestinationAllRetainers.HasValue && filter.DestinationAllRetainers.Value &&
                            characterMonitor.IsRetainer(character.CharacterId) && (displayDestinationCrossCharacter ||
                                                                           characterMonitor
                                                                               .BelongsToActiveCharacter(
                                                                                   character.CharacterId)))
                        {
                            if (!destinationInventories.ContainsKey(inventoryKey))
                            {
                                destinationInventories.Add(inventoryKey, inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
                            }
                        }

                        if (filter.DestinationAllFreeCompanies.HasValue && filter.DestinationAllFreeCompanies.Value &&
                            characterMonitor.IsFreeCompany(character.CharacterId) &&
                            (displayDestinationCrossCharacter ||
                             characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                        {
                            if (!destinationInventories.ContainsKey(inventoryKey))
                            {
                                destinationInventories.Add(inventoryKey, inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
                            }
                        }

                        if (filter.DestinationAllHouses.HasValue && filter.DestinationAllHouses.Value &&
                            characterMonitor.IsHousing(character.CharacterId) &&
                            (displayDestinationCrossCharacter ||
                             characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                        {
                            if (!destinationInventories.ContainsKey(inventoryKey))
                            {
                                destinationInventories.Add(inventoryKey, inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
                            }
                        }

                        if (filter.DestinationAllCharacters.HasValue && filter.DestinationAllCharacters.Value &&
                            characterMonitor.ActiveCharacterId == character.CharacterId &&
                            (displayDestinationCrossCharacter ||
                             characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                        {
                            if (!destinationInventories.ContainsKey(inventoryKey))
                            {
                                destinationInventories.Add(inventoryKey, inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
                            }
                        }

                        if (filter.DestinationInventories.Contains((character.CharacterId,inventoryKey.type.ToInventoryCategory())) &&
                            (displayDestinationCrossCharacter ||
                             characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                        {
                            if (!destinationInventories.ContainsKey(inventoryKey))
                            {
                                destinationInventories.Add(inventoryKey, inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
                            }
                        }

                        if (filter.DestinationCategories != null &&
                            filter.DestinationCategories.Contains(inventory.Key.ToInventoryCategory()) &&
                            (displayDestinationCrossCharacter ||
                             characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                        {
                            if (!destinationInventories.ContainsKey(inventoryKey))
                            {
                                destinationInventories.Add(inventoryKey, inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
                            }
                        }
                    }
                }

                //Filter the source and destination inventories based on the applicable items so we have less to sort
                Dictionary<(ulong, InventoryType), List<FilteredItem>> filteredSources = new();
                //Dictionary<(ulong, InventoryCategory), List<InventoryItem>> filteredDestinations = new();
                var sourceKeys = sourceInventories.Select(c => c.Key);
                PluginLog.Verbose(sourceInventories.Count() + " inventories to examine.");
                foreach (var sourceInventory in sourceInventories)
                {
                    if (!filteredSources.ContainsKey(sourceInventory.Key))
                    {
                        filteredSources.Add(sourceInventory.Key, new List<FilteredItem>());
                    }

                    foreach (var item in sourceInventory.Value)
                    {
                        var filteredItem = filter.FilterItem(item);
                        if (filteredItem != null)
                        {
                            filteredSources[sourceInventory.Key].Add(filteredItem);
                        }
                    }
                }

                var slotsAvailable = new Dictionary<(ulong, InventoryType), Queue<InventoryItem>>();
                var itemLocations = new Dictionary<int, List<InventoryItem>>();
                var absoluteItemLocations = new Dictionary<int, HashSet<(ulong, InventoryType)>>();
                foreach (var destinationInventory in destinationInventories)
                {
                    foreach (var destinationItem in destinationInventory.Value)
                    {
                        if (!slotsAvailable.ContainsKey(destinationInventory.Key))
                        {
                            slotsAvailable.Add(destinationInventory.Key, new Queue<InventoryItem>());
                        }

                        destinationItem.TempQuantity = destinationItem.Quantity;
                        if (destinationItem.IsEmpty && !destinationItem.IsEquippedGear)
                        {
                            slotsAvailable[destinationInventory.Key].Enqueue(destinationItem);
                        }
                        else
                        {
                            var filteredDestinationItem = filter.FilterItem(destinationItem);
                            if (filteredDestinationItem != null)
                            {
                                var itemHashCode = destinationItem.GenerateHashCode(IgnoreHQWhenSorting ?? false);
                                if (!itemLocations.ContainsKey(itemHashCode))
                                {
                                    itemLocations.Add(itemHashCode, new List<InventoryItem>());
                                }

                                itemLocations[itemHashCode].Add(destinationItem);
                            }
                        }
                    }
                }

                foreach (var sourceInventory in filteredSources)
                {
                    //PluginLog.Verbose("Found " + sourceInventory.Value.Count + " items in " + sourceInventory.Key + " " + sourceInventory.Key.Item2.ToString());
                    for (var index = 0; index < sourceInventory.Value.Count; index++)
                    {
                        var filteredItem = sourceInventory.Value[index];
                        var sourceItem = filteredItem.Item;
                        if (sourceItem.IsEmpty) continue;
                        if (filteredItem.QuantityRequired == null)
                        {
                            sourceItem.TempQuantity = sourceItem.Quantity;
                        }
                        else
                        {
                            sourceItem.TempQuantity = Math.Min(filteredItem.QuantityRequired.Value, sourceItem.Quantity);
                        }
                        //Item already seen, try to put it into that container
                        var hashCode = sourceItem.GenerateHashCode(IgnoreHQWhenSorting ?? false);
                        if (itemLocations.ContainsKey(hashCode))
                        {
                            for (var i = 0; i < itemLocations[hashCode].Count; i++)
                            {
                                var existingItem = itemLocations[hashCode][i];
                                //Don't compare inventory to itself
                                if (existingItem.RetainerId == sourceItem.RetainerId && existingItem.SortedCategory == sourceItem.SortedCategory)
                                {
                                    continue;
                                }

                                if (!existingItem.FullStack)
                                {
                                    var existingCapacity = existingItem.RemainingTempStack;
                                    var canFit = Math.Min(existingCapacity, sourceItem.TempQuantity);
                                    if (canFit != 0)
                                    {
                                        //All the item can fit, stick it in and continue
                                        if (filter.InActiveInventories(activeCharacter, activeRetainer,
                                            sourceInventory.Key.Item1, existingItem.RetainerId))
                                        {
                                            sortedItems.Add(new SortingResult(sourceInventory.Key.Item1,
                                                existingItem.RetainerId, sourceItem.SortedContainer,
                                                existingItem.SortedContainer,existingItem.BagLocation(existingItem.SortedContainer),false, sourceItem, (int) canFit));
                                        }

                                        if (!absoluteItemLocations.ContainsKey(hashCode))
                                        {
                                            absoluteItemLocations.Add(hashCode,
                                                new HashSet<(ulong, InventoryType)>());
                                        }

                                        absoluteItemLocations[hashCode]
                                            .Add((existingItem.RetainerId, existingItem.SortedContainer));
                                        existingItem.TempQuantity += canFit;
                                        sourceItem.TempQuantity -= canFit;
                                    }
                                }
                                else
                                {
                                    if (!absoluteItemLocations.ContainsKey(hashCode))
                                    {
                                        absoluteItemLocations.Add(hashCode, new HashSet<(ulong, InventoryType)>());
                                    }

                                    absoluteItemLocations[hashCode]
                                        .Add((existingItem.RetainerId, existingItem.SortedContainer));
                                }

                                if (sourceItem.TempQuantity == 0)
                                {
                                    break;
                                }
                            }


                            //The item is empty, continue, otherwise work out what inventory we should try to place it in
                            if (sourceItem.TempQuantity == 0)
                            {
                                continue;
                            }
                            else
                            {
                                if (absoluteItemLocations.ContainsKey(hashCode))
                                {
                                    var seenInventoryLocations = absoluteItemLocations[hashCode];
                                    while (seenInventoryLocations.Count != 0 && sourceItem.TempQuantity != 0)
                                    {
                                        var seenInventoryLocation = seenInventoryLocations.First();
                                        if (slotsAvailable.ContainsKey(seenInventoryLocation))
                                        {
                                            var slotCount = slotsAvailable[seenInventoryLocation].Count;
                                            if (slotCount != 0)
                                            {
                                                var nextEmptySlot = slotsAvailable[seenInventoryLocation].Dequeue();
                                                if (sourceItem.Item is {IsUnique: false})
                                                {
                                                    if (sourceInventory.Key.Item1 != seenInventoryLocation.Item1 ||
                                                        sourceItem.SortedContainer != seenInventoryLocation.Item2)
                                                    {
                                                        if (filter.InActiveInventories(activeCharacter, activeRetainer,
                                                            sourceInventory.Key.Item1, seenInventoryLocation.Item1))
                                                        {
                                                            //PluginLog.Verbose(
                                                            //    "Added item to filter result as we've seen the item before: " +
                                                            //    sourceItem.FormattedName);
                                                            sortedItems.Add(new SortingResult(sourceInventory.Key.Item1,
                                                                seenInventoryLocation.Item1, sourceItem.SortedContainer,
                                                                seenInventoryLocation.Item2, nextEmptySlot.BagLocation(nextEmptySlot.SortedContainer),true, sourceItem,
                                                                (int) sourceItem.TempQuantity));
                                                        }

                                                        sourceItem.TempQuantity -= sourceItem.TempQuantity;
                                                    }
                                                }

                                                continue;
                                            }
                                        }

                                        seenInventoryLocations.Remove(seenInventoryLocation);
                                    }
                                }
                            }
                        }

                        if (filter.DuplicatesOnly == true)
                        {
                            continue;
                        }

                        if (sourceItem.TempQuantity == 0)
                        {
                            continue;
                        }

                        if (slotsAvailable.Count == 0)
                        {
                            continue;
                        }

                        foreach (var slot in slotsAvailable)
                        {
                            if (slot.Value.Count == 0)
                            {
                                slotsAvailable.Remove(slot.Key);
                            }
                        }
                        
                        var nextSlots = slotsAvailable.Where(c =>
                            c.Value.Count != 0 &&
                            filter.InActiveInventories(activeCharacter, activeRetainer, sourceInventory.Key.Item1,
                                c.Key.Item1) && !(c.Key.Item1 == sourceItem.RetainerId &&
                                                  c.Key.Item2.ToInventoryCategory() == sourceItem.SortedCategory)).ToList();

                        if (!nextSlots.Any())
                        {
                            continue;
                        }

                        var nextSlot = nextSlots.First();

                        //Don't compare inventory to itself
                        if (nextSlot.Value.Count != 0)
                        {
                            var nextEmptySlot = nextSlot.Value.Dequeue();
                            if (filter.InActiveInventories(activeCharacter, activeRetainer, sourceInventory.Key.Item1,
                                nextSlot.Key.Item1))
                            {
                                //This check stops the item from being sorted into it's own bag, this generally means its already in the optimal place
                                if (sourceInventory.Key.Item1 != nextSlot.Key.Item1 ||
                                    sourceItem.SortedContainer != nextSlot.Key.Item2)
                                {
                                    sortedItems.Add(new SortingResult(sourceInventory.Key.Item1, nextSlot.Key.Item1,
                                        sourceItem.SortedContainer, nextSlot.Key.Item2, nextEmptySlot.BagLocation(nextEmptySlot.SortedContainer),true, sourceItem,
                                        (int) sourceItem.TempQuantity));
                                    
                                    //We want to add the empty slot to the list of locations we know about, we need to create a copy and add that so any further items with the same ID can properly check how much room is left in the stack
                                    nextEmptySlot = new InventoryItem(nextEmptySlot);
                                    nextEmptySlot.ItemId = sourceItem.ItemId;
                                    nextEmptySlot.Flags = sourceItem.Flags;
                                    nextEmptySlot.Quantity = sourceItem.Quantity;
                                    
                                    //Add the destination item into the list of locations in case we have an empty slot for an item but multiple sources of the item.
                                    var newLocationHash = sourceItem.GenerateHashCode(IgnoreHQWhenSorting ?? false);
                                    itemLocations.TryAdd(newLocationHash, new List<InventoryItem>());
                                    itemLocations[newLocationHash].Add(nextEmptySlot);
                                    absoluteItemLocations.TryAdd(newLocationHash,
                                        new HashSet<(ulong, InventoryType)>());
                                    absoluteItemLocations[newLocationHash].Add((nextSlot.Key.Item1, nextEmptySlot.SortedContainer));
                                }
                            }
                        }
                        else
                        {
                            // PluginLog.Verbose("Added item to unsortable list, maybe I should show these somewhere: " +
                            //                  sourceItem.FormattedName);
                            unsortableItems.Add(sourceItem);
                        }
                    }
                }
            }
            else if(filter.FilterType == FilterType.SearchFilter)
            {
                //Determine which source and destination inventories we actually need to examine
                Dictionary<(ulong, InventoryType), InventoryItem?[]> sourceInventories = new();
                foreach (var character in inventories)
                {
                    foreach (var inventory in character.GetAllInventoriesByType())
                    {
                        var inventoryKey = (character.CharacterId, inventory.Key);
                        if (filter.SourceAllRetainers.HasValue && filter.SourceAllRetainers.Value && characterMonitor.IsRetainer(character.CharacterId) && (displaySourceCrossCharacter || characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                        {
                            if (!sourceInventories.ContainsKey(inventoryKey))
                            {
                                sourceInventories.Add(inventoryKey, inventory.Value);
                            }
                        }
                        if (filter.SourceAllFreeCompanies.HasValue && filter.SourceAllFreeCompanies.Value && characterMonitor.IsFreeCompany(character.CharacterId) && (displaySourceCrossCharacter || characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                        {
                            if (!sourceInventories.ContainsKey(inventoryKey))
                            {
                                sourceInventories.Add(inventoryKey, inventory.Value);
                            }
                        }
                        if (filter.SourceAllHouses.HasValue && filter.SourceAllHouses.Value && characterMonitor.IsHousing(character.CharacterId) && (displaySourceCrossCharacter || characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                        {
                            if (!sourceInventories.ContainsKey(inventoryKey))
                            {
                                sourceInventories.Add(inventoryKey, inventory.Value);
                            }
                        }
                        if (filter.SourceAllCharacters.HasValue && filter.SourceAllCharacters.Value && characterMonitor.IsCharacter(character.CharacterId) && (displaySourceCrossCharacter || characterMonitor.ActiveCharacterId == character.CharacterId))
                        {
                            if (!sourceInventories.ContainsKey(inventoryKey))
                            {
                                sourceInventories.Add(inventoryKey, inventory.Value);
                            }
                        }

                        var inventoryCategory = inventoryKey.Key.ToInventoryCategory();
                        if (filter.SourceInventories.Contains((inventoryKey.CharacterId,inventoryCategory)) && (displaySourceCrossCharacter || characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                        {
                            if (!sourceInventories.ContainsKey(inventoryKey))
                            {
                                sourceInventories.Add(inventoryKey, inventory.Value);
                            }
                        }
                        if (filter.SourceCategories != null && filter.SourceCategories.Contains(inventoryCategory) && (displaySourceCrossCharacter || characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                        {
                            if (!sourceInventories.ContainsKey(inventoryKey))
                            {
                                sourceInventories.Add(inventoryKey, inventory.Value);
                            }
                        }
                        
                        if (filter.SourceWorlds != null && filter.SourceWorlds.Contains(characterMonitor.GetCharacterById(character.CharacterId)?.WorldId ?? 0))
                        {
                            if (!sourceInventories.ContainsKey(inventoryKey))
                            {
                                sourceInventories.Add(inventoryKey, inventory.Value);
                            }
                        }
                    }
                }

                HashSet<int> distinctItems = new HashSet<int>();
                HashSet<int> duplicateItems = new HashSet<int>();

                //Filter the source and destination inventories based on the applicable items so we have less to sort
                Dictionary<(ulong, InventoryType), List<FilteredItem>> filteredSources = new();
                //Dictionary<(ulong, InventoryCategory), List<InventoryItem>> filteredDestinations = new();
                PluginLog.Verbose(sourceInventories.Count() + " inventories to examine.");
                foreach (var sourceInventory in sourceInventories)
                {
                    if (!filteredSources.ContainsKey(sourceInventory.Key))
                    {
                        filteredSources.Add(sourceInventory.Key, new List<FilteredItem>());
                    }
                    foreach (var item in sourceInventory.Value)
                    {
                        if (item != null)
                        {
                            var filteredItem = filter.FilterItem(item);
                            if (filteredItem != null)
                            {
                                filteredSources[sourceInventory.Key].Add(filteredItem);
                            }
                        }
                    }
                }
                if (filter.DuplicatesOnly.HasValue && filter.DuplicatesOnly == true)
                {
                    foreach (var filteredSource in filteredSources)
                    {
                        foreach (var item in filteredSource.Value)
                        {
                            var hashCode = item.Item.GenerateHashCode(IgnoreHQWhenSorting ?? false);
                            if (distinctItems.Contains(hashCode))
                            {
                                if (!duplicateItems.Contains(hashCode))
                                {
                                    duplicateItems.Add(hashCode);
                                }
                            }
                            else
                            {
                                distinctItems.Add(hashCode);
                            }
                        }
                    }
                }

                foreach (var filteredSource in filteredSources)
                {
                    foreach (var filteredItem in filteredSource.Value)
                    {
                        var item = filteredItem.Item;
                        if (filter.DuplicatesOnly.HasValue && filter.DuplicatesOnly == true)
                        {
                            if (duplicateItems.Contains(item.GenerateHashCode(IgnoreHQWhenSorting ?? false)))
                            {
                                sortedItems.Add(new SortingResult(filteredSource.Key.Item1, item.SortedContainer,
                                    item, (int)item.Quantity));
                            }

                        }
                        else
                        {
                            sortedItems.Add(new SortingResult(filteredSource.Key.Item1, item.SortedContainer,
                                item, (int)item.Quantity));    
                        }
                        
                    }
                }
            }
            else if(filter.FilterType == FilterType.HistoryFilter)
            {
                var history = PluginService.InventoryHistory.GetHistory();
                var matchedItems = new List<InventoryChange>();
                foreach (var item in history)
                {
                    var wasMatched = false;
                    if (item.FromItem != null)
                    {
                        var characterId = item.FromItem.RetainerId;
                        var inventoryCategory = item.FromItem.SortedCategory;
                        wasMatched = MatchHistoryItem(item, characterId, inventoryCategory);
                    }

                    if (item.ToItem != null)
                    {
                        if (!wasMatched)
                        {
                            var characterIdTo = item.ToItem.RetainerId;
                            var inventoryCategoryTo = item.ToItem.SortedCategory;
                            wasMatched = MatchHistoryItem(item, characterIdTo, inventoryCategoryTo);
                        }
                    }

                    if (wasMatched)
                    {
                        matchedItems.Add(item);
                    }
                }
                
                bool MatchHistoryItem(InventoryChange item, ulong characterId, InventoryCategory inventoryCategory)
                {
                    if (filter.SourceAllRetainers.HasValue && filter.SourceAllRetainers.Value &&
                        characterMonitor.IsRetainer(characterId) &&
                        (displaySourceCrossCharacter || characterMonitor.BelongsToActiveCharacter(characterId)))
                    {
                        return true;
                    }

                    if (filter.SourceAllFreeCompanies.HasValue && filter.SourceAllFreeCompanies.Value &&
                        characterMonitor.IsFreeCompany(characterId) &&
                        (displaySourceCrossCharacter || characterMonitor.BelongsToActiveCharacter(characterId)))
                    {
                        return true;
                    }

                    if (filter.SourceAllHouses.HasValue && filter.SourceAllHouses.Value && characterMonitor.IsHousing(characterId) &&
                        (displaySourceCrossCharacter || characterMonitor.BelongsToActiveCharacter(characterId)))
                    {
                        return true;
                    }

                    if (filter.SourceAllCharacters.HasValue && filter.SourceAllCharacters.Value &&
                        characterMonitor.IsCharacter(characterId) &&
                        (displaySourceCrossCharacter || characterMonitor.ActiveCharacterId == characterId))
                    {
                        return true;
                    }

                    if (filter.SourceInventories.Contains((characterId, inventoryCategory)) &&
                        (displaySourceCrossCharacter || characterMonitor.BelongsToActiveCharacter(characterId)))
                    {
                        return true;
                    }

                    if (filter.SourceCategories != null && filter.SourceCategories.Contains(inventoryCategory) &&
                        (displaySourceCrossCharacter || characterMonitor.BelongsToActiveCharacter(characterId)))
                    {
                        return true;
                    }

                    if (filter.SourceWorlds != null &&
                        filter.SourceWorlds.Contains(characterMonitor.GetCharacterById(characterId)?.WorldId ?? 0))
                    {
                        return true;
                    }

                    return false;
                }

                foreach (var change in matchedItems.OrderByDescending(c => c.ChangeDate ?? new DateTime()))
                {
                    if (filter.FilterItem(change))
                    {
                        inventoryHistory.Add(change);
                    }
                }
            }
            else
            {
                items = Service.ExcelCache.AllItems.Select(c => c.Value).Where(filter.FilterItem).Where(c => c.RowId != 0).ToList();
                
            }

            
            return new FilterResult(sortedItems, unsortableItems, items, inventoryHistory);
        }

        public Task<FilterResult> GenerateFilteredList(List<Inventory>? inventories = null)
        {
            if (inventories == null)
            {
                inventories = PluginService.InventoryMonitor.Inventories.Select(c => c.Value).ToList();
            }
            return Task<FilterResult>.Factory.StartNew(() => GenerateFilteredListInternal(this, inventories));
        }
        
        #endregion

        public FilterConfiguration? Clone()
        {
            FilterResult = null;
            var clone = this.Copy();
            return clone;
        }
    }

    public enum HighlightMode
    {
        Never,
        Always,
        InUi,
        OutUi
    }
}
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CsvHelper;
using Dalamud.Logging;
using InventoryTools.Extensions;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;

namespace InventoryTools.Logic
{
    public class FilterConfiguration
    {
        public delegate void ConfigurationChangedDelegate(FilterConfiguration filterConfiguration);
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
        private Dictionary<string, uint>? _uintFilters = new();
        private Dictionary<string, List<uint>>? _uintChoiceFilters = new();
        private Dictionary<string, List<ulong>>? _ulongChoiceFilters = new();
        private Dictionary<string, List<string>>? _stringChoiceFilters = new();
        private Dictionary<string, Vector4>? _colorFilters = new();
        private string? _name = "";
        private string _key = "";
        private ulong _ownerId = 0;
        private bool? _filterItemsInRetainers;
        private bool? _sourceAllRetainers;
        private bool? _sourceAllCharacters;
        private bool? _destinationAllRetainers;
        private bool? _destinationAllCharacters;
        private bool? _sourceIncludeCrossCharacter;
        private bool? _destinationIncludeCrossCharacter;
        private int? _freezeColumns;
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
        private string? _highlightWhen = null;
        private List<string>? _columns;
        private string? _icon;
        private static readonly byte CurrentVersion = 1;
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
        
        [JsonIgnore]
        private FilterResult? _filterResult = null;

        private string? _tableId = null;
        public bool NeedsRefresh { get; set; } = true;
        public HighlightMode HighlightMode { get; set; } = HighlightMode.Never;

        [JsonIgnore]
        public FilterResult? FilterResult
        {
            get
            {
                if (_filterResult == null || NeedsRefresh)
                {
                    StartRefresh();
                }
                return _filterResult;
            }
            set => _filterResult = value;
        }
        
        [JsonIgnore]
        private bool _refreshing;

        public async void StartRefresh()
        {
            if (_refreshing)
            {
                return;
            }
            _refreshing = true;
            
            _filterResult = await PluginService.FilterManager.GenerateFilteredList(this,
                PluginService.InventoryMonitor.Inventories);
            NeedsRefresh = false;
            ListUpdated?.Invoke(this);
            _refreshing = false;
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

        public void AddDefaultColumns()
        {
            Columns = new List<string>();
            if (FilterType == FilterType.SearchFilter)
            {
                Columns.Add("IconColumn");
                Columns.Add("NameColumn");
                Columns.Add("TypeColumn");
                Columns.Add("QuantityColumn");
                Columns.Add("SourceColumn");
                Columns.Add("LocationColumn");
            }
            else if (FilterType == FilterType.SortingFilter)
            {
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
            }
        }

        public FilterConfiguration()
        {
        }

        public void Refresh()
        {
            
        }

        public FilterTable GenerateTable()
        {
            FilterTable table = new FilterTable(this);
            table.RefreshColumns();
            table.ShowFilterRow = true;
            return table;
        }


        public List<(ulong, InventoryCategory)> SourceInventories
        {
            get => _sourceInventories;
            set { _sourceInventories = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public List<uint> ItemUiCategoryId
        {
            get => _itemUiCategoryId;
            set { _itemUiCategoryId = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public List<uint> ItemSearchCategoryId
        {
            get => _itemSearchCategoryId;
            set { _itemSearchCategoryId = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public List<uint> EquipSlotCategoryId
        {
            get => _equipSlotCategoryId;
            set { _equipSlotCategoryId = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public List<uint> ItemSortCategoryId
        {
            get => _itemSortCategoryId;
            set { _itemSortCategoryId = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public List<(ulong, InventoryCategory)> DestinationInventories
        {
            get => _destinationInventories;
            set { _destinationInventories = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public bool? IsHq
        {
            get => _isHq;
            set { _isHq = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public bool? IsCollectible
        {
            get => _isCollectible;
            set { _isCollectible = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public string Name
        {
            get => _name == null ? "" :  _name;
            set { _name = value;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public bool? DuplicatesOnly
        {
            get => _duplicatesOnly;
            set { _duplicatesOnly = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public bool? FilterItemsInRetainers
        {
            get => _filterItemsInRetainers;
            set { _filterItemsInRetainers = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public bool DisplayInTabs
        {
            get => _displayInTabs;
            set { _displayInTabs = value;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public bool OpenAsWindow
        {
            get => _openAsWindow;
            set { _openAsWindow = value;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public string Quantity
        {
            get => _quantity;
            set { _quantity = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public string ILevel
        {
            get => _iLevel;
            set { _iLevel = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public string Spiritbond
        {
            get => _spiritbond;
            set { _spiritbond = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public string NameFilter
        {
            get => _nameFilter;
            set { _nameFilter = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
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
                ConfigurationChanged?.Invoke(this);
            }            
        }
        
        public string? HighlightWhen
        {
            get => _highlightWhen;
            set
            {
                _highlightWhen = value;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public bool? SourceAllCharacters
        {
            get => _sourceAllCharacters;
            set { _sourceAllCharacters = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }            
        }

        public bool? DestinationAllRetainers
        {
            get => _destinationAllRetainers;
            set { _destinationAllRetainers = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }            
        }

        public bool? SourceIncludeCrossCharacter
        {
            get => _sourceIncludeCrossCharacter;
            set { _sourceIncludeCrossCharacter = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }            
        }

        public bool? DestinationIncludeCrossCharacter
        {
            get => _destinationIncludeCrossCharacter;
            set { _destinationIncludeCrossCharacter = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }            
        }

        public int? FreezeColumns
        {
            get => _freezeColumns;
            set { _freezeColumns = value;
                NeedsRefresh = true;
                TableConfigurationChanged?.Invoke(this);
            }            
        }

        public HashSet<InventoryCategory>? DestinationCategories
        {
            get => _destinationCategories;
            set { _destinationCategories = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }            
        }

        public HashSet<InventoryCategory>? SourceCategories
        {
            get => _sourceCategories;
            set { _sourceCategories = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }            
        }

        public bool? DestinationAllCharacters
        {
            get => _destinationAllCharacters;
            set { _destinationAllCharacters = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }            
        }
        
        public string ShopSellingPrice
        {
            get => _shopSellingPrice;
            set
            {
                _shopSellingPrice = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);                
            }
        }

        public string ShopBuyingPrice
        {
            get => _shopBuyingPrice;
            set
            {
                _shopBuyingPrice = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public string MarketAveragePrice
        {
            get => _marketAveragePrice;
            set
            {
                _marketAveragePrice = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public string MarketTotalAveragePrice
        {
            get => _marketTotalAveragePrice;
            set
            {
                _marketTotalAveragePrice = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public bool? CanBeBought
        {
            get => _canBeBought;
            set
            {
                _canBeBought = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }
        
        public bool? IsAvailableAtTimedNode
        {
            get => _isAvailableAtTimedNode;
            set
            {
                _isAvailableAtTimedNode = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
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
                ConfigurationChanged?.Invoke(this);
            }
        }
        
        public Vector4? DestinationHighlightColor
        {
            get => _destinationHighlightColor;
            set
            {
                _destinationHighlightColor = value;
                ConfigurationChanged?.Invoke(this);
            }
        }
        
        public Vector4? RetainerListColor
        {
            get => _retainerListColor;
            set
            {
                _retainerListColor = value;
                ConfigurationChanged?.Invoke(this);
            }
        }
        
        public Vector4? TabHighlightColor
        {
            get => _tabHighlightColor;
            set
            {
                _tabHighlightColor = value;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public bool? InvertHighlighting
        {
            get => _invertHighlighting;
            set
            {
                _invertHighlighting = value;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public bool? InvertDestinationHighlighting
        {
            get => _invertDestinationHighlighting;
            set
            {
                _invertDestinationHighlighting = value;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public bool? InvertTabHighlighting
        {
            get => _invertTabHighlighting;
            set
            {
                _invertTabHighlighting = value;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public bool? HighlightDestination
        {
            get => _highlightDestination;
            set
            {
                _highlightDestination = value;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public bool? HighlightDestinationEmpty
        {
            get => _highlightDestinationEmpty;
            set
            {
                _highlightDestinationEmpty = value;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public event ConfigurationChangedDelegate? ConfigurationChanged;
        public event TableConfigurationChangedDelegate? TableConfigurationChanged;
        public event ListUpdatedDelegate? ListUpdated;

        public bool FilterItem(Item item)
        {
            foreach (var filter in PluginService.PluginLogic.AvailableFilters)
            {
                if (!filter.FilterItem(this, item))
                {
                    return false;
                }
            }

            return true;
        }
        public bool FilterItem(InventoryItem item)
        {
            foreach (var filter in PluginService.PluginLogic.AvailableFilters)
            {
                if (!filter.FilterItem(this, item))
                {
                    return false;
                }
            }
            
            return true;
        }

        public void AddColumn(string columnName)
        {
            if (_columns == null)
            {
                _columns = new List<string>();
            }
            _columns.Add(columnName);
            GenerateNewTableId();
            TableConfigurationChanged?.Invoke(this);
        }
        
        public void UpColumn(string columnName)
        {
            if (this._columns == null)
            {
                return;
            }
            _columns = _columns.MoveUp(columnName);
            GenerateNewTableId();
            TableConfigurationChanged?.Invoke(this);
        }
        
        public void DownColumn(string columnName)
        {
            if (this._columns == null)
            {
                return;
            }
            _columns = _columns.MoveDown(columnName);
            GenerateNewTableId();
            TableConfigurationChanged?.Invoke(this);
        }

        public void RemoveColumn(string columnName)
        {
            if (this._columns == null)
            {
                return;
            }
            _columns = _columns.Where(c => c != columnName).ToList();
            GenerateNewTableId();
            TableConfigurationChanged?.Invoke(this);
        }
        
        public void AddDestinationInventory((ulong, InventoryCategory) inventory)
        {
            if (!DestinationInventories.Contains(inventory))
            {
                DestinationInventories.Add(inventory);
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public void RemoveDestinationInventory((ulong, InventoryCategory) inventory)
        {
            if (DestinationInventories.Contains(inventory))
            {
                DestinationInventories.Remove(inventory);
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public void AddSourceInventory((ulong, InventoryCategory) inventory)
        {
            if (!SourceInventories.Contains(inventory))
            {
                SourceInventories.Add(inventory);
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public void RemoveSourceInventory((ulong, InventoryCategory) inventory)
        {
            if (SourceInventories.Contains(inventory))
            {
                SourceInventories.Remove(inventory);
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public void AddItemUiCategory(uint itemUiCategoryId)
        {
            if (!ItemUiCategoryId.Contains(itemUiCategoryId))
            {
                ItemUiCategoryId.Add(itemUiCategoryId);
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public void RemoveItemUiCategory(uint itemUiCategoryId)
        {
            if (ItemUiCategoryId.Contains(itemUiCategoryId))
            {
                ItemUiCategoryId.Remove(itemUiCategoryId);
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public void AddItemSearchCategory(uint itemSearchCategoryId)
        {
            if (!ItemSearchCategoryId.Contains(itemSearchCategoryId))
            {
                ItemSearchCategoryId.Add(itemSearchCategoryId);
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public void RemoveItemSearchCategory(uint itemSearchCategoryId)
        {
            if (ItemSearchCategoryId.Contains(itemSearchCategoryId))
            {
                ItemSearchCategoryId.Remove(itemSearchCategoryId);
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
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
                return "";
            }
        }

        public List<string>? Columns
        {
            get => _columns;
            set
            {
                _columns = value;
                TableConfigurationChanged?.Invoke(this);
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
            ConfigurationChanged?.Invoke(this);
        }

        public void UpdateColorFilter(string key, Vector4 value)
        {
            if (ColorFilters.ContainsKey(key) && ColorFilters[key] == value)
            {
                return;
            }

            ColorFilters[key] = value;
            NeedsRefresh = true;
            ConfigurationChanged?.Invoke(this);
        }

        public void RemoveBooleanFilter(string key)
        {
            if (BooleanFilters.ContainsKey(key))
            {
                BooleanFilters.Remove(key);
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public void RemoveColorFilter(string key)
        {
            if (ColorFilters.ContainsKey(key))
            {
                ColorFilters.Remove(key);
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
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
            ConfigurationChanged?.Invoke(this);
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
            ConfigurationChanged?.Invoke(this);
        }

        public void UpdateUintChoiceFilter(string key, List<uint> value)
        {
            UintChoiceFilters[key] = value;
            NeedsRefresh = true;
            ConfigurationChanged?.Invoke(this);
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
            ConfigurationChanged?.Invoke(this);
        }

        public void UpdateUlongChoiceFilter(string key, List<ulong> value)
        {
            UlongChoiceFilters[key] = value;
            NeedsRefresh = true;
            ConfigurationChanged?.Invoke(this);
        }

        public void UpdateStringChoiceFilter(string key, List<string> value)
        {
            StringChoiceFilters[key] = value;
            NeedsRefresh = true;
            ConfigurationChanged?.Invoke(this);
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
            if (FilterItemsInRetainers.HasValue)
            {
                if (FilterItemsInRetainers.Value && activeRetainerId != 0)
                {
                    if (activeCharacterId == sourceCharacterId && activeRetainerId == destinationCharacterId)
                    {
                        return true;
                    }
                    if (activeRetainerId == sourceCharacterId && activeRetainerId != destinationCharacterId)
                    {
                        return true;
                    }
                    return false;
                }
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

                return categories;
            }
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
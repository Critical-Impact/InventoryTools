using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
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
        private FilterItemsRetainerEnum _filterItemsInRetainersEnum;
        private bool? _sourceAllRetainers;
        private bool? _sourceAllCharacters;
        private bool? _destinationAllRetainers;
        private bool? _destinationAllCharacters;
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
        private bool _craftListDefault = false;
        private string? _highlightWhen = null;
        private int _tableHeight = 32;
        private int _craftTableHeight = 32;
        private List<string>? _columns;
        private List<string>? _craftColumns;
        private string? _icon;
        private static readonly byte CurrentVersion = 1;
        
        //Crafting
        private CraftList? _craftList = null;
        private bool? _simpleCraftingMode = null;
        private bool? _useORFiltering = null;
        private bool _hideCompletedRows = false;
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
        
        [JsonIgnore]
        private bool _refreshing;

        public async void StartRefresh()
        {
            if (_refreshing)
            {
                return;
            }

            PluginLog.Debug("Started a refresh on the filter configuration.");
            _refreshing = true;
            CraftList.BeenUpdated = false;

            if (this.FilterType == FilterType.CraftFilter)
            {
                //Clean up this sloppy function
                var playerBags = PluginService.InventoryMonitor.GetSpecificInventory(PluginService.CharacterMonitor.ActiveCharacter,
                   InventoryCategory.CharacterBags);
                var crystalBags = PluginService.InventoryMonitor.GetSpecificInventory(PluginService.CharacterMonitor.ActiveCharacter,
                   InventoryCategory.Crystals);
                var retainerSetting = this.FilterItemsInRetainersEnum;
                _filterItemsInRetainersEnum = FilterItemsRetainerEnum.No;
                var tempFilterResult = await GenerateFilteredList(PluginService.InventoryMonitor.Inventories);
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
                _filterResult = await GenerateFilteredList(PluginService.InventoryMonitor.Inventories);
            }

            else
            {
                _filterResult = await GenerateFilteredList(PluginService.InventoryMonitor.Inventories);
            }
            
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
            }
            else if (FilterType == FilterType.CraftFilter)
            {
                if (Columns == null)
                {
                    Columns = new List<string>();
                }

                if (CraftColumns == null)
                {
                    CraftColumns = new List<string>();
                }
                Columns.Add("IconColumn");
                Columns.Add("NameColumn");
                Columns.Add("CraftAmountAvailableColumn");
                Columns.Add("QuantityColumn");
                Columns.Add("SourceColumn");
                Columns.Add("LocationColumn");
                
                CraftColumns = new List<string>();
                AddCraftColumn("IconColumn");
                AddCraftColumn("NameColumn");
                if (SimpleCraftingMode == true)
                {
                    AddCraftColumn("CraftAmountRequiredColumn");
                    AddCraftColumn("CraftSimpleColumn");
                }
                else
                {
                    AddCraftColumn("QuantityAvailableColumn");
                    AddCraftColumn("CraftAmountRequiredColumn");
                    AddCraftColumn("CraftAmountReadyColumn");
                    AddCraftColumn("CraftAmountAvailableColumn");
                    AddCraftColumn("CraftAmountUnavailableColumn");
                    AddCraftColumn("CraftAmountCanCraftColumn");
                }
                AddCraftColumn("MarketBoardMinPriceColumn");
                AddCraftColumn("MarketBoardMinTotalPriceColumn");
                AddCraftColumn("AcquisitionSourceIconsColumn");
                AddCraftColumn("CraftGatherColumn");
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

        public int TableHeight
        {
            get => _tableHeight;
            set { _tableHeight = value;
                NeedsRefresh = true;
                TableConfigurationChanged?.Invoke(this);
            }
        }


        public int CraftTableHeight
        {
            get => _craftTableHeight;
            set { _craftTableHeight = value;
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

        [Obsolete]
        public bool? FilterItemsInRetainers
        {
            get => _filterItemsInRetainers;
            set { _filterItemsInRetainers = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public FilterItemsRetainerEnum FilterItemsInRetainersEnum
        {
            get => _filterItemsInRetainersEnum;
            set { _filterItemsInRetainersEnum = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
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

        
        public bool? UseORFiltering
        {
            get => _useORFiltering;
            set
            {
                _useORFiltering = value;
                ConfigurationChanged?.Invoke(this);
            }
        }

        
        public bool HideCompletedRows
        {
            get => _hideCompletedRows;
            set
            {
                _hideCompletedRows = value;
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

        public int? FreezeCraftColumns
        {
            get => _freezeCraftColumns;
            set { _freezeCraftColumns = value;
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

        public bool CraftListDefault
        {
            get => _craftListDefault;
            set
            {
                _craftListDefault = value;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public bool? SimpleCraftingMode
        {
            get => _simpleCraftingMode;
            set
            {
                _simpleCraftingMode = value;
                TableConfigurationChanged?.Invoke(this);
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
        
        public FilteredItem? FilterItem(InventoryItem item)
        {
            uint? requiredAmount = null;
            if (FilterType == FilterType.CraftFilter)
            {
                var requiredMaterialsList = CraftList.BeenUpdated ? CraftList.GetAvailableMaterialsList().Where(c => c.Value != 0).ToDictionary(c => c.Key, c => c.Value) : CraftList.GetRequiredMaterialsList();
                if (!requiredMaterialsList.ContainsKey(item.ItemId))
                {
                    return null;
                }

                if (CraftList.BeenUpdated)
                {
                    var retrieveMaterialsList = CraftList.GetQuantityToRetrieveList();
                    if (retrieveMaterialsList.ContainsKey(item.ItemId))
                    {
                        requiredAmount = retrieveMaterialsList[item.ItemId];
                    }
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
            _columns.Add(columnName);
            GenerateNewTableId();
            TableConfigurationChanged?.Invoke(this);
        }

        public void AddCraftColumn(string columnName)
        {
            if (_craftColumns == null)
            {
                _craftColumns = new List<string>();
            }
            _craftColumns.Add(columnName);
            GenerateNewCraftTableId();
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
                TableConfigurationChanged?.Invoke(this);
            }
        }

        public List<string>? CraftColumns
        {
            get => _craftColumns;
            set
            {
                _craftColumns = value;
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
            if (FilterItemsInRetainersEnum is FilterItemsRetainerEnum.Yes or FilterItemsRetainerEnum.Only && activeRetainerId != 0)
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
                ConfigurationChanged?.Invoke(this);
            }
        }

        public void SetOrder(uint order)
        {
            _order = order;
        }

        public void NotifyConfigurationChange()
        {
            ConfigurationChanged?.Invoke(this);
        }


        #region Filter Generation
        public FilterResult GenerateFilteredListInternal(FilterConfiguration filter, Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>> inventories)
        {
            var sortedItems = new List<SortingResult>();
            var unsortableItems = new List<InventoryItem>();
            var items = new List<ItemEx>();
            var characterMonitor = PluginService.CharacterMonitor;
            var activeCharacter = characterMonitor.ActiveCharacter;
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
                    foreach (var inventory in character.Value)
                    {
                        foreach (var type in inventory.Key.GetTypes())
                        {
                            var inventoryKey = (character.Key, type);
                            if (filter.SourceAllRetainers.HasValue && filter.SourceAllRetainers.Value &&
                                characterMonitor.IsRetainer(character.Key) && (displaySourceCrossCharacter ||
                                                                               characterMonitor
                                                                                   .BelongsToActiveCharacter(
                                                                                       character.Key)))
                            {
                                if (!sourceInventories.ContainsKey(inventoryKey))
                                {
                                    sourceInventories.Add(inventoryKey, inventory.Value.Where(c => c.SortedContainer == type).ToList());
                                }
                            }

                            if (filter.SourceAllCharacters.HasValue && filter.SourceAllCharacters.Value &&
                                !characterMonitor.IsRetainer(character.Key) &&
                                characterMonitor.ActiveCharacter == character.Key && (displaySourceCrossCharacter ||
                                    characterMonitor.BelongsToActiveCharacter(character.Key)))
                            {
                                if (inventoryKey.Item2.ToInventoryCategory() is not InventoryCategory.FreeCompanyBags &&
                                    !sourceInventories.ContainsKey(inventoryKey))
                                {
                                    sourceInventories.Add(inventoryKey, inventory.Value.Where(c => c.SortedContainer == type).ToList());
                                }
                            }

                            if (filter.SourceInventories.Contains((character.Key, inventoryKey.type.ToInventoryCategory())) && (displaySourceCrossCharacter ||
                                characterMonitor.BelongsToActiveCharacter(character.Key)))
                            {

                                if (!sourceInventories.ContainsKey(inventoryKey))
                                {
                                    sourceInventories.Add(inventoryKey, inventory.Value.Where(c => c.SortedContainer == type).ToList());
                                }
                            }

                            if (filter.SourceCategories != null &&
                                filter.SourceCategories.Contains(inventoryKey.Item2.ToInventoryCategory()) && (displaySourceCrossCharacter ||
                                    characterMonitor.BelongsToActiveCharacter(character.Key)))
                            {
                                if (!sourceInventories.ContainsKey(inventoryKey))
                                {
                                    sourceInventories.Add(inventoryKey, inventory.Value.Where(c => c.SortedContainer == type).ToList());
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
                                characterMonitor.IsRetainer(character.Key) && (displayDestinationCrossCharacter ||
                                                                               characterMonitor
                                                                                   .BelongsToActiveCharacter(
                                                                                       character.Key)))
                            {
                                if (!destinationInventories.ContainsKey(inventoryKey))
                                {
                                    destinationInventories.Add(inventoryKey, inventory.Value.Where(c => c.SortedContainer == type).ToList());
                                }
                            }

                            if (filter.DestinationAllCharacters.HasValue && filter.DestinationAllCharacters.Value &&
                                characterMonitor.ActiveCharacter == character.Key &&
                                (displayDestinationCrossCharacter ||
                                 characterMonitor.BelongsToActiveCharacter(character.Key)))
                            {
                                if (!destinationInventories.ContainsKey(inventoryKey))
                                {
                                    destinationInventories.Add(inventoryKey, inventory.Value.Where(c => c.SortedContainer == type).ToList());
                                }
                            }

                            if (filter.DestinationInventories.Contains((character.Key,inventoryKey.type.ToInventoryCategory())) &&
                                (displayDestinationCrossCharacter ||
                                 characterMonitor.BelongsToActiveCharacter(character.Key)))
                            {
                                if (!destinationInventories.ContainsKey(inventoryKey))
                                {
                                    destinationInventories.Add(inventoryKey, inventory.Value.Where(c => c.SortedContainer == type).ToList());
                                }
                            }

                            if (filter.DestinationCategories != null &&
                                filter.DestinationCategories.Contains(inventory.Key) &&
                                (displayDestinationCrossCharacter ||
                                 characterMonitor.BelongsToActiveCharacter(character.Key)))
                            {
                                if (!destinationInventories.ContainsKey(inventoryKey))
                                {
                                    destinationInventories.Add(inventoryKey, inventory.Value.Where(c => c.SortedContainer == type).ToList());
                                }
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
                            filteredSources[sourceInventory.Key].Add(filteredItem.Value);
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
                                var itemHashCode = destinationItem.GetHashCode();
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
                        var hashCode = sourceItem.GetHashCode();
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
                                    continue;
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

                        var nextSlot = slotsAvailable.First();
                        while (nextSlot.Value.Count == 0 && slotsAvailable.Count != 0)
                        {
                            slotsAvailable.Remove(nextSlot.Key);
                            if (slotsAvailable.Count == 0)
                            {
                                continue;
                            }

                            nextSlot = slotsAvailable.First();
                        }

                        if (nextSlot.Key.Item1 == sourceItem.RetainerId && nextSlot.Key.Item2.ToInventoryCategory() == sourceItem.SortedCategory)
                        {
                            continue;
                        }

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
                                    //PluginLog.Verbose("Added item to filter result in next available slot: " +
                                    //                  sourceItem.FormattedName);
                                    sortedItems.Add(new SortingResult(sourceInventory.Key.Item1, nextSlot.Key.Item1,
                                        sourceItem.SortedContainer, nextSlot.Key.Item2, nextEmptySlot.BagLocation(nextEmptySlot.SortedContainer),true, sourceItem,
                                        (int) sourceItem.TempQuantity));
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
                Dictionary<(ulong, InventoryCategory), List<InventoryItem>> sourceInventories = new();
                foreach (var character in inventories)
                {
                    foreach (var inventory in character.Value)
                    {
                        var inventoryKey = (character.Key, inventory.Key);
                        if (filter.SourceAllRetainers.HasValue && filter.SourceAllRetainers.Value && characterMonitor.IsRetainer(character.Key) && (displaySourceCrossCharacter || characterMonitor.BelongsToActiveCharacter(character.Key)))
                        {
                            if (!sourceInventories.ContainsKey(inventoryKey))
                            {
                                sourceInventories.Add(inventoryKey, inventory.Value);
                            }
                        }
                        if (filter.SourceAllCharacters.HasValue && filter.SourceAllCharacters.Value && !characterMonitor.IsRetainer(character.Key) && (displaySourceCrossCharacter || characterMonitor.ActiveCharacter == character.Key))
                        {
                            if (!sourceInventories.ContainsKey(inventoryKey))
                            {
                                sourceInventories.Add(inventoryKey, inventory.Value);
                            }
                        }
                        if (filter.SourceInventories.Contains(inventoryKey) && (displaySourceCrossCharacter || characterMonitor.BelongsToActiveCharacter(character.Key)))
                        {
                            if (!sourceInventories.ContainsKey(inventoryKey))
                            {
                                sourceInventories.Add(inventoryKey, inventory.Value);
                            }
                        }
                        if (filter.SourceCategories != null && filter.SourceCategories.Contains(inventoryKey.Item2) && (displaySourceCrossCharacter || characterMonitor.BelongsToActiveCharacter(character.Key)))
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
                Dictionary<(ulong, InventoryCategory), List<FilteredItem>> filteredSources = new();
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
                        var filteredItem = filter.FilterItem(item);
                        if (filteredItem != null)
                        {
                            filteredSources[sourceInventory.Key].Add(filteredItem.Value);
                        }
                    }
                }
                if (filter.DuplicatesOnly.HasValue && filter.DuplicatesOnly == true)
                {
                    foreach (var filteredSource in filteredSources)
                    {
                        foreach (var item in filteredSource.Value)
                        {
                            var hashCode = item.GetHashCode();
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
                            if (duplicateItems.Contains(item.GetHashCode()))
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
            else
            {
                items = Service.ExcelCache.AllItems.Select(c => c.Value).Where(filter.FilterItem).ToList();
                
            }

            
            return new FilterResult(sortedItems, unsortableItems, items);
        }

        public Task<FilterResult> GenerateFilteredList(Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>> inventories)
        {
            return Task<FilterResult>.Factory.StartNew(() => GenerateFilteredListInternal(this, inventories));
        }
        
        #endregion

        public FilterConfiguration? Clone()
        {
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
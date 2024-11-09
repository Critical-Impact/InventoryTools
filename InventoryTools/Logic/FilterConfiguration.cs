using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;

using ImGuiNET;
using InventoryTools.Attributes;
using InventoryTools.Converters;
using InventoryTools.Logic.Filters;
using Newtonsoft.Json;

namespace InventoryTools.Logic
{
    public class FilterConfiguration
    {
        private List<(ulong, InventoryCategory)> _destinationInventories = new();
        private bool _displayInTabs = true;
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
        private List<CuratedItem>? _curatedItems;
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
        private List<ColumnConfiguration>? _columns;
        private List<ColumnConfiguration>? _craftColumns;
        private string? _icon;
        private HashSet<uint>? _sourceWorlds;
        private Vector4 _craftHeaderColour = new (0.0f, 0.439f, 1f, 1f);
        private CraftDisplayMode _craftDisplayMode = CraftDisplayMode.SingleTable;
        private bool _isEphemeralCraftList = false;
        private string? _defaultSortColumn = null;
        private ImGuiSortDirection? _defaultSortOrder = null;

        /// <summary>
        /// Is the configuration dirty?
        /// </summary>
        [JsonIgnore] public bool ConfigurationDirty { get; set; }
        /// <summary>
        /// Is the table related configuration dirty?
        /// </summary>
        [JsonIgnore] public bool TableConfigurationDirty { get; set; }
        /// <summary>
        /// Does the list need a refresh?
        /// </summary>
        [JsonIgnore] [DefaultValue(true)] public bool NeedsRefresh { get; set; } = true;
        /// <summary>
        /// Is this list currently being refreshed?
        /// </summary>
        [JsonIgnore] public bool Refreshing { get; set; } = false;
        /// <summary>
        /// Should this list be allowed to refresh? This stops the list from being refreshed until it's been seen
        /// </summary>
        [JsonIgnore] public bool AllowRefresh { get; set; } = false;
        /// <summary>
        /// Is this list being viewed in a window?
        /// </summary>
        [JsonIgnore] public bool Active { get; set; }

        //Crafting
        private CraftList? _craftList = null;
        private bool? _simpleCraftingMode = null;
        private bool? _useORFiltering = null;


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
        private List<SearchResult>? _searchResults = null;

        private string? _tableId = null;
        private string? _craftTableId = null;
        public HighlightMode HighlightMode { get; set; } = HighlightMode.Never;

        public List<CuratedItem>? CuratedItems
        {
            get => _curatedItems;
            set => _curatedItems = value;
        }

        [JsonIgnore]
        public List<SearchResult>? SearchResults
        {
            get => _searchResults;
            set => _searchResults = value;
        }

        public FilterConfiguration(string name, string key, FilterType filterType)
        {
            FilterType = filterType;
            Name = name;
            Key = key;
        }

        public FilterConfiguration(string name, FilterType filterType)
        {
            FilterType = filterType;
            Name = name;
            Key = Guid.NewGuid().ToString("N");
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

        public FilterConfiguration()
        {
        }


        public List<(ulong, InventoryCategory)> SourceInventories
        {
            get => _sourceInventories;
            set { _sourceInventories = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public List<uint> ItemUiCategoryId
        {
            get => _itemUiCategoryId;
            set { _itemUiCategoryId = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public List<uint> ItemSearchCategoryId
        {
            get => _itemSearchCategoryId;
            set { _itemSearchCategoryId = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public List<uint> EquipSlotCategoryId
        {
            get => _equipSlotCategoryId;
            set { _equipSlotCategoryId = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        [DefaultValue(24)]
        public int TableHeight
        {
            get => _tableHeight;
            set { _tableHeight = value;
                NeedsRefresh = true;
                TableConfigurationDirty = true;
            }
        }

        [DefaultValue(24)]
        public int CraftTableHeight
        {
            get => _craftTableHeight;
            set { _craftTableHeight = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        [Vector4Default("0.0, 0.439, 1, 1")]
        public Vector4 CraftHeaderColour
        {
            get => _craftHeaderColour;
            set { _craftHeaderColour = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public List<uint> ItemSortCategoryId
        {
            get => _itemSortCategoryId;
            set { _itemSortCategoryId = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public List<(ulong, InventoryCategory)> DestinationInventories
        {
            get => _destinationInventories;
            set { _destinationInventories = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public bool? IsHq
        {
            get => _isHq;
            set { _isHq = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public bool? IsCollectible
        {
            get => _isCollectible;
            set { _isCollectible = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public bool IsEphemeralCraftList
        {
            get => _isEphemeralCraftList;
            set { _isEphemeralCraftList = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
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
                    ConfigurationDirty = true;
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
                    var actualName = Name == "" ? "Untitled" : Name;
                    if (IsEphemeralCraftList)
                    {
                        actualName += " (*)";
                    }
                    _nameAsBytes = System.Text.Encoding.UTF8.GetBytes(actualName);
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
                ConfigurationDirty = true;
            }
        }

        [Obsolete]
        public bool? FilterItemsInRetainers
        {
            get => _filterItemsInRetainers;
            set { _filterItemsInRetainers = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
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
                ConfigurationDirty = true;
            }
        }

        public bool DisplayInTabs
        {
            get => _displayInTabs;
            set { _displayInTabs = value;
                ConfigurationDirty = true;
            }
        }

        public bool OpenAsWindow
        {
            get => _openAsWindow;
            set { _openAsWindow = value;
                ConfigurationDirty = true;
            }
        }

        public string Quantity
        {
            get => _quantity;
            set { _quantity = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public string ILevel
        {
            get => _iLevel;
            set { _iLevel = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public string Spiritbond
        {
            get => _spiritbond;
            set { _spiritbond = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public string NameFilter
        {
            get => _nameFilter;
            set { _nameFilter = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
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
                ConfigurationDirty = true;
            }
        }

        public bool? SourceAllHouses
        {
            get => _sourceAllHouses;
            set { _sourceAllHouses = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public bool? SourceAllFreeCompanies
        {
            get => _sourceAllFreeCompanies;
            set { _sourceAllFreeCompanies = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public string? HighlightWhen
        {
            get => _highlightWhen;
            set
            {
                _highlightWhen = value;
                ConfigurationDirty = true;
            }
        }


        public bool? UseORFiltering
        {
            get => _useORFiltering;
            set
            {
                _useORFiltering = value;
                ConfigurationDirty = true;
            }
        }

        public bool? SourceAllCharacters
        {
            get => _sourceAllCharacters;
            set { _sourceAllCharacters = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public bool? DestinationAllRetainers
        {
            get => _destinationAllRetainers;
            set { _destinationAllRetainers = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public bool? DestinationAllFreeCompanies
        {
            get => _destinationAllFreeCompanies;
            set { _destinationAllFreeCompanies = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public bool? DestinationAllHouses
        {
            get => _destinationAllHouses;
            set { _destinationAllHouses = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public bool? SourceIncludeCrossCharacter
        {
            get => _sourceIncludeCrossCharacter;
            set { _sourceIncludeCrossCharacter = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public bool? DestinationIncludeCrossCharacter
        {
            get => _destinationIncludeCrossCharacter;
            set { _destinationIncludeCrossCharacter = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public int? FreezeColumns
        {
            get => _freezeColumns;
            set { _freezeColumns = value;
                NeedsRefresh = true;
                TableConfigurationDirty = true;
            }
        }

        public int? FreezeCraftColumns
        {
            get => _freezeCraftColumns;
            set { _freezeCraftColumns = value;
                NeedsRefresh = true;
                TableConfigurationDirty = true;
            }
        }

        public HashSet<InventoryCategory>? DestinationCategories
        {
            get => _destinationCategories;
            set { _destinationCategories = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public HashSet<InventoryCategory>? SourceCategories
        {
            get => _sourceCategories;
            set { _sourceCategories = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public bool? DestinationAllCharacters
        {
            get => _destinationAllCharacters;
            set { _destinationAllCharacters = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public string ShopSellingPrice
        {
            get => _shopSellingPrice;
            set
            {
                _shopSellingPrice = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public string ShopBuyingPrice
        {
            get => _shopBuyingPrice;
            set
            {
                _shopBuyingPrice = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public string MarketAveragePrice
        {
            get => _marketAveragePrice;
            set
            {
                _marketAveragePrice = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public string MarketTotalAveragePrice
        {
            get => _marketTotalAveragePrice;
            set
            {
                _marketTotalAveragePrice = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public bool? CanBeBought
        {
            get => _canBeBought;
            set
            {
                _canBeBought = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public bool? IsAvailableAtTimedNode
        {
            get => _isAvailableAtTimedNode;
            set
            {
                _isAvailableAtTimedNode = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
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
                ConfigurationDirty = true;
            }
        }

        public Vector4? DestinationHighlightColor
        {
            get => _destinationHighlightColor;
            set
            {
                _destinationHighlightColor = value;
                ConfigurationDirty = true;
            }
        }

        public Vector4? RetainerListColor
        {
            get => _retainerListColor;
            set
            {
                _retainerListColor = value;
                ConfigurationDirty = true;
            }
        }

        public Vector4? TabHighlightColor
        {
            get => _tabHighlightColor;
            set
            {
                _tabHighlightColor = value;
                ConfigurationDirty = true;
            }
        }

        public bool? InvertHighlighting
        {
            get => _invertHighlighting;
            set
            {
                _invertHighlighting = value;
                ConfigurationDirty = true;
            }
        }

        public bool? InvertDestinationHighlighting
        {
            get => _invertDestinationHighlighting;
            set
            {
                _invertDestinationHighlighting = value;
                ConfigurationDirty = true;
            }
        }

        public bool? InvertTabHighlighting
        {
            get => _invertTabHighlighting;
            set
            {
                _invertTabHighlighting = value;
                ConfigurationDirty = true;
            }
        }

        public bool? HighlightDestination
        {
            get => _highlightDestination;
            set
            {
                _highlightDestination = value;
                ConfigurationDirty = true;
            }
        }

        public bool? HighlightDestinationEmpty
        {
            get => _highlightDestinationEmpty;
            set
            {
                _highlightDestinationEmpty = value;
                ConfigurationDirty = true;
            }
        }

        public bool? IgnoreHQWhenSorting
        {
            get => _ignoreHQWhenSorting;
            set
            {
                _ignoreHQWhenSorting = value;
                ConfigurationDirty = true;
            }
        }

        public bool CraftListDefault
        {
            get => _craftListDefault;
            set
            {
                _craftListDefault = value;
                ConfigurationDirty = true;
            }
        }

        public bool? SimpleCraftingMode
        {
            get => _simpleCraftingMode;
            set
            {
                _simpleCraftingMode = value;
                TableConfigurationDirty = true;
            }
        }

        public HashSet<uint>? SourceWorlds
        {
            get => _sourceWorlds;
            set { _sourceWorlds = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public string? DefaultSortColumn
        {
            get => _defaultSortColumn;
            set { _defaultSortColumn = value;
                NeedsRefresh = true;
                TableConfigurationDirty = true;
                ConfigurationDirty = true;
            }
        }

        public ImGuiSortDirection? DefaultSortOrder
        {
            get => _defaultSortOrder;
            set { _defaultSortOrder = value;
                NeedsRefresh = true;
                TableConfigurationDirty = true;
                ConfigurationDirty = true;
            }
        }

        public CraftDisplayMode CraftDisplayMode
        {
            get => _craftDisplayMode;
            set { _craftDisplayMode = value;
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public bool FilterItem(List<IFilter> filters, ItemRow item)
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
            for (var index = 0; index < filters.Count; index++)
            {
                var filter = filters[index];
                if (UseORFiltering != null && UseORFiltering == true)
                {
                    if (filter.FilterItem(this, (ItemRow)item) == true)
                    {
                        matchesAny = true;
                    }
                }
                else
                {
                    if (filter.FilterItem(this, (ItemRow)item) == false)
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

        public bool FilterItem(List<IFilter> filters, InventoryChange item)
        {
            var matchesAny = false;
            for (var index = 0; index < filters.Count; index++)
            {
                var filter = filters[index];
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

        public FilteredItem? FilterItem(List<IFilter> filters, InventoryItem item)
        {
            if (item.ItemId == 0)
            {
                return null;
            }
            uint? requiredAmount = null;
            if (FilterType == FilterType.CraftFilter)
            {
                var requiredMaterial = CraftList.GetItemById(item.ItemId, item.IsHQ, item.Item.Base.CanBeHq);
                if (requiredMaterial == null)
                {
                    return null;
                }

                requiredAmount = CraftList.BeenUpdated ? requiredMaterial.QuantityWillRetrieve : requiredMaterial.QuantityRequired;
            }

            var matchesAny = false;
            for (var index = 0; index < filters.Count; index++)
            {
                var filter = filters[index];
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

        public ColumnConfiguration? GetColumn(string columnKey)
        {
            var columns = _columns;

            return columns?.FirstOrDefault(c => c?.Key == columnKey, null);
        }

        public ColumnConfiguration? GetCraftColumn(string columnKey)
        {
            var columns = _craftColumns;

            return columns?.FirstOrDefault(c => c?.Key == columnKey, null);
        }

        public void AddColumn(ColumnConfiguration column, bool notify = true)
        {
            if (_columns == null)
            {
                _columns = new List<ColumnConfiguration>();
            }
            _columns.Add(column);
            if (notify)
            {
                GenerateNewTableId();
                TableConfigurationDirty = true;
            }
        }

        public void AddCraftColumn(ColumnConfiguration craftColumn, bool notify = true)
        {
            if (_craftColumns == null)
            {
                _craftColumns = new List<ColumnConfiguration>();
            }
            _craftColumns.Add(craftColumn);
            if (notify)
            {
                GenerateNewTableId();
                TableConfigurationDirty = true;
            }
        }

        public void AddCuratedItem(CuratedItem curatedItem)
        {
            if (_curatedItems == null)
            {
                _curatedItems = new();
            }
            _curatedItems.Add(curatedItem);
            ConfigurationDirty = true;
        }

        public void RemoveCuratedItem(CuratedItem curatedItem)
        {
            if (_curatedItems == null)
            {
                _curatedItems = new();
            }
            _curatedItems.Remove(curatedItem);
            ConfigurationDirty = true;
        }

        public void ClearCuratedItems()
        {
            if (_curatedItems == null)
            {
                _curatedItems = new();
            }
            _curatedItems.Clear();
            ConfigurationDirty = true;
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
                else if (FilterType.HasFlag(FilterType.HistoryFilter))
                {
                    return "History List";
                }
                else if (FilterType.HasFlag(FilterType.CuratedList))
                {
                    return "Curated List";
                }
                return "";
            }
        }

        [JsonConverter(typeof(ColumnConverter))]
        public List<ColumnConfiguration>? Columns
        {
            get => _columns;
            set
            {
                _columns = value;
                TableConfigurationDirty = true;
            }
        }

        [JsonConverter(typeof(ColumnConverter))]
        public List<ColumnConfiguration>? CraftColumns
        {
            get => _craftColumns;
            set
            {
                _craftColumns = value;
                TableConfigurationDirty = true;
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
            ConfigurationDirty = true;
        }

        public void UpdateColorFilter(string key, Vector4 value)
        {
            if (ColorFilters.ContainsKey(key) && ColorFilters[key] == value)
            {
                return;
            }

            ColorFilters[key] = value;
            NeedsRefresh = true;
            ConfigurationDirty = true;
        }

        public void RemoveBooleanFilter(string key)
        {
            if (BooleanFilters.ContainsKey(key))
            {
                BooleanFilters.Remove(key);
                NeedsRefresh = true;
                ConfigurationDirty = true;
            }
        }

        public void RemoveColorFilter(string key)
        {
            if (ColorFilters.ContainsKey(key))
            {
                ColorFilters.Remove(key);
                NeedsRefresh = true;
                ConfigurationDirty = true;
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
            ConfigurationDirty = true;
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
            ConfigurationDirty = true;
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
            ConfigurationDirty = true;
        }

        public void UpdateUintChoiceFilter(string key, List<uint> value)
        {
            UintChoiceFilters[key] = value;
            NeedsRefresh = true;
            ConfigurationDirty = true;
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
            ConfigurationDirty = true;
        }

        public void UpdateUlongChoiceFilter(string key, List<ulong> value)
        {
            UlongChoiceFilters[key] = value;
            NeedsRefresh = true;
            ConfigurationDirty = true;
        }

        public void UpdateStringChoiceFilter(string key, List<string> value)
        {
            StringChoiceFilters[key] = value;
            NeedsRefresh = true;
            ConfigurationDirty = true;
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
                ConfigurationDirty = true;
            }
        }

        public void SetOrder(uint order)
        {
            _order = order;
        }

        public void NotifyConfigurationChange()
        {
            ConfigurationDirty = true;
        }

        public void AddItemsToList(List<(uint, uint)> items)
        {
            if (this.FilterType == FilterType.CraftFilter)
            {
                foreach (var item in items)
                {
                    bool isHq = item.Item1 > 1000000;
                    var itemId = item.Item1 % 500000;
                    CraftList.AddCraftItem(itemId, item.Item2, isHq ? FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.HighQuality : FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.None);
                }
                NeedsRefresh = true;
            }
            else if (this.FilterType == FilterType.CuratedList)
            {
                foreach (var item in items)
                {
                    bool isHq = item.Item1 > 1000000;
                    var itemId = item.Item1 % 500000;
                    AddCuratedItem(new CuratedItem(itemId, item.Item2,
                        isHq ? FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.HighQuality : FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags.None));
                }

                NeedsRefresh = true;
            }
        }


        public FilterConfiguration? Clone()
        {
            SearchResults = null;
            var clone = this.Copy();
            SearchResults = null;
            if (clone != null && this.FilterType == FilterType.CraftFilter)
            {
                var clonedCraftList = CraftList.Clone();
                clone._craftList = clonedCraftList;
            }
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
using System;
using System.Collections.Generic;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Models;
using Dalamud.Logging;
using Dalamud.Plugin;
using InventoryTools.Extensions;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;

namespace InventoryTools.Logic
{
    public class FilterConfiguration
    {
        public delegate void ConfigurationChangedDelegate(FilterConfiguration filterConfiguration);
        public delegate void ListUpdatedDelegate(FilterConfiguration filterConfiguration);

        private List<(ulong, InventoryCategory)> _destinationInventories = new();
        private bool _displayInTabs = false;
        private bool? _duplicatesOnly;
        private List<uint> _equipSlotCategoryId = new();
        private bool? _isCollectible;
        private bool? _isHQ;
        private List<uint> _itemSearchCategoryId = new();
        private List<uint> _itemSortCategoryId = new();
        private List<uint> _itemUiCategoryId = new();
        private string _name = "";
        private string _key = "";
        private ulong _ownerId = 0;
        private bool? _filterItemsInRetainers;
        private bool? _sourceAllRetainers;
        private bool? _sourceAllCharacters;
        private bool? _destinationAllRetainers;
        private bool? _destinationAllCharacters;
        private string _quantity = "";
        private string _spiritbond = "";
        private string _nameFilter = "";
        private string _iLevel = "";
        private string _shopSellingPrice = "";
        private string _shopBuyingPrice = "";
        private bool? _canBeBought;
        private List<(ulong, InventoryCategory)> _sourceInventories = new();
        private FilterType _filterType;
        
        [JsonIgnore]
        private FilterResult? _filterResult = null;

        public bool NeedsRefresh { get; set; } = true;
        public HighlightMode HighlightMode { get; set; } = HighlightMode.Never;

        [JsonIgnore]
        public FilterResult? FilterResult
        {
            get
            {
                if (_filterResult == null || NeedsRefresh)
                {
                    _filterResult = FilterManager.GenerateFilteredList(this,
                        PluginLogic.InventoryMonitor.Inventories);
                    NeedsRefresh = false;
                    ListUpdated?.Invoke(this);
                }
                return _filterResult;
            }
            set => _filterResult = value;
        }

        public FilterConfiguration(string name, string key, FilterType filterType)
        {
            FilterType = filterType;
            Name = name;
            Key = key;
        }

        public void Refresh()
        {
            
        }

        public FilterTable GenerateTable()
        {
            FilterTable table = new FilterTable(this);
            if (FilterType == FilterType.SearchFilter)
            {
                table.AddColumn(new NameColumn());
                table.AddColumn(new TypeColumn());
                table.AddColumn(new SourceColumn());
                table.AddColumn(new LocationColumn());
                table.AddColumn(new QuantityColumn());
                table.AddColumn(new ItemILevelColumn());
                table.AddColumn(new UiCategoryColumn());
                table.AddColumn(new SearchCategoryColumn());
                table.ShowFilterRow = true;
            }
            else
            {
                table.AddColumn(new NameColumn());
                table.AddColumn(new TypeColumn());
                table.AddColumn(new SourceColumn());
                table.AddColumn(new LocationColumn());
                table.AddColumn(new DestinationColumn());
                table.AddColumn(new QuantityColumn());
                table.AddColumn(new ItemILevelColumn());
                table.AddColumn(new UiCategoryColumn());
                table.AddColumn(new SearchCategoryColumn());
                table.ShowFilterRow = true;
            }

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
            get => _isHQ;
            set { _isHQ = value;
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

        public string Quantity
        {
            get => _quantity;
            set { _quantity = value;
                NeedsRefresh = true;
                ConfigurationChanged?.Invoke(this);
            }
        }

        public string iLevel
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


        public ulong OwnerId
        {
            get => _ownerId;
            set => _ownerId = value;
        }

        public event ConfigurationChangedDelegate ConfigurationChanged;
        public event ListUpdatedDelegate ListUpdated;

        public bool FilterItem(InventoryItem item)
        {
            var matches = true;
            if (item.IsEmpty)
            {
                return false;
            }
            if (this.ItemUiCategoryId.Count != 0)
            {
                if (!ItemUiCategoryId.Contains(item.ItemUICategory.RowId))
                {
                    matches = false;
                }
            }
            if (this.ItemSearchCategoryId.Count != 0)
            {
                if (!ItemSearchCategoryId.Contains(item.ItemSearchCategory.RowId))
                {
                    matches = false;
                }
                
            }
            if (this.ItemSortCategoryId.Count != 0)
            {
                if (!ItemSortCategoryId.Contains(item.ItemSortCategory.RowId))
                {
                    matches = false;
                }
                
            }
            if (this._isHQ != null)
            {
                if (item.IsHQ != IsHq)
                {
                    matches = false;
                }
            }
            if (this.IsCollectible != null)
            {
                if (item.IsCollectible != IsCollectible)
                {
                    matches = false;
                }
                
            }

            if (!string.IsNullOrEmpty(this.NameFilter))
            {
                if (!item.Item.Name.ToString().ToLower().PassesFilter(this.NameFilter.ToLower()))
                {
                    matches = false;
                }
            }

            if (!string.IsNullOrEmpty(this.Quantity))
            {
                if (!item.Quantity.PassesFilter(this.Quantity))
                {
                    matches = false;
                }
            }

            if (!string.IsNullOrEmpty(this.iLevel))
            {
                if (!item.Item.LevelItem.Row.PassesFilter(this.iLevel) || item.EquipSlotCategory.RowId == 0)
                {
                    matches = false;
                }
            }

            if (!string.IsNullOrEmpty(this.Spiritbond))
            {
                var spiritBond = item.Spiritbond;
                var itemSpiritBond = spiritBond / 100;
                if (!itemSpiritBond.PassesFilter(this.Spiritbond) || item.IsCollectible)
                {
                    matches = false;
                }
            }

            if (!string.IsNullOrEmpty(this.ShopBuyingPrice))
            {
                if (!item.Item.PriceMid.PassesFilter(this.ShopBuyingPrice) || !item.CanBeBought)
                {
                    matches = false;
                }
            }

            if (!string.IsNullOrEmpty(this.ShopSellingPrice))
            {
                if (!item.Item.PriceLow.PassesFilter(this.ShopSellingPrice))
                {
                    matches = false;
                }
            }
            
            return matches;
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
                if (FilterType == FilterType.SearchFilter)
                {
                    return "Search Filter";
                }
                else if (FilterType == FilterType.SortingFilter)
                {
                    return "Sort Filter";
                }
                return "";
            }
        }

        public bool InActiveInventories(ulong activeCharacterId, ulong activeRetainerId, ulong sourceCharacterId,
            ulong destinationCharacterId)
        {
            if (FilterItemsInRetainers.HasValue)
            {
                if (FilterItemsInRetainers.Value && activeRetainerId != 0)
                {
                    PluginLog.Log("filtering, " + activeCharacterId + ":" + activeRetainerId + ":" + sourceCharacterId + ":" + destinationCharacterId);
                    if (activeCharacterId == sourceCharacterId && activeRetainerId == destinationCharacterId)
                    {
                        return true;
                    }
                    return false;
                }
            }
            return true;
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
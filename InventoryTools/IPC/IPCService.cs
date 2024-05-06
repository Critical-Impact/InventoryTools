using System;
using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using InventoryTools.Lists;
using InventoryTools.Logic;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;
using InventoryItem = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem;

namespace InventoryTools.IPC;

public class IPCService : IDisposable
{
    private readonly ICallGateProvider<uint, ulong?, uint>? _inventoryCountByType;
    private readonly ICallGateProvider<uint[], ulong?, uint>? _inventoryCountByTypes;
    private readonly ICallGateProvider<uint, ulong, int, uint>? _itemCount;
    private readonly ICallGateProvider<uint, ulong, int, uint>? _itemCountHQ;
    private readonly ICallGateProvider<uint, bool, uint[], uint>? _itemCountOwned;
    private readonly ICallGateProvider<string, bool>? _enableUiFilter;
    private readonly ICallGateProvider<bool>? _disableUiFilter;
    private readonly ICallGateProvider<string, bool>? _toggleUiFilter;
    private readonly ICallGateProvider<string, bool>? _enableBackgroundFilter;
    private readonly ICallGateProvider<bool>? _disableBackgroundFilter;
    private readonly ICallGateProvider<string, bool>? _toggleBackgroundFilter;
    private readonly ICallGateProvider<string, bool>? _enableCraftList;
    private readonly ICallGateProvider<bool>? _disableCraftList;
    private readonly ICallGateProvider<string, bool>? _toggleCraftList;
    private readonly ICallGateProvider<string, uint, uint, bool>? _addItemToCraftList;
    private readonly ICallGateProvider<string, uint, uint, bool>? _removeItemFromCraftList;
    private readonly ICallGateProvider<string, Dictionary<uint, uint>>? _getFilterItems;
    private readonly ICallGateProvider<string, Dictionary<uint, uint>>? _getCraftItems;
    private readonly ICallGateProvider<Dictionary<uint, uint>>? _getRetrievalItems;
    private readonly ICallGateProvider<ulong, HashSet<ulong[]>> _getCharacterItems;
    private readonly ICallGateProvider<bool, HashSet<ulong>> _getCharactersOwnedByActive;
    private readonly ICallGateProvider<ulong, uint, HashSet<ulong[]>> _getCharacterItemsByType;
    private readonly ICallGateProvider<(uint, InventoryItem.ItemFlags, ulong, uint), bool>? _itemAdded;
    private readonly ICallGateProvider<(uint, InventoryItem.ItemFlags, ulong, uint), bool>? _itemRemoved;
    private readonly ICallGateProvider<Dictionary<string,string>>? _getCraftLists;
    private readonly ICallGateProvider<Dictionary<string,string>>? _getSearchFilters;
    private readonly ICallGateProvider<string, Dictionary<uint, uint>, string>? _addNewCraftList;
    private readonly ICallGateProvider<ulong>? _currentCharacter;
    private readonly ICallGateProvider<ulong?, bool>? _retainerChanged;
    private readonly ICallGateProvider<bool>? _isInitialized;
    private readonly ICallGateProvider<bool, bool>? _initialized;
    private readonly bool _initalizedIpc;

    private readonly ILogger<IPCService> _logger;
    private readonly ICharacterMonitor _characterMonitor;
    private readonly IListService _listService;
    private readonly IInventoryMonitor _inventoryMonitor;
    private readonly ListFilterService _listFilterService;
    private bool _disposed;

    public IPCService(DalamudPluginInterface pluginInterface, ILogger<IPCService> logger, ICharacterMonitor characterMonitor, IListService listService, IInventoryMonitor inventoryMonitor, ListFilterService listFilterService)
    {
        _logger = logger;
        _characterMonitor = characterMonitor;
        _listService = listService;
        _inventoryMonitor = inventoryMonitor;
        _listFilterService = listFilterService;

        _inventoryCountByType = pluginInterface.GetIpcProvider<uint, ulong?, uint>("AllaganTools.InventoryCountByType");
        _inventoryCountByType.RegisterFunc(InventoryCountByType);
        
        _inventoryCountByTypes = pluginInterface.GetIpcProvider<uint[], ulong?, uint>("AllaganTools.InventoryCountByTypes");
        _inventoryCountByTypes.RegisterFunc(InventoryCountByTypes);
        
        _itemCount = pluginInterface.GetIpcProvider<uint, ulong, int, uint>("AllaganTools.ItemCount");
        _itemCount.RegisterFunc(ItemCount);

        _itemCountHQ = pluginInterface.GetIpcProvider<uint, ulong, int, uint>("AllaganTools.ItemCountHQ");
        _itemCountHQ.RegisterFunc(ItemCountHQ);

        _itemCountOwned = pluginInterface.GetIpcProvider<uint, bool, uint[], uint>("AllaganTools.ItemCountOwned");
        _itemCountOwned.RegisterFunc(ItemCountOwned);

        _enableUiFilter = pluginInterface.GetIpcProvider<string, bool>("AllaganTools.EnableUiFilter");
        _enableUiFilter.RegisterFunc(EnableUiFilter);

        _disableUiFilter = pluginInterface.GetIpcProvider<bool>("AllaganTools.DisableUiFilter");
        _disableUiFilter.RegisterFunc(DisableUiFilter);

        _toggleUiFilter = pluginInterface.GetIpcProvider<string, bool>("AllaganTools.ToggleUiFilter");
        _toggleUiFilter.RegisterFunc(ToggleUiFilter);

        _enableBackgroundFilter = pluginInterface.GetIpcProvider<string, bool>("AllaganTools.EnableBackgroundFilter");
        _enableBackgroundFilter.RegisterFunc(EnableBackgroundFilter);

        _disableBackgroundFilter = pluginInterface.GetIpcProvider<bool>("AllaganTools.DisableBackgroundFilter");
        _disableBackgroundFilter.RegisterFunc(DisableBackgroundFilter);

        _toggleBackgroundFilter = pluginInterface.GetIpcProvider<string, bool>("AllaganTools.ToggleBackgroundFilter");
        _toggleBackgroundFilter.RegisterFunc(ToggleBackgroundFilter);

        _enableCraftList = pluginInterface.GetIpcProvider<string, bool>("AllaganTools.EnableCraftList");
        _enableCraftList.RegisterFunc(EnableCraftList);

        _disableCraftList = pluginInterface.GetIpcProvider<bool>("AllaganTools.DisableCraftList");
        _disableCraftList.RegisterFunc(DisableCraftList);

        _toggleCraftList = pluginInterface.GetIpcProvider<string, bool>("AllaganTools.ToggleCraftList");
        _toggleCraftList.RegisterFunc(ToggleCraftList);

        _addItemToCraftList = pluginInterface.GetIpcProvider<string, uint, uint, bool>("AllaganTools.AddItemToCraftList");
        _addItemToCraftList.RegisterFunc(AddItemToCraftList);

        _removeItemFromCraftList = pluginInterface.GetIpcProvider<string, uint, uint, bool>("AllaganTools.RemoveItemFromCraftList");
        _removeItemFromCraftList.RegisterFunc(RemoveItemFromCraftList);

        _getFilterItems = pluginInterface.GetIpcProvider<string, Dictionary<uint, uint>>("AllaganTools.GetFilterItems");
        _getFilterItems.RegisterFunc(GetFilterItems);

        _getCraftItems = pluginInterface.GetIpcProvider<string, Dictionary<uint, uint>>("AllaganTools.GetCraftItems");
        _getCraftItems.RegisterFunc(GetCraftItems);

        _getRetrievalItems = pluginInterface.GetIpcProvider<Dictionary<uint, uint>>("AllaganTools.GetRetrievalItems");
        _getRetrievalItems.RegisterFunc(GetRetrievalItems);

        _getCharacterItems = pluginInterface.GetIpcProvider<ulong, HashSet<ulong[]>>("AllaganTools.GetCharacterItems");
        _getCharacterItems.RegisterFunc(GetCharacterItems);

        _getCharactersOwnedByActive = pluginInterface.GetIpcProvider<bool, HashSet<ulong>>("AllaganTools.GetCharactersOwnedByActive");
        _getCharactersOwnedByActive.RegisterFunc(GetCharactersOwnedByActive);
        _getCraftItems.RegisterFunc(GetCraftItems);

        _getCharacterItemsByType = pluginInterface.GetIpcProvider<ulong,uint, HashSet<ulong[]>>("AllaganTools.GetCharacterItemsByType");
        _getCharacterItemsByType.RegisterFunc(GetCharacterItemsByType);

        _getSearchFilters = pluginInterface.GetIpcProvider<Dictionary<string,string>>("AllaganTools.GetSearchFilters");
        _getSearchFilters.RegisterFunc(GetSearchFilters);

        _getCraftLists = pluginInterface.GetIpcProvider<Dictionary<string,string>>("AllaganTools.GetCraftLists");
        _getCraftLists.RegisterFunc(GetCraftLists);

        _addNewCraftList = pluginInterface.GetIpcProvider<string, Dictionary<uint, uint>, string>("AllaganTools.AddNewCraftList");
        _addNewCraftList.RegisterFunc(AddNewCraftList);

        _currentCharacter = pluginInterface.GetIpcProvider<ulong>("AllaganTools.CurrentCharacter");
        _currentCharacter.RegisterFunc(CurrentCharacter);

        _isInitialized = pluginInterface.GetIpcProvider<bool>("AllaganTools.IsInitialized");
        _isInitialized.RegisterFunc(IsInitialized);

        
        //Events
        _retainerChanged = pluginInterface.GetIpcProvider<ulong?, bool>("AllaganTools.RetainerChanged");
        _itemAdded = pluginInterface.GetIpcProvider<(uint, InventoryItem.ItemFlags, ulong, uint), bool>("AllaganTools.ItemAdded");
        _itemRemoved = pluginInterface.GetIpcProvider<(uint, InventoryItem.ItemFlags, ulong, uint), bool>("AllaganTools.ItemRemoved");
        _initialized = pluginInterface.GetIpcProvider<bool, bool>("AllaganTools.Initialized");

        //External Events
        characterMonitor.OnActiveRetainerChanged += CharacterMonitorOnOnActiveRetainerChanged;
        _inventoryMonitor.OnInventoryChanged += InventoryMonitorOnOnInventoryChanged;

        _initalizedIpc = true;
        
        _initialized.SendMessage(true);
    }

    private void InventoryMonitorOnOnInventoryChanged(List<InventoryChange> inventoryChanges, InventoryMonitor.ItemChanges? changedItems)
    {
        if (changedItems != null)
        {
            foreach (var changedItem in changedItems.NewItems)
            {
                if (changedItem.ItemId != 1)
                {
                    _itemAdded?.SendMessage((changedItem.ItemId, changedItem.Flags, changedItem.CharacterId,
                        (uint)changedItem.Quantity));
                }
            }

            foreach (var changedItem in changedItems.RemovedItems)
            {
                if (changedItem.ItemId != 1)
                {
                    _itemRemoved?.SendMessage((changedItem.ItemId, changedItem.Flags, changedItem.CharacterId,
                        (uint)changedItem.Quantity));
                }
            }
        }
    }

    private uint ItemCount(uint itemId, ulong characterId, int inventoryType)
    {
        return (uint)_inventoryMonitor.AllItems.Where(c => c.ItemId == itemId && (inventoryType == -1 || (uint)c.SortedContainer == inventoryType) && (c.RetainerId == characterId)).Sum(c => c.Quantity);
    }

    private uint ItemCountHQ(uint itemId, ulong characterId, int inventoryType)
    {
        return (uint)_inventoryMonitor.AllItems.Where(c => c.ItemId == itemId && c.Flags == InventoryItem.ItemFlags.HQ && (inventoryType == -1 || (uint)c.SortedContainer == inventoryType) && (c.RetainerId == characterId)).Sum(c => c.Quantity);
    }

    private uint ItemCountOwned(uint itemId, bool currentCharacterOnly, uint[] inventoryTypes)
    {
        return (uint)_inventoryMonitor.AllItems.Where(item =>
                item.ItemId == itemId
                && inventoryTypes.Contains((uint)item.SortedContainer)
                && _characterMonitor.Characters.ContainsKey(item.RetainerId)
                && (!currentCharacterOnly || _characterMonitor.BelongsToActiveCharacter(item.RetainerId)))
            .Sum(c => c.Quantity);
    }

    private bool IsInitialized()
    {
        return _initalizedIpc;
    }

    private ulong CurrentCharacter()
    {
        return _characterMonitor.ActiveCharacterId;
    }

    private Dictionary<string, string> GetSearchFilters()
    {
        var filters = _listService.Lists.Where(c => c.FilterType == FilterType.SearchFilter);
        var keyNameDict = new Dictionary<string, string>();
        foreach (var filter in filters)
        {
            keyNameDict.Add(filter.Key, filter.Name);
        }
        return keyNameDict;
    }

    private string AddNewCraftList(string craftListName, Dictionary<uint, uint> items)
    {
        var newCraftFilter = _listService.AddNewCraftList(craftListName);
        foreach (var item in items)
        {
            newCraftFilter.CraftList.AddCraftItem(item.Key, item.Value);
        }

        return newCraftFilter.Key;
    }

    private Dictionary<string, string> GetCraftLists()
    {
        var craftLists = _listService.Lists.Where(c => c.FilterType == FilterType.CraftFilter && !c.CraftListDefault);
        var keyNameDict = new Dictionary<string, string>();
        foreach (var craftList in craftLists)
        {
            keyNameDict.Add(craftList.Key, craftList.Name);
        }

        return keyNameDict;
    }

    private Dictionary<uint, uint> GetCraftItems(string filterKey)
    {
        var filter = _listService.GetListByKeyOrName(filterKey);
        var craftItems = new Dictionary<uint, uint>();
        
        if (filter != null && filter.FilterType == FilterType.CraftFilter)
        {
            foreach (var craftItem in filter.CraftList.CraftItems)
            {
                if (craftItem.IsOutputItem)
                {
                    if (craftItems.ContainsKey(craftItem.ItemId))
                    {
                        craftItems[craftItem.ItemId] += craftItem.QuantityRequired;
                    }
                    else
                    {
                        craftItems.Add(craftItem.ItemId, craftItem.QuantityRequired);
                    }
                }
            }
        }

        return craftItems;
    }

    private Dictionary<uint, uint> GetRetrievalItems()
    {
        var filter = _listService.GetActiveCraftList();
        var retrievalItems = new Dictionary<uint, uint>();

        if (filter != null && filter.FilterType == FilterType.CraftFilter)
        {
            foreach (((var itemId, _), var quantity) in filter.CraftList.GetQuantityToRetrieveList())
            {
                if (quantity == 0) continue;

                if (retrievalItems.ContainsKey(itemId))
                {
                    retrievalItems[itemId] += quantity;
                } else {
                    retrievalItems.Add(itemId, quantity);
                }
            }
        }

        return retrievalItems;
    }

    private HashSet<ulong> GetCharactersOwnedByActive(bool includeOwner)
        => _inventoryMonitor.Inventories.Select(pair => pair.Key)
            .Where(characterId => _characterMonitor.BelongsToActiveCharacter(characterId) && (includeOwner || characterId != _characterMonitor.ActiveCharacterId))
            .ToHashSet();

    private HashSet<ulong[]> GetCharacterItems(ulong characterId)
    {
        var items = _inventoryMonitor.Inventories
            .First(pair => pair.Key == characterId).Value
            .GetAllInventories().SelectMany(item => item)
            .Where(item => item != null);

        if (items == null) return new();
        return items.Select(item => item!.ToNumeric())
            .ToHashSet();
    }

    private HashSet<ulong[]> GetCharacterItemsByType(ulong characterId, uint inventoryType)
    {
        var characterInventories = _inventoryMonitor.Inventories
            .First(pair => pair.Key == characterId).Value;
        
        var items = (characterInventories
                .GetInventoryByType((InventoryType)inventoryType) ?? Array.Empty<CriticalCommonLib.Models.InventoryItem?>())
            .Where(item => item != null);

        return items.Select(item => item!.ToNumeric())
            .ToHashSet();
    }
    private Dictionary<uint, uint> GetFilterItems(string filterKey)
    {
        var filter = _listService.GetListByKeyOrName(filterKey);
        var filterItems = new Dictionary<uint, uint>();
        
        if (filter != null)
        {
            if (filter.FilterType == FilterType.CraftFilter)
            {
                foreach (var craftItem in filter.CraftList.CraftItems)
                {
                    if (craftItem.IsOutputItem)
                    {
                        if (filterItems.ContainsKey(craftItem.ItemId))
                        {
                            filterItems[craftItem.ItemId] += craftItem.QuantityRequired;
                        }
                        else
                        {
                            filterItems.Add(craftItem.ItemId, craftItem.QuantityRequired);
                        }
                    }
                }
            }
            if (filter.FilterType == FilterType.SearchFilter || filter.FilterType == FilterType.SortingFilter)
            {
                var filterResult = _listFilterService.RefreshList(filter);
                foreach (var sortedItem in filterResult.SortedItems)
                {
                    if (filterItems.ContainsKey(sortedItem.InventoryItem.ItemId))
                    {
                        filterItems[sortedItem.InventoryItem.ItemId] += sortedItem.InventoryItem.Quantity;
                    }
                    else
                    {
                        filterItems.Add(sortedItem.InventoryItem.ItemId, sortedItem.InventoryItem.Quantity);
                    }
                }
            }
            if (filter.FilterType == FilterType.GameItemFilter)
            {
                var filterResult = _listFilterService.RefreshList(filter);
                foreach (var sortedItem in filterResult.AllItems)
                {
                    if (filterItems.ContainsKey(sortedItem.RowId))
                    {
                        filterItems[sortedItem.RowId] += 0;
                    }
                    else
                    {
                        filterItems.Add(sortedItem.RowId, 0);
                    }
                }
            }
        }

        return filterItems;
    }

    private bool RemoveItemFromCraftList(string filterKey, uint itemId, uint quantity)
    {
        var filter = _listService.GetListByKeyOrName(filterKey);
        
        if (filter is { FilterType: FilterType.CraftFilter })
        {
            filter.CraftList.RemoveCraftItem(itemId, quantity, InventoryItem.ItemFlags.None);
            filter.NeedsRefresh = true;
        }

        return false;
    }

    private bool AddItemToCraftList(string filterKey, uint itemId, uint quantity)
    {
        var filter = _listService.GetListByKeyOrName(filterKey);
        
        if (filter is { FilterType: FilterType.CraftFilter })
        {
            filter.CraftList.AddCraftItem(itemId, quantity, InventoryItem.ItemFlags.None);
            filter.NeedsRefresh = true;
        }

        return false;
    }

    private bool ToggleUiFilter(string filterKey)
    {
        var filter = _listService.GetListByKeyOrName(filterKey);

        if (filter == null)
        {
            filter = _listService.GetList(filterKey);
        }
        
        if (filter != null)
        {
            _listService.ToggleActiveUiList(filter);
            return true;
        }

        return false;
    }

    private bool DisableUiFilter()
    {
        _listService.ClearActiveUiList();
        return true;
    }

    private bool EnableUiFilter(string filterKey)
    {
        var filter = _listService.GetListByKeyOrName(filterKey);
        
        if (filter != null)
        {
            _listService.SetActiveUiList(filter);
            return true;
        }

        return false;
    }

    private bool ToggleBackgroundFilter(string filterKey)
    {
        var filter = _listService.GetListByKeyOrName(filterKey);
        
        if (filter != null)
        {
            _listService.ToggleActiveBackgroundList(filter);
            return true;
        }

        return false;
    }

    private bool DisableBackgroundFilter()
    {
        _listService.ClearActiveBackgroundList();
        return true;
    }

    private bool EnableBackgroundFilter(string filterKey)
    {
        var filter = _listService.GetListByKeyOrName(filterKey);
        
        if (filter != null)
        {
            _listService.SetActiveBackgroundList(filter);
            return true;
        }

        return false;
    }

    private bool ToggleCraftList(string filterKey)
    {
        var filter = _listService.GetListByKeyOrName(filterKey);

        if (filter == null)
        {
            filter = _listService.GetList(filterKey);
        }
        
        if (filter != null)
        {
            _listService.ToggleActiveCraftList(filter);
            return true;
        }

        return false;
    }

    private bool DisableCraftList()
    {
        _listService.ClearActiveCraftList();
        return true;
    }

    private bool EnableCraftList(string filterKey)
    {
        var filter = _listService.GetListByKeyOrName(filterKey);
        
        if (filter != null)
        {
            _listService.SetActiveCraftList(filter);
            return true;
        }

        return false;
    }

    private void CharacterMonitorOnOnActiveRetainerChanged(ulong retainerId)
    {
        _retainerChanged?.SendMessage(retainerId);
    }

    private uint InventoryCountByTypes(uint[] inventoryTypes, ulong? characterId)
    {
        return (uint)_inventoryMonitor.AllItems.Where(c => inventoryTypes.Contains((uint)c.SortedContainer) && (characterId == null || c.RetainerId == characterId))
            .Sum(c => c.Quantity);
    }

    private uint InventoryCountByType(uint inventoryType, ulong? characterId)
    {
        return (uint)_inventoryMonitor.AllItems.Where(c => (uint)c.SortedContainer == inventoryType && (characterId == null || c.RetainerId == characterId)).Sum(c => c.Quantity);
    }            
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _characterMonitor.OnActiveRetainerChanged -= CharacterMonitorOnOnActiveRetainerChanged;
            _inventoryMonitor.OnInventoryChanged -= InventoryMonitorOnOnInventoryChanged;
            _inventoryCountByType?.UnregisterFunc();
            _inventoryCountByTypes?.UnregisterFunc();
            _itemCount?.UnregisterFunc();
            _itemCountHQ?.UnregisterFunc();
            _itemCountOwned?.UnregisterFunc();
            _enableUiFilter?.UnregisterFunc();
            _disableUiFilter?.UnregisterFunc();
            _toggleUiFilter?.UnregisterFunc();
            _enableBackgroundFilter?.UnregisterFunc();
            _disableBackgroundFilter?.UnregisterFunc();
            _toggleBackgroundFilter?.UnregisterFunc();
            _enableCraftList?.UnregisterFunc();
            _disableCraftList?.UnregisterFunc();
            _toggleCraftList?.UnregisterFunc();
            _addItemToCraftList?.UnregisterFunc();
            _removeItemFromCraftList?.UnregisterFunc();
            _getFilterItems?.UnregisterFunc();
            _getCraftItems?.UnregisterFunc();
            _getRetrievalItems?.UnregisterFunc();
            _itemAdded?.UnregisterFunc();
            _getCraftLists?.UnregisterFunc();
            _getSearchFilters?.UnregisterFunc();
            _addNewCraftList?.UnregisterFunc();
            _currentCharacter?.UnregisterFunc();
            _retainerChanged?.UnregisterFunc();
            _getCharacterItems?.UnregisterFunc();
            _getCharacterItemsByType?.UnregisterFunc();
            _getCharactersOwnedByActive?.UnregisterFunc();
            _isInitialized?.UnregisterFunc();
        }
        _disposed = true;         

    }
    
    ~IPCService()
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
}

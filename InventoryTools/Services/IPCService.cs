using System;
using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using InventoryTools.Logic;
using InventoryTools.Services.Interfaces;
using InventoryItem = FFXIVClientStructs.FFXIV.Client.Game.InventoryItem;

namespace InventoryTools.Services;

public class IPCService : IDisposable
{
    private readonly ICallGateProvider<uint, ulong?, uint>? _inventoryCountByType;
    private readonly ICallGateProvider<uint[], ulong?, uint>? _inventoryCountByTypes;
    private readonly ICallGateProvider<uint, ulong, int, uint>? _itemCount;
    private readonly ICallGateProvider<string, bool>? _enableUiFilter;
    private readonly ICallGateProvider<bool>? _disableUiFilter;
    private readonly ICallGateProvider<string, bool>? _toggleUiFilter;
    private readonly ICallGateProvider<string, bool>? _enableBackgroundFilter;
    private readonly ICallGateProvider<bool>? _disableBackgroundFilter;
    private readonly ICallGateProvider<string, bool>? _toggleBackgroundFilter;
    private readonly ICallGateProvider<string, uint, uint, bool>? _addItemToCraftList;
    private readonly ICallGateProvider<string, uint, uint, bool>? _removeItemFromCraftList;
    private readonly ICallGateProvider<string, Dictionary<uint, uint>>? _getFilterItems;
    private readonly ICallGateProvider<string, Dictionary<uint, uint>>? _getCraftItems;
    private readonly ICallGateProvider<(uint, InventoryItem.ItemFlags, ulong, uint), bool>? _itemAdded;
    private readonly ICallGateProvider<(uint, InventoryItem.ItemFlags, ulong, uint), bool>? _itemRemoved;
    private readonly ICallGateProvider<Dictionary<string,string>>? _getCraftLists;
    private readonly ICallGateProvider<string, Dictionary<uint, uint>, string>? _addNewCraftList;
    private readonly ICallGateProvider<ulong>? _currentCharacter;
    private readonly ICallGateProvider<ulong?, bool>? _retainerChanged;
    private readonly ICallGateProvider<bool>? _isInitialized;
    private readonly ICallGateProvider<bool, bool>? _initialized;
    private readonly bool _initalizedIpc;

    private readonly ICharacterMonitor _characterMonitor;
    private readonly IFilterService _filterService;
    private readonly IInventoryMonitor _inventoryMonitor;
    private bool _disposed;

    public IPCService(DalamudPluginInterface pluginInterface, ICharacterMonitor characterMonitor, IFilterService filterService, IInventoryMonitor inventoryMonitor)
    {
        _characterMonitor = characterMonitor;
        _filterService = filterService;
        _inventoryMonitor = inventoryMonitor;
        
        _inventoryCountByType = pluginInterface.GetIpcProvider<uint, ulong?, uint>("AllaganTools.InventoryCountByType");
        _inventoryCountByType.RegisterFunc(InventoryCountByType);
        
        _inventoryCountByTypes = pluginInterface.GetIpcProvider<uint[], ulong?, uint>("AllaganTools.InventoryCountByTypes");
        _inventoryCountByTypes.RegisterFunc(InventoryCountByTypes);
        
        _itemCount = pluginInterface.GetIpcProvider<uint, ulong, int, uint>("AllaganTools.ItemCount");
        _itemCount.RegisterFunc(ItemCount);

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

        _addItemToCraftList = pluginInterface.GetIpcProvider<string, uint, uint, bool>("AllaganTools.AddItemToCraftList");
        _addItemToCraftList.RegisterFunc(AddItemToCraftList);

        _removeItemFromCraftList = pluginInterface.GetIpcProvider<string, uint, uint, bool>("AllaganTools.RemoveItemFromCraftList");
        _removeItemFromCraftList.RegisterFunc(RemoveItemFromCraftList);

        _getFilterItems = pluginInterface.GetIpcProvider<string, Dictionary<uint, uint>>("AllaganTools.GetFilterItems");
        _getFilterItems.RegisterFunc(GetFilterItems);

        _getCraftItems = pluginInterface.GetIpcProvider<string, Dictionary<uint, uint>>("AllaganTools.GetCraftItems");
        _getCraftItems.RegisterFunc(GetCraftItems);

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

    private void InventoryMonitorOnOnInventoryChanged(List<InventoryChange> inventoryChanges)
    {
        foreach (var changedItem in changedItems.NewItems)
        {
            if (changedItem.ItemId != 1)
            {
                _itemAdded?.SendMessage((changedItem.ItemId, changedItem.Flags, changedItem.CharacterId, (uint)changedItem.Quantity));
            }
        }
        foreach (var changedItem in changedItems.RemovedItems)
        {
            if (changedItem.ItemId != 1)
            {
                _itemRemoved?.SendMessage((changedItem.ItemId, changedItem.Flags, changedItem.CharacterId, (uint)changedItem.Quantity));
            }
        }
    }

    private uint ItemCount(uint itemId, ulong characterId, int inventoryType)
    {
        return (uint)_inventoryMonitor.AllItems.Where(c => c.ItemId == itemId && (inventoryType == -1 || (uint)c.SortedContainer == inventoryType) && (c.RetainerId == characterId)).Sum(c => c.Quantity);
    }

    private bool IsInitialized()
    {
        return _initalizedIpc;
    }

    private ulong CurrentCharacter()
    {
        return _characterMonitor.ActiveCharacterId;
    }

    private string AddNewCraftList(string craftListName, Dictionary<uint, uint> items)
    {
        var newCraftFilter = _filterService.AddNewCraftFilter();
        newCraftFilter.Name = craftListName;
        foreach (var item in items)
        {
            newCraftFilter.CraftList.AddCraftItem(item.Key, item.Value);
        }

        return newCraftFilter.Key;
    }

    private Dictionary<string, string> GetCraftLists()
    {
        var craftLists = _filterService.FiltersList.Where(c => c.FilterType == FilterType.CraftFilter && !c.CraftListDefault);
        var keyNameDict = new Dictionary<string, string>();
        foreach (var craftList in craftLists)
        {
            keyNameDict.Add(craftList.Key, craftList.Name);
        }

        return keyNameDict;
    }

    private Dictionary<uint, uint> GetCraftItems(string filterKey)
    {
        var filter = _filterService.GetFilterByKeyOrName(filterKey);
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

    private Dictionary<uint, uint> GetFilterItems(string filterKey)
    {
        var filter = _filterService.GetFilterByKeyOrName(filterKey);
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
                var filterResult = filter.GenerateFilteredList(PluginService.InventoryMonitor.Inventories.Select(c => c.Value).ToList()).Result;
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
            //TODO: Add history IPC
            if (filter.FilterType == FilterType.GameItemFilter)
            {
                var filterResult = filter.GenerateFilteredList(PluginService.InventoryMonitor.Inventories.Select(c => c.Value).ToList()).Result;
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
        var filter = _filterService.GetFilterByKeyOrName(filterKey);
        
        if (filter is { FilterType: FilterType.CraftFilter })
        {
            filter.CraftList.RemoveCraftItem(itemId, quantity, InventoryItem.ItemFlags.None);
        }

        return false;
    }

    private bool AddItemToCraftList(string filterKey, uint itemId, uint quantity)
    {
        var filter = _filterService.GetFilterByKeyOrName(filterKey);
        
        if (filter is { FilterType: FilterType.CraftFilter })
        {
            filter.CraftList.AddCraftItem(itemId, quantity, InventoryItem.ItemFlags.None);
        }

        return false;
    }

    private bool ToggleUiFilter(string filterKey)
    {
        var filter = _filterService.GetFilterByKeyOrName(filterKey);

        if (filter == null)
        {
            filter = _filterService.GetFilter(filterKey);
        }
        
        if (filter != null)
        {
            _filterService.ToggleActiveUiFilter(filter);
            return true;
        }

        return false;
    }

    private bool DisableUiFilter()
    {
        _filterService.ClearActiveUiFilter();
        return true;
    }

    private bool EnableUiFilter(string filterKey)
    {
        var filter = _filterService.GetFilterByKeyOrName(filterKey);
        
        if (filter != null)
        {
            _filterService.SetActiveUiFilter(filter);
            return true;
        }

        return false;
    }

    private bool ToggleBackgroundFilter(string filterKey)
    {
        var filter = _filterService.GetFilterByKeyOrName(filterKey);
        
        if (filter != null)
        {
            _filterService.ToggleActiveBackgroundFilter(filter);
            return true;
        }

        return false;
    }

    private bool DisableBackgroundFilter()
    {
        _filterService.ClearActiveBackgroundFilter();
        return true;
    }

    private bool EnableBackgroundFilter(string filterKey)
    {
        var filter = _filterService.GetFilterByKeyOrName(filterKey);
        
        if (filter != null)
        {
            _filterService.SetActiveBackgroundFilter(filter);
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
            _enableUiFilter?.UnregisterFunc();
            _disableUiFilter?.UnregisterFunc();
            _toggleUiFilter?.UnregisterFunc();
            _enableBackgroundFilter?.UnregisterFunc();
            _disableBackgroundFilter?.UnregisterFunc();
            _toggleBackgroundFilter?.UnregisterFunc();
            _addItemToCraftList?.UnregisterFunc();
            _removeItemFromCraftList?.UnregisterFunc();
            _getFilterItems?.UnregisterFunc();
            _getCraftItems?.UnregisterFunc();
            _itemAdded?.UnregisterFunc();
            _getCraftLists?.UnregisterFunc();
            _addNewCraftList?.UnregisterFunc();
            _currentCharacter?.UnregisterFunc();
            _retainerChanged?.UnregisterFunc();
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
            PluginLog.Error("There is a disposable object which hasn't been disposed before the finalizer call: " + (this.GetType ().Name));
        }
#endif
        Dispose (true);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Lists;

/// <summary>
/// A class that takes a set of items, a filter configuration and returns a filter result
/// </summary>
public class ListFilterService : DisposableMediatorBackgroundService
{

    private readonly InventoryToolsConfiguration _configuration;
    private readonly ICharacterMonitor _characterMonitor;
    private readonly HostedInventoryHistory _inventoryHistory;
    private readonly IInventoryMonitor _inventoryMonitor;
    private readonly IMarketCache _marketCache;
    private readonly CraftPricer _craftPricer;
    private readonly IFilterService _filterService;
    private readonly ExcelCache _excelCache;

    public IBackgroundTaskQueue FilterQueue { get; }

    public ListFilterService(InventoryToolsConfiguration configuration, ICharacterMonitor characterMonitor, HostedInventoryHistory inventoryHistory, IInventoryMonitor inventoryMonitor, IBackgroundTaskQueue filterQueue, ILogger<ListFilterService> logger, IMarketCache marketCache, CraftPricer craftPricer, IFilterService filterService, MediatorService mediatorService, ExcelCache excelCache) : base(logger, mediatorService)
    {
        _configuration = configuration;
        _characterMonitor = characterMonitor;
        _inventoryHistory = inventoryHistory;
        _inventoryMonitor = inventoryMonitor;
        _marketCache = marketCache;
        _craftPricer = craftPricer;
        _filterService = filterService;
        _excelCache = excelCache;
        FilterQueue = filterQueue;
        MediatorService.Subscribe<RequestListUpdateMessage>(this, message => RequestRefresh(message.FilterConfiguration));
    }

    public List<SearchResult> RefreshList(FilterConfiguration filterConfiguration)
    {
        var inventories = _inventoryMonitor.Inventories.Select(c => c.Value).ToList();

        List<SearchResult>? searchResult;
        if (filterConfiguration.FilterType == FilterType.CraftFilter)
        {
            filterConfiguration.CraftList.BeenGenerated = false;
            filterConfiguration.CraftList.BeenUpdated = false;
            var playerBags = _inventoryMonitor.GetSpecificInventory(_characterMonitor.ActiveCharacterId,
               InventoryCategory.CharacterBags);
            var crystalBags = _inventoryMonitor.GetSpecificInventory(_characterMonitor.ActiveCharacterId,
               InventoryCategory.Crystals);
            var currencyBags = _inventoryMonitor.GetSpecificInventory(_characterMonitor.ActiveCharacterId,
               InventoryCategory.Currency);

            GenerateSources(filterConfiguration, inventories.ToList(), out var sourceInventories);

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

            foreach (var inventory in sourceInventories)
            {
                //Assume we have no retainer active because we want all the possible items
                if (filterConfiguration.InActiveInventories(_characterMonitor.ActiveCharacterId, 0, inventory.Key.Item1, _characterMonitor.ActiveCharacterId))
                {
                    foreach (var item in inventory.Value)
                    {
                        if (item == null) continue;
                        if (!externalSources.ContainsKey(item.ItemId))
                        {
                            externalSources.Add(item.ItemId,new List<CraftItemSource>());
                        }
                        externalSources[item.ItemId].Add(new CraftItemSource(item.ItemId, item.Quantity, item.IsHQ));
                    }
                }
            }

            filterConfiguration.CraftList.GenerateCraftChildren();
            var materials = filterConfiguration.CraftList.GetMaterialsList().ToList();
            var craftListConfiguration = new CraftListConfiguration(characterSources, externalSources, null, _craftPricer);
            if (craftListConfiguration.WorldPreferences == null)
            {
                craftListConfiguration.WorldPreferences = new();
            }
            if (filterConfiguration.GetBooleanFilter("CraftWorldPriceUseActiveWorld") == true && _characterMonitor.ActiveCharacter != null)
            {
                if (!craftListConfiguration.WorldPreferences.Contains(_characterMonitor.ActiveCharacter.ActiveWorldId))
                {
                    craftListConfiguration.WorldPreferences.Add(_characterMonitor.ActiveCharacter.ActiveWorldId);
                }
            }
            if (filterConfiguration.GetBooleanFilter("CraftWorldPriceUseHomeWorld") == true && _characterMonitor.ActiveCharacter != null)
            {
                if (!craftListConfiguration.WorldPreferences.Contains(_characterMonitor.ActiveCharacter.ActiveWorldId))
                {
                    craftListConfiguration.WorldPreferences.Add(_characterMonitor.ActiveCharacter.ActiveWorldId);
                }
            }

            if (filterConfiguration.GetBooleanFilter("CraftWorldPriceUseDefaults") == true)
            {
                foreach (var worldId in _configuration.MarketBoardWorldIds)
                {
                    if (!craftListConfiguration.WorldPreferences.Contains(worldId))
                    {
                        craftListConfiguration.WorldPreferences.Add(worldId);
                    }
                }
            }
            foreach (var worldId in filterConfiguration.CraftList.WorldPricePreference)
            {
                if (!craftListConfiguration.WorldPreferences.Contains(worldId))
                {
                    craftListConfiguration.WorldPreferences.Add(worldId);
                }
            }
            var pricingData = _craftPricer.GetItemPricingDictionary(materials, craftListConfiguration.WorldPreferences ?? new(), true);
            craftListConfiguration.PricingSource = pricingData;
            filterConfiguration.CraftList.Update(craftListConfiguration, _craftPricer);
            filterConfiguration.CraftList.ClearGroupCache();
            filterConfiguration.CraftList.NeedsRefresh = false;
            filterConfiguration.NeedsRefresh = false;

            searchResult = GenerateFilterResult(filterConfiguration, inventories.ToList());
            filterConfiguration.NeedsRefresh = false;
            filterConfiguration.Refreshing = false;
            filterConfiguration.SearchResults = searchResult;
            MediatorService.Publish(new ListUpdatedMessage(filterConfiguration));
            return searchResult;
        }

        searchResult = GenerateFilterResult(filterConfiguration, inventories.ToList());
        filterConfiguration.NeedsRefresh = false;
        filterConfiguration.Refreshing = false;
        filterConfiguration.SearchResults = searchResult;
        MediatorService.Publish(new ListUpdatedMessage(filterConfiguration));
        return searchResult;
    }

    private List<SearchResult> GenerateFilterResult(FilterConfiguration filter, List<Inventory> inventories)
    {
        var searchResults = new List<SearchResult>();

        var activeCharacter = _characterMonitor.ActiveCharacterId;
        var activeRetainer = _characterMonitor.ActiveRetainerId;
        var displaySourceCrossCharacter = filter.SourceIncludeCrossCharacter ?? _configuration.DisplayCrossCharacter;
        var displayDestinationCrossCharacter = filter.DestinationIncludeCrossCharacter ?? _configuration.DisplayCrossCharacter;

        Logger.LogTrace("Filter Information:");
        Logger.LogTrace("Filter Type: " + filter.FilterType);

        if (filter.FilterType == FilterType.SortingFilter || filter.FilterType == FilterType.CraftFilter)
        {
            //Determine which source and destination inventories we actually need to examine
            GenerateSourceAndDestinations(filter, inventories, out var sourceInventories, out var destinationInventories);

            //Filter the source and destination inventories based on the applicable items so we have less to sort
            Dictionary<(ulong, InventoryType), List<FilteredItem>> filteredSources = new();
            //Dictionary<(ulong, InventoryCategory), List<InventoryItem>> filteredDestinations = new();
            var sourceKeys = sourceInventories.Select(c => c.Key);
            Logger.LogTrace(sourceInventories.Count() + " inventories to examine.");
            if (filter.FilterType == FilterType.CraftFilter)
            {
                filter.CraftList.GetFlattenedMergedMaterials(true);
            }

            foreach (var availableFilter in _filterService.AvailableFilters)
            {
                availableFilter.InvalidateSearchCache();
            }

            foreach (var sourceInventory in sourceInventories)
            {
                if (!filteredSources.ContainsKey(sourceInventory.Key))
                {
                    filteredSources.Add(sourceInventory.Key, new List<FilteredItem>());
                }

                foreach (var item in sourceInventory.Value)
                {
                    var filteredItem = filter.FilterItem(_filterService.AvailableFilters, item);
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
                        var filteredDestinationItem = filter.FilterItem(_filterService.AvailableFilters, destinationItem);
                        if (filteredDestinationItem != null)
                        {
                            var itemHashCode = destinationItem.GenerateHashCode(filter.IgnoreHQWhenSorting ?? false);
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
                //_logger.LogTrace("Found " + sourceInventory.Value.Count + " items in " + sourceInventory.Key + " " + sourceInventory.Key.Item2.ToString());
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
                    var hashCode = sourceItem.GenerateHashCode(filter.IgnoreHQWhenSorting ?? false);
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
                                        var sortingResult = new SortingResult(sourceInventory.Key.Item1,
                                            existingItem.RetainerId, sourceItem.SortedContainer,
                                            existingItem.SortedContainer,existingItem.BagLocation(existingItem.SortedContainer),false, sourceItem, existingItem, (int) canFit);

                                        searchResults.Add(new SearchResult(sortingResult));
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
                                                        //_logger.LogTrace(
                                                        //    "Added item to filter result as we've seen the item before: " +
                                                        //    sourceItem.FormattedName);
                                                        var sortingResult = new SortingResult(sourceInventory.Key.Item1,
                                                            seenInventoryLocation.Item1, sourceItem.SortedContainer,
                                                            seenInventoryLocation.Item2, nextEmptySlot.BagLocation(nextEmptySlot.SortedContainer),true, sourceItem, nextEmptySlot,
                                                            (int) sourceItem.TempQuantity);
                                                        searchResults.Add(new SearchResult(sortingResult));
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
                                var sortingResult = new SortingResult(sourceInventory.Key.Item1, nextSlot.Key.Item1,
                                    sourceItem.SortedContainer, nextSlot.Key.Item2, nextEmptySlot.BagLocation(nextEmptySlot.SortedContainer),true, sourceItem, nextEmptySlot,
                                    (int) sourceItem.TempQuantity);
                                searchResults.Add(new SearchResult(sortingResult));

                                //We want to add the empty slot to the list of locations we know about, we need to create a copy and add that so any further items with the same ID can properly check how much room is left in the stack
                                nextEmptySlot = new InventoryItem(nextEmptySlot);
                                nextEmptySlot.ItemId = sourceItem.ItemId;
                                nextEmptySlot.Flags = sourceItem.Flags;
                                nextEmptySlot.Quantity = sourceItem.Quantity;

                                //Add the destination item into the list of locations in case we have an empty slot for an item but multiple sources of the item.
                                var newLocationHash = sourceItem.GenerateHashCode(filter.IgnoreHQWhenSorting ?? false);
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
                        // _logger.LogTrace("Added item to unsortable list, maybe I should show these somewhere: " +
                        //                  sourceItem.FormattedName);
                        var sortingResult = new SortingResult(sourceInventory.Key.Item1, sourceItem.SortedContainer, sourceItem, (int) sourceItem.TempQuantity, false);
                        searchResults.Add(new SearchResult(sortingResult));
                    }
                }
            }
        }
        else if(filter.FilterType == FilterType.SearchFilter)
        {
            //Determine which source and destination inventories we actually need to examine
            GenerateSources(filter, inventories, out var sourceInventories);

            HashSet<int> distinctItems = new HashSet<int>();
            HashSet<int> duplicateItems = new HashSet<int>();

            //Filter the source and destination inventories based on the applicable items so we have less to sort
            Dictionary<(ulong, InventoryType), List<FilteredItem>> filteredSources = new();
            //Dictionary<(ulong, InventoryCategory), List<InventoryItem>> filteredDestinations = new();
            Logger.LogTrace(sourceInventories.Count() + " inventories to examine.");
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
                        var filteredItem = filter.FilterItem(_filterService.AvailableFilters, item);
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
                        var hashCode = item.Item.GenerateHashCode(filter.IgnoreHQWhenSorting ?? false);
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
                        if (duplicateItems.Contains(item.GenerateHashCode(filter.IgnoreHQWhenSorting ?? false)))
                        {
                            var sortingResult = new SortingResult(filteredSource.Key.Item1, item.SortedContainer,
                                item, (int)item.Quantity);
                            searchResults.Add(new SearchResult(sortingResult));
                        }

                    }
                    else
                    {
                        var sortingResult = new SortingResult(filteredSource.Key.Item1, item.SortedContainer,
                            item, (int)item.Quantity);
                        searchResults.Add(new SearchResult(sortingResult));
                    }

                }
            }
        }
        else if(filter.FilterType == FilterType.HistoryFilter)
        {
            var history = _inventoryHistory.GetHistory();
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
                    _characterMonitor.IsRetainer(characterId) &&
                    (displaySourceCrossCharacter || _characterMonitor.BelongsToActiveCharacter(characterId)))
                {
                    return true;
                }

                if (filter.SourceAllFreeCompanies.HasValue && filter.SourceAllFreeCompanies.Value &&
                    _characterMonitor.IsFreeCompany(characterId) &&
                    (displaySourceCrossCharacter || _characterMonitor.BelongsToActiveCharacter(characterId)))
                {
                    return true;
                }

                if (filter.SourceAllHouses.HasValue && filter.SourceAllHouses.Value && _characterMonitor.IsHousing(characterId) &&
                    (displaySourceCrossCharacter || _characterMonitor.BelongsToActiveCharacter(characterId)))
                {
                    return true;
                }

                if (filter.SourceAllCharacters.HasValue && filter.SourceAllCharacters.Value &&
                    _characterMonitor.IsCharacter(characterId) &&
                    (displaySourceCrossCharacter || _characterMonitor.ActiveCharacterId == characterId))
                {
                    return true;
                }

                if (filter.SourceInventories.Contains((characterId, inventoryCategory)) &&
                    (displaySourceCrossCharacter || _characterMonitor.BelongsToActiveCharacter(characterId)))
                {
                    return true;
                }

                if (filter.SourceCategories != null && filter.SourceCategories.Contains(inventoryCategory) &&
                    (displaySourceCrossCharacter || _characterMonitor.BelongsToActiveCharacter(characterId)))
                {
                    return true;
                }

                if (filter.SourceWorlds != null &&
                    filter.SourceWorlds.Contains(_characterMonitor.GetCharacterById(characterId)?.WorldId ?? 0))
                {
                    return true;
                }

                return false;
            }

            foreach (var change in matchedItems.OrderByDescending(c => c.ChangeDate ?? new DateTime()))
            {
                if (filter.FilterItem(_filterService.AvailableFilters, change))
                {
                    searchResults.Add(new SearchResult(change));
                }
            }
        }
        else if (filter.FilterType == FilterType.CuratedList)
        {
            if (filter.CuratedItems != null)
            {
                foreach (var curatedItem in filter.CuratedItems)
                {
                    var itemEx = _excelCache.GetItemExSheet().GetRow(curatedItem.ItemId);
                    if (itemEx != null)
                    {
                        searchResults.Add(new SearchResult(itemEx, curatedItem));
                    }
                }
            }
        }
        else
        {
            searchResults = _excelCache.AllItems.Select(c => c.Value).Where(c => filter.FilterItem(_filterService.AvailableFilters, c)).Where(c => c.RowId != 0).Select(c => new SearchResult(c)).ToList();
        }


        return searchResults;
    }

    private void GenerateSources(FilterConfiguration filter, List<Inventory> inventories, out Dictionary<(ulong, InventoryType), InventoryItem?[]> sourceInventories)
    {
        var displaySourceCrossCharacter = filter.SourceIncludeCrossCharacter ?? _configuration.DisplayCrossCharacter;
        sourceInventories = new();

        foreach (var character in inventories)
        {
            foreach (var inventory in character.GetAllInventoriesByType())
            {
                var inventoryKey = (character.CharacterId, inventory.Key);
                if (filter.SourceAllRetainers.HasValue && filter.SourceAllRetainers.Value &&
                    _characterMonitor.IsRetainer(character.CharacterId) && (displaySourceCrossCharacter ||
                                                                            _characterMonitor.BelongsToActiveCharacter(
                                                                               character.CharacterId)))
                {
                    if (!sourceInventories.ContainsKey(inventoryKey))
                    {
                        sourceInventories.Add(inventoryKey, inventory.Value);
                    }
                }

                if (filter.SourceAllFreeCompanies.HasValue && filter.SourceAllFreeCompanies.Value &&
                    _characterMonitor.IsFreeCompany(character.CharacterId) && (displaySourceCrossCharacter ||
                                                                               _characterMonitor.BelongsToActiveCharacter(
                                                                                   character.CharacterId)))
                {
                    if (!sourceInventories.ContainsKey(inventoryKey))
                    {
                        sourceInventories.Add(inventoryKey, inventory.Value);
                    }
                }

                if (filter.SourceAllHouses.HasValue && filter.SourceAllHouses.Value &&
                    _characterMonitor.IsHousing(character.CharacterId) && (displaySourceCrossCharacter ||
                                                                           _characterMonitor.BelongsToActiveCharacter(
                                                                               character.CharacterId)))
                {
                    if (!sourceInventories.ContainsKey(inventoryKey))
                    {
                        sourceInventories.Add(inventoryKey, inventory.Value);
                    }
                }

                if (filter.SourceAllCharacters.HasValue && filter.SourceAllCharacters.Value &&
                    _characterMonitor.IsCharacter(character.CharacterId) && (displaySourceCrossCharacter ||
                                                                             _characterMonitor.ActiveCharacterId ==
                                                                             character.CharacterId))
                {
                    if (!sourceInventories.ContainsKey(inventoryKey))
                    {
                        sourceInventories.Add(inventoryKey, inventory.Value);
                    }
                }

                var inventoryCategory = inventoryKey.Key.ToInventoryCategory();
                if (filter.SourceInventories.Contains((inventoryKey.CharacterId, inventoryCategory)) &&
                    (displaySourceCrossCharacter || _characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                {
                    if (!sourceInventories.ContainsKey(inventoryKey))
                    {
                        sourceInventories.Add(inventoryKey, inventory.Value);
                    }
                }

                if (filter.SourceCategories != null && filter.SourceCategories.Contains(inventoryCategory) &&
                    (displaySourceCrossCharacter || _characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                {
                    if (!sourceInventories.ContainsKey(inventoryKey))
                    {
                        sourceInventories.Add(inventoryKey, inventory.Value);
                    }
                }

                if (filter.SourceWorlds != null &&
                    filter.SourceWorlds.Contains(_characterMonitor.GetCharacterById(character.CharacterId)?.WorldId ?? 0))
                {
                    if (!sourceInventories.ContainsKey(inventoryKey))
                    {
                        sourceInventories.Add(inventoryKey, inventory.Value);
                    }
                }
            }
        }
    }

    private void GenerateSourceAndDestinations(FilterConfiguration filter, List<Inventory> inventories,
        out Dictionary<(ulong, InventoryType), List<InventoryItem>> sourceInventories, out Dictionary<(ulong, InventoryType), List<InventoryItem>> destinationInventories)
    {
        var displaySourceCrossCharacter = filter.SourceIncludeCrossCharacter ?? _configuration.DisplayCrossCharacter;
        var displayDestinationCrossCharacter = filter.DestinationIncludeCrossCharacter ?? _configuration.DisplayCrossCharacter;

        sourceInventories = new();
        destinationInventories = new();
        foreach (var character in inventories)
        {
            foreach (var inventory in character.GetAllInventoriesByType())
            {
                var type = inventory.Key;
                var inventoryKey = (character.CharacterId, type);
                if (filter.SourceAllRetainers.HasValue && filter.SourceAllRetainers.Value &&
                    _characterMonitor.IsRetainer(character.CharacterId) && (displaySourceCrossCharacter ||
                                                                            _characterMonitor
                                                                               .BelongsToActiveCharacter(
                                                                                   character.CharacterId)))
                {
                    if (!sourceInventories.ContainsKey(inventoryKey))
                    {
                        sourceInventories.Add(inventoryKey,
                            inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
                    }
                }

                if (filter.SourceAllCharacters.HasValue && filter.SourceAllCharacters.Value &&
                    _characterMonitor.IsCharacter(character.CharacterId) &&
                    _characterMonitor.ActiveCharacterId == character.CharacterId && (displaySourceCrossCharacter ||
                        _characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                {
                    if (inventoryKey.Item2.ToInventoryCategory() is not InventoryCategory.FreeCompanyBags &&
                        !sourceInventories.ContainsKey(inventoryKey))
                    {
                        sourceInventories.Add(inventoryKey,
                            inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
                    }
                }

                if (filter.SourceAllFreeCompanies.HasValue && filter.SourceAllFreeCompanies.Value &&
                    _characterMonitor.IsFreeCompany(character.CharacterId) && (displaySourceCrossCharacter ||
                                                                               _characterMonitor.BelongsToActiveCharacter(
                                                                                   character.CharacterId)))
                {
                    if (!sourceInventories.ContainsKey(inventoryKey))
                    {
                        sourceInventories.Add(inventoryKey,
                            inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
                    }
                }

                if (filter.SourceAllHouses.HasValue && filter.SourceAllHouses.Value &&
                    _characterMonitor.IsHousing(character.CharacterId) && (displaySourceCrossCharacter ||
                                                                           _characterMonitor.BelongsToActiveCharacter(
                                                                               character.CharacterId)))
                {
                    if (!sourceInventories.ContainsKey(inventoryKey))
                    {
                        sourceInventories.Add(inventoryKey,
                            inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
                    }
                }

                if (filter.SourceInventories.Contains((character.CharacterId, inventoryKey.type.ToInventoryCategory())) &&
                    (displaySourceCrossCharacter ||
                     _characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                {
                    if (!sourceInventories.ContainsKey(inventoryKey))
                    {
                        sourceInventories.Add(inventoryKey,
                            inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
                    }
                }

                if (filter.SourceCategories != null &&
                    filter.SourceCategories.Contains(inventoryKey.Item2.ToInventoryCategory()) &&
                    (displaySourceCrossCharacter ||
                     _characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                {
                    if (!sourceInventories.ContainsKey(inventoryKey))
                    {
                        sourceInventories.Add(inventoryKey,
                            inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
                    }
                }

                if (filter.SourceWorlds != null &&
                    filter.SourceWorlds.Contains(_characterMonitor.GetCharacterById(character.CharacterId)?.WorldId ?? 0))
                {
                    if (!sourceInventories.ContainsKey(inventoryKey))
                    {
                        sourceInventories.Add(inventoryKey,
                            inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
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
                    _characterMonitor.IsRetainer(character.CharacterId) && (displayDestinationCrossCharacter ||
                                                                            _characterMonitor
                                                                                .BelongsToActiveCharacter(
                                                                                    character.CharacterId)))
                {
                    if (!destinationInventories.ContainsKey(inventoryKey))
                    {
                        destinationInventories.Add(inventoryKey,
                            inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
                    }
                }

                if (filter.DestinationAllFreeCompanies.HasValue && filter.DestinationAllFreeCompanies.Value &&
                    _characterMonitor.IsFreeCompany(character.CharacterId) &&
                    (displayDestinationCrossCharacter ||
                     _characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                {
                    if (!destinationInventories.ContainsKey(inventoryKey))
                    {
                        destinationInventories.Add(inventoryKey,
                            inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
                    }
                }

                if (filter.DestinationAllHouses.HasValue && filter.DestinationAllHouses.Value &&
                    _characterMonitor.IsHousing(character.CharacterId) &&
                    (displayDestinationCrossCharacter ||
                     _characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                {
                    if (!destinationInventories.ContainsKey(inventoryKey))
                    {
                        destinationInventories.Add(inventoryKey,
                            inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
                    }
                }

                if (filter.DestinationAllCharacters.HasValue && filter.DestinationAllCharacters.Value &&
                    _characterMonitor.ActiveCharacterId == character.CharacterId &&
                    (displayDestinationCrossCharacter ||
                     _characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                {
                    if (!destinationInventories.ContainsKey(inventoryKey))
                    {
                        destinationInventories.Add(inventoryKey,
                            inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
                    }
                }

                if (filter.DestinationInventories.Contains((character.CharacterId,
                        inventoryKey.type.ToInventoryCategory())) &&
                    (displayDestinationCrossCharacter ||
                     _characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                {
                    if (!destinationInventories.ContainsKey(inventoryKey))
                    {
                        destinationInventories.Add(inventoryKey,
                            inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
                    }
                }

                if (filter.DestinationCategories != null &&
                    filter.DestinationCategories.Contains(inventory.Key.ToInventoryCategory()) &&
                    (displayDestinationCrossCharacter ||
                     _characterMonitor.BelongsToActiveCharacter(character.CharacterId)))
                {
                    if (!destinationInventories.ContainsKey(inventoryKey))
                    {
                        destinationInventories.Add(inventoryKey,
                            inventory.Value.Where(c => c != null && c.SortedContainer == type).ToList()!);
                    }
                }
            }
        }
    }

    public Task RequestRefresh(FilterConfiguration configuration, List<Inventory>? inventories = null)
    {
        if (!configuration.AllowRefresh)
        {
            return Task.CompletedTask;
        }
        configuration.Refreshing = true;
        return FilterQueue.QueueBackgroundWorkItemAsync(token =>
        {
            return Task.Run(() => RefreshList(configuration), token);
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await BackgroundProcessing(stoppingToken);
    }

    private async Task BackgroundProcessing(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem =
                await FilterQueue.DequeueAsync(stoppingToken);

            try
            {
                await workItem(stoppingToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,
                    "Error occurred executing {WorkItem}.", nameof(workItem));
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        Logger.LogTrace("Queued Hosted Service is stopping.");

        await base.StopAsync(stoppingToken);
    }
}
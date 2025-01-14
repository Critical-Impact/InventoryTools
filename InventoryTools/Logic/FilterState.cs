using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Model;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic
{
    public class FilterState
    {
        public FilterState(ILogger<FilterState> logger, ICharacterMonitor characterMonitor, WindowService windowService, IGameUiManager gameUiManager, IInventoryMonitor inventoryMonitor, InventoryToolsConfiguration configuration, ExcelSheet<CabinetCategory> cabinetCategorySheet)
        {
            _logger = logger;
            _characterMonitor = characterMonitor;
            _windowService = windowService;
            _gameUiManager = gameUiManager;
            _inventoryMonitor = inventoryMonitor;
            _configuration = configuration;
            _cabinetCategorySheet = cabinetCategorySheet;
        }

        public void Initialize(FilterConfiguration filterConfiguration)
        {
            FilterConfiguration = filterConfiguration;
        }

        public FilterConfiguration FilterConfiguration;
        private readonly ILogger<FilterState> _logger;
        private readonly ICharacterMonitor _characterMonitor;
        private readonly WindowService _windowService;
        private readonly IGameUiManager _gameUiManager;
        private readonly IInventoryMonitor _inventoryMonitor;
        private readonly InventoryToolsConfiguration _configuration;
        private readonly ExcelSheet<CabinetCategory> _cabinetCategorySheet;
        public RenderTableBase? FilterTable;
        public ulong? ActiveRetainerId => _characterMonitor.ActiveRetainerId == 0 ? null : _characterMonitor.ActiveRetainerId;
        public ulong? ActiveFreeCompanyId => _characterMonitor.ActiveFreeCompanyId == 0 ? null : _characterMonitor.ActiveFreeCompanyId;
        public ulong? ActiveHousingId => _characterMonitor.ActiveHouseId == 0 ? null : _characterMonitor.ActiveHouseId;

        public bool InvertHighlighting
        {
            get
            {
                var activeFilter = FilterConfiguration;
                return activeFilter.InvertHighlighting ?? _configuration.InvertHighlighting;
            }
        }

        public bool InvertDestinationHighlighting
        {
            get
            {
                var activeFilter = FilterConfiguration;
                return activeFilter.InvertDestinationHighlighting ?? _configuration.InvertDestinationHighlighting;
            }
        }

        public bool InvertTabHighlighting
        {
            get
            {
                var activeFilter = FilterConfiguration;
                return activeFilter.InvertTabHighlighting ?? _configuration.InvertTabHighlighting;
            }
        }

        public Vector4 BagHighlightColor => FilterConfiguration.HighlightColor ?? _configuration.HighlightColor;
        public Vector4 BagDestinationHighlightColor => FilterConfiguration.DestinationHighlightColor ?? _configuration.DestinationHighlightColor;
        public Vector4 TabHighlightColor => FilterConfiguration.TabHighlightColor ?? _configuration.TabHighlightColor;

        public bool ShouldHighlight
        {
            get
            {
                var activeFilter = FilterConfiguration;
                RenderTableBase? activeTable = FilterTable;
                bool shouldHighlight = false;
                if (_windowService.HasFilterWindowOpen)
                {
                    if (activeTable != null)
                    {
                        //Allow table to override highlight mode on filter
                        var activeTableHighlightItems = activeTable.HighlightItems;
                        if (activeTableHighlightItems)
                        {
                            shouldHighlight = activeTableHighlightItems;
                            if (activeFilter.HighlightWhen is "When Searching" || activeFilter.HighlightWhen == null && _configuration.HighlightWhen == "When Searching")
                            {
                                if (!activeTable.IsSearching)
                                {
                                    return false;
                                }
                            }

                            if (activeFilter.FilterItemsInRetainersEnum == FilterItemsRetainerEnum.Only)
                            {
                                if (_characterMonitor.ActiveRetainerId == 0 && !_gameUiManager.IsWindowVisible(WindowName.RetainerList))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
                else
                {
                    shouldHighlight = true;
                    if (activeFilter.FilterItemsInRetainersEnum == FilterItemsRetainerEnum.Only)
                    {
                        if (_characterMonitor.ActiveRetainerId == 0 && !_gameUiManager.IsWindowVisible(WindowName.RetainerList))
                        {
                            shouldHighlight = false;
                        }
                    }

                }

                return shouldHighlight;
            }
        }

        public bool ShouldHighlightDestination => ShouldHighlight && FilterConfiguration.HighlightDestination != null && FilterConfiguration.HighlightDestination.Value || FilterConfiguration.HighlightDestination == null && _configuration.HighlightDestination;
        public bool ShouldHighlightDestinationEmpty => ShouldHighlight && FilterConfiguration.HighlightDestinationEmpty != null && FilterConfiguration.HighlightDestinationEmpty.Value || FilterConfiguration.HighlightDestinationEmpty == null && _configuration.HighlightDestinationEmpty;

        public Vector4? GetTabHighlight(Dictionary<Vector2, Vector4?> bagHighlights)
        {
            if (InvertHighlighting)
            {
                if (InvertTabHighlighting)
                {
                    return bagHighlights.All(c => c.Value != null) ? TabHighlightColor : null;
                }
                else
                {
                    return bagHighlights.All(c => c.Value != null) ? null : TabHighlightColor;
                }
            }
            else
            {
                if (InvertTabHighlighting)
                {
                    return bagHighlights.All(c => c.Value == null) ? TabHighlightColor : null;
                }
                else
                {
                    return bagHighlights.Any(c => c.Value != null) ? TabHighlightColor : null;
                }
            }
        }

        public Vector4? GetTabHighlights(List<Dictionary<Vector2, Vector4?>> bagHighlights)
        {
            if (InvertHighlighting)
            {
                if (InvertTabHighlighting)
                {
                    return bagHighlights.All(c => c.All(d => d.Value != null)) ? TabHighlightColor : null;
                }
                else
                {
                    return bagHighlights.Any(c => c.Any(d => d.Value == null)) ? TabHighlightColor : null;
                }
            }
            else
            {
                if (InvertTabHighlighting)
                {
                    return bagHighlights.Any(c => c.Any(d => d.Value != null)) ? null : TabHighlightColor;
                }
                else
                {
                    return bagHighlights.Any(c => c.Any(d => d.Value != null)) ? TabHighlightColor : null;
                }
            }
        }

        public List<Vector4?> GetSelectIconStringItems(List<IShop> shops, List<SearchResult>? resultOverride = null)
        {
            var itemHighlights = new List<Vector4?>();
            if (_characterMonitor.ActiveCharacterId == 0)
            {
                return itemHighlights;
            }
            var filterResult = resultOverride ?? FilterResult;
            if (filterResult != null)
            {
                HashSet<uint> requiredItems;
                if (FilterConfiguration.FilterType == FilterType.CraftFilter)
                {
                    requiredItems = FilterConfiguration.CraftList.GetFlattenedMergedMaterials().Where(c => c.IngredientPreference.Type is IngredientPreferenceType.Buy or IngredientPreferenceType.Item or IngredientPreferenceType.HouseVendor ).Select(c => c.Item.RowId).Distinct()
                        .ToHashSet();
                }
                else if (filterResult.Count != 0)
                {
                    requiredItems = filterResult.Select(c => c.Item.RowId).Distinct().ToHashSet();
                }
                else
                {
                    requiredItems = new HashSet<uint>();
                }

                foreach (var shop in shops)
                {
                    var shouldHighlight = shop.Items.Any(c => requiredItems.Contains(c.RowId));
                    itemHighlights.Add(shouldHighlight ? FilterConfiguration.RetainerListColor ?? _configuration.RetainerListColor : null);
                }
            }

            return itemHighlights;
        }

        public HashSet<uint> GetItemIds(List<SearchResult>? resultOverride = null)
        {
            var itemIds = new HashSet<uint>();
            if (_characterMonitor.ActiveCharacterId == 0)
            {
                return itemIds;
            }
            var filterResult = resultOverride ?? FilterResult;
            if (filterResult != null)
            {
                if (FilterConfiguration.FilterType == FilterType.CraftFilter)
                {
                    itemIds = FilterConfiguration.CraftList.GetFlattenedMergedMaterials().Where(c => c.IngredientPreference.Type is IngredientPreferenceType.Buy or IngredientPreferenceType.Item or IngredientPreferenceType.HouseVendor).Select(c => c.Item.RowId).Distinct()
                        .ToHashSet();
                }
                else if (filterResult.Count != 0)
                {
                    itemIds = filterResult.Select(c => c.Item.RowId).Distinct().ToHashSet();
                }
            }

            return itemIds;
        }

        public Dictionary<string, Vector4?> GetArmoireHighlights(List<SearchResult>? resultOverride = null)
        {
            var bagHighlights = new Dictionary<string, Vector4?>();
            if (_characterMonitor.ActiveCharacterId == 0)
            {
                return bagHighlights;
            }
            var filterResult = resultOverride ?? FilterResult;
            if (filterResult != null)
            {
                var fullInventory =
                    _inventoryMonitor.GetSpecificInventory(_characterMonitor
                        .ActiveCharacterId, InventoryCategory.Armoire);

                var itemIds = fullInventory.Select(c => c.ItemId).Distinct().ToImmutableHashSet();

                var filteredItems = filterResult.Where(c =>
                {
                    if (c.SortingResult == null)
                    {
                        return itemIds.Contains(c.Item.RowId);
                    }
                    return c.SortingResult is {SourceBag: InventoryType.Armoire};
                });
                foreach (var item in filteredItems)
                {
                    if(MatchesFilter(FilterConfiguration, item, InvertHighlighting) || MatchesRetainerFilter(FilterConfiguration, item, InvertHighlighting))
                    {
                        if (!bagHighlights.ContainsKey(item.Item.NameString))
                        {
                            bagHighlights.Add(item.Item.NameString, BagHighlightColor);
                        }
                    }
                }

                if (InvertHighlighting)
                {
                    var invertedHighlights = new Dictionary<string, Vector4?>();
                    foreach (var item in fullInventory)
                    {
                        if (!bagHighlights.ContainsKey(item.FormattedName))
                        {
                            if (!invertedHighlights.ContainsKey(item.FormattedName))
                            {
                                invertedHighlights.Add(item.FormattedName, BagHighlightColor);
                            }
                        }
                    }

                    return invertedHighlights;
                }


                return bagHighlights;
            }

            return new Dictionary<string, Vector4?>();
        }

        public Dictionary<uint, Vector4?> GetArmoireTabHighlights(List<SearchResult>? resultOverride = null)
        {
            var bagHighlights = new Dictionary<uint, Vector4?>();
            if (_characterMonitor.ActiveCharacterId == 0)
            {
                return bagHighlights;
            }
            var filterResult = resultOverride ?? FilterResult;
            if (filterResult != null)
            {
                var fullInventory =
                    _inventoryMonitor.GetSpecificInventory(_characterMonitor
                        .ActiveCharacterId, InventoryCategory.Armoire);

                var itemIds = fullInventory.Select(c => c.ItemId).Distinct().ToImmutableHashSet();

                var filteredItems = filterResult.Where(c =>
                {
                    if (c.SortingResult == null)
                    {
                        return itemIds.Contains(c.Item.RowId);
                    }
                    return c.SortingResult is {SourceBag: InventoryType.Armoire};
                });
                var cabinetDictionary = _cabinetCategorySheet.Where(c => c.Category.RowId != 0).ToDictionary(c => c.Category.RowId, c => (uint)c.MenuOrder - 1);
                foreach (var item in filteredItems)
                {
                    if (item.Item.CabinetCategory == null)
                    {
                        continue;
                    }
                    if((MatchesFilter(FilterConfiguration, item, InvertHighlighting) || MatchesRetainerFilter(FilterConfiguration, item, InvertHighlighting)))
                    {
                        if (cabinetDictionary.ContainsKey(item.Item.CabinetCategory.RowId) && !bagHighlights.ContainsKey(cabinetDictionary[item.Item.CabinetCategory.RowId]))
                        {
                            bagHighlights.Add(cabinetDictionary[item.Item.CabinetCategory.RowId], TabHighlightColor);
                        }
                    }
                }

                if (InvertTabHighlighting)
                {
                    var invertedHighlights = new Dictionary<uint, Vector4?>();

                    foreach (var cab in cabinetDictionary)
                    {
                        if (!bagHighlights.ContainsKey(cab.Value))
                        {
                            if (!invertedHighlights.ContainsKey(cab.Value))
                            {
                                invertedHighlights.Add(cab.Value, TabHighlightColor);
                            }
                        }
                        else
                        {
                            if (!invertedHighlights.ContainsKey(cab.Value))
                            {
                                invertedHighlights.Add(cab.Value, null);
                            }
                        }
                    }

                    return invertedHighlights;

                }

                return bagHighlights;
            }

            return new Dictionary<uint, Vector4?>();
        }

        public Dictionary<Vector2, Vector4?> GetGlamourHighlights(AtkInventoryMiragePrismBox.DresserTab dresserTab, int page, bool displayEquippableOnly,uint classJobSelected, List<SearchResult>? resultOverride = null)
        {
            var inventoryType = InventoryType.GlamourChest;
            var characterId = GetCharacterId(inventoryType);
            var filterResult = resultOverride ?? FilterResult;
            if (characterId == null || filterResult == null)
            {
                return new Dictionary<Vector2, Vector4?>();
            }

            var inventoryContents = _inventoryMonitor.GetSpecificInventory(characterId.Value, inventoryType);

            var bagLayout = GenerateBagLayout(inventoryType);

            var filteredItems = inventoryContents.Where(c =>
                AtkInventoryMiragePrismBox.EquipSlotCategoryToDresserTab(c.Item.Base.EquipSlotCategory.ValueNullable) ==
                dresserTab);

            if (classJobSelected != 0)
            {
                filteredItems = filteredItems.Where(c => c.Item.ClassJobCategory?.ClassJobIds.Contains(classJobSelected) ?? false);
            }

            if (displayEquippableOnly && _characterMonitor.ActiveCharacter != null)
            {
                var race = _characterMonitor.ActiveCharacter.Race;
                var gender = _characterMonitor.ActiveCharacter.Gender;
                filteredItems = filteredItems.Where(c => c.Item.CanBeEquippedByRaceGender(race, gender));

            }

            inventoryContents =
                filteredItems.Skip(page * 50).Take(50).OrderBy(c => c.SortedSlotIndex).ToList();

            var slotToPosition = new Dictionary<int, Vector2>();

            for (var index = 0; index < inventoryContents.Count; index++)
            {
                var item = inventoryContents[index];
                slotToPosition[item.SortedSlotIndex] = item.BagLocation(InventoryType.GlamourChest, index);
            }

            var contentsMap = inventoryContents.GroupBy(c => c.ItemId).ToDictionary(c => c.Key, c => c.ToList());

            foreach (var searchResult in filterResult)
            {
                if (searchResult.SortingResult != null)
                {
                    var matchesSource = searchResult.SortingResult.SourceBag == inventoryType &&
                                        (MatchesFilter(FilterConfiguration, searchResult, InvertHighlighting) ||
                                         MatchesRetainerFilter(FilterConfiguration, searchResult, InvertHighlighting) ||
                                         MatchesFreeCompanyFilter(FilterConfiguration, searchResult.SortingResult, InvertHighlighting) ||
                                         MatchesHousingFilter(FilterConfiguration, searchResult.SortingResult, InvertHighlighting));

                    if (!matchesSource)
                    {
                        continue;
                    }
                }

                if (contentsMap.TryGetValue(searchResult.ItemId, out var value))
                {
                    foreach (var content in value)
                    {
                        Vector2? bagLocation = slotToPosition.TryGetValue(content.SortedSlotIndex, out var slotPosition) ? slotPosition : null;
                        if (bagLocation != null && bagLayout.ContainsKey(bagLocation.Value))
                        {
                            bagLayout[bagLocation.Value] = content.IsEmpty ? null : content;
                        }
                    }
                }
            }

            var bagHighlights = new Dictionary<Vector2, Vector4?>();
            foreach (var item in bagLayout)
            {
                bagHighlights[item.Key] = InvertHighlighting ? item.Value == null ? BagHighlightColor : null : item.Value == null ? null : BagHighlightColor;
            }

            return bagHighlights;
        }

        public ulong? GetCharacterId(InventoryType inventoryType)
        {
            var category = inventoryType.ToInventoryCategory();
            switch (category)
            {
                case InventoryCategory.CharacterBags:
                    return _characterMonitor.ActiveCharacterId;
                    break;
                case InventoryCategory.CharacterSaddleBags:
                    return _characterMonitor.ActiveCharacterId;
                    break;
                case InventoryCategory.CharacterPremiumSaddleBags:
                    return _characterMonitor.ActiveCharacterId;
                    break;
                case InventoryCategory.RetainerBags:
                    return ActiveRetainerId;
                    break;
                case InventoryCategory.CharacterArmoryChest:
                    return _characterMonitor.ActiveCharacterId;
                    break;
                case InventoryCategory.CharacterEquipped:
                    return _characterMonitor.ActiveCharacterId;
                    break;
                case InventoryCategory.RetainerEquipped:
                    return ActiveRetainerId;
                    break;
                case InventoryCategory.RetainerMarket:
                    return ActiveRetainerId;
                    break;
                case InventoryCategory.GlamourChest:
                    return _characterMonitor.ActiveCharacterId;
                    break;
                case InventoryCategory.Armoire:
                    return _characterMonitor.ActiveCharacterId;
                    break;
                case InventoryCategory.FreeCompanyBags:
                    return _characterMonitor.ActiveFreeCompanyId;
                    break;
                default:
                    return null;
            }
        }

        public Dictionary<Vector2, InventoryItem?> GenerateBagLayout(InventoryType inventoryType)
        {
            var layout = new Dictionary<Vector2, InventoryItem?>();
            var category = inventoryType.ToInventoryCategory();
            if (category is InventoryCategory.CharacterBags or InventoryCategory.RetainerBags or InventoryCategory.CharacterSaddleBags or InventoryCategory.CharacterPremiumSaddleBags)
            {
                for (var x = 0; x < 5; x++)
                {
                    for (var y = 0; y < 7; y++)
                    {
                        var position = new Vector2(x,y);
                        layout.Add(position, null);
                    }
                }
            }
            if (category is InventoryCategory.FreeCompanyBags or InventoryCategory.CharacterArmoryChest)
            {
                for (int x = 0; x < 50; x++)
                {
                    var position = new Vector2(x,0);
                    layout.Add(position, null);
                }
            }
            if (category is InventoryCategory.GlamourChest)
            {
                for (int x = 0; x < 10; x++)
                {
                    for (int y = 0; y < 5; y++)
                    {
                        var position = new Vector2(x,y);
                        layout.Add(position, null);
                    }
                }
            }

            return layout;
        }

        public Dictionary<Vector2, Vector4?> GetBagHighlights(InventoryType inventoryType, List<SearchResult>? resultOverride = null)
        {
            var characterId = GetCharacterId(inventoryType);
            var filterResult = resultOverride ?? FilterResult;
            if (characterId == null || filterResult == null)
            {
                return new Dictionary<Vector2, Vector4?>();
            }

            var inventoryContents = _inventoryMonitor.GetSpecificInventory(characterId.Value, inventoryType);

            var contentsMap = inventoryContents.GroupBy(c => c.ItemId).ToDictionary(c => c.Key, c => c.ToList());
            var bagLayout = GenerateBagLayout(inventoryType);
            var bagDestinationLayout = GenerateBagLayout(inventoryType);

            foreach (var searchResult in filterResult)
            {
                bool destination = false;
                if (searchResult.SortingResult != null)
                {
                    var matchesSource = searchResult.SortingResult.SourceBag == inventoryType &&
                                        (MatchesFilter(FilterConfiguration, searchResult, InvertHighlighting) ||
                                         MatchesRetainerFilter(FilterConfiguration, searchResult, InvertHighlighting) ||
                                         MatchesFreeCompanyFilter(FilterConfiguration, searchResult.SortingResult, InvertHighlighting) ||
                                         MatchesHousingFilter(FilterConfiguration, searchResult.SortingResult, InvertHighlighting));

                    var matchesDestination = ShouldHighlightDestination && searchResult.SortingResult.DestinationBag == inventoryType &&
                                             (MatchesFilter(FilterConfiguration, searchResult, InvertHighlighting) ||
                                              MatchesRetainerFilter(FilterConfiguration, searchResult, InvertHighlighting,
                                                  true) || MatchesFreeCompanyFilter(FilterConfiguration, searchResult.SortingResult,
                                                  InvertHighlighting, true) ||
                                              MatchesHousingFilter(FilterConfiguration, searchResult.SortingResult, InvertHighlighting,
                                                  true));
                    if (!matchesSource && !matchesDestination)
                    {
                        continue;
                    }

                    if (matchesDestination)
                    {
                        destination = true;
                    }
                }

                if (contentsMap.TryGetValue(searchResult.ItemId, out var value))
                {
                    foreach (var content in value)
                    {
                        var bagLocation = content.BagLocation(inventoryType);
                        if (destination)
                        {
                            if (bagDestinationLayout.ContainsKey(bagLocation))
                            {
                                bagDestinationLayout[bagLocation] = content;
                            }
                        }
                        else
                        {
                            if (bagLayout.ContainsKey(bagLocation))
                            {
                                bagLayout[bagLocation] = content.IsEmpty ? null : content;
                            }
                        }
                    }
                }
                else if (destination && ShouldHighlightDestinationEmpty && (searchResult.SortingResult?.IsEmptyDestinationSlot ?? false))
                {
                    var bagLocation = searchResult.SortingResult.BagLocation;
                    if (bagDestinationLayout.ContainsKey(bagLocation))
                    {
                        bagDestinationLayout[bagLocation] = searchResult.SortingResult.DestinationItem;
                    }
                }
            }

            var bagHighlights = new Dictionary<Vector2, Vector4?>();
            foreach (var item in bagLayout)
            {
                bagHighlights[item.Key] = InvertHighlighting ? item.Value == null ? BagHighlightColor : null : item.Value == null ? null : BagHighlightColor;
            }
            foreach (var item in bagDestinationLayout)
            {
                if (item.Value != null)
                {
                    bagHighlights[item.Key] = InvertDestinationHighlighting
                        ?
                        item.Value == null ? BagDestinationHighlightColor : null
                        : item.Value == null
                            ? null
                            : BagDestinationHighlightColor;
                }
            }


            return bagHighlights;
        }

        private bool MatchesRetainerFilter(FilterConfiguration activeFilter, SearchResult searchResult, bool invertHighlighting = false, bool destinationFilter = false)
        {
            if (searchResult.InventoryItem == null)
            {
                return false;
            }

            bool matches = true;

            if (searchResult.SortingResult != null)
            {

                if (searchResult.SortingResult.SourceRetainerId != _characterMonitor.ActiveRetainerId)
                {
                    return false;
                }

                if (destinationFilter && searchResult.SortingResult.SourceRetainerId == ActiveRetainerId)
                {
                    return false;
                }
            }

            if (matches)
            {
                if (!searchResult.InventoryItem.IsEmpty)
                {
                    return true;
                }
            }

            if (searchResult.InventoryItem.IsEmpty && invertHighlighting)
            {
                return true;
            }

            return false;
        }

        private bool MatchesFreeCompanyFilter(FilterConfiguration activeFilter, SortingResult item, bool invertHighlighting = false, bool destinationFilter = false)
        {
            bool matches = (activeFilter.FilterType.HasFlag(FilterType.SearchFilter) || activeFilter.FilterType.HasFlag(FilterType.SortingFilter) || activeFilter.FilterType.HasFlag(FilterType.CraftFilter));
            if (item.SourceRetainerId != _characterMonitor.ActiveFreeCompanyId)
            {
                return false;
            }

            if (destinationFilter && item.SourceRetainerId == ActiveFreeCompanyId)
            {
                return false;
            }
            if (matches)
            {
                if (!item.InventoryItem.IsEmpty)
                {
                    return true;
                }
            }

            if (item.InventoryItem.IsEmpty && invertHighlighting)
            {
                return true;
            }

            return false;
        }

        private bool MatchesHousingFilter(FilterConfiguration activeFilter, SortingResult item, bool invertHighlighting = false, bool destinationFilter = false)
        {
            bool matches = (activeFilter.FilterType.HasFlag(FilterType.SearchFilter) || activeFilter.FilterType.HasFlag(FilterType.SortingFilter) || activeFilter.FilterType.HasFlag(FilterType.CraftFilter));
            if (item.SourceRetainerId != _characterMonitor.ActiveHouseId)
            {
                return false;
            }

            if (destinationFilter && item.SourceRetainerId == ActiveHousingId)
            {
                return false;
            }
            if (matches)
            {
                if (!item.InventoryItem.IsEmpty)
                {
                    return true;
                }
            }

            if (item.InventoryItem.IsEmpty && invertHighlighting)
            {
                return true;
            }

            return false;
        }

        private bool MatchesFilter(FilterConfiguration activeFilter, SearchResult searchResult, bool invertHighlighting = false)
        {
            if (searchResult.InventoryItem == null)
            {
                return false;
            }

            bool matches = false;

            if (searchResult.SortingResult != null)
            {
                if (activeFilter.FilterType == FilterType.SearchFilter && searchResult.SortingResult.SourceRetainerId == _characterMonitor.ActiveCharacterId)
                {
                    matches = true;
                }

                if (searchResult.SortingResult.SourceRetainerId == _characterMonitor.ActiveCharacterId &&
                    (ActiveRetainerId == null ||
                     ActiveRetainerId != null &&
                     searchResult.SortingResult.DestinationRetainerId ==
                     ActiveRetainerId))
                {
                    matches = true;
                }
            }
            else
            {
                matches = true;
            }



            if (matches)
            {
                if (!searchResult.InventoryItem.IsEmpty)
                {
                    return true;
                }

            }

            if (searchResult.InventoryItem.IsEmpty && invertHighlighting)
            {
                return false;
            }

            return false;
        }

        public bool HasFilterResult
        {
            get
            {
                return FilterConfiguration.SearchResults != null;
            }
        }

        public List<SearchResult>? FilterResult
        {
            get
            {
                var activeFilter = FilterConfiguration;
                RenderTableBase? activeTable = FilterTable;
                var filteredList = activeFilter.SearchResults;

                if (activeTable != null)
                {
                    filteredList = activeTable.SearchResults;
                }
                else if (activeFilter.SearchResults != null)
                {
                    filteredList = activeFilter.SearchResults;
                }

                return filteredList;
            }
        }
    }
}
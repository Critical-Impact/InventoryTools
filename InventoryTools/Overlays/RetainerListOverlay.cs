using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using InventoryTools.Logic;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Overlays
{
    public class RetainerListOverlay: GameOverlay<AtkRetainerList>, IAtkOverlayState
    {
        private readonly ICharacterMonitor _characterMonitor;
        private readonly IInventoryMonitor _inventoryMonitor;
        private readonly InventoryToolsConfiguration _configuration;

        public RetainerListOverlay(ILogger<RetainerListOverlay> logger, AtkRetainerList overlay, ICharacterMonitor characterMonitor, IInventoryMonitor inventoryMonitor, InventoryToolsConfiguration configuration) : base(logger,overlay)
        {
            _characterMonitor = characterMonitor;
            _inventoryMonitor = inventoryMonitor;
            _configuration = configuration;
        }
        public override bool ShouldDraw { get; set; }

        public override bool Draw()
        {
            if (!HasState || !AtkOverlay.HasAddon)
            {
                return false;
            }
            var atkUnitBase = AtkOverlay.AtkUnitBase;
            if (atkUnitBase != null)
            {
                this.AtkOverlay.SetNames(RetainerNames, RetainerColors);
                return true;
            }

            return false;
        }
        
        public override void Clear()
        {
            var atkUnitBase = AtkOverlay.AtkUnitBase;
            if (atkUnitBase != null)
            {
                this.AtkOverlay.SetNames(RetainerNames, new Dictionary<ulong, Vector4>());
            }
        }

        public override void Setup()
        {
            
        }


        public Dictionary<ulong, string> RetainerNames = new Dictionary<ulong, string>();
        public Dictionary<ulong, Vector4> RetainerColors = new Dictionary<ulong, Vector4>();
        

        public override bool HasState { get; set; }
        public override bool NeedsStateRefresh { get; set; }

        public override void UpdateState(FilterState? newState)
        {
            if (_characterMonitor.ActiveCharacterId == 0)
            {
                return;
            }

            if (!_configuration.ShowItemNumberRetainerList)
            {
                if (HasState)
                {
                    HasState = false;
                    Clear();
                }

                return;
            }
            if (newState != null && AtkOverlay.HasAddon && newState.ShouldHighlight && newState.HasFilterResult)
            {
                HasState = true;
                var filterResult = newState.FilterResult;
                var filterConfiguration = newState.FilterConfiguration;
                var currentCharacterId = _characterMonitor.LocalContentId;
                if (filterResult != null)
                {
                    if (filterResult.Count != 0)
                    {
                        if (filterConfiguration.FilterType == FilterType.SearchFilter ||
                            filterConfiguration.FilterType == FilterType.GameItemFilter)
                        {
                            var allItems = filterResult;
                            Dictionary<ulong, HashSet<uint>> hasItems = new();
                            Dictionary<ulong, int> characterTotals = new();

                            foreach (var character in _characterMonitor.GetRetainerCharacters(_characterMonitor
                                         .ActiveCharacterId))
                            {
                                hasItems[character.Key] = new HashSet<uint>();
                                characterTotals[character.Key] = 0;
                            }

                            foreach (var item in _inventoryMonitor.AllItems)
                            {
                                if (item.IsEmpty) continue;
                                if (hasItems.ContainsKey(item.RetainerId))
                                {
                                    hasItems[item.RetainerId].Add(item.ItemId);
                                }
                            }


                            foreach (var item in allItems)
                            {
                                foreach (var character in hasItems)
                                {
                                    if (character.Value.Contains(item.Item.RowId))
                                    {
                                        characterTotals[character.Key]++;
                                    }
                                }
                            }

                            var finalTotals = characterTotals.Where(c => c.Value != 0).ToList();
                            RetainerNames = finalTotals.ToDictionary(c => c.Key, GenerateNewName);
                            RetainerColors = finalTotals.ToDictionary(c => c.Key,
                                c => filterConfiguration.RetainerListColor ??
                                     _configuration.RetainerListColor);
                            Draw();
                            return;

                        }

                        if (filterConfiguration.FilterType == FilterType.SortingFilter ||
                            filterConfiguration.FilterType == FilterType.CraftFilter)
                        {
                            var filteredList = filterResult;
                            var grouping = filteredList.Where(c =>
                                    c.InventoryItem != null && c.SortingResult != null && !c.InventoryItem.IsEmpty &&
                                    (c.SortingResult.SourceRetainerId == currentCharacterId ||
                                     c.SortingResult.DestinationRetainerId == currentCharacterId ||
                                     _characterMonitor.BelongsToActiveCharacter(c.SortingResult.SourceRetainerId)) &&
                                    c.SortingResult.DestinationRetainerId != null)
                                .GroupBy(c =>
                                    c.SortingResult!.DestinationRetainerId == currentCharacterId ||
                                    (_characterMonitor.BelongsToActiveCharacter(c.SortingResult!.SourceRetainerId) &&
                                     _characterMonitor.IsHousing(c.SortingResult!.DestinationRetainerId!.Value))
                                        ? c.SortingResult!.SourceRetainerId
                                        : c.SortingResult!.DestinationRetainerId!.Value).Where(c => c.Any()).ToList();

                            RetainerColors = grouping.ToDictionary(c => c.Key,
                                c => filterConfiguration.RetainerListColor ??
                                     _configuration.RetainerListColor);
                            RetainerNames = grouping.ToDictionary(c => c.Key, GenerateNewName);
                            Draw();
                            return;
                        }
                        else
                        {
                            var filteredList = filterResult;
                            var grouping = filteredList
                                .Where(c => c.InventoryItem != null && c.SortingResult != null &&
                                            !c.InventoryItem.IsEmpty).GroupBy(c => c.SortingResult!.SourceRetainerId)
                                .Where(c => c.Any()).ToList();
                            RetainerNames = grouping.ToDictionary(c => c.Key, GenerateNewName);
                            RetainerColors = grouping.ToDictionary(c => c.Key,
                                c => filterConfiguration.RetainerListColor ??
                                     _configuration.RetainerListColor);

                            Draw();
                            return;
                        }
                    }
                }
            }
            if (HasState)
            {
                RetainerNames = _characterMonitor.Characters.Where(c => c.Value.CharacterId == _characterMonitor.ActiveCharacterId).ToDictionary(c => c.Key, c => c.Value.Name);
                Clear();
            }

            HasState = false;
        }

        private string GenerateNewName(IGrouping<ulong, SearchResult> c)
        {
            if (_characterMonitor.Characters.ContainsKey(c.Key))
            {
                return _characterMonitor.Characters[c.Key].FormattedName + " (" + c.Count() + ")";
            }
            return "Unknown "  + "(" + c.Count() + ")";
        }

        private string GenerateNewName(KeyValuePair<ulong, int> c)
        {
            if (_characterMonitor.Characters.ContainsKey(c.Key))
            {
                return _characterMonitor.Characters[c.Key].FormattedName + " (" + c.Value + ")";
            }
            return "Unknown "  + "(" + c.Value + ")";
        }
        
    }
}
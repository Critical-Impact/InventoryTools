using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Models;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic
{
    public struct FilterState
    {
        public FilterConfiguration FilterConfiguration;
        public FilterTable? FilterTable;
        public ulong? ActiveRetainerId;

        public bool InvertHighlighting
        {
            get
            {
                var activeFilter = FilterConfiguration;
                return activeFilter.InvertHighlighting ?? ConfigurationManager.Config.InvertHighlighting;
            }
        }

        public bool InvertTabHighlighting
        {
            get
            {
                var activeFilter = FilterConfiguration;
                return activeFilter.InvertTabHighlighting ?? ConfigurationManager.Config.InvertTabHighlighting;
            }
        }

        public Vector4 BagHighlightColor => FilterConfiguration.HighlightColor ?? ConfigurationManager.Config.HighlightColor;
        public Vector4 TabHighlightColor => FilterConfiguration.TabHighlightColor ?? ConfigurationManager.Config.TabHighlightColor;

        public bool ShouldHighlight
        {
            get
            {
                var activeFilter = FilterConfiguration;
                FilterTable? activeTable = FilterTable;
                bool shouldHighlight = false;
                if (ConfigurationManager.Config.IsVisible)
                {
                    if (activeTable != null)
                    {
                        //Allow table to override highlight mode on filter
                        if (activeTable.HighlightItems)
                        {
                            shouldHighlight = activeTable.HighlightItems;
                            if (activeFilter.HighlightWhen is "When Searching" || activeFilter.HighlightWhen == null && ConfigurationManager.Config.HighlightWhen == "When Searching")
                            {
                                if (!activeTable.IsSearching)
                                {
                                    shouldHighlight = false;
                                }
                            }
                        }
                    }
                }
                else
                {
                    shouldHighlight = true;
                }

                return shouldHighlight;
            }
        }

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

        public Dictionary<Vector2, Vector4?> GetBagHighlights(InventoryType bag)
        {
            var bagHighlights = new Dictionary<Vector2, Vector4?>();

            if (FilterResult.HasValue)
            {
                if (FilterResult.Value.AllItems.Count != 0)
                {
                    var allItems = FilterResult.Value.AllItems;
                    Dictionary<uint, HashSet<Vector2>> availableItems = new ();
                    
                    foreach (var item in PluginService.InventoryMonitor.AllItems)
                    {
                        if (item.SortedContainer == bag && item.RetainerId == PluginService.CharacterMonitor.ActiveCharacter)
                        {
                            if (!availableItems.ContainsKey(item.ItemId))
                            {
                                availableItems[item.ItemId] = new HashSet<Vector2>();
                            }

                            var bagLocation = item.BagLocation(bag);
                            if (!availableItems[item.ItemId].Contains(bagLocation))
                            {
                                availableItems[item.ItemId].Add(bagLocation);
                            }
                        }
                    }

                    
                    foreach (var item in allItems)
                    {
                        foreach (var availableItem in availableItems)
                        {
                            if (availableItem.Key == item.RowId)
                            {
                                foreach (var position in availableItem.Value)
                                {
                                    if (!InvertHighlighting)
                                    {
                                        bagHighlights.Add(position, BagHighlightColor);
                                    }
                                    else if (InvertHighlighting)
                                    {
                                        bagHighlights.Add(position, null);
                                    }
                                }
                            }
                        }
                    }

                    if (bag is InventoryType.Bag0 or InventoryType.Bag1 or InventoryType.Bag2 or InventoryType.Bag3 or InventoryType.RetainerBag0 or InventoryType.RetainerBag1 or InventoryType.RetainerBag2 or InventoryType.RetainerBag3 or InventoryType.RetainerBag4 or InventoryType.SaddleBag0 or InventoryType.SaddleBag1 or InventoryType.PremiumSaddleBag0 or InventoryType.PremiumSaddleBag1)
                    {
                        for (int x = 0; x < 5; x++)
                        {
                            for (int y = 0; y < 7; y++)
                            {
                                var position = new Vector2(x,y);
                                if(!bagHighlights.ContainsKey(position))
                                {
                                    if (InvertHighlighting)
                                    {
                                        bagHighlights.Add(position, BagHighlightColor);
                                    }
                                    else
                                    {
                                        bagHighlights.Add(position, null);
                                    }
                                }
                            }
                        }
                    }

                    if (bag is InventoryType.ArmoryBody or InventoryType.ArmoryEar or InventoryType.ArmoryFeet or InventoryType.ArmoryHand or InventoryType.ArmoryHead or InventoryType.ArmoryLegs or InventoryType.ArmoryMain or InventoryType.ArmoryNeck or InventoryType.ArmoryOff or InventoryType.ArmoryRing  or InventoryType.ArmoryWrist or InventoryType.ArmorySoulCrystal  or InventoryType.FreeCompanyBag0 or InventoryType.FreeCompanyBag1 or InventoryType.FreeCompanyBag2 or InventoryType.FreeCompanyBag3 or InventoryType.FreeCompanyBag4)
                    {
                        for (int x = 0; x < 50; x++)
                        {
                            var position = new Vector2(x,0);
                            if(!bagHighlights.ContainsKey(position))
                            {
                                if (InvertHighlighting)
                                {
                                    bagHighlights.Add(position, BagHighlightColor);
                                }
                                else
                                {
                                    bagHighlights.Add(position, null);
                                }
                            }
                        }
                    }
                    return bagHighlights;
                }

                
                foreach (var item in FilterResult.Value.SortedItems)
                {
                    if(item.SourceBag == bag && (MatchesFilter(FilterConfiguration, item, InvertHighlighting) || MatchesRetainerFilter(FilterConfiguration, item, InvertHighlighting)))
                    {
                        var itemBagLocation = item.BagLocation;
                        if (!bagHighlights.ContainsKey(itemBagLocation))
                        {
                            if (!InvertHighlighting && !item.InventoryItem.IsEmpty)
                            {
                                bagHighlights.Add(itemBagLocation, BagHighlightColor);
                            }
                            else if (InvertHighlighting && item.InventoryItem.IsEmpty)
                            {
                                bagHighlights.Add(itemBagLocation, BagHighlightColor);
                            }
                            else if(InvertHighlighting)
                            {
                                bagHighlights.Add(itemBagLocation, null);
                            }
                            else
                            {
                                bagHighlights.Add(itemBagLocation, null);
                            }
                        }
                    }
                }
            }

            if (bag is InventoryType.Bag0 or InventoryType.Bag1 or InventoryType.Bag2 or InventoryType.Bag3 or InventoryType.RetainerBag0 or InventoryType.RetainerBag1 or InventoryType.RetainerBag2 or InventoryType.RetainerBag3 or InventoryType.RetainerBag4 or InventoryType.SaddleBag0 or InventoryType.SaddleBag1 or InventoryType.PremiumSaddleBag0 or InventoryType.PremiumSaddleBag1)
            {
                for (int x = 0; x < 5; x++)
                {
                    for (int y = 0; y < 7; y++)
                    {
                        var position = new Vector2(x,y);
                        if(!bagHighlights.ContainsKey(position))
                        {
                            if (InvertHighlighting)
                            {
                                bagHighlights.Add(position, BagHighlightColor);
                            }
                            else
                            {
                                bagHighlights.Add(position, null);
                            }
                        }
                    }
                }
            }
            
            if (bag is InventoryType.ArmoryBody or InventoryType.ArmoryEar or InventoryType.ArmoryFeet or InventoryType.ArmoryHand or InventoryType.ArmoryHead or InventoryType.ArmoryLegs or InventoryType.ArmoryMain or InventoryType.ArmoryNeck or InventoryType.ArmoryOff or InventoryType.ArmoryRing  or InventoryType.ArmoryWrist or InventoryType.ArmorySoulCrystal or InventoryType.FreeCompanyBag0 or InventoryType.FreeCompanyBag1 or InventoryType.FreeCompanyBag2 or InventoryType.FreeCompanyBag3 or InventoryType.FreeCompanyBag4)
            {
                for (int x = 0; x < 50; x++)
                {
                    var position = new Vector2(x,0);
                    if(!bagHighlights.ContainsKey(position))
                    {
                        if (InvertHighlighting)
                        {
                            bagHighlights.Add(position, BagHighlightColor);
                        }
                        else
                        {
                            bagHighlights.Add(position, null);
                        }
                    }
                }
            }
            return bagHighlights;
        }
        
        private bool MatchesRetainerFilter(FilterConfiguration activeFilter, SortingResult item, bool invertHighlighting = false)
        {
            bool matches = (activeFilter.FilterType.HasFlag(FilterType.SearchFilter) || activeFilter.FilterType.HasFlag(FilterType.SortingFilter)) && item.SourceRetainerId == PluginService.CharacterMonitor.ActiveRetainer;

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
        
        private bool MatchesFilter(FilterConfiguration activeFilter, SortingResult item, bool invertHighlighting = false)
        {
            bool matches = false;
            if (activeFilter.FilterType == FilterType.SearchFilter &&
                item.SourceRetainerId == Service.ClientState.LocalContentId)
            {
                matches = true;
            }
            else if (activeFilter.FilterType == FilterType.GameItemFilter)
            {
                matches = true;
            }
            
            if (item.SourceRetainerId == Service.ClientState.LocalContentId && (ActiveRetainerId == null ||
                ActiveRetainerId != null &&
                item.DestinationRetainerId ==
                ActiveRetainerId))
            {
                matches = true;
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
                return false;
            }

            return false;
        }

        public FilterResult? FilterResult
        {
            get
            {
                var activeFilter = FilterConfiguration;
                FilterTable? activeTable = FilterTable;
                FilterResult? filteredList = activeFilter.FilterResult;
                
                if (activeTable != null)
                {
                    filteredList = new FilterResult(activeTable.SortedItems.ToList(), new List<InventoryItem>(), activeTable.Items);
                }
                else if (activeFilter.FilterResult.HasValue)
                {
                    filteredList = activeFilter.FilterResult.Value;
                }

                return filteredList;
            }
        }
    }
}
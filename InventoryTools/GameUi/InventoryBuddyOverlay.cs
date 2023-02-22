using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services.Ui;
using InventoryTools.Logic;

namespace InventoryTools.GameUi
{
    public class InventoryBuddyOverlay : AtkInventoryBuddy, IAtkOverlayState
    {
        public override bool Draw()
        {
            if (!HasState || !HasAddon)
            {
                return false;
            }
            var atkUnitBase = AtkUnitBase;
            if (atkUnitBase != null)
            {
                this.SetTabColors(TabColours);
                if (CurrentTab == 0)
                {
                    this.SetColors(InventoryType.SaddleBag0, Bag1InventoryColours);
                    this.SetColors(InventoryType.SaddleBag1, Bag2InventoryColours);
                }
                else if (CurrentTab == 1)
                {
                    this.SetColors(InventoryType.PremiumSaddleBag0, PBag1InventoryColours);
                    this.SetColors(InventoryType.PremiumSaddleBag1, PBag2InventoryColours);
                }
                else
                {
                    this.SetColors(InventoryType.SaddleBag0, Bag1InventoryColours);
                    this.SetColors(InventoryType.SaddleBag1, Bag2InventoryColours);
                }

                return true;
            }

            return false;
        }
        
        public Dictionary<Vector2, Vector4?> Bag1InventoryColours = new();
        public Dictionary<Vector2, Vector4?> Bag2InventoryColours = new();
        public Dictionary<Vector2, Vector4?> PBag1InventoryColours = new();
        public Dictionary<Vector2, Vector4?> PBag2InventoryColours = new();
        public Dictionary<uint, Vector4?> TabColours = new();
        public Dictionary<Vector2, Vector4?> EmptyDictionary = new();
        public Dictionary<uint, Vector4?> EmptyTabs = new() { {0, null}, {1, null}, {2, null}, {3, null} };

        public override void Setup()
        {
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 7; y++)
                {
                    EmptyDictionary.Add(new Vector2(x,y), null);
                }
            }

        }

        public bool HasState { get; set; }
        public bool NeedsStateRefresh { get; set; }

        public void UpdateState(FilterState? newState)
        {
            if (PluginService.CharacterMonitor.ActiveCharacterId == 0)
            {
                return;
            }
            if (newState != null && HasAddon && newState.ShouldHighlight && newState.HasFilterResult)
            {
                HasState = true;
                var filterResult = newState.FilterResult;
                if (filterResult != null)
                {
                    Bag1InventoryColours = newState.GetBagHighlights(InventoryType.SaddleBag0);
                    Bag2InventoryColours = newState.GetBagHighlights(InventoryType.SaddleBag1);
                    PBag1InventoryColours = newState.GetBagHighlights(InventoryType.PremiumSaddleBag0);
                    PBag2InventoryColours = newState.GetBagHighlights(InventoryType.PremiumSaddleBag1);
                    var tab1 = newState.GetTabHighlights(new List<Dictionary<Vector2, Vector4?>>() {Bag1InventoryColours, Bag2InventoryColours});
                    var tab2 = newState.GetTabHighlights(new List<Dictionary<Vector2, Vector4?>>() {PBag1InventoryColours, PBag2InventoryColours});
                    TabColours[0] = tab1;
                    TabColours[1] = tab2;
                    Draw();
                    return;
                }
            }
            
            if (HasState)
            {
                Bag1InventoryColours = EmptyDictionary;
                Bag2InventoryColours = EmptyDictionary;
                PBag1InventoryColours = EmptyDictionary;
                PBag2InventoryColours = EmptyDictionary;
                Clear();
            }

            HasState = false;
        }

        public void Clear()
        {
            var atkUnitBase = AtkUnitBase;
            if (atkUnitBase != null)
            {
                this.SetColors(InventoryType.SaddleBag0, EmptyDictionary);
                this.SetColors(InventoryType.SaddleBag1, EmptyDictionary);
                this.SetTabColors(EmptyTabs);
            }
        }
    }
}
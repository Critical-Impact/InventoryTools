using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services.Ui;
using InventoryTools.Logic;

namespace InventoryTools.GameUi
{
    public class InventoryLargeOverlay : AtkInventoryLarge, IAtkOverlayState
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
                    this.SetColors(InventoryType.Bag0, Bag1InventoryColours);
                    this.SetColors(InventoryType.Bag1, Bag2InventoryColours);
                }
                else if (CurrentTab == 1)
                {
                    this.SetColors(InventoryType.Bag2, Bag3InventoryColours);
                    this.SetColors(InventoryType.Bag3, Bag4InventoryColours);
                }
                else
                {
                    this.SetColors(InventoryType.Bag0, EmptyDictionary);
                    this.SetColors(InventoryType.Bag1, EmptyDictionary);
                }

                return true;
            }

            return false;
        }
        
        public Dictionary<Vector2, Vector4?> Bag1InventoryColours = new();
        public Dictionary<Vector2, Vector4?> Bag2InventoryColours = new();
        public Dictionary<Vector2, Vector4?> Bag3InventoryColours = new();
        public Dictionary<Vector2, Vector4?> Bag4InventoryColours = new();
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
            if (newState != null && HasAddon && newState.Value.ShouldHighlight && newState.Value.HasFilterResult)
            {
                var filterResult = newState.Value.FilterResult;
                if (filterResult.HasValue)
                {
                    HasState = true;
                    Bag1InventoryColours = newState.Value.GetBagHighlights(InventoryType.Bag0);
                    Bag2InventoryColours = newState.Value.GetBagHighlights(InventoryType.Bag1);
                    Bag3InventoryColours = newState.Value.GetBagHighlights(InventoryType.Bag2);
                    Bag4InventoryColours = newState.Value.GetBagHighlights(InventoryType.Bag3);
                    var tab1 = newState.Value.GetTabHighlights(new List<Dictionary<Vector2, Vector4?>>() {Bag1InventoryColours, Bag2InventoryColours});
                    var tab2 = newState.Value.GetTabHighlights(new List<Dictionary<Vector2, Vector4?>>() {Bag3InventoryColours, Bag4InventoryColours});
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
                Bag3InventoryColours = EmptyDictionary;
                Bag4InventoryColours = EmptyDictionary;
                Clear();
            }

            HasState = false;
        }

        public void Clear()
        {
            var atkUnitBase = AtkUnitBase;
            if (atkUnitBase != null)
            {
                this.SetColors(InventoryType.Bag0, EmptyDictionary);
                this.SetColors(InventoryType.Bag1, EmptyDictionary);
                this.SetTabColors(EmptyTabs);
            }
        }
    }
}
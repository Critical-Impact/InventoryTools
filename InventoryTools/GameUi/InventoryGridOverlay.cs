using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services.Ui;
using InventoryTools.Logic;

namespace InventoryTools.GameUi
{
    public class InventoryGridOverlay : AtkInventoryGrid, IAtkOverlayState
    {
        public override bool Draw()
        {
            var atkUnitBase = AtkUnitBase;
            if (atkUnitBase != null && HasState)
            {
                this.SetTabColors(TabColours);
                if (CurrentTab == 0)
                {
                    this.SetColors(Bag1InventoryColours);
                }
                else if (CurrentTab == 1)
                {
                    this.SetColors(Bag2InventoryColours);
                }
                else if (CurrentTab == 2)
                {
                    this.SetColors( Bag3InventoryColours);
                }
                else if (CurrentTab == 3)
                {
                    this.SetColors(Bag4InventoryColours);
                }
                else
                {
                    this.SetColors(EmptyDictionary);
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
            if (PluginService.CharacterMonitor.ActiveCharacter == 0)
            {
                return;
            }
            if (AtkUnitBase != null && newState != null)
            {
                HasState = true;
                var filterResult = newState.Value.FilterResult;
                if (newState.Value.ShouldHighlight && filterResult.HasValue)
                {
                    Bag1InventoryColours = newState.Value.GetBagHighlights(InventoryType.Bag0);
                    Bag2InventoryColours = newState.Value.GetBagHighlights(InventoryType.Bag1);
                    Bag3InventoryColours = newState.Value.GetBagHighlights(InventoryType.Bag2);
                    Bag4InventoryColours = newState.Value.GetBagHighlights(InventoryType.Bag3);
                    var tab1 = newState.Value.GetTabHighlight(Bag1InventoryColours);
                    var tab2 = newState.Value.GetTabHighlight(Bag2InventoryColours);
                    var tab3 = newState.Value.GetTabHighlight(Bag3InventoryColours);
                    var tab4 = newState.Value.GetTabHighlight(Bag4InventoryColours);
                    TabColours[0] = tab1;
                    TabColours[1] = tab2;
                    TabColours[2] = tab3;
                    TabColours[3] = tab4;
                    Draw();
                    return;
                }
            }
            HasState = false;

            Bag1InventoryColours = EmptyDictionary;
            Bag2InventoryColours = EmptyDictionary;
            Bag3InventoryColours = EmptyDictionary;
            Bag4InventoryColours = EmptyDictionary;
            Clear();
        }

        public void Clear()
        {
            var atkUnitBase = AtkUnitBase;
            if (atkUnitBase != null)
            {
                this.SetColors(EmptyDictionary);
                this.SetTabColors(EmptyTabs);
            }
        }
    }
}
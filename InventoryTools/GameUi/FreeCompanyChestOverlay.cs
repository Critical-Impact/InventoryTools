using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Addons;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services.Ui;
using InventoryTools.Logic;

namespace InventoryTools.GameUi
{
    public class FreeCompanyChestOverlay : AtkFreeCompanyChest, IAtkOverlayState
    {
        public override bool Draw()
        {
            var atkUnitBase = AtkUnitBase;
            if (atkUnitBase != null && HasState)
            {
                this.SetTabColors(TabColours);
                if (CurrentTab == FreeCompanyTab.One)
                {
                    this.SetColors(Bag1InventoryColours);
                }
                else if (CurrentTab == FreeCompanyTab.Two)
                {
                    this.SetColors(Bag2InventoryColours);
                }
                else if (CurrentTab == FreeCompanyTab.Three)
                {
                    this.SetColors( Bag3InventoryColours);
                }
                else if (CurrentTab == FreeCompanyTab.Four)
                {
                    this.SetColors(Bag4InventoryColours);
                }
                else if (CurrentTab == FreeCompanyTab.Five)
                {
                    this.SetColors(Bag5InventoryColours);
                }
                else
                {
                    this.Clear();
                }

                return true;
            }

            return false;
        }
        
        public Dictionary<Vector2, Vector4?> Bag1InventoryColours = new();
        public Dictionary<Vector2, Vector4?> Bag2InventoryColours = new();
        public Dictionary<Vector2, Vector4?> Bag3InventoryColours = new();
        public Dictionary<Vector2, Vector4?> Bag4InventoryColours = new();
        public Dictionary<Vector2, Vector4?> Bag5InventoryColours = new();
        public Dictionary<FreeCompanyTab, Vector4?> TabColours = new();
        public Dictionary<Vector2, Vector4?> EmptyDictionary = new();
        public Dictionary<FreeCompanyTab, Vector4?> EmptyTabs = new() { {FreeCompanyTab.One, null}, {FreeCompanyTab.Two, null}, {FreeCompanyTab.Three, null}, {FreeCompanyTab.Four, null}, {FreeCompanyTab.Five, null} };

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
                    Bag1InventoryColours = newState.Value.GetBagHighlights(InventoryType.FreeCompanyBag0);
                    Bag2InventoryColours = newState.Value.GetBagHighlights(InventoryType.FreeCompanyBag1);
                    Bag3InventoryColours = newState.Value.GetBagHighlights(InventoryType.FreeCompanyBag2);
                    Bag4InventoryColours = newState.Value.GetBagHighlights(InventoryType.FreeCompanyBag3);
                    Bag5InventoryColours = newState.Value.GetBagHighlights(InventoryType.FreeCompanyBag4);
                    var tab1 = newState.Value.GetTabHighlight(Bag1InventoryColours);
                    var tab2 = newState.Value.GetTabHighlight(Bag2InventoryColours);
                    var tab3 = newState.Value.GetTabHighlight(Bag3InventoryColours);
                    var tab4 = newState.Value.GetTabHighlight(Bag4InventoryColours);
                    var tab5 = newState.Value.GetTabHighlight(Bag5InventoryColours);
                    TabColours[FreeCompanyTab.One] = tab1;
                    TabColours[FreeCompanyTab.Two] = tab2;
                    TabColours[FreeCompanyTab.Three] = tab3;
                    TabColours[FreeCompanyTab.Four] = tab4;
                    TabColours[FreeCompanyTab.Five] = tab5;
                    Draw();
                    return;
                }
            }
            HasState = false;

            Bag1InventoryColours = EmptyDictionary;
            Bag2InventoryColours = EmptyDictionary;
            Bag3InventoryColours = EmptyDictionary;
            Bag4InventoryColours = EmptyDictionary;
            Bag5InventoryColours = EmptyDictionary;
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
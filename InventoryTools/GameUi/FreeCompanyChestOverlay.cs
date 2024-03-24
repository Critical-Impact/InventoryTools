using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Addons;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using InventoryTools.Logic;
using Microsoft.Extensions.Logging;

namespace InventoryTools.GameUi
{
    public class FreeCompanyChestOverlay: GameOverlay<AtkFreeCompanyChest>, IAtkOverlayState
    {
        private readonly ICharacterMonitor _characterMonitor;

        public FreeCompanyChestOverlay(ILogger<FreeCompanyChestOverlay> logger, AtkFreeCompanyChest overlay, ICharacterMonitor characterMonitor) : base(logger,overlay)
        {
            _characterMonitor = characterMonitor;
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
                if (AtkOverlay.CurrentTab == FreeCompanyTab.One)
                {
                    AtkOverlay.SetColors(Bag1InventoryColours);
                }
                else if (AtkOverlay.CurrentTab == FreeCompanyTab.Two)
                {
                    AtkOverlay.SetColors(Bag2InventoryColours);
                }
                else if (AtkOverlay.CurrentTab == FreeCompanyTab.Three)
                {
                    AtkOverlay.SetColors( Bag3InventoryColours);
                }
                else if (AtkOverlay.CurrentTab == FreeCompanyTab.Four)
                {
                    AtkOverlay.SetColors(Bag4InventoryColours);
                }
                else if (AtkOverlay.CurrentTab == FreeCompanyTab.Five)
                {
                    AtkOverlay.SetColors(Bag5InventoryColours);
                }
                else
                {
                    Clear();
                }
                AtkOverlay.SetTabColors(TabColours);

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
                for (int y = 0; y < 10; y++)
                {
                    EmptyDictionary.Add(new Vector2(x,y), null);
                }
            }

        }

        public override bool HasState { get; set; }
        public override bool NeedsStateRefresh { get; set; }

        public override void UpdateState(FilterState? newState)
        {
            if (_characterMonitor.ActiveCharacterId == 0)
            {
                return;
            }
            if (newState != null && AtkOverlay.HasAddon && newState.ShouldHighlight && newState.HasFilterResult)
            {
                HasState = true;
                var filterResult = newState.FilterResult;
                if (filterResult != null)
                {
                    Bag1InventoryColours = newState.GetBagHighlights(InventoryType.FreeCompanyBag0);
                    Bag2InventoryColours = newState.GetBagHighlights(InventoryType.FreeCompanyBag1);
                    Bag3InventoryColours = newState.GetBagHighlights(InventoryType.FreeCompanyBag2);
                    Bag4InventoryColours = newState.GetBagHighlights(InventoryType.FreeCompanyBag3);
                    Bag5InventoryColours = newState.GetBagHighlights(InventoryType.FreeCompanyBag4);
                    var tab1 = newState.GetTabHighlight(Bag1InventoryColours);
                    var tab2 = newState.GetTabHighlight(Bag2InventoryColours);
                    var tab3 = newState.GetTabHighlight(Bag3InventoryColours);
                    var tab4 = newState.GetTabHighlight(Bag4InventoryColours);
                    var tab5 = newState.GetTabHighlight(Bag5InventoryColours);
                    TabColours[FreeCompanyTab.One] = tab1;
                    TabColours[FreeCompanyTab.Two] = tab2;
                    TabColours[FreeCompanyTab.Three] = tab3;
                    TabColours[FreeCompanyTab.Four] = tab4;
                    TabColours[FreeCompanyTab.Five] = tab5;
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
                Bag5InventoryColours = EmptyDictionary;
                Clear();
            }

            HasState = false;
        }

        public override void Clear()
        {
            var atkUnitBase = AtkOverlay.AtkUnitBase;
            if (atkUnitBase != null)
            {
                this.AtkOverlay.SetColors(EmptyDictionary);
                this.AtkOverlay.SetTabColors(EmptyTabs);
            }
        }
    }
}
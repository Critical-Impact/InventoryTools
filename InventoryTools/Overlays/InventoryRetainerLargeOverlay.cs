using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using InventoryTools.Logic;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Overlays
{
    public class InventoryRetainerLargeOverlay: GameOverlay<AtkRetainerLarge>, IAtkOverlayState
    {
        private readonly ICharacterMonitor _characterMonitor;

        public InventoryRetainerLargeOverlay(ILogger<InventoryRetainerLargeOverlay> logger, AtkRetainerLarge overlay, ICharacterMonitor characterMonitor) : base(logger,overlay)
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
                this.AtkOverlay.SetTabColors(TabColours);
                if (AtkOverlay.CurrentTab == 0)
                {
                    this.AtkOverlay.SetColors(InventoryType.RetainerBag0, Bag1InventoryColours);
                    this.AtkOverlay.SetColors(InventoryType.RetainerBag1, Bag2InventoryColours);
                }
                else if (AtkOverlay.CurrentTab == 1)
                {
                    this.AtkOverlay.SetColors(InventoryType.RetainerBag2, Bag3InventoryColours);
                    this.AtkOverlay.SetColors(InventoryType.RetainerBag3, Bag4InventoryColours);
                }
                else
                {
                    this.AtkOverlay.SetColors(InventoryType.RetainerBag4, Bag5InventoryColours);
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
                    Bag1InventoryColours = newState.GetBagHighlights(InventoryType.RetainerBag0);
                    Bag2InventoryColours = newState.GetBagHighlights(InventoryType.RetainerBag1);
                    Bag3InventoryColours = newState.GetBagHighlights(InventoryType.RetainerBag2);
                    Bag4InventoryColours = newState.GetBagHighlights(InventoryType.RetainerBag3);
                    Bag5InventoryColours = newState.GetBagHighlights(InventoryType.RetainerBag4);
                    var tab1 = newState.GetTabHighlights(new List<Dictionary<Vector2, Vector4?>>() {Bag1InventoryColours, Bag2InventoryColours});
                    var tab2 = newState.GetTabHighlights(new List<Dictionary<Vector2, Vector4?>>() {Bag3InventoryColours, Bag4InventoryColours});
                    var tab3 = newState.GetTabHighlight(Bag5InventoryColours);
                    TabColours[0] = tab1;
                    TabColours[1] = tab2;
                    TabColours[2] = tab3;
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
                this.AtkOverlay.SetColors(InventoryType.RetainerBag0, EmptyDictionary);
                this.AtkOverlay.SetColors(InventoryType.RetainerBag1, EmptyDictionary);
                this.AtkOverlay.SetColors(InventoryType.RetainerBag2, EmptyDictionary);
                this.AtkOverlay.SetColors(InventoryType.RetainerBag3, EmptyDictionary);
                this.AtkOverlay.SetColors(InventoryType.RetainerBag4, EmptyDictionary);
                this.AtkOverlay.SetTabColors(EmptyTabs);
            }
        }
    }
}
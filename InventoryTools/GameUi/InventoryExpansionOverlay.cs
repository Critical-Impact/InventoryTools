using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using InventoryTools.Logic;
using Microsoft.Extensions.Logging;

namespace InventoryTools.GameUi
{
    public class InventoryExpansionOverlay: GameOverlay<AtkInventoryExpansion>, IAtkOverlayState
    {
        private readonly ICharacterMonitor _characterMonitor;

        public InventoryExpansionOverlay(ILogger<InventoryExpansionOverlay> logger, AtkInventoryExpansion overlay, ICharacterMonitor characterMonitor) : base(logger,overlay)
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
                AtkOverlay.SetColors(InventoryType.Bag0, Bag1InventoryColours);
                AtkOverlay.SetColors(InventoryType.Bag1, Bag2InventoryColours);
                AtkOverlay.SetColors(InventoryType.Bag2, Bag3InventoryColours);
                AtkOverlay.SetColors(InventoryType.Bag3, Bag4InventoryColours);
                return true;
            }

            return false;
        }
        
        public Dictionary<Vector2, Vector4?> Bag1InventoryColours = new();
        public Dictionary<Vector2, Vector4?> Bag2InventoryColours = new();
        public Dictionary<Vector2, Vector4?> Bag3InventoryColours = new();
        public Dictionary<Vector2, Vector4?> Bag4InventoryColours = new();
        public Dictionary<Vector2, Vector4?> EmptyDictionary = new();

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
                    Bag1InventoryColours = newState.GetBagHighlights(InventoryType.Bag0);
                    Bag2InventoryColours = newState.GetBagHighlights(InventoryType.Bag1);
                    Bag3InventoryColours = newState.GetBagHighlights(InventoryType.Bag2);
                    Bag4InventoryColours = newState.GetBagHighlights(InventoryType.Bag3);
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

        public override void Clear()
        {
            var atkUnitBase = AtkOverlay.AtkUnitBase;
            if (atkUnitBase != null)
            {
                AtkOverlay.SetColors(InventoryType.Bag0, EmptyDictionary);
                AtkOverlay.SetColors(InventoryType.Bag1, EmptyDictionary);
                AtkOverlay.SetColors(InventoryType.Bag2, EmptyDictionary);
                AtkOverlay.SetColors(InventoryType.Bag3, EmptyDictionary);
            }
        }
    }
}
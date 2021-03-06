using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services.Ui;
using InventoryTools.Logic;

namespace InventoryTools.GameUi
{
    public class InventoryExpansionOverlay : AtkInventoryExpansion, IAtkOverlayState
    {
        public override bool Draw()
        {
            var atkUnitBase = AtkUnitBase;
            if (atkUnitBase != null && HasState)
            {
                this.SetColors(InventoryType.Bag0, Bag1InventoryColours);
                this.SetColors(InventoryType.Bag1, Bag2InventoryColours);
                this.SetColors(InventoryType.Bag2, Bag3InventoryColours);
                this.SetColors(InventoryType.Bag3, Bag4InventoryColours);
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
                    Draw();
                    return;
                }
            }

            Bag1InventoryColours = EmptyDictionary;
            Bag2InventoryColours = EmptyDictionary;
            Bag3InventoryColours = EmptyDictionary;
            Bag4InventoryColours = EmptyDictionary;
            if (HasState)
            {
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
                this.SetColors(InventoryType.Bag2, EmptyDictionary);
                this.SetColors(InventoryType.Bag3, EmptyDictionary);
            }
        }
    }
}
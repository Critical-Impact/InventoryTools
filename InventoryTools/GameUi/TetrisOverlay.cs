using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services.Ui;
using InventoryTools.Logic;
using InventoryTools.Misc;

namespace InventoryTools.GameUi
{
    public class TetrisOverlay : AtkInventoryExpansion, IAtkOverlayState
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
                var colours = TetrisGame.Instance.DrawScene();
                foreach (var colour in colours)
                {
                    SetColors(colour.Key, colour.Value);
                }

                this.HideIcons(InventoryType.Bag0);
                this.HideIcons(InventoryType.Bag1);
                this.HideIcons(InventoryType.Bag2);
                this.HideIcons(InventoryType.Bag3);
                this.SetText("Current Score: " + TetrisGame.Instance.Game.Score, "Lines: " + TetrisGame.Instance.Game.Lines);
                return true;
            }

            return false;
        }
        
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
            if (AtkUnitBase == null)
            {
                return;
            }
            HasState = true;
            Clear();
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
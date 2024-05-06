using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using InventoryTools.Logic;
using InventoryTools.Misc;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Overlays
{
    public class TetrisOverlay: GameOverlay<AtkInventoryExpansion>, IAtkOverlayState
    {
        private readonly ICharacterMonitor _characterMonitor;
        private readonly TetrisGame _tetrisGame;

        public TetrisOverlay(ILogger<TetrisOverlay> logger, AtkInventoryExpansion overlay, ICharacterMonitor characterMonitor, TetrisGame tetrisGame) : base(logger,overlay)
        {
            _characterMonitor = characterMonitor;
            _tetrisGame = tetrisGame;
            Enabled = false;
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
                var colours = _tetrisGame.DrawScene();
                foreach (var colour in colours)
                {
                    AtkOverlay.SetColors(colour.Key, colour.Value);
                }

                this.AtkOverlay.HideIcons(InventoryType.Bag0);
                this.AtkOverlay.HideIcons(InventoryType.Bag1);
                this.AtkOverlay.HideIcons(InventoryType.Bag2);
                this.AtkOverlay.HideIcons(InventoryType.Bag3);
                this.AtkOverlay.SetText("Current Score: " + _tetrisGame.Game.Score, "Lines: " + _tetrisGame.Game.Lines);
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

        public override bool HasState { get; set; }
        public override bool NeedsStateRefresh { get; set; }

        public override void UpdateState(FilterState? newState)
        {
            if (_characterMonitor.ActiveCharacterId == 0)
            {
                return;
            }
            if (AtkOverlay.AtkUnitBase == null)
            {
                return;
            }
            HasState = true;
            Draw();
        }

        public override void Clear()
        {
            var atkUnitBase = AtkOverlay.AtkUnitBase;
            if (atkUnitBase != null)
            {
                this.AtkOverlay.SetColors(InventoryType.Bag0, EmptyDictionary);
                this.AtkOverlay.SetColors(InventoryType.Bag1, EmptyDictionary);
                this.AtkOverlay.SetColors(InventoryType.Bag2, EmptyDictionary);
                this.AtkOverlay.SetColors(InventoryType.Bag3, EmptyDictionary);
            }
        }
    }
}
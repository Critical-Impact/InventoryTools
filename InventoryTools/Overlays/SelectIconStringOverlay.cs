using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using InventoryTools.Logic;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Overlays
{
    public class SelectIconStringOverlay: GameOverlay<AtkSelectIconString>, IAtkOverlayState
    {
        private readonly ICharacterMonitor _characterMonitor;

        public SelectIconStringOverlay(ILogger<SelectIconStringOverlay> logger, AtkSelectIconString overlay, ICharacterMonitor characterMonitor) : base(logger,overlay)
        {
            _characterMonitor = characterMonitor;
        }
        public override bool ShouldDraw { get; set; }

        public override bool Draw()
        {
            if (!HasState || !AtkOverlay.HasAddon)
            {
                Logger.LogTrace("no state and no addon");
                return false;
            }
            var atkUnitBase = AtkOverlay.AtkUnitBase;
            if (atkUnitBase != null)
            {
                Logger.LogTrace("has atk base, setting colors");
                this.AtkOverlay.SetColors(SelectItems);
                return true;
            }

            return false;
        }
        
        public List<Vector4?> SelectItems = new();

        public override void Setup()
        {

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
                    Logger.LogTrace("Attempting to update state for SelectIconString");

                    SelectItems = newState.GetSelectIconStringItems();
                    Draw();
                    return;
                }
            }
            
            if (HasState)
            {
                Logger.LogTrace("Clearing select items");
                SelectItems = new List<Vector4?>();
                Clear();
            }

            HasState = false;
        }

        public override void Clear()
        {
            var atkUnitBase = AtkOverlay.AtkUnitBase;
            if (atkUnitBase != null)
            {
                this.AtkOverlay.ResetColors();
            }
        }
    }
}
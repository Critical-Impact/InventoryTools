using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using InventoryTools.Logic;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Overlays
{
    public class CabinetWithdrawOverlay: GameOverlay<AtkCabinetWithdraw>, IAtkOverlayState
    {
        private readonly ICharacterMonitor _characterMonitor;

        public CabinetWithdrawOverlay(ILogger<CabinetWithdrawOverlay> logger, AtkCabinetWithdraw overlay, ICharacterMonitor characterMonitor) : base(logger,overlay)
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
                this.AtkOverlay.SetColours(Colours);
                this.AtkOverlay.SetTabColors(TabColours);
                return true;
            }

            return false;
        }

        public Dictionary<uint, Vector4?> TabColours = new();
        public Dictionary<string, Vector4?> Colours = new();
        public Dictionary<uint, Vector4?> EmptyTabs = new() { {0, null}, {1, null}, {2, null}, {3, null}, {4, null} , {5, null} , {6, null}  , {7, null}  , {8, null} };

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
                    Colours = newState.GetArmoireHighlights();
                    TabColours = newState.GetArmoireTabHighlights();
                    Draw();
                    return;
                }
            }
            if (HasState)
            {
                Clear();
            }

            HasState = false;
        }

        public override void Clear()
        {
            var atkUnitBase = AtkOverlay.AtkUnitBase;
            if (atkUnitBase != null)
            {
                this.AtkOverlay.SetColours(new Dictionary<string, Vector4?>());
                this.AtkOverlay.SetTabColors(EmptyTabs);
            }
        }
    }
}
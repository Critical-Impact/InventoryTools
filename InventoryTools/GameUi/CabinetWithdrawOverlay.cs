using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Services.Ui;
using InventoryTools.Logic;

namespace InventoryTools.GameUi
{
    public class CabinetWithdrawOverlay : AtkCabinetWithdraw, IAtkOverlayState
    {
        public override bool Draw()
        {
            var atkUnitBase = AtkUnitBase;
            if (atkUnitBase != null && HasState)
            {
                this.SetColours(Colours);
                this.SetTabColors(TabColours);
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
                    Colours = newState.Value.GetArmoireHighlights();
                    TabColours = newState.Value.GetArmoireTabHighlights(CurrentTab);
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

        public void Clear()
        {
            var atkUnitBase = AtkUnitBase;
            if (atkUnitBase != null)
            {
                this.SetColours(new Dictionary<string, Vector4?>());
                this.SetTabColors(EmptyTabs);
            }
        }
    }
}
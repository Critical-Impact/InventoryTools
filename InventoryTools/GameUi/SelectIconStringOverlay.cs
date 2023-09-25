using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Services.Ui;
using Dalamud.Logging;
using InventoryTools.Logic;

namespace InventoryTools.GameUi
{
    public class SelectIconStringOverlay : AtkSelectIconString, IAtkOverlayState
    {
        public override bool ShouldDraw { get; set; }

        public override bool Draw()
        {
            if (!HasState || !HasAddon)
            {
                Service.Log.Verbose("no state and no addon");
                return false;
            }
            var atkUnitBase = AtkUnitBase;
            if (atkUnitBase != null)
            {
                Service.Log.Verbose("has atk base, setting colors");
                this.SetColors(SelectItems);
                return true;
            }

            return false;
        }
        
        public List<Vector4?> SelectItems = new();

        public override void Setup()
        {

        }

        public bool HasState { get; set; }
        public bool NeedsStateRefresh { get; set; }

        public void UpdateState(FilterState? newState)
        {
            if (PluginService.CharacterMonitor.ActiveCharacterId == 0)
            {
                return;
            }
            if (newState != null && HasAddon && newState.ShouldHighlight && newState.HasFilterResult)
            {
                HasState = true;
                var filterResult = newState.FilterResult;
                if (filterResult != null)
                {
                    Service.Log.Verbose("Attempting to update state for SelectIconString");

                    SelectItems = newState.GetSelectIconStringItems();
                    Draw();
                    return;
                }
            }
            
            if (HasState)
            {
                Service.Log.Verbose("Clearing select items");
                SelectItems = new List<Vector4?>();
                Clear();
            }

            HasState = false;
        }

        public void Clear()
        {
            var atkUnitBase = AtkUnitBase;
            if (atkUnitBase != null)
            {
                this.ResetColors();
            }
        }
    }
}
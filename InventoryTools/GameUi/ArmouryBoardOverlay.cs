using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services.Ui;
using Dalamud.Logging;
using InventoryTools.Logic;

namespace InventoryTools.GameUi
{
    public class ArmouryBoardOverlay: AtkArmouryBoard, IAtkOverlayState
    {
        public override bool ShouldDraw { get; set; }
        public override bool Draw()
        {
            var atkUnitBase = AtkUnitBase;
            if (atkUnitBase != null && HasState)
            {
                var currentBagLocation = CurrentBagLocation;
                if (currentBagLocation != null && BagColours.ContainsKey(currentBagLocation.Value))
                {
                    PluginLog.Log("Rendering highlights for " + currentBagLocation.ToString());
                    this.SetColors(currentBagLocation.Value, BagColours[currentBagLocation.Value]);
                }

                return true;
            }

            return false;
        }
        
        public Dictionary<InventoryType, Dictionary<Vector2,Vector4?>> BagColours = new();
        public Dictionary<Vector2, Vector4?> EmptyDictionary = new();


        public override void Setup()
        {
            for (int x = 0; x < 49; x++)
            {
                EmptyDictionary.Add(new Vector2(x,0), null);
            }
        }

        public bool HasState { get; set; }
        
        public void UpdateState(FilterState? newState)
        {
            if (PluginService.CharacterMonitor.ActiveCharacter == 0)
            {
                return;
            }
            if (newState != null)
            {
                HasState = true;
                var filterResult = newState.Value.FilterResult;
                if (newState.Value.ShouldHighlight && filterResult.HasValue)
                {
                    foreach (var bag in BagToNumber.Keys)
                    {
                        BagColours[bag] = newState.Value.GetBagHighlights(bag);
                    }

                    Draw();
                    return;
                }
            }
            HasState = false;
            foreach (var bag in BagToNumber.Keys)
            {
                BagColours[bag] = EmptyDictionary;
            }

            Clear();
        }

        public void Clear()
        {
            var atkUnitBase = AtkUnitBase;
            if (atkUnitBase != null)
            {
                var currentBagLocation = CurrentBagLocation;
                if (currentBagLocation != null && BagColours.ContainsKey(currentBagLocation.Value))
                {
                    this.SetColors(currentBagLocation.Value, EmptyDictionary);
                }
            }
        }
    }
}
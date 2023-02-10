using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services.Ui;
using InventoryTools.Logic;

namespace InventoryTools.GameUi
{
    public class ArmouryBoardOverlay: AtkArmouryBoard, IAtkOverlayState
    {
        public override bool ShouldDraw { get; set; }
        public override bool Draw()
        {
            if (!HasState || !HasAddon)
            {
                return false;
            }
            var atkUnitBase = AtkUnitBase;
            if (atkUnitBase != null)
            {
                this.SetTabColors(TabColours);
                var currentBagLocation = CurrentBagLocation;
                if (currentBagLocation != null && BagColours.ContainsKey(currentBagLocation.Value))
                {
                    this.SetColors(currentBagLocation.Value, BagColours[currentBagLocation.Value]);
                }

                return true;
            }

            return false;
        }
        
        private int? _storedTab = null;
        
        public override void Update()
        {
            var currentTab = CurrentTab;
            if (currentTab != -1 && currentTab != _storedTab)
            {
                _storedTab = currentTab;
                Draw();
            }
        }

        
        public Dictionary<InventoryType, Dictionary<Vector2,Vector4?>> BagColours = new();
        public Dictionary<InventoryType, Vector4?> TabColours = new();
        public Dictionary<Vector2, Vector4?> EmptyDictionary = new();
        public Dictionary<InventoryType, Vector4?> EmptyTabs = new();


        public override void Setup()
        {
            for (int x = 0; x < 49; x++)
            {
                EmptyDictionary.Add(new Vector2(x,0), null);
            }
            foreach(var type in this.BagToNumber)
            {
                EmptyTabs.Add(type.Key, null);
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

            if (newState != null && HasAddon && newState.Value.ShouldHighlight && newState.Value.HasFilterResult)
            {
                HasState = true;
                var filterResult = newState.Value.FilterResult;
                if (filterResult.HasValue)
                {
                    foreach (var bag in BagToNumber.Keys)
                    {
                        BagColours[bag] = newState.Value.GetBagHighlights(bag);
                        TabColours[bag] = newState.Value.GetTabHighlight(BagColours[bag]);
                    }

                    Draw();
                    return;
                }
            }

            if (HasState)
            {
                
                foreach (var bag in BagToNumber.Keys)
                {
                    BagColours[bag] = EmptyDictionary;
                }
                Clear();
            }

            HasState = false;

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
                    this.SetTabColors( EmptyTabs);
                }
            }
        }
    }
}
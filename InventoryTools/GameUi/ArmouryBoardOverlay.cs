using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Enums;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using InventoryTools.Logic;
using Microsoft.Extensions.Logging;

namespace InventoryTools.GameUi
{
    public class ArmouryBoardOverlay: GameOverlay<AtkArmouryBoard>
    {
        private readonly ICharacterMonitor _characterMonitor;

        public ArmouryBoardOverlay(ILogger<ArmouryBoardOverlay> logger, AtkArmouryBoard overlay, ICharacterMonitor characterMonitor) : base(logger,overlay)
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
                this.AtkOverlay.SetTabColors(TabColours);
                var currentBagLocation = AtkOverlay.CurrentBagLocation;
                if (currentBagLocation != null && BagColours.ContainsKey(currentBagLocation.Value))
                {
                    this.AtkOverlay.SetColors(currentBagLocation.Value, BagColours[currentBagLocation.Value]);
                }

                return true;
            }

            return false;
        }
        
        private int? _storedTab = null;
        
        public override void Update()
        {
            var currentTab = AtkOverlay.CurrentTab;
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
            foreach(var type in this.AtkOverlay.BagToNumber)
            {
                EmptyTabs.Add(type.Key, null);
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
                    foreach (var bag in AtkOverlay.BagToNumber.Keys)
                    {
                        BagColours[bag] = newState.GetBagHighlights(bag);
                        TabColours[bag] = newState.GetTabHighlight(BagColours[bag]);
                    }

                    Draw();
                    return;
                }
            }

            if (HasState)
            {
                
                foreach (var bag in AtkOverlay.BagToNumber.Keys)
                {
                    BagColours[bag] = EmptyDictionary;
                }
                Clear();
            }

            HasState = false;

        }

        public override void Clear()
        {
            var atkUnitBase = AtkOverlay.AtkUnitBase;
            if (atkUnitBase != null)
            {
                var currentBagLocation = AtkOverlay.CurrentBagLocation;
                if (currentBagLocation != null && BagColours.ContainsKey(currentBagLocation.Value))
                {
                    this.AtkOverlay.SetColors(currentBagLocation.Value, EmptyDictionary);
                    this.AtkOverlay.SetTabColors( EmptyTabs);
                }
            }
        }
    }
}
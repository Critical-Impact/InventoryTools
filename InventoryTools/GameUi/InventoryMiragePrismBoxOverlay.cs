using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Services.Ui;
using Dalamud.Logging;
using InventoryTools.Logic;

namespace InventoryTools.GameUi
{
    public class InventoryMiragePrismBoxOverlay : AtkInventoryMiragePrismBox, IAtkOverlayState
    {
        public bool HasState { get; set; }
        public bool NeedsStateRefresh { get; set; }

        public Dictionary<DresserTab, Dictionary<Vector2, Vector4?>> ChestInventoryColours = new();
        public Dictionary<uint, Vector4?> TabColours = new();
        public Dictionary<uint, Vector4?> EmptyTabs = new() { {0, null}, {1, null}, {2, null}, {3, null}, {4, null}, {5, null}, {6, null}, {7, null}, {8, null}, {9, null}, {10, null} };
        public Dictionary<Vector2, Vector4?> EmptyDictionary = new();

        public int? _storedTab = null;
        public int? _currentPage = null;
        public uint? _classJobSelected = null;
        public bool? _onlyDisplay = null;
        
        public override void Update()
        {
            var currentTab = CurrentTab;
            var currentPage = CurrentPage;
            var classJobSelected = ClassJobSelected;
            var onlyDisplay = OnlyDisplayRaceGenderItems;
            if (currentTab != -1 && currentTab != _storedTab)
            {
                _storedTab = currentTab;
                NeedsStateRefresh = true;
            }
            if (currentPage != -1 && currentPage != _currentPage)
            {
                _currentPage = currentPage;
                NeedsStateRefresh = true;
            }
            if (classJobSelected != 0 && classJobSelected != _classJobSelected)
            {
                _classJobSelected = classJobSelected;
                NeedsStateRefresh = true;
            }
            if (onlyDisplay != _onlyDisplay)
            {
                _onlyDisplay = onlyDisplay;
                NeedsStateRefresh = true;
            }
        }

        public override bool Draw()
        {
            var atkUnitBase = AtkUnitBase;
            if (atkUnitBase != null && HasState)
            {
                DresserTab? dresserTab = null;
                switch (CurrentTab)
                {
                    case 0:
                        dresserTab = DresserTab.MainHand;
                        break;
                    case 1:
                        dresserTab = DresserTab.OffHand;
                        break;
                    case 2:
                        dresserTab = DresserTab.Head;
                        break;
                    case 3:
                        dresserTab = DresserTab.Body;
                        break;
                    case 4:
                        dresserTab = DresserTab.Hands;
                        break;
                    case 5:
                        dresserTab = DresserTab.Legs;
                        break;
                    case 6:
                        dresserTab = DresserTab.Feet;
                        break;
                    case 7:
                        dresserTab = DresserTab.Ears;
                        break;
                    case 8:
                        dresserTab = DresserTab.Neck;
                        break;
                    case 9:
                        dresserTab = DresserTab.Wrists;
                        break;
                    case 10:
                        dresserTab = DresserTab.Fingers;
                        break;
                }

                if (dresserTab != null && ChestInventoryColours.ContainsKey(dresserTab.Value))
                {
                    this.SetColors(ChestInventoryColours[dresserTab.Value]);
                }
                this.SetTabColors(TabColours);

                return true;
            }

            return false;
        }

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
                    ChestInventoryColours[DresserTab.Body] = newState.Value.GetGlamourHighlights(DresserTab.Body, CurrentPage, OnlyDisplayRaceGenderItems, ClassJobSelected);
                    ChestInventoryColours[DresserTab.Ears] = newState.Value.GetGlamourHighlights(DresserTab.Ears, CurrentPage, OnlyDisplayRaceGenderItems, ClassJobSelected);
                    ChestInventoryColours[DresserTab.Feet] = newState.Value.GetGlamourHighlights(DresserTab.Feet, CurrentPage, OnlyDisplayRaceGenderItems, ClassJobSelected);
                    ChestInventoryColours[DresserTab.Fingers] = newState.Value.GetGlamourHighlights(DresserTab.Fingers, CurrentPage, OnlyDisplayRaceGenderItems, ClassJobSelected);
                    ChestInventoryColours[DresserTab.Hands] = newState.Value.GetGlamourHighlights(DresserTab.Hands, CurrentPage, OnlyDisplayRaceGenderItems, ClassJobSelected);
                    ChestInventoryColours[DresserTab.Head] = newState.Value.GetGlamourHighlights(DresserTab.Head, CurrentPage, OnlyDisplayRaceGenderItems, ClassJobSelected);
                    ChestInventoryColours[DresserTab.Legs] = newState.Value.GetGlamourHighlights(DresserTab.Legs, CurrentPage, OnlyDisplayRaceGenderItems, ClassJobSelected);
                    ChestInventoryColours[DresserTab.Neck] = newState.Value.GetGlamourHighlights(DresserTab.Neck, CurrentPage, OnlyDisplayRaceGenderItems, ClassJobSelected);
                    ChestInventoryColours[DresserTab.Wrists] = newState.Value.GetGlamourHighlights(DresserTab.Wrists, CurrentPage, OnlyDisplayRaceGenderItems, ClassJobSelected);
                    ChestInventoryColours[DresserTab.MainHand] = newState.Value.GetGlamourHighlights(DresserTab.MainHand, CurrentPage, OnlyDisplayRaceGenderItems, ClassJobSelected);
                    ChestInventoryColours[DresserTab.OffHand] = newState.Value.GetGlamourHighlights(DresserTab.OffHand, CurrentPage, OnlyDisplayRaceGenderItems, ClassJobSelected);
                    TabColours[0] = newState.Value.GetTabHighlight(ChestInventoryColours[DresserTab.MainHand]);
                    TabColours[1] =  newState.Value.GetTabHighlight(ChestInventoryColours[DresserTab.OffHand]);
                    TabColours[2] = newState.Value.GetTabHighlight(ChestInventoryColours[DresserTab.Head]);
                    TabColours[3] = newState.Value.GetTabHighlight(ChestInventoryColours[DresserTab.Body]);
                    TabColours[4] = newState.Value.GetTabHighlight(ChestInventoryColours[DresserTab.Hands]);
                    TabColours[5] = newState.Value.GetTabHighlight(ChestInventoryColours[DresserTab.Legs]);
                    TabColours[6] = newState.Value.GetTabHighlight(ChestInventoryColours[DresserTab.Feet]);
                    TabColours[7] = newState.Value.GetTabHighlight(ChestInventoryColours[DresserTab.Ears]);
                    TabColours[8] = newState.Value.GetTabHighlight(ChestInventoryColours[DresserTab.Neck]);
                    TabColours[9] = newState.Value.GetTabHighlight(ChestInventoryColours[DresserTab.Wrists]);
                    TabColours[10] = newState.Value.GetTabHighlight(ChestInventoryColours[DresserTab.Fingers]);
                    Draw();
                    return;
                }
            }
            ChestInventoryColours[DresserTab.Body] = EmptyDictionary;
            ChestInventoryColours[DresserTab.Ears] = EmptyDictionary;
            ChestInventoryColours[DresserTab.Feet] = EmptyDictionary;
            ChestInventoryColours[DresserTab.Fingers] = EmptyDictionary;
            ChestInventoryColours[DresserTab.Hands] = EmptyDictionary;
            ChestInventoryColours[DresserTab.Head] = EmptyDictionary;
            ChestInventoryColours[DresserTab.Legs] = EmptyDictionary;
            ChestInventoryColours[DresserTab.Neck] = EmptyDictionary;
            ChestInventoryColours[DresserTab.Wrists] = EmptyDictionary;
            ChestInventoryColours[DresserTab.MainHand] = EmptyDictionary;
            ChestInventoryColours[DresserTab.OffHand] = EmptyDictionary;
            if (HasState)
            {
                Clear();
            }

            HasState = false;
        }
        
        public override void Setup()
        {
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    EmptyDictionary.Add(new Vector2(x,y), null);
                }
            }

        }

        public void Clear()
        {
            var atkUnitBase = AtkUnitBase;
            if (atkUnitBase != null)
            {
                this.SetColors(EmptyDictionary);
                this.SetTabColors(EmptyTabs);
            }
        }
    }
}
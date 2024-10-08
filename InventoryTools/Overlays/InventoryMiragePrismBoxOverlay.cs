using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using InventoryTools.Logic;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Overlays
{
    public class InventoryMiragePrismBoxOverlay: GameOverlay<AtkInventoryMiragePrismBox>, IAtkOverlayState
    {
        private readonly ICharacterMonitor _characterMonitor;

        public InventoryMiragePrismBoxOverlay(ILogger<InventoryMiragePrismBoxOverlay> logger, AtkInventoryMiragePrismBox overlay, ICharacterMonitor characterMonitor) : base(logger,overlay)
        {
            _characterMonitor = characterMonitor;
        }

        public override bool HasState { get; set; }
        public override bool NeedsStateRefresh { get; set; }

        public Dictionary<AtkInventoryMiragePrismBox.DresserTab, Dictionary<Vector2, Vector4?>> ChestInventoryColours = new();
        public Dictionary<uint, Vector4?> TabColours = new();
        public Dictionary<uint, Vector4?> EmptyTabs = new() { {0, null}, {1, null}, {2, null}, {3, null}, {4, null}, {5, null}, {6, null}, {7, null}, {8, null}, {9, null}, {10, null} };
        public Dictionary<Vector2, Vector4?> EmptyDictionary = new();

        public int? _storedTab = null;
        public int? _currentPage = null;
        public uint? _classJobSelected = null;
        public bool? _onlyDisplay = null;

        public override void Update()
        {
            if (!HasState || !AtkOverlay.HasAddon)
            {
                return;
            }
            var currentTab = AtkOverlay.CurrentTab;
            var currentPage = AtkOverlay.CurrentPage;
            var classJobSelected = AtkOverlay.ClassJobSelected;
            var onlyDisplay = AtkOverlay.OnlyDisplayRaceGenderItems;
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
            if (classJobSelected != _classJobSelected)
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
                AtkInventoryMiragePrismBox.DresserTab? dresserTab = null;
                switch (AtkOverlay.CurrentTab)
                {
                    case 0:
                        dresserTab = AtkInventoryMiragePrismBox.DresserTab.MainHand;
                        break;
                    case 1:
                        dresserTab = AtkInventoryMiragePrismBox.DresserTab.OffHand;
                        break;
                    case 2:
                        dresserTab = AtkInventoryMiragePrismBox.DresserTab.Head;
                        break;
                    case 3:
                        dresserTab = AtkInventoryMiragePrismBox.DresserTab.Body;
                        break;
                    case 4:
                        dresserTab = AtkInventoryMiragePrismBox.DresserTab.Hands;
                        break;
                    case 5:
                        dresserTab = AtkInventoryMiragePrismBox.DresserTab.Legs;
                        break;
                    case 6:
                        dresserTab = AtkInventoryMiragePrismBox.DresserTab.Feet;
                        break;
                    case 7:
                        dresserTab = AtkInventoryMiragePrismBox.DresserTab.Ears;
                        break;
                    case 8:
                        dresserTab = AtkInventoryMiragePrismBox.DresserTab.Neck;
                        break;
                    case 9:
                        dresserTab = AtkInventoryMiragePrismBox.DresserTab.Wrists;
                        break;
                    case 10:
                        dresserTab = AtkInventoryMiragePrismBox.DresserTab.Fingers;
                        break;
                }

                if (dresserTab != null && ChestInventoryColours.ContainsKey(dresserTab.Value))
                {
                    this.AtkOverlay.SetColors(ChestInventoryColours[dresserTab.Value]);
                }
                this.AtkOverlay.SetTabColors(TabColours);

                return true;
            }

            return false;
        }

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
                    ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Body] = newState.GetGlamourHighlights(AtkInventoryMiragePrismBox.DresserTab.Body, AtkOverlay.CurrentPage, AtkOverlay.OnlyDisplayRaceGenderItems, AtkOverlay.ClassJobSelected);
                    ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Ears] = newState.GetGlamourHighlights(AtkInventoryMiragePrismBox.DresserTab.Ears, AtkOverlay.CurrentPage, AtkOverlay.OnlyDisplayRaceGenderItems, AtkOverlay.ClassJobSelected);
                    ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Feet] = newState.GetGlamourHighlights(AtkInventoryMiragePrismBox.DresserTab.Feet, AtkOverlay.CurrentPage, AtkOverlay.OnlyDisplayRaceGenderItems, AtkOverlay.ClassJobSelected);
                    ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Fingers] = newState.GetGlamourHighlights(AtkInventoryMiragePrismBox.DresserTab.Fingers, AtkOverlay.CurrentPage, AtkOverlay.OnlyDisplayRaceGenderItems, AtkOverlay.ClassJobSelected);
                    ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Hands] = newState.GetGlamourHighlights(AtkInventoryMiragePrismBox.DresserTab.Hands, AtkOverlay.CurrentPage, AtkOverlay.OnlyDisplayRaceGenderItems, AtkOverlay.ClassJobSelected);
                    ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Head] = newState.GetGlamourHighlights(AtkInventoryMiragePrismBox.DresserTab.Head, AtkOverlay.CurrentPage, AtkOverlay.OnlyDisplayRaceGenderItems, AtkOverlay.ClassJobSelected);
                    ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Legs] = newState.GetGlamourHighlights(AtkInventoryMiragePrismBox.DresserTab.Legs, AtkOverlay.CurrentPage, AtkOverlay.OnlyDisplayRaceGenderItems, AtkOverlay.ClassJobSelected);
                    ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Neck] = newState.GetGlamourHighlights(AtkInventoryMiragePrismBox.DresserTab.Neck, AtkOverlay.CurrentPage, AtkOverlay.OnlyDisplayRaceGenderItems, AtkOverlay.ClassJobSelected);
                    ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Wrists] = newState.GetGlamourHighlights(AtkInventoryMiragePrismBox.DresserTab.Wrists, AtkOverlay.CurrentPage, AtkOverlay.OnlyDisplayRaceGenderItems, AtkOverlay.ClassJobSelected);
                    ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.MainHand] = newState.GetGlamourHighlights(AtkInventoryMiragePrismBox.DresserTab.MainHand, AtkOverlay.CurrentPage, AtkOverlay.OnlyDisplayRaceGenderItems, AtkOverlay.ClassJobSelected);
                    ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.OffHand] = newState.GetGlamourHighlights(AtkInventoryMiragePrismBox.DresserTab.OffHand, AtkOverlay.CurrentPage, AtkOverlay.OnlyDisplayRaceGenderItems, AtkOverlay.ClassJobSelected);
                    TabColours[0] = newState.GetTabHighlight(ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.MainHand]);
                    TabColours[1] =  newState.GetTabHighlight(ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.OffHand]);
                    TabColours[2] = newState.GetTabHighlight(ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Head]);
                    TabColours[3] = newState.GetTabHighlight(ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Body]);
                    TabColours[4] = newState.GetTabHighlight(ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Hands]);
                    TabColours[5] = newState.GetTabHighlight(ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Legs]);
                    TabColours[6] = newState.GetTabHighlight(ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Feet]);
                    TabColours[7] = newState.GetTabHighlight(ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Ears]);
                    TabColours[8] = newState.GetTabHighlight(ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Neck]);
                    TabColours[9] = newState.GetTabHighlight(ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Wrists]);
                    TabColours[10] = newState.GetTabHighlight(ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Fingers]);
                    Draw();
                    return;
                }
            }

            if (HasState)
            {
                ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Body] = EmptyDictionary;
                ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Ears] = EmptyDictionary;
                ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Feet] = EmptyDictionary;
                ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Fingers] = EmptyDictionary;
                ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Hands] = EmptyDictionary;
                ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Head] = EmptyDictionary;
                ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Legs] = EmptyDictionary;
                ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Neck] = EmptyDictionary;
                ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.Wrists] = EmptyDictionary;
                ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.MainHand] = EmptyDictionary;
                ChestInventoryColours[AtkInventoryMiragePrismBox.DresserTab.OffHand] = EmptyDictionary;
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

        public override void Clear()
        {
            var atkUnitBase = AtkOverlay.AtkUnitBase;
            if (atkUnitBase != null)
            {
                this.AtkOverlay.SetColors(EmptyDictionary);
                this.AtkOverlay.SetTabColors(EmptyTabs);
            }
        }
    }
}
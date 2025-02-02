using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using CriticalCommonLib.Models;
using Dalamud.Configuration;
using Dalamud.Interface.Colors;
using InventoryTools.Attributes;
using InventoryTools.Converters;
using InventoryTools.Logic;
using InventoryTools.Logic.Editors;
using InventoryTools.Logic.Settings;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OtterGui.Classes;

namespace InventoryTools
{
    [Serializable]
    public class InventoryToolsConfiguration : IPluginConfiguration, IConfigurable<bool?>, IConfigurable<int?>, IConfigurable<Enum?>, IConfigurable<Dictionary<Type, bool>>, IConfigurable<Vector4?>, IConfigurable<uint?>
    {
        [JsonIgnore]
        public bool IsDirty { get; set; }

        private bool _automaticallyDownloadMarketPrices;
        private bool _colorRetainerList = true;

        private bool _displayCrossCharacter = true;
        private bool _displayTooltip = true;

        private Vector4 _highlightColor = new (0.007f, 0.008f,
            0.007f, 0.212f);

        private Vector4 _destinationHighlightColor = new Vector4(0.321f, 0.239f, 0.03f, 1f);

        private Vector4 _retainerListColor = ImGuiColors.HealerGreen;

        private string _highlightWhen = "When Searching";
        private bool _invertHighlighting = true;
        private bool _invertDestinationHighlighting;
        private bool _invertTabHighlighting;
        private bool _highlightDestination;
        private bool _highlightDestinationEmpty;
        private bool _addMoreInformationContextMenu;
        private bool _addToCraftListContextMenu;
        private bool _addToActiveCraftListContextMenu;
        private bool _openCraftingLogContextMenu;
        private bool _openGatheringLogContextMenu;
        private bool _openFishingLogContextMenu;
        private bool _itemSearchContextMenu;

        private bool _isVisible;
        private int _marketRefreshTimeHours = 24;
        private int _marketSaleHistoryLimit = 7;
        private bool _showItemNumberRetainerList = true;
        private bool _historyEnabled;
        private bool _addTitleMenuButton;

        private Vector4 _tabHighlightColor = new (0.007f, 0.008f,
            0.007f, 0.2f);

        public List<FilterConfiguration> FilterConfigurations = new();

        public Dictionary<ulong, Character> SavedCharacters = new();

        private Dictionary<ulong, HashSet<uint>> _acquiredItems = new();
        public bool InventoriesMigrated { get; set; } = false;
        public bool InventoriesMigratedToCsv { get; set; } = false;

        private HashSet<string>? _openWindows = new();
        private Dictionary<string, Vector2>? _savedWindowPositions = new();
        private List<InventoryChangeReason> _historyTrackReasons = new();
        private List<uint>? _tooltipWhitelistCategories = new();
        private bool _tooltipWhitelistBlacklist;
        private List<InventorySearchScope>? _tooltipSearchScope;
        private List<InventorySearchScope>? _itemSearchScope;
        private HashSet<string>? _windowsIgnoreEscape = new HashSet<string>();
        private HashSet<uint>? _favouriteItemsList = new HashSet<uint>();
        private TooltipAmountOwnedSort _tooltipAmountOwnedSort = TooltipAmountOwnedSort.Alphabetically;
        private Dictionary<string, bool>? _booleanSettings = new();
        private Dictionary<string, int>? _integerSettings = new();
        private Dictionary<string, uint>? _uintegerSettings = new();
        private Dictionary<string, Vector4>? _vector4Settings = new();
        private Dictionary<string, Enum>? _enumSettings = new();
        private Dictionary<string, Dictionary<Type, bool>>? _typeDictionarySettings = new();
        private Dictionary<ItemInfoType, TooltipSourceSetting>? _tooltipInfoSourceSetting = new();
        private Dictionary<ItemInfoType, TooltipSourceSetting>? _tooltipInfoUseSetting = new();

        [JsonProperty] [DefaultValue(300)] public int CraftWindowSplitterPosition { get; set; } = 300;

        public void ClearDirtyFlags()
        {
            this.IsDirty = false;
            foreach (var filter in FilterConfigurations)
            {
                filter.ConfigurationDirty = false;
                filter.TableConfigurationDirty = false;
            }
        }

        public HashSet<string> WindowsIgnoreEscape
        {
            get => _windowsIgnoreEscape ??= new();
            set
            {
                _windowsIgnoreEscape = value;
                IsDirty = true;
            }
        }

        public HashSet<uint> FavouriteItemsList
        {
            get => _favouriteItemsList ??= new();
            set
            {
                _favouriteItemsList = value;
                IsDirty = true;
            }
        }

        public Dictionary<ItemInfoType, TooltipSourceSetting> TooltipInfoSourceSetting
        {
            get => _tooltipInfoSourceSetting ??= new();
            set
            {
                _tooltipInfoSourceSetting = value;
                IsDirty = true;
            }
        }

        public Dictionary<ItemInfoType, TooltipSourceSetting> TooltipInfoUseSetting
        {
            get => _tooltipInfoUseSetting ??= new();
            set
            {
                _tooltipInfoUseSetting = value;
                IsDirty = true;
            }
        }

        public bool IsFavouriteItem(uint itemId)
        {
            return FavouriteItemsList.Contains(itemId);
        }

        public void FavouriteItem(uint itemId)
        {
            FavouriteItemsList.Add(itemId);
            IsDirty = true;
        }

        public void UnfavouriteItem(uint itemId)
        {
            FavouriteItemsList.Remove(itemId);
            IsDirty = true;
        }

        public void ToggleFavouriteItem(uint itemId)
        {
            if (FavouriteItemsList.Contains(itemId))
            {
                FavouriteItemsList.Remove(itemId);
            }
            else
            {
                FavouriteItemsList.Add(itemId);
            }
            IsDirty = true;
        }




        public bool HistoryEnabled
        {
            get => _historyEnabled;
            set
            {
                _historyEnabled = value;
                IsDirty = true;
            }
        }
        public bool AddTitleMenuButton
        {
            get => _addTitleMenuButton;
            set
            {
                _addTitleMenuButton = value;
                IsDirty = true;
            }
        }
        public bool TooltipWhitelistBlacklist
        {
            get => _tooltipWhitelistBlacklist;
            set
            {
                _tooltipWhitelistBlacklist = value;
                IsDirty = true;
            }
        }
        public List<uint> TooltipWhitelistCategories
        {
            get => _tooltipWhitelistCategories ??= new();
            set
            {
                _tooltipWhitelistCategories = value;
                IsDirty = true;
            }
        }

        public List<InventoryChangeReason> HistoryTrackReasons
        {
            get
            {
                return _historyTrackReasons;
            }
            set
            {
                _historyTrackReasons = value;
                IsDirty = true;
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                IsDirty = true;
            }
        }

        public bool AddMoreInformationContextMenu
        {
            get => _addMoreInformationContextMenu;
            set
            {
                _addMoreInformationContextMenu = value;
                IsDirty = true;
            }
        }

        [DefaultValue(true)]
        public bool AddToCraftListContextMenu
        {
            get => _addToCraftListContextMenu;
            set
            {
                _addToCraftListContextMenu = value;
                IsDirty = true;
            }
        }
        [DefaultValue(false)]
        public bool AddToActiveCraftListContextMenu
        {
            get => _addToActiveCraftListContextMenu;
            set
            {
                _addToActiveCraftListContextMenu = value;
                IsDirty = true;
            }
        }
        [DefaultValue(false)]
        public bool OpenCraftingLogContextMenu
        {
            get => _openCraftingLogContextMenu;
            set
            {
                _openCraftingLogContextMenu = value;
                IsDirty = true;
            }
        }
        [DefaultValue(false)]
        public bool OpenGatheringLogContextMenu
        {
            get => _openGatheringLogContextMenu;
            set
            {
                _openGatheringLogContextMenu = value;
                IsDirty = true;
            }
        }
        [DefaultValue(false)]
        public bool OpenFishingLogContextMenu
        {
            get => _openFishingLogContextMenu;
            set
            {
                _openFishingLogContextMenu = value;
                IsDirty = true;
            }
        }
        [DefaultValue(false)]
        public bool ItemSearchContextMenu
        {
            get => _itemSearchContextMenu;
            set
            {
                _itemSearchContextMenu = value;
                IsDirty = true;
            }
        }

        public List<InventorySearchScope>? ItemSearchScope
        {
            get => _itemSearchScope;
            set
            {
                _itemSearchScope = value;
                IsDirty = true;
            }
        }

        public void AddWindowToIgnoreEscape(Type windowType)
        {
            WindowsIgnoreEscape.Add(windowType.Name);
            IsDirty = true;
        }

        public void RemoveWindowFromIgnoreEscape(Type windowType)
        {
            WindowsIgnoreEscape.Remove(windowType.Name);
            IsDirty = true;
        }

        public void SetWindowIgnoreEscape(Type windowType, bool ignoreEscape)
        {
            if (ignoreEscape)
            {
                AddWindowToIgnoreEscape(windowType);
            }
            else
            {
                RemoveWindowFromIgnoreEscape(windowType);
            }
        }

        public bool DoesWindowIgnoreEscape(Type windowName)
        {
            return WindowsIgnoreEscape.Contains(windowName.Name);
        }

        public int SelectedConfigurationPage { get; set; }
        public bool ShowFilterTab { get; set; } = true;
        public bool SwitchFiltersAutomatically { get; set; } = true;
        public bool SwitchCraftListsAutomatically { get; set; } = true;
        private bool _tooltipCurrentCharacter;
        private bool _tooltipDisplayAmountOwned = true;
        private bool _tooltipDisplayUnlock;
        private List<ulong>? _tooltipDisplayUnlockCharacters = new();
        private bool _tooltipDisplayMarketAveragePrice;
        private bool _tooltipDisplayMarketLowestPrice = true;
        private bool _tooltipAddCharacterNameOwned;
        private bool _tooltipDisplayRetrieveAmount;
        private int _tooltipLocationLimit = 10;
        private bool _tooltipDisplayHeader;
        private int _tooltipHeaderLines;
        private int _tooltipFooterLines;
        private TooltipLocationDisplayMode _tooltipLocationDisplayMode = TooltipLocationDisplayMode.CharacterCategoryQuantityQuality;
        private WindowLayout _craftWindowLayout =  WindowLayout.Tabs;
        private WindowLayout _filtersLayout = WindowLayout.Tabs;
        private uint? _tooltipColor;
        private HashSet<NotificationPopup>? _notificationsSeen = new ();

        [Vector4Default("0.007, 0.008,0.007, 0.212")]
        public Vector4 HighlightColor
        {
            get => _highlightColor;
            set
            {
                _highlightColor = value;
                IsDirty = true;
            }
        }

        [Vector4Default("0.321, 0.239, 0.03, 1")]
        public Vector4 DestinationHighlightColor
        {
            get => _destinationHighlightColor;
            set
            {
                _destinationHighlightColor = value;
                IsDirty = true;
            }
        }

        public Vector4 RetainerListColor
        {
            get => _retainerListColor;
            set
            {
                _retainerListColor = value;
                IsDirty = true;
            }
        }

        [Vector4Default("0.007, 0.008,0.007, 0.2")]
        public Vector4 TabHighlightColor
        {
            get => _tabHighlightColor;
            set
            {
                _tabHighlightColor = value;
                IsDirty = true;
            }
        }

        public bool DisplayCrossCharacter
        {
            get => _displayCrossCharacter;
            set
            {
                _displayCrossCharacter = value;
                IsDirty = true;
            }
        }

        public bool DisplayTooltip
        {
            get => _displayTooltip;
            set
            {
                _displayTooltip = value;
                IsDirty = true;
            }
        }

        public bool TooltipCurrentCharacter
        {
            get => _tooltipCurrentCharacter;
            set
            {
                _tooltipCurrentCharacter = value;
                IsDirty = true;
            }
        }

        public bool TooltipDisplayAmountOwned
        {
            get => _tooltipDisplayAmountOwned;
            set
            {
                _tooltipDisplayAmountOwned = value;
                IsDirty = true;
            }
        }
        public bool TooltipDisplayUnlock
        {
            get => _tooltipDisplayUnlock;
            set
            {
                _tooltipDisplayUnlock = value;
                IsDirty = true;
            }
        }
        public List<ulong>? TooltipDisplayUnlockCharacters
        {
            get => _tooltipDisplayUnlockCharacters;
            set
            {
                _tooltipDisplayUnlockCharacters = value;
                IsDirty = true;
            }
        }

        public bool TooltipAddCharacterNameOwned
        {
            get => _tooltipAddCharacterNameOwned;
            set
            {
                _tooltipAddCharacterNameOwned = value;
                IsDirty = true;
            }
        }

        public bool TooltipDisplayMarketAveragePrice
        {
            get => _tooltipDisplayMarketAveragePrice;
            set
            {
                _tooltipDisplayMarketAveragePrice = value;
                IsDirty = true;
            }
        }

        public bool TooltipDisplayMarketLowestPrice
        {
            get => _tooltipDisplayMarketLowestPrice;
            set
            {
                _tooltipDisplayMarketLowestPrice = value;
                IsDirty = true;
            }
        }

        public bool TooltipDisplayRetrieveAmount
        {
            get => _tooltipDisplayRetrieveAmount;
            set
            {
                _tooltipDisplayRetrieveAmount = value;
                IsDirty = true;
            }
        }

        [DefaultValue(10)]
        public int TooltipLocationLimit
        {
            get => _tooltipLocationLimit;
            set
            {
                _tooltipLocationLimit = value;
                IsDirty = true;
            }
        }
        public TooltipLocationDisplayMode TooltipLocationDisplayMode
        {
            get => _tooltipLocationDisplayMode;
            set
            {
                _tooltipLocationDisplayMode = value;
                IsDirty = true;
            }
        }
        public WindowLayout CraftWindowLayout
        {
            get => _craftWindowLayout;
            set
            {
                _craftWindowLayout = value;
                IsDirty = true;
            }
        }
        public WindowLayout FiltersLayout
        {
            get => _filtersLayout;
            set
            {
                _filtersLayout = value;
                IsDirty = true;
            }
        }

        public bool AutomaticallyDownloadMarketPrices
        {
            get => _automaticallyDownloadMarketPrices;
            set
            {
                _automaticallyDownloadMarketPrices = value;
                IsDirty = true;
            }
        }

        public List<InventorySearchScope>? TooltipSearchScope
        {
            get => _tooltipSearchScope;
            set
            {
                _tooltipSearchScope = value;
                IsDirty = true;
            }
        }

        [DefaultValue(24)]
        public int MarketRefreshTimeHours
        {
            get => _marketRefreshTimeHours;
            set
            {
                _marketRefreshTimeHours = value;
                IsDirty = true;
            }
        }

        [DefaultValue(7)]
        public int MarketSaleHistoryLimit
        {
            get => _marketSaleHistoryLimit;
            set
            {
                _marketSaleHistoryLimit = value;
                IsDirty = true;
            }
        }

        public bool ColorRetainerList
        {
            get => _colorRetainerList;
            set
            {
                _colorRetainerList = value;
                IsDirty = true;
            }
        }

        public bool ShowItemNumberRetainerList
        {
            get => _showItemNumberRetainerList;
            set
            {
                _showItemNumberRetainerList = value;
                IsDirty = true;
            }
        }

        public bool InvertHighlighting
        {
            get => _invertHighlighting;
            set
            {
                _invertHighlighting = value;
                IsDirty = true;
            }
        }

        public bool InvertDestinationHighlighting
        {
            get => _invertDestinationHighlighting;
            set
            {
                _invertDestinationHighlighting = value;
                IsDirty = true;
            }
        }

        public bool InvertTabHighlighting
        {
            get => _invertTabHighlighting;
            set
            {
                _invertTabHighlighting = value;
                IsDirty = true;
            }
        }

        public string HighlightWhen
        {
            get => _highlightWhen;
            set
            {
                _highlightWhen = value;
                IsDirty = true;
            }
        }

        public bool HighlightDestination
        {
            get => _highlightDestination;
            set
            {
                _highlightDestination = value;
                IsDirty = true;
            }
        }

        public bool HighlightDestinationEmpty
        {
            get => _highlightDestinationEmpty;
            set
            {
                _highlightDestinationEmpty = value;
                IsDirty = true;
            }
        }

        public bool TrackMobSpawns
        {
            get => _trackMobSpawns;
            set
            {
                _trackMobSpawns = value;
                IsDirty = true;
            }
        }

        public bool TooltipDisplayHeader
        {
            get => _tooltipDisplayHeader;
            set
            {
                _tooltipDisplayHeader = value;
                IsDirty = true;
            }
        }

        public HashSet<NotificationPopup> NotificationsSeen
        {
            get => _notificationsSeen ??= new HashSet<NotificationPopup>();
            set
            {
                _notificationsSeen = value;
                IsDirty = true;
            }
        }

        public bool HasSeenNotification(NotificationPopup popup)
        {
            return NotificationsSeen.Contains(popup);
        }

        public void MarkNotificationSeen(NotificationPopup popup)
        {
            NotificationsSeen.Add(popup);
            IsDirty = true;
        }

        public Dictionary<ulong, HashSet<uint>> AcquiredItems
        {
            get => _acquiredItems ??= new Dictionary<ulong, HashSet<uint>>();
            set => _acquiredItems = value;
        }

        public HashSet<string> WizardVersionsSeen
        {
            get => _wizardVersionsSeen ??= new();
            set
            {
                _wizardVersionsSeen = value;
                IsDirty = true;
            }
        }

        [DefaultValue(true)]
        public bool MarketBoardUseActiveWorld
        {
            get => _marketBoardUseActiveWorld;
            set
            {
                _marketBoardUseActiveWorld = value;
                IsDirty = true;
            }
        }

        [DefaultValue(true)]
        public bool MarketBoardUseHomeWorld
        {
            get => _marketBoardUseHomeWorld;
            set
            {
                _marketBoardUseHomeWorld = value;
                IsDirty = true;
            }
        }

        public List<uint> MarketBoardWorldIds
        {
            get => _marketBoardWorldIds ??= new List<uint>();
            set
            {
                _marketBoardWorldIds = value;
                IsDirty = true;
            }
        }


        public bool SeenWizardVersion(string versionNumber)
        {
            return WizardVersionsSeen.Contains(versionNumber);
        }

        public void MarkWizardVersionSeen(string versionNumber)
        {
            WizardVersionsSeen.Add(versionNumber);
            IsDirty = true;
        }

        [DefaultValue(true)]
        public bool ShowWizardNewFeatures
        {
            get => _showWizardNewFeatures;
            set
            {
                _showWizardNewFeatures = value;
                IsDirty = true;
            }
        }

        [DefaultValue(Logic.Settings.TooltipAmountOwnedSort.Alphabetically)]
        public TooltipAmountOwnedSort TooltipAmountOwnedSort
        {
            get => _tooltipAmountOwnedSort;
            set
            {
                _tooltipAmountOwnedSort = value;
                IsDirty = true;
            }
        }


        public string? ActiveUiFilter { get; set; } = null;

        public bool TetrisEnabled { get; set; } = false;

        public string? ActiveBackgroundFilter { get; set; }

        public string? ActiveCraftList { get; set; } = null;

        public bool SaveBackgroundFilter { get; set; } = false;

        public bool FirstRun { get; set; } = true;

        [DefaultValue(true)]
        private bool _showWizardNewFeatures { get; set; } = true;

        private HashSet<string>? _wizardVersionsSeen { get; set; }
        public int SelectedHelpPage { get; set; }
        #if DEBUG
        public int SelectedDebugPage { get; set; }
        #endif
        public bool AutoSave { get; set; } = true;
        public int AutoSaveMinutes { get; set; } = 10;
        public int InternalVersion { get; set; } = 0;
        public int Version { get; set; }

        public uint? TooltipColor
        {
            get => _tooltipColor;
            set => _tooltipColor = value;
        }

        public ModifiableHotkey? MoreInformationHotKey
        {
            get => _moreInformationHotKey;
            set => _moreInformationHotKey = value;
        }

        public ModifiableHotkey? OpenCraftingLogHotKey
        {
            get => _openCraftingLogHotKey;
            set => _openCraftingLogHotKey = value;
        }

        public ModifiableHotkey? OpenGatheringLogHotKey
        {
            get => _openGatheringLogHotKey;
            set => _openGatheringLogHotKey = value;
        }

        public ModifiableHotkey? OpenFishingLogHotKey
        {
            get => _openFishingLogHotKey;
            set => _openFishingLogHotKey = value;
        }

        public ModifiableHotkey? OpenItemLogHotKey
        {
            get => _openItemLogHotKey;
            set => _openItemLogHotKey = value;
        }

        public ConcurrentDictionary<string,ModifiableHotkey> Hotkeys
        {
            get
            {
                if (_hotkeys == null)
                {
                    _hotkeys = new ConcurrentDictionary<string, ModifiableHotkey>();
                }
                return _hotkeys;
            }
            set
            {
                _hotkeys = value;
            }
        }

        private ModifiableHotkey? _moreInformationHotKey;
        private ModifiableHotkey? _openCraftingLogHotKey;
        private ModifiableHotkey? _openGatheringLogHotKey;
        private ModifiableHotkey? _openFishingLogHotKey;
        private ModifiableHotkey? _openItemLogHotKey;
        private ConcurrentDictionary<string,ModifiableHotkey>? _hotkeys;
        private bool _trackMobSpawns;
        private bool _marketBoardUseActiveWorld = true;
        private bool _marketBoardUseHomeWorld = true;
        private List<uint>? _marketBoardWorldIds;

        public ModifiableHotkey? GetHotkey(string hotkey)
        {
            if(Hotkeys.TryGetValue(hotkey, out var modifiableHotkey))
            {
                return modifiableHotkey;
            }

            return null;
        }

        public HashSet<string> OpenWindows
        {
            get
            {
                if (_openWindows == null)
                {
                    _openWindows = new HashSet<string>();
                }
                return _openWindows;
            }
            set => _openWindows = value;
        }
        public Dictionary<string, Vector2> SavedWindowPositions
        {
            get
            {
                if (_savedWindowPositions == null)
                {
                    _savedWindowPositions = new Dictionary<string, Vector2>();
                }
                return _savedWindowPositions;
            }
            set => _savedWindowPositions = value;
        }

        public int TooltipHeaderLines
        {
            get => _tooltipHeaderLines;
            set
            {
                _tooltipHeaderLines = value;
                IsDirty = true;
            }
        }

        public int TooltipFooterLines
        {
            get => _tooltipFooterLines;
            set
            {
                _tooltipFooterLines = value;
                IsDirty = true;
            }
        }

        public LogLevel LogLevel { get; set; } = LogLevel.Information;

        public Dictionary<string, bool> BooleanSettings
        {
            get => _booleanSettings ??= new Dictionary<string, bool>();
            set => _booleanSettings = value;
        }

        public Dictionary<string, int> IntegerSettings
        {
            get => _integerSettings ??= new Dictionary<string, int>();
            set => _integerSettings = value;
        }

        public Dictionary<string, uint> UIntegerSettings
        {
            get => _uintegerSettings ??= new Dictionary<string, uint>();
            set => _uintegerSettings = value;
        }

        [JsonConverter(typeof(EnumDictionaryConverter))]
        public Dictionary<string, Enum> EnumSettings
        {
            get => _enumSettings ??= new Dictionary<string, Enum>();
            set => _enumSettings = value;
        }

        [JsonConverter(typeof(TypeDictionaryConverter))]
        public Dictionary<string, Dictionary<Type, bool>> TypeDictionarySettings
        {
            get => _typeDictionarySettings ??= new Dictionary<string, Dictionary<Type, bool>>();
            set => _typeDictionarySettings = value;
        }

        public Dictionary<string, Vector4> Vector4Settings
        {
            get => _vector4Settings ??= new Dictionary<string, Vector4>();
            set => _vector4Settings = value;
        }


        //Configuration Helpers

        public Dictionary<ulong, Character> GetSavedRetainers()
        {
            return SavedCharacters;
        }

        public List<FilterConfiguration> GetSavedFilters()
        {
            return FilterConfigurations;
        }

        public void MarkReloaded()
        {
            if (!SaveBackgroundFilter)
            {
                ActiveBackgroundFilter = null;
            }
        }

        public bool HasDefaultCraftList()
        {
            if (FilterConfigurations.Any(c => c.FilterType == FilterType.CraftFilter && c.CraftListDefault))
            {
                return true;
            }

            return false;
        }

        public bool HasList(string name)
        {
            if (FilterConfigurations.Any(c => c.Name == name))
            {
                return true;
            }

            return false;
        }


        public bool? Get(string key, bool? defaultValue)
        {
            return this.BooleanSettings.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public void Set(string key, int? newValue)
        {
            if (newValue == null)
            {
                this.IntegerSettings.Remove(key);
            }
            else
            {
                this.IntegerSettings[key] = newValue.Value;
            }

            this.IsDirty = true;
        }

        public void Set(string key, bool? newValue)
        {
            if (newValue == null)
            {
                this.BooleanSettings.Remove(key);
            }
            else
            {
                this.BooleanSettings[key] = newValue.Value;
            }

            this.IsDirty = true;
        }

        public int? Get(string key, int? defaultValue)
        {
            return this.IntegerSettings.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public Enum? Get(string key, Enum? defaultValue)
        {
            return this.EnumSettings.GetValueOrDefault(key);
        }

        public void Set(string key, Enum? newValue)
        {
            if (newValue == null)
            {
                this.EnumSettings.Remove(key);
            }
            else
            {
                this.EnumSettings[key] = newValue;
            }

            this.IsDirty = true;
        }

        public Dictionary<Type, bool>? Get(string key, Dictionary<Type, bool>? defaultValue)
        {
            return this.TypeDictionarySettings.GetValueOrDefault(key);
        }

        public void Set(string key, Dictionary<Type, bool>? newValue)
        {
            if (newValue == null)
            {
                this.TypeDictionarySettings.Remove(key);
            }
            else
            {
                this.TypeDictionarySettings[key] = newValue;
            }

            this.IsDirty = true;
        }

        public Vector4? Get(string key, Vector4? defaultValue)
        {
            return this.Vector4Settings.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public void Set(string key, Vector4? newValue)
        {
            if (newValue == null)
            {
                this.Vector4Settings.Remove(key);
            }
            else
            {
                this.Vector4Settings[key] = newValue.Value;
            }

            this.IsDirty = true;
        }

        public uint? Get(string key, uint? defaultValue)
        {
            return this.UIntegerSettings.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public void Set(string key, uint? newValue)
        {
            if (newValue == null)
            {
                this.UIntegerSettings.Remove(key);
            }
            else
            {
                this.UIntegerSettings[key] = newValue.Value;
            }

            this.IsDirty = true;
        }
    }
}
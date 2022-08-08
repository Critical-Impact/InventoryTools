using System;
using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using Dalamud.Configuration;
using Dalamud.Interface.Colors;
using InventoryTools.Logic;
using Newtonsoft.Json;
using OtterGui.Classes;

namespace InventoryTools
{
    [Serializable]
    public class InventoryToolsConfiguration : IPluginConfiguration
    {
        public delegate void ConfigurationChangedDelegate();

        private bool _automaticallyDownloadMarketPrices = false;
        private bool _colorRetainerList = true;

        private bool _displayCrossCharacter = true;
        private bool _displayTooltip = true;

        private Vector4 _highlightColor = new (0.007f, 0.008f,
            0.007f, 0.212f);

        private Vector4 _destinationHighlightColor = new Vector4(0.321f, 0.239f, 0.03f, 1f);

        private Vector4 _retainerListColor = ImGuiColors.HealerGreen;

        private string _highlightWhen = "Always";
        private bool _invertHighlighting = true;
        private bool _invertDestinationHighlighting = false;
        private bool _invertTabHighlighting = false;
        private bool _highlightDestination = false;
        private bool _highlightDestinationEmpty = false;
        private bool _addMoreInformationContextMenu = false;

        private bool _isVisible;
        private int _marketRefreshTimeHours = 24;
        private int _marketSaleHistoryLimit = 7;
        private bool _showItemNumberRetainerList = true;

        private Vector4 _tabHighlightColor = new (0.007f, 0.008f,
            0.007f, 1.0f);

        public List<FilterConfiguration> FilterConfigurations = new();

        public Dictionary<ulong, Character> SavedCharacters = new();

        private Dictionary<ulong, HashSet<uint>> _acquiredItems = new();

        [JsonIgnore]
        public Dictionary<ulong, Dictionary<InventoryCategory,List<InventoryItem>>> SavedInventories = new ();

        public bool InventoriesMigrated { get; set; } = false;

        private HashSet<string>? _openWindows = new();

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                ConfigurationChanged?.Invoke();
            }
        }

        public bool AddMoreInformationContextMenu
        {
            get => _addMoreInformationContextMenu;
            set
            {
                _addMoreInformationContextMenu = value;
                ConfigurationChanged?.Invoke();
            }
        }

        public int SelectedConfigurationPage { get; set; }
        public bool ShowFilterTab { get; set; } = true;
        public bool SwitchFiltersAutomatically { get; set; } = true;
        private bool _tooltipDisplayAmountOwned = true;
        private bool _tooltipDisplayMarketAveragePrice = true;
        private bool _tooltipDisplayMarketLowestPrice = false;
        private bool _tooltipAddCharacterNameOwned = false;
        private uint? _tooltipColor = null;
        public Vector4 HighlightColor
        {
            get => _highlightColor;
            set
            {
                _highlightColor = value;
                ConfigurationChanged?.Invoke();
            }
        }
        public Vector4 DestinationHighlightColor
        {
            get => _destinationHighlightColor;
            set
            {
                _destinationHighlightColor = value;
                ConfigurationChanged?.Invoke();
            }
        }

        public Vector4 RetainerListColor
        {
            get => _retainerListColor;
            set
            {
                _retainerListColor = value;
                ConfigurationChanged?.Invoke();
            }
        }

        public Vector4 TabHighlightColor
        {
            get => _tabHighlightColor;
            set
            {
                _tabHighlightColor = value;
                ConfigurationChanged?.Invoke();
            }
        }

        public bool DisplayCrossCharacter
        {
            get => _displayCrossCharacter;
            set
            {
                _displayCrossCharacter = value;
                ConfigurationChanged?.Invoke();
            }
        }

        public bool DisplayTooltip
        {
            get => _displayTooltip;
            set
            {
                _displayTooltip = value;
                ConfigurationChanged?.Invoke();
            }
        }
        
        public bool TooltipDisplayAmountOwned
        {
            get => _tooltipDisplayAmountOwned;
            set
            {
                _tooltipDisplayAmountOwned = value;
                ConfigurationChanged?.Invoke();
            }
        }
        
        public bool TooltipAddCharacterNameOwned
        {
            get => _tooltipAddCharacterNameOwned;
            set
            {
                _tooltipAddCharacterNameOwned = value;
                ConfigurationChanged?.Invoke();
            }
        }

        public bool TooltipDisplayMarketAveragePrice
        {
            get => _tooltipDisplayMarketAveragePrice;
            set
            {
                _tooltipDisplayMarketAveragePrice = value;
                ConfigurationChanged?.Invoke();
            }
        }

        public bool TooltipDisplayMarketLowestPrice
        {
            get => _tooltipDisplayMarketLowestPrice;
            set
            {
                _tooltipDisplayMarketLowestPrice = value;
                ConfigurationChanged?.Invoke();
            }
        }

        public bool AutomaticallyDownloadMarketPrices
        {
            get => _automaticallyDownloadMarketPrices;
            set
            {
                _automaticallyDownloadMarketPrices = value;
                Cache.CacheAutoRetrieve = value;
                ConfigurationChanged?.Invoke();
            }
        }

        public int MarketRefreshTimeHours
        {
            get => _marketRefreshTimeHours;
            set
            {
                _marketRefreshTimeHours = value;
                if (_marketRefreshTimeHours == 0)
                {
                    _marketRefreshTimeHours = 24;
                }
                Cache.CacheTimeHours = _marketRefreshTimeHours;
                ConfigurationChanged?.Invoke();
            }
        }

        public int MarketSaleHistoryLimit
        {
            get => _marketSaleHistoryLimit;
            set
            {
                _marketSaleHistoryLimit = value;
                if (_marketSaleHistoryLimit == 0)
                {
                    _marketSaleHistoryLimit = 7;
                }
                Universalis.SetSaleHistoryLimit(_marketRefreshTimeHours);
                ConfigurationChanged?.Invoke();
            }
        }

        public bool ColorRetainerList
        {
            get => _colorRetainerList;
            set
            {
                _colorRetainerList = value;
                ConfigurationChanged?.Invoke();
            }
        }

        public bool ShowItemNumberRetainerList
        {
            get => _showItemNumberRetainerList;
            set
            {
                _showItemNumberRetainerList = value;
                ConfigurationChanged?.Invoke();
            }
        }

        public bool InvertHighlighting
        {
            get => _invertHighlighting;
            set
            {
                _invertHighlighting = value;
                ConfigurationChanged?.Invoke();
            }
        }

        public bool InvertDestinationHighlighting
        {
            get => _invertDestinationHighlighting;
            set
            {
                _invertDestinationHighlighting = value;
                ConfigurationChanged?.Invoke();
            }
        }

        public bool InvertTabHighlighting
        {
            get => _invertTabHighlighting;
            set
            {
                _invertTabHighlighting = value;
                ConfigurationChanged?.Invoke();
            }
        }

        public string HighlightWhen
        {
            get => _highlightWhen;
            set
            {
                _highlightWhen = value;
                ConfigurationChanged?.Invoke();
            }
        }

        public bool HighlightDestination
        {
            get => _highlightDestination;
            set
            {
                _highlightDestination = value;
                ConfigurationChanged?.Invoke();
            }
        }

        public bool HighlightDestinationEmpty
        {
            get => _highlightDestinationEmpty;
            set
            {
                _highlightDestinationEmpty = value;
                ConfigurationChanged?.Invoke();
            }
        }
        
        public Dictionary<ulong, HashSet<uint>> AcquiredItems
        {
            get => _acquiredItems ?? new Dictionary<ulong, HashSet<uint>>();
            set => _acquiredItems = value;
        }

        public string? ActiveUiFilter { get; set; } = null;

        public bool TetrisEnabled { get; set; } = false;

        public string? ActiveBackgroundFilter { get; set; } = null;
        public bool SaveBackgroundFilter { get; set; } = false;

        public bool FirstRun { get; set; } = true;
        public bool IntroShown { get; set; } = false;
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

        public ModifiableHotkey? MoreInformationHotKey { get; set; }

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

        public event ConfigurationChangedDelegate? ConfigurationChanged;

        //Configuration Helpers
        public Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>> GetSavedInventory()
        {
            return SavedInventories;
        }

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
    }
}
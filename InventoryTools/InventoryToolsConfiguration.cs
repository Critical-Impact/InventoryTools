using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CriticalCommonLib.Resolvers;
using Dalamud.Configuration;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using Dalamud.Plugin;
using InventoryTools.Logic;
using Newtonsoft.Json;

namespace InventoryTools
{
    [Serializable]
    public class InventoryToolsConfiguration : IPluginConfiguration
    {
        public delegate void ConfigurationChangedDelegate();

        private bool _automaticallyDownloadMarketPrices;
        private bool _colorRetainerList = true;

        private bool _displayCrossCharacter = true;
        private bool _displayTooltip = true;

        private Vector4 _highlightColor = new (0.007f, 0.008f,
            0.007f, 0.212f);

        private Vector4 _retainerListColor = ImGuiColors.HealerGreen;

        private string _highlightWhen = "Always";
        private bool _invertHighlighting = true;
        private bool _invertTabHighlighting = false;

        private bool _isVisible;
        private int _marketRefreshTimeHours = 24;
        private bool _showItemNumberRetainerList = true;

        private Vector4 _tabHighlightColor = new (0.007f, 0.008f,
            0.007f, 1.0f);

        public List<FilterConfiguration> FilterConfigurations = new();

        public Dictionary<ulong, Character> SavedCharacters = new();

        private Dictionary<ulong, HashSet<uint>> _acquiredItems = new();

        [JsonIgnore]
        public Dictionary<ulong, Dictionary<InventoryCategory,List<InventoryItem>>> SavedInventories = new ();

        public bool InventoriesMigrated { get; set; } = false;

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                ConfigurationChanged?.Invoke();
            }
        }

        public int SelectedConfigurationPage { get; set; }
        public bool ShowFilterTab { get; set; } = true;
        public bool SwitchFiltersAutomatically { get; set; } = true;
        private bool _tooltipDisplayAmountOwned = true;
        private bool _tooltipDisplayMarketAveragePrice = true;
        private bool _tooltipDisplayMarketLowestPrice = false;
        public Vector4 HighlightColor
        {
            get => _highlightColor;
            set
            {
                _highlightColor = value;
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
        
        public Dictionary<ulong, HashSet<uint>> AcquiredItems
        {
            get => _acquiredItems ?? new Dictionary<ulong, HashSet<uint>>();
            set => _acquiredItems = value;
        }

        public string? ActiveUiFilter { get; set; } = null;

        [JsonIgnore]
        public bool TetrisEnabled { get; set; } = false;

        [JsonIgnore]
        public string? ActiveBackgroundFilter { get; set; } = null;

        public bool FirstRun { get; set; } = true;
        public int SelectedHelpPage { get; set; }
        #if DEBUG
        public int SelectedDebugPage { get; set; }
        #endif
        public bool AutoSave { get; set; } = true;
        public int AutoSaveMinutes { get; set; } = 10;
        public int InternalVersion { get; set; } = 0;
        public int Version { get; set; }
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
    }
}
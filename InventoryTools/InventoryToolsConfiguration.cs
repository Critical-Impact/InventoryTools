using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using CriticalCommonLib.Models;
using Dalamud.Configuration;
using Dalamud.Logging;
using Dalamud.Plugin;
using InventoryTools.Logic;
using InventoryTools.Resolvers;
using Newtonsoft.Json;

namespace InventoryTools
{
    [Serializable]
    public class InventoryToolsConfiguration : IPluginConfiguration
    {
        public int Version { get; set; }

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
        public bool RestorePreviousFilter { get; set; } = true;

        public Vector4 HighlightColor
        {
            get => _highlightColor;
            set
            {
                _highlightColor = value;
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
        
        public bool AutomaticallyDownloadMarketPrices
        {
            get => _automaticallyDownloadMarketPrices;
            set
            {
                _automaticallyDownloadMarketPrices = value;
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

        public string HighlightWhen
        {
            get => _highlightWhen;
            set
            {
                _highlightWhen = value;
                ConfigurationChanged?.Invoke();
            }
        }

        public enum HighlightWhenEnum
        {
            Always,
            WhenSearching
        }

        public string ActiveUiFilter { get; set; } = null;
        [JsonIgnore]
        public string ActiveBackgroundFilter { get; set; } = null;
        public bool FirstRun { get; set; } = true;
        public int SelectedHelpPage { get; set; }
        #if DEBUG
        public int SelectedDebugPage { get; set; }
        #endif
        public bool AutoSave { get; set; } = true;
        public int AutoSaveMinutes { get; set; } = 10;
        public int InternalVersion { get; set; } = 0;

        public delegate void ConfigurationChangedDelegate();
        public event ConfigurationChangedDelegate ConfigurationChanged; 

        //Game Caches
        public Dictionary<ulong, Dictionary<InventoryCategory,List<InventoryItem>>> SavedInventories = new ();
        
        public Dictionary<ulong, Character> SavedCharacters = new();
        
        public List<FilterConfiguration> FilterConfigurations = new();

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
        
        // Add any other properties or methods here.
        [JsonIgnore] 
        private DalamudPluginInterface pluginInterface;

        private bool _isVisible;
        private Vector4 _highlightColor = new Vector4(0.007f, 0.008f,
            0.007f, 0.212f);
        private Vector4 _tabHighlightColor = new Vector4(0.007f, 0.008f,
            0.007f, 1.0f);

        private bool _displayCrossCharacter = true;
        private bool _colorRetainerList = true;
        private bool _showItemNumberRetainerList = true;
        private bool _displayTooltip = true;
        private bool _invertHighlighting = true;
        private string _highlightWhen = "Always";
        private bool _automaticallyDownloadMarketPrices;
        private int _marketRefreshTimeHours = 24;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }
        
        public void Save()
        {
            PluginLog.Verbose("Saving inventory tools configuration");
            //Save the configuration manually so we can set ReferenceLoopHandling
            File.WriteAllText(pluginInterface.ConfigFile.FullName, JsonConvert.SerializeObject((object) this, Formatting.None, new JsonSerializerSettings()
            {
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                TypeNameHandling = TypeNameHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                ContractResolver = new MinifyResolver()
            }));
        }
    }
}
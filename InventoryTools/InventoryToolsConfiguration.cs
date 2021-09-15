using System;
using System.Collections.Generic;
using System.IO;
using CriticalCommonLib.Models;
using Dalamud.Configuration;
using Dalamud.Logging;
using Dalamud.Plugin;
using InventoryTools.Logic;
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

        public bool DisplayCrossCharacter { get; set; } = true;
        public string ActiveUiFilter { get; set; } = null;
        public string ActiveBackgroundFilter { get; set; } = null;
        public bool FirstRun { get; set; } = true;

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

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
        }

        public void Save()
        {
            PluginLog.Verbose("Saving inventory tools configuration");
            //Save the configuration manually so we can set ReferenceLoopHandling
            File.WriteAllText(pluginInterface.ConfigFile.FullName, JsonConvert.SerializeObject((object) this, Formatting.Indented, new JsonSerializerSettings()
            {
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                TypeNameHandling = TypeNameHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));
        }
    }
}
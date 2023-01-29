using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Resolvers;
using Dalamud.Logging;
using Dispatch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InventoryTools.Logic
{
    public static class ConfigurationManager
    {
        public static InventoryToolsConfiguration Config
        {
            get;
            set;
        } = null!;

        public static string ConfigurationFile
        {
            get
            {
                return Service.Interface.ConfigFile.ToString();
            }
        }
        public static string InventoryFile
        {
            get
            {
                return Path.Join(Service.Interface.ConfigDirectory.FullName, "inventories.json");
            }
        }
        public static void Load()
        {
            PluginLog.Verbose("Loading configuration");
            if (!File.Exists(ConfigurationFile))
            {
                Config = new InventoryToolsConfiguration();
                Config.MarkReloaded();
                return;
            }

            string jsonText = File.ReadAllText(ConfigurationFile);
            var inventoryToolsConfiguration = JsonConvert.DeserializeObject<InventoryToolsConfiguration>(jsonText, new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                ContractResolver = MinifyResolver
            });
            if (inventoryToolsConfiguration == null)
            {
                Config = new InventoryToolsConfiguration();
                Config.MarkReloaded();
                return;
            }
            if (!inventoryToolsConfiguration.InventoriesMigrated)
            {
                PluginLog.Verbose("Migrating inventories");
                var temp =  JObject.Parse(jsonText);
                if (temp.ContainsKey("SavedInventories"))
                {
                    var inventories = temp["SavedInventories"]?.ToObject<Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>>>();
                    inventoryToolsConfiguration.SavedInventories = inventories ??
                                                                   new Dictionary<ulong, Dictionary<InventoryCategory,
                                                                       List<InventoryItem>>>();
                }

                inventoryToolsConfiguration.InventoriesMigrated = true;
            }
            else
            {
                inventoryToolsConfiguration.SavedInventories = LoadSavedInventories() ?? new();
            }
            Config = inventoryToolsConfiguration;
            Config.MarkReloaded();
        }
        public static void LoadFromFile(string file, string inventoryFileName)
        {
            PluginLog.Verbose("Loading configuration");
            if (!File.Exists(file))
            {
                Config = new InventoryToolsConfiguration();
                Config.MarkReloaded();
                return;
            }

            string jsonText = File.ReadAllText(file);
            var inventoryToolsConfiguration = JsonConvert.DeserializeObject<InventoryToolsConfiguration>(jsonText, new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                ContractResolver = MinifyResolver
            });
            if (inventoryToolsConfiguration == null)
            {
                Config = new InventoryToolsConfiguration();
                Config.MarkReloaded();
                return;
            }
            if (!inventoryToolsConfiguration.InventoriesMigrated)
            {
                PluginLog.Verbose("Migrating inventories");
                var temp =  JObject.Parse(jsonText);
                if (temp.ContainsKey("SavedInventories"))
                {
                    var inventories = temp["SavedInventories"]?.ToObject<Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>>>();
                    inventoryToolsConfiguration.SavedInventories = inventories ??
                                                                   new Dictionary<ulong, Dictionary<InventoryCategory,
                                                                       List<InventoryItem>>>();
                }

                inventoryToolsConfiguration.InventoriesMigrated = true;
            }
            else
            {
                inventoryToolsConfiguration.SavedInventories = LoadSavedInventories(inventoryFileName) ?? new();
            }
            Config = inventoryToolsConfiguration;
            Config.MarkReloaded();
        }
        
        public static void Save()
        {
            PluginLog.Verbose("Saving allagan tools configuration");
            try
            {
                File.WriteAllText(ConfigurationFile, JsonConvert.SerializeObject(Config, Formatting.None, new JsonSerializerSettings()
                {
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                    TypeNameHandling = TypeNameHandling.Objects,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                    ContractResolver = MinifyResolver
                }));
                SaveSavedInventories(Config.SavedInventories);
            }
            catch (Exception e)
            {
                PluginLog.Error($"Failed to save allagan tools configuration due to {e.Message}");
            }
        }

        private static SerialQueue _saveQueue = new SerialQueue();
        
        public static void SaveAsync()
        {
            _saveQueue.DispatchAsync(Save);
        }

        public static void ClearQueue()
        {
            _saveQueue.Dispose();
            _saveQueue = null!;
        }

        public static Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>>? LoadSavedInventories(string? fileName = null)
        {
            try
            {
                fileName ??= InventoryFile;
                PluginLog.Verbose("Loading inventories from " + fileName);
                var cacheFile = new FileInfo(fileName);
                string json = File.ReadAllText(cacheFile.FullName, Encoding.UTF8);
                return JsonConvert.DeserializeObject<Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>>>(json, new JsonSerializerSettings()
                {
                    DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                    ContractResolver = MinifyResolver
                });
            }
            catch (Exception e)
            {
                PluginLog.Error("Error while parsing saved saved inventory data, " + e.Message);
                return null;
            }
        }

        public static MinifyResolver MinifyResolver => _minifyResolver ??= new();
        private static MinifyResolver? _minifyResolver;

        public static void SaveSavedInventories(
            Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>> savedInventories)
        {
            Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>> newSavedInventories =
                new Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>>();
            
            foreach (var key in savedInventories.Keys)
            {
                newSavedInventories[key] = new Dictionary<InventoryCategory, List<InventoryItem>>();
                var inventoryDict = savedInventories[key];
                foreach (var key2 in inventoryDict.Keys)
                {
                    var newList = inventoryDict[key2].ToList();
                    newSavedInventories[key][key2] = newList;
                }
            }
            var cacheFile = new FileInfo(InventoryFile);
            PluginLog.Verbose("Saving inventory data");
            try
            {
                File.WriteAllText(cacheFile.FullName, JsonConvert.SerializeObject(newSavedInventories, Formatting.None, new JsonSerializerSettings()
                {
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                    TypeNameHandling = TypeNameHandling.Objects,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                    ContractResolver = MinifyResolver
                }));
            }
            catch (Exception e)
            {
                PluginLog.Error($"Failed to save inventories due to {e.Message}");
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CriticalCommonLib;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CriticalCommonLib.Resolvers;
using Dalamud.Logging;
using Dalamud.Plugin;
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
                return;
            }

            string jsonText = File.ReadAllText(ConfigurationFile);
            var inventoryToolsConfiguration = JsonConvert.DeserializeObject<InventoryToolsConfiguration>(jsonText, new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                ContractResolver = new MinifyResolver()
            });
            if (inventoryToolsConfiguration == null)
            {
                Config = new InventoryToolsConfiguration();
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
        }
        public static void LoadFromFile(string file, string inventoryFileName)
        {
            PluginLog.Verbose("Loading configuration");
            if (!File.Exists(file))
            {
                Config = new InventoryToolsConfiguration();
                return;
            }

            string jsonText = File.ReadAllText(file);
            var inventoryToolsConfiguration = JsonConvert.DeserializeObject<InventoryToolsConfiguration>(jsonText, new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                ContractResolver = new MinifyResolver()
            });
            if (inventoryToolsConfiguration == null)
            {
                Config = new InventoryToolsConfiguration();
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
        }
        
        public static void Save()
        {
            PluginLog.Verbose("Saving inventory tools configuration");
            try
            {
                File.WriteAllText(ConfigurationFile, JsonConvert.SerializeObject(Config, Formatting.None, new JsonSerializerSettings()
                {
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                    TypeNameHandling = TypeNameHandling.Objects,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                    ContractResolver = new MinifyResolver()
                }));
                SaveSavedInventories(Config.SavedInventories);
            }
            catch (Exception e)
            {
                PluginLog.Error($"Failed to save inventory tools configuration due to {e.Message}");
            }
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
                    ContractResolver = new MinifyResolver()
                });
            }
            catch (Exception e)
            {
                PluginLog.Error("Error while parsing saved saved inventory data, " + e.Message);
                return null;
            }
        }

        public static void SaveSavedInventories(
            Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>> savedInventories)
        {
            var cacheFile = new FileInfo(InventoryFile);
            PluginLog.Verbose("Saving inventory data");
            try
            {
                File.WriteAllText(cacheFile.FullName, JsonConvert.SerializeObject((object)savedInventories, Formatting.None, new JsonSerializerSettings()
                {
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                    TypeNameHandling = TypeNameHandling.Objects,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                    ContractResolver = new MinifyResolver()
                }));
            }
            catch (Exception e)
            {
                PluginLog.Error($"Failed to save inventories due to {e.Message}");
            }
        }
    }
}
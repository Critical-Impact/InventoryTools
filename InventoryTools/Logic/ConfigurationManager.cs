using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CriticalCommonLib.Models;
using CriticalCommonLib.Resolvers;
using Dalamud.Logging;
using Dispatch;
using LuminaSupplemental.Excel.Model;
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
                return PluginService.PluginInterfaceService.ConfigFile.ToString();
            }
        }
        public static string InventoryFile
        {
            get
            {
                return Path.Join(PluginService.PluginInterfaceService.ConfigDirectory.FullName, "inventories.json");
            }
        }
        public static string InventoryCsv
        {
            get
            {
                return Path.Join(PluginService.PluginInterfaceService.ConfigDirectory.FullName, "inventories.csv");
            }
        }
        public static string MobSpawnFile
        {
            get
            {
                return Path.Join(PluginService.PluginInterfaceService.ConfigDirectory.FullName, "mob_spawns.csv");
            }
        }
        public static void Load()
        {
            PluginLog.Verbose("Loading configuration");
            Stopwatch loadConfigStopwatch = new Stopwatch();
            loadConfigStopwatch.Start();
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
            loadConfigStopwatch.Stop();
            PluginLog.Verbose("Took " + loadConfigStopwatch.Elapsed.TotalSeconds + " to load configuration.");
            loadConfigStopwatch.Restart();
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
            if (!inventoryToolsConfiguration.InventoriesMigratedToCsv)
            {
                PluginLog.Verbose("Marked inventories to now load from CSV");
                inventoryToolsConfiguration.SavedInventories = LoadInventories(InventoryFile) ?? new();
                inventoryToolsConfiguration.InventoriesMigratedToCsv = true;
            }
            else
            {
                var items = LoadInventoriesFromCsv(out bool success);
                if (success)
                {
                    var parsedItems =
                        new Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>>();
                    foreach (var item in items)
                    {
                        parsedItems.TryAdd(item.RetainerId, new Dictionary<InventoryCategory, List<InventoryItem>>());
                        parsedItems[item.RetainerId].TryAdd(item.SortedCategory, new List<InventoryItem>());
                        parsedItems[item.RetainerId][item.SortedCategory].Add(item);
                    }

                    inventoryToolsConfiguration.SavedInventories = parsedItems;
                }
                else
                {
                    inventoryToolsConfiguration.SavedInventories = new Dictionary<ulong, Dictionary<InventoryCategory,
                        List<InventoryItem>>>();
                }
            }
            loadConfigStopwatch.Stop();
            PluginLog.Verbose("Took " + loadConfigStopwatch.Elapsed.TotalSeconds + " to load inventories.");
            Config = inventoryToolsConfiguration;
            Config.MarkReloaded();
        }
        public static void LoadFromFile(string file, string? inventoryFileName)
        {
            PluginLog.Verbose("Loading configuration");
            Stopwatch loadConfigStopwatch = new Stopwatch();
            loadConfigStopwatch.Start();
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
            loadConfigStopwatch.Stop();
            PluginLog.Verbose("Took " + loadConfigStopwatch.Elapsed.TotalSeconds + " to load configuration.");
            loadConfigStopwatch.Restart();
            if (!inventoryToolsConfiguration.InventoriesMigrated)
            {
                PluginLog.Verbose("Marked inventories to now load from json.");
                var temp =  JObject.Parse(jsonText);
                if (temp.ContainsKey("SavedInventories"))
                {
                    var inventories = temp["SavedInventories"]?.ToObject<Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>>>();
                    inventoryToolsConfiguration.SavedInventories = inventories ??
                                                                   new Dictionary<ulong, Dictionary<InventoryCategory,
                                                                       List<InventoryItem>>>();
                }

                inventoryToolsConfiguration.InventoriesMigrated = true;
                inventoryToolsConfiguration.InventoriesMigratedToCsv = true;
            }
            if (!inventoryToolsConfiguration.InventoriesMigratedToCsv)
            {
                PluginLog.Verbose("Marked inventories to now load from CSV");
                inventoryToolsConfiguration.SavedInventories = LoadInventories(inventoryFileName) ?? new();
                inventoryToolsConfiguration.InventoriesMigratedToCsv = true;
            }
            else
            {
                var items = LoadInventoriesFromCsv(out bool success, inventoryFileName);
                if (success)
                {
                    var parsedItems =
                        new Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>>();
                    foreach (var item in items)
                    {
                        parsedItems.TryAdd(item.RetainerId, new Dictionary<InventoryCategory, List<InventoryItem>>());
                        parsedItems[item.RetainerId].TryAdd(item.SortedCategory, new List<InventoryItem>());
                        parsedItems[item.RetainerId][item.SortedCategory].Add(item);
                    }

                    inventoryToolsConfiguration.SavedInventories = parsedItems;
                }
                else
                {
                    inventoryToolsConfiguration.SavedInventories = new Dictionary<ulong, Dictionary<InventoryCategory,
                                                                       List<InventoryItem>>>();
                }
            }
            loadConfigStopwatch.Stop();
            PluginLog.Verbose("Took " + loadConfigStopwatch.Elapsed.TotalSeconds + " to load inventories.");
            Config = inventoryToolsConfiguration;
            Config.MarkReloaded();
        }
        
        public static void Save()
        {
            Stopwatch loadConfigStopwatch = new Stopwatch();
            loadConfigStopwatch.Start();
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
                loadConfigStopwatch.Stop();
                PluginLog.Verbose("Took " + loadConfigStopwatch.Elapsed.TotalSeconds + " to save configuration.");

                SaveInventories(Config.SavedInventories);
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

        public static Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>>? LoadInventories(string? fileName = null)
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

        public static void SaveInventories(
            Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>> savedInventories)
        {
            PluginLog.Verbose("Saving inventory data");
            Stopwatch loadConfigStopwatch = new Stopwatch();
            loadConfigStopwatch.Start();            
            try
            {
                var items = savedInventories.SelectMany(c => c.Value.SelectMany(d => d.Value)).ToList();
                if (!SaveInventoriesToCsv(items))
                {
                    PluginLog.Error($"Failed to save inventories due to a parsing error.");
                }
            }
            catch (Exception e)
            {
                PluginLog.Error($"Failed to save inventories due to {e.Message}");
            }
            loadConfigStopwatch.Stop();
            PluginLog.Verbose("Took " + loadConfigStopwatch.Elapsed.TotalSeconds + " to save inventories.");

        }

        public static bool SaveInventoriesToCsv(List<InventoryItem> items)
        {
            return CsvLoader.ToCsvRaw<InventoryItem>(items, Path.Join(PluginService.PluginInterfaceService.ConfigDirectory.FullName, "inventories.csv"));
        }

        public static List<InventoryItem> LoadInventoriesFromCsv(out bool success, string? csvPath = null)
        {
            var items = CsvLoader.LoadCsv<InventoryItem>(csvPath ?? InventoryCsv, out success);
            if (success && items != null)
            {
                return items;
            }

            return new List<InventoryItem>();
        }

        public static void Dereference()
        {
            Config = null!;
            _saveQueue = null;
            _minifyResolver = null;

        }
    }
}
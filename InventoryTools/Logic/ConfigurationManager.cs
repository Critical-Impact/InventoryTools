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
        public static string HistoryCsv
        {
            get
            {
                return Path.Join(PluginService.PluginInterfaceService.ConfigDirectory.FullName, "history.csv");
            }
        }
        

        /// <summary>
        /// Loads the primary configuration file
        /// </summary>
        /// <param name="file">An alternate file to load</param>
        public static void Load(string? file = null)
        {
            PluginLog.Verbose("Loading configuration");
            Stopwatch loadConfigStopwatch = new Stopwatch();
            loadConfigStopwatch.Start();
            if (!File.Exists(file ?? ConfigurationFile))
            {
                Config = new InventoryToolsConfiguration();
                Config.MarkReloaded();
                return;
            }
            string jsonText = File.ReadAllText(file ?? ConfigurationFile);
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
            PluginLog.Verbose("Took " + loadConfigStopwatch.Elapsed.TotalSeconds + " to load main configuration file.");
            Config = inventoryToolsConfiguration;
            Config.MarkReloaded();
        }

        public static List<InventoryItem> LoadInventory(string? file = null)
        {
            PluginLog.Verbose("Loading inventory");
            Stopwatch loadConfigStopwatch = new Stopwatch();
            loadConfigStopwatch.Start();

            var inventories = new List<InventoryItem>();


            if (!Config.InventoriesMigrated)
            {
                if (File.Exists(ConfigurationFile))
                {
                    string jsonText = File.ReadAllText(ConfigurationFile);
                    var inventoryToolsConfiguration = JsonConvert.DeserializeObject<InventoryToolsConfiguration>(
                        jsonText, new JsonSerializerSettings()
                        {
                            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                            ContractResolver = MinifyResolver
                        });
                    if (inventoryToolsConfiguration != null)
                    {
                        PluginLog.Verbose("Migrating inventories");
                        var temp = JObject.Parse(jsonText);
                        if (temp.ContainsKey("SavedInventories"))
                        {
                            var savedInventories = temp["SavedInventories"]
                                ?.ToObject<Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>>>();

                            var parsedInventories = savedInventories ??
                                                    new Dictionary<ulong, Dictionary<
                                                        InventoryCategory,
                                                        List<InventoryItem>>>();
                            foreach (var parsedInventory in parsedInventories)
                            {
                                foreach (var category in parsedInventory.Value)
                                {
                                    foreach (var item in category.Value)
                                    {
                                        inventories.Add(item);
                                    }
                                }
                            }
                        }
                    }
                }

                Config.InventoriesMigrated = true;
            }
            else if (!Config.InventoriesMigratedToCsv)
            {
                PluginLog.Verbose("Marked inventories to now load from CSV");
                var parsedInventories = LoadInventoriesJson(InventoryFile) ?? new();
                foreach (var parsedInventory in parsedInventories)
                {
                    foreach (var category in parsedInventory.Value)
                    {
                        foreach (var item in category.Value)
                        {
                            inventories.Add(item);
                        }
                    }
                }
                Config.InventoriesMigratedToCsv = true;
            }
            else
            {
                inventories = LoadInventoriesFromCsv(out bool success);
            }
            loadConfigStopwatch.Stop();
            PluginLog.Verbose("Took " + loadConfigStopwatch.Elapsed.TotalSeconds + " to load inventories.");
            loadConfigStopwatch.Restart();
            return inventories;
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

        [Obsolete]
        public static Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>>? LoadInventoriesJson(string? fileName = null)
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

        [Obsolete]
        public static void SaveInventoriesToJson(
            Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>> savedInventories)
        {
            PluginLog.Verbose("Saving inventory data");
            Stopwatch loadConfigStopwatch = new Stopwatch();
            loadConfigStopwatch.Start();            
            try
            {
                var items = savedInventories.SelectMany(c => c.Value.SelectMany(d => d.Value)).ToList();
                if (!SaveInventories(items))
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

        public static bool SaveInventories(List<InventoryItem> items)
        {
            return CsvLoader.ToCsvRaw<InventoryItem>(items, Path.Join(PluginService.PluginInterfaceService.ConfigDirectory.FullName, "inventories.csv"));
        }
        
        public static List<InventoryItem> LoadInventoriesFromCsv(out bool success, string? csvPath = null)
        {
            var inventoryCsv = csvPath ?? InventoryCsv;
            if (Path.Exists(inventoryCsv))
            {
                try
                {
                    var items = CsvLoader.LoadCsv<InventoryItem>(inventoryCsv);
                    success = true;
                    return items;
                }
                catch (Exception e)
                {
                    success = false;
                    PluginLog.Error("Failed to load inventories from CSV");
                    PluginLog.Error(e.Message);
                }
            }
            else
            {
                success = true;
                PluginLog.Verbose("Not loading inventories, file does not exist.");
            }


            return new List<InventoryItem>();
        }
        
        public static List<InventoryChange> LoadHistoryFromCsv(out bool success, string? csvPath = null)
        {
            var historyCsv = csvPath ?? HistoryCsv;
            if (Path.Exists(historyCsv))
            {
                try
                {
                    var items = CsvLoader.LoadCsv<InventoryChange>(historyCsv);
                    success = true;
                    return items;
                }
                catch (Exception e)
                {
                    success = false;
                    PluginLog.Error("Failed to load history from CSV");
                    PluginLog.Error(e.Message);
                }
            }
            else
            {
                success = true;
                PluginLog.Verbose("Not loading history, file does not exist.");
            }


            return new List<InventoryChange>();
        }
        
        public static bool SaveHistory(List<InventoryChange> changes)
        {
            return CsvLoader.ToCsvRaw<InventoryChange>(changes, Path.Join(PluginService.PluginInterfaceService.ConfigDirectory.FullName, "history.csv"));
        }

        public static void Dereference()
        {
            Config = null!;
            _saveQueue = null;
            _minifyResolver = null;

        }
    }
}
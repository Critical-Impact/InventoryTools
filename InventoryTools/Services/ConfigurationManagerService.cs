using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Models;
using CriticalCommonLib.Resolvers;
using DalaMock.Shared.Interfaces;
using Dalamud.Plugin.Services;
using InventoryTools.Converters;
using LuminaSupplemental.Excel.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Task = System.Threading.Tasks.Task;

namespace InventoryTools.Services
{
    using Dalamud.Plugin;

    public class ConfigurationManagerService : BackgroundService
    {
        public ILogger<ConfigurationManagerService> Logger { get; }

        public delegate void ConfigurationChangedDelegate();
        private readonly IFramework _framework;
        private bool _configurationLoaded = false;

        public event ConfigurationChangedDelegate? ConfigurationChanged;

        public ConfigurationManagerService(IFramework framework, IDalamudPluginInterface pluginInterfaceService, ILogger<ConfigurationManagerService> logger, IBackgroundTaskQueue saveQueue)
        {
            Logger = logger;
            _pluginInterfaceService = pluginInterfaceService;
            _saveQueue = saveQueue;
            _framework = framework;
            _framework.Update += OnUpdate;
        }

        private void OnUpdate(IFramework framework)
        {
            if (_configurationLoaded)
            {
                if (Config.IsDirty)
                {
                    Config.IsDirty = false;
                    ConfigurationChanged?.Invoke();
                    SaveAsync();
                }
            }
        }

        public InventoryToolsConfiguration Config
        {
            get;
            set;
        } = null!;

        public string ConfigurationFile
        {
            get
            {
                return _pluginInterfaceService.ConfigFile.ToString();
            }
        }
        public string InventoryFile
        {
            get
            {
                return Path.Join(_pluginInterfaceService.ConfigDirectory.FullName, "inventories.json");
            }
        }
        public string InventoryCsv
        {
            get
            {
                return Path.Join(_pluginInterfaceService.ConfigDirectory.FullName, "inventories.csv");
            }
        }
        public string MobSpawnFile
        {
            get
            {
                return Path.Join(_pluginInterfaceService.ConfigDirectory.FullName, "mob_spawns.csv");
            }
        }
        public string HistoryCsv
        {
            get
            {
                return Path.Join(_pluginInterfaceService.ConfigDirectory.FullName, "history.csv");
            }
        }

        public InventoryToolsConfiguration GetConfig()
        {
            return Config;
        }


        /// <summary>
        /// Loads the primary configuration file
        /// </summary>
        /// <param name="file">An alternate file to load</param>
        public void Load(string? file = null)
        {
            Logger.LogTrace("Loading configuration");
            Stopwatch loadConfigStopwatch = new Stopwatch();
            loadConfigStopwatch.Start();
            if (!File.Exists(file ?? ConfigurationFile))
            {
                Config = new InventoryToolsConfiguration();
                Config.MarkReloaded();
                _configurationLoaded = true;
                return;
            }
            string jsonText = File.ReadAllText(file ?? ConfigurationFile);
            var inventoryToolsConfiguration = JsonConvert.DeserializeObject<InventoryToolsConfiguration>(jsonText, new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>()
                {
                  new ColumnConverter()
                },
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                TypeNameHandling = TypeNameHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                ContractResolver = MinifyResolver
            });
            if (inventoryToolsConfiguration == null)
            {
                Config = new InventoryToolsConfiguration();
                Config.MarkReloaded();
                _configurationLoaded = true;
                return;
            }
            loadConfigStopwatch.Stop();
            Logger.LogTrace("Took " + loadConfigStopwatch.Elapsed.TotalSeconds + " to load main configuration file.");
            Config = inventoryToolsConfiguration;
            Config.MarkReloaded();
            _configurationLoaded = true;
        }

        public List<InventoryItem> LoadInventory(string? file = null)
        {
            Logger.LogTrace("Loading inventory");
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
                            Converters = new List<JsonConverter>()
                            {
                                new ColumnConverter()
                            },
                            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                            ContractResolver = MinifyResolver
                        });
                    if (inventoryToolsConfiguration != null)
                    {
                        Logger.LogTrace("Migrating inventories");
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
                Logger.LogTrace("Marked inventories to now load from CSV");
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
            Logger.LogTrace("Took " + loadConfigStopwatch.Elapsed.TotalSeconds + " to load inventories.");
            loadConfigStopwatch.Restart();
            return inventories;
        }

        public void Save()
        {
            Stopwatch loadConfigStopwatch = new Stopwatch();
            loadConfigStopwatch.Start();
            Logger.LogTrace("Saving allagan tools configuration");
            try
            {
                File.WriteAllText(ConfigurationFile, JsonConvert.SerializeObject(Config, Formatting.None, new JsonSerializerSettings()
                {
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                    TypeNameHandling = TypeNameHandling.None,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                    ContractResolver = MinifyResolver
                }));
                loadConfigStopwatch.Stop();
                Logger.LogTrace("Took " + loadConfigStopwatch.Elapsed.TotalSeconds + " to save configuration.");
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to save allagan tools configuration due to {e.Message}");
            }
        }

        private void SaveAsync()
        {
            _saveQueue.QueueBackgroundWorkItemAsync(token => Task.Run(Save, token));
        }


        /// <summary>
        /// Load a inventories json file, this is no longer the preferred format to store inventories in.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>>? LoadInventoriesJson(string? fileName = null)
        {
            try
            {
                fileName ??= InventoryFile;
                Logger.LogTrace("Loading inventories from " + fileName);
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
                Logger.LogError("Error while parsing saved saved inventory data, " + e.Message);
                return null;
            }
        }

        public MinifyResolver MinifyResolver => _minifyResolver ??= new();
        private MinifyResolver? _minifyResolver;
        private readonly IDalamudPluginInterface _pluginInterfaceService;
        private readonly IBackgroundTaskQueue _saveQueue;

        [Obsolete]
        public void SaveInventoriesToJson(
            Dictionary<ulong, Dictionary<InventoryCategory, List<InventoryItem>>> savedInventories)
        {
            Logger.LogTrace("Saving inventory data");
            Stopwatch loadConfigStopwatch = new Stopwatch();
            loadConfigStopwatch.Start();
            try
            {
                var items = savedInventories.SelectMany(c => c.Value.SelectMany(d => d.Value)).ToList();
                if (!SaveInventories(items))
                {
                    Logger.LogError($"Failed to save inventories due to a parsing error.");
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to save inventories due to {e.Message}");
            }
            loadConfigStopwatch.Stop();
            Logger.LogTrace("Took " + loadConfigStopwatch.Elapsed.TotalSeconds + " to save inventories.");

        }

        public bool SaveInventories(List<InventoryItem> items)
        {
            return CsvLoader.ToCsvRaw<InventoryItem>(items, Path.Join(_pluginInterfaceService.ConfigDirectory.FullName, "inventories.csv"));
        }

        public List<InventoryItem> LoadInventoriesFromCsv(out bool success, string? csvPath = null)
        {
            var inventoryCsv = csvPath ?? InventoryCsv;
            if (Path.Exists(inventoryCsv))
            {
                try
                {
                    var items = CsvLoader.LoadCsv<InventoryItem>(inventoryCsv, out _);
                    success = true;
                    return items;
                }
                catch (Exception e)
                {
                    success = false;
                    Logger.LogError("Failed to load inventories from CSV");
                    Logger.LogError(e.Message);
                }
            }
            else
            {
                success = true;
                Logger.LogTrace("Not loading inventories, file does not exist.");
            }


            return new List<InventoryItem>();
        }

        public List<InventoryChange> LoadHistoryFromCsv(out bool success, string? csvPath = null)
        {
            var historyCsv = csvPath ?? HistoryCsv;
            if (Path.Exists(historyCsv))
            {
                try
                {
                    var items = CsvLoader.LoadCsv<InventoryChange>(historyCsv, out _);
                    success = true;
                    return items;
                }
                catch (Exception e)
                {
                    success = false;
                    Logger.LogError("Failed to load history from CSV");
                    Logger.LogError(e.Message);
                }
            }
            else
            {
                success = true;
                Logger.LogTrace("Not loading history, file does not exist.");
            }


            return new List<InventoryChange>();
        }

        public bool SaveHistory(List<InventoryChange> changes)
        {
            return CsvLoader.ToCsvRaw<InventoryChange>(changes, Path.Join(_pluginInterfaceService.ConfigDirectory.FullName, "history.csv"));
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await BackgroundProcessing(stoppingToken);
        }

        private async Task BackgroundProcessing(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem =
                    await _saveQueue.DequeueAsync(stoppingToken);

                try
                {
                    await workItem(stoppingToken);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex,
                        "Error occurred executing {WorkItem}.", nameof(workItem));
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            Logger.LogTrace("Configuration manager save queue is stopping.");
            Save();
            await base.StopAsync(stoppingToken);
        }

        private bool disposed = false;
        public override void Dispose()
        {
            _framework.Update -= OnUpdate;
            disposed = true;
        }
    }
}
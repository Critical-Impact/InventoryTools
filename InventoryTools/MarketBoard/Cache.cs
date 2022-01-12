using CriticalCommonLib.Services;
using Dalamud.Logging;
using InventoryTools.Resolvers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace InventoryTools.MarketBoard
{
    internal class CacheEntry
    {
        public uint ItemId { get; set; }
        public Rootobject Data { get; set; }
        public DateTime LastUpdate { get; set; } = DateTime.Now;

    }

    internal class Cache
    {
        private static readonly Dictionary<uint, CacheEntry> MBCache = new Dictionary<uint, CacheEntry>();
        private static readonly Stopwatch StoreTimer = new();
        private static bool IsLoaded = false;
        private static InventoryToolsConfiguration _configuration;

        internal static void LoadCache(InventoryToolsConfiguration configuration)
        {
            try
            {
                var cacheFile = new FileInfo(Service.PluginInterface.ConfigDirectory.FullName + "/universalis.json");
                string json = File.ReadAllText(cacheFile.FullName, Encoding.UTF8);
                var oldCache = JsonConvert.DeserializeObject<Dictionary<uint, CacheEntry>>(json);
                if (oldCache != null)
                    foreach (var item in oldCache)
                    {
                        if (item.Value != null && item.Value.Data != null)
                        {
                            MBCache[item.Key] = item.Value;
                        }
                    }
            }
            catch (Exception e)
            {
                PluginLog.Verbose("Error while parsing saved universalis data, " + e.Message);
            }

            _configuration = configuration;

            IsLoaded = true;
        }


        internal static void StoreCache()
        {
            if (StoreTimer.IsRunning && StoreTimer.Elapsed < TimeSpan.FromSeconds(120) || !IsLoaded)
            {
                return;
            }

            if (!StoreTimer.IsRunning)
            {
                StoreTimer.Start();
            }

            var cacheFile = new FileInfo(Service.PluginInterface.ConfigDirectory.FullName + "/universalis.json");

            PluginLog.Verbose("Saving Universalis Cache");
            File.WriteAllText(cacheFile.FullName, JsonConvert.SerializeObject((object)MBCache, Formatting.None, new JsonSerializerSettings()
            {
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                TypeNameHandling = TypeNameHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                ContractResolver = new MinifyResolver()
            }));

            StoreTimer.Restart();
        }

        private static readonly Stopwatch CheckTimer = new();
        internal static void CheckCache()
        {
            if (CheckTimer.IsRunning && CheckTimer.Elapsed < TimeSpan.FromSeconds(300))
            {
                return;
            }

            if (!CheckTimer.IsRunning)
            {
                CheckTimer.Start();
            }

            PluginLog.Verbose("Checking Cache...");
            foreach (var item in MBCache)
            {
                if (item.Value == null || item.Value.Data == null)
                {
                    PluginLog.Verbose($"{item} is null");
                    GetData(item.Key, fromCheck: true);
                }
                else
                {
                    var now = DateTime.Now;
                    var diff = now - item.Value.LastUpdate;
                    if (diff.TotalHours > _configuration.MarketRefreshTimeHours)
                    {
                        GetData(item.Key, fromCheck: true);
                    }
                }
            }

            StoreCache();
            CheckTimer.Restart();
        }

        internal static Rootobject GetData(uint itemID, bool fromCheck = false, bool forceCheck = false)
        {
            if (!fromCheck && !forceCheck)
            {
                CheckCache();
            }

            if (ExcelCache.GetItem(itemID).IsUntradable)
            {
                return new Rootobject();
            }

            if (!fromCheck && MBCache.ContainsKey(itemID) && !forceCheck)
            {
                return MBCache[itemID].Data;
            }

            if (!_configuration.AutomaticallyDownloadMarketPrices && !forceCheck)
            {
                return new Rootobject();
            }

            var data = Universalis.GetMarketBoardData(itemID);

            return data;
        }

        internal static void UpdateEntry(uint itemId, Rootobject rootobject)
        {
            var entry = new CacheEntry();
            entry.ItemId = itemId;
            entry.Data = rootobject;
            MBCache[itemId] = entry;
            StoreCache();
        }
    }
}

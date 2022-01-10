using Dalamud.Logging;
using InventoryTools.Resolvers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        private static readonly TimeSpan MaxDiff = new TimeSpan(24, 0, 0);
        private static readonly Stopwatch StoreTimer = new();
        private static bool IsLoaded = false;

        internal static void LoadCache()
        {
            try
            {
                var cacheFile = new FileInfo(Service.PluginInterface.ConfigDirectory.FullName + "/universalis.json");
                string json = File.ReadAllText(cacheFile.FullName, Encoding.UTF8);

                var oldCache = JsonConvert.DeserializeObject<Dictionary<uint, CacheEntry>>(json);
                foreach (var item in oldCache)
                {
                    if (item.Value != null && item.Value.Data != null)
                    {
                        MBCache[item.Key] = item.Value;
                    }
                }
            }
            catch (Exception)
            {
            }

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
                    GetData(item.Key, true);
                }
                else
                {
                    var now = DateTime.Now;
                    var diff = now - item.Value.LastUpdate;
                    if (diff > MaxDiff)
                    {
                        GetData(item.Key, true);
                    }
                }
            }

            StoreCache();
            CheckTimer.Restart();
        }

        internal static Rootobject GetData(uint itemID, bool fromCheck = false)
        {
            if (!fromCheck)
            {
                CheckCache();
            }

            if (!fromCheck && MBCache.ContainsKey(itemID))
            {
                return MBCache[itemID].Data;
            }

            var data = Universalis.GetMarketBoardData(itemID);

            var entry = new CacheEntry();
            entry.ItemId = itemID;
            entry.Data = data;
            MBCache[itemID] = entry;

            StoreCache();
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

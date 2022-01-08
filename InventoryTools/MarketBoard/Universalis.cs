using CriticalCommonLib.Models;
using Dalamud.Logging;
using Dispatch;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryTools.MarketBoard
{

    internal class Universalis
    {
        private static SerialQueue taskQueue = new SerialQueue();
        private static Dictionary<InventoryItem, Rootobject> Cache = new Dictionary<InventoryItem, Rootobject>();

        internal static void Dispose()
        {
            taskQueue.Dispose();
        }

        internal static Rootobject GetMarketBoardData(InventoryItem item, string datacenter)
        {
            if (item == null)
            {
                return new Rootobject();
            }


            if (Cache.ContainsKey(item))
            {
                return Cache[item];
            }


            Cache[item] = null;


            taskQueue.DispatchAsync(() =>
                {
                    string url = $"https://universalis.app/api/{datacenter}/{item.ItemId}";
                    PluginLog.LogVerbose(url);

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.AutomaticDecompression = DecompressionMethods.GZip;

                    Rootobject listing = new Rootobject();

                    using (WebResponse response = request.GetResponse())
                    {
                        try
                        {
                            HttpWebResponse webresponse = (HttpWebResponse)response;

                            if (webresponse.StatusCode == HttpStatusCode.TooManyRequests)
                            {
                                PluginLog.Warning("Universalis: too many requests!");
                                // sleep for 1 minute if too many requests
                                Thread.Sleep(60000);

                                request = (HttpWebRequest)WebRequest.Create(url);
                                webresponse = (HttpWebResponse)request.GetResponse();
                            }
                            else
                            {
                                PluginLog.LogVerbose($"Universalis: {webresponse.StatusCode}");
                            }

                            var reader = new StreamReader(webresponse.GetResponseStream());
                            var value = reader.ReadToEnd();
                            PluginLog.LogVerbose(value);
                            listing = JsonConvert.DeserializeObject<Rootobject>(value);


                            if (listing != null)
                            {
                                Cache[item] = listing;
                            }

                            // Simple way to prevent too many requests
                            Thread.Sleep(500);
                        }
                        catch (Exception ex)
                        {
                            PluginLog.Debug(ex.ToString());
                        }
                    }

                });


            return null;
        }

    }


    public class Rootobject
    {
        public int itemID { get; set; }
        public int worldID { get; set; }
        public long lastUploadTime { get; set; }
        public Listing[] listings { get; set; }
        public Recenthistory[] recentHistory { get; set; }
        public float currentAveragePrice { get; set; }
        public float currentAveragePriceNQ { get; set; }
        public float currentAveragePriceHQ { get; set; }
        public float regularSaleVelocity { get; set; }
        public float nqSaleVelocity { get; set; }
        public float hqSaleVelocity { get; set; }
        public float averagePrice { get; set; }
        public float averagePriceNQ { get; set; }
        public float averagePriceHQ { get; set; }
        public float minPrice { get; set; }
        public float minPriceNQ { get; set; }
        public float minPriceHQ { get; set; }
        public float maxPrice { get; set; }
        public float maxPriceNQ { get; set; }
        public float maxPriceHQ { get; set; }
        public Stacksizehistogram stackSizeHistogram { get; set; }
        public Stacksizehistogramnq stackSizeHistogramNQ { get; set; }
        public Stacksizehistogramhq stackSizeHistogramHQ { get; set; }
        public string worldName { get; set; }
    }

    public class Stacksizehistogram
    {
        public int _1 { get; set; }
    }

    public class Stacksizehistogramnq
    {
        public int _1 { get; set; }
    }

    public class Stacksizehistogramhq
    {
        public int _1 { get; set; }
    }

    public class Listing
    {
        public int lastReviewTime { get; set; }
        public int pricePerUnit { get; set; }
        public int quantity { get; set; }
        public int stainID { get; set; }
        public string creatorName { get; set; }
        public object creatorID { get; set; }
        public bool hq { get; set; }
        public bool isCrafted { get; set; }
        public object listingID { get; set; }
        public object[] materia { get; set; }
        public bool onMannequin { get; set; }
        public int retainerCity { get; set; }
        public string retainerID { get; set; }
        public string retainerName { get; set; }
        public string sellerID { get; set; }
        public int total { get; set; }
    }

    public class Recenthistory
    {
        public bool hq { get; set; }
        public int pricePerUnit { get; set; }
        public int quantity { get; set; }
        public int timestamp { get; set; }
        public string buyerName { get; set; }
        public int total { get; set; }
    }

}

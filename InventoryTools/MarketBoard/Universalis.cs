using CriticalCommonLib.Models;
using Dalamud.Logging;
using Dispatch;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryTools.MarketBoard
{

    internal class Universalis
    {
        private static SerialQueue taskQueue = new SerialQueue();
        private static List<IDisposable> disposables = new List<IDisposable>();
        

        internal static void Dispose()
        {
            taskQueue.Dispose();
        }
        internal static Rootobject GetMarketBoardData(InventoryItem item)
        {
            if (!item.CanBeBought)
            {
                return new Rootobject();
            }

            return GetMarketBoardData(item.ItemId);
        }

        internal static Rootobject GetMarketBoardData(uint itemId)
        {
            if (Service.ClientState != null && Service.ClientState.IsLoggedIn && Service.ClientState.LocalPlayer != null)
            {
                string datacenter = Service.ClientState.LocalPlayer.CurrentWorld.GameData.Name.RawString;

                var dispatch = taskQueue.DispatchAsync(() =>
                    {
                        string url = $"https://universalis.app/api/{datacenter}/{itemId}";
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
                                    if (listing.listings != null && listing.listings.Length > 0)
                                    {
                                        var list = new List<Listing>(listing.listings);

                                        var listings = list.OrderBy(item => item.pricePerUnit).ToList();
                                        int counter = 0;
                                        int counterHQ = 0;
                                        double sumPricePerUnit = 0;
                                        double sumPricePerUnitHQ = 0;
                                        for (int i = 0; i < listings.Count && i < 10; i++)
                                        {
                                            if (listings[i].hq)
                                            {
                                                var pricePerUnit = listings[i].pricePerUnit;
                                                if (pricePerUnit > (sumPricePerUnitHQ / counterHQ) * 10)
                                                {
                                                    continue;
                                                }

                                                counterHQ++;
                                                sumPricePerUnitHQ += pricePerUnit;
                                            }
                                            else
                                            {
                                                var pricePerUnit = listings[i].pricePerUnit;
                                                if (pricePerUnit > (sumPricePerUnit / counter) * 10)
                                                {
                                                    continue;
                                                }

                                                counter++;
                                                sumPricePerUnit += pricePerUnit;
                                            }
                                        }

                                        if (counter != 0)
                                        {
                                            listing.calculcatedPrice = (sumPricePerUnit / counter).ToString("0.00");
                                        }
                                        else
                                        {
                                            listing.calculcatedPrice = "N/A";
                                        }

                                        if (counterHQ != 0)
                                        {
                                            listing.calculcatedPriceHQ = (sumPricePerUnitHQ / counterHQ).ToString("0.00");
                                        }
                                        else
                                        {
                                            listing.calculcatedPriceHQ = "N/A";
                                        }
                                    }
                                    PluginLog.Verbose("Universalis: item updated");
                                    Cache.UpdateEntry(itemId, listing);
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
                disposables.Add(dispatch);
            }

            return null;
        }

        private static void CheckQueue()
        {
            Task checkTask = new Task(async () =>
                {
                    await Universalis.taskQueue;

                });
            checkTask.ContinueWith(task =>
            {
                Cache.CheckCache();
            });

        }
    }


    public class Rootobject
    {
        
        public int itemID { internal get; set; }
        
        public int worldID { internal  get; set; }
        
        public long lastUploadTime { internal get; set; }
        
        public Listing[] listings { internal get; set; }
        
        public Recenthistory[] recentHistory { internal get; set; }
        
        public float currentAveragePrice { internal get; set; }
        
        public float currentAveragePriceNQ { internal get; set; }
        
        public float currentAveragePriceHQ { internal get; set; }
        
        public float regularSaleVelocity { internal get; set; }
        
        public float nqSaleVelocity { internal get; set; }
        
        public float hqSaleVelocity { internal get; set; }
        
        public float averagePrice { internal get; set; }
        
        public float averagePriceNQ { internal get; set; }
        
        public float averagePriceHQ { internal get; set; }
        
        public float minPrice { internal get; set; }
        
        public float minPriceNQ { internal get; set; }
        
        public float minPriceHQ { internal get; set; }
        
        public float maxPrice { internal get; set; }
        
        public float maxPriceNQ { internal get; set; }
        
        public float maxPriceHQ { internal get; set; }
        
        public Stacksizehistogram stackSizeHistogram { internal get; set; }
        
        public Stacksizehistogramnq stackSizeHistogramNQ { internal get; set; }
        
        public Stacksizehistogramhq stackSizeHistogramHQ { internal get; set; }
        
        public string worldName { internal get; set; }
        public string calculcatedPrice { get; set; } = "0";
        public string calculcatedPriceHQ { get; set; } = "0";
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

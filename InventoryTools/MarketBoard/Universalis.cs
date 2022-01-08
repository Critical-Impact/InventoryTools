using CriticalCommonLib.Models;
using Dalamud.Logging;
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

        private static LimitedConcurrencyLevelTaskScheduler taskScheduler = new LimitedConcurrencyLevelTaskScheduler(1);
        private static TaskFactory taskFactory = new TaskFactory(taskScheduler);
        private static CancellationTokenSource cts = new CancellationTokenSource();

        private static Dictionary<InventoryItem, Rootobject> Cache = new Dictionary<InventoryItem, Rootobject>();

        internal static void Dispose()
        {
            cts.Dispose();
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

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            taskFactory.StartNew(() =>
            {
                string url = $"https://universalis.app/api/{datacenter}/{item.ItemId}";
                PluginLog.LogVerbose(url);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip;

                Rootobject listing = new Rootobject();

                Task<WebResponse> response = request.GetResponseAsync();
                response.ContinueWith(t =>
                {
                    try
                    {
                        var reader = new StreamReader(t.Result.GetResponseStream());
                        var value = reader.ReadToEnd();
                        PluginLog.LogVerbose(value);
                        listing = JsonConvert.DeserializeObject<Rootobject>(value);

                        if (listing != null)
                        {
                            Cache[item] = listing;
                        }

                        // Simple way to prevent too many requests
                        Thread.Sleep(1500);
                    }
                    catch (Exception ex)
                    {

                    }

                });
            }, cts.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

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




    // Code from https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskscheduler?view=net-6.0

    // Provides a task scheduler that ensures a maximum concurrency level while
    // running on top of the thread pool.
    public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
    {
        // Indicates whether the current thread is processing work items.
        [ThreadStatic]
        private static bool _currentThreadIsProcessingItems;

        // The list of tasks to be executed
        private readonly LinkedList<Task> _tasks = new LinkedList<Task>(); // protected by lock(_tasks)

        // The maximum concurrency level allowed by this scheduler.
        private readonly int _maxDegreeOfParallelism;

        // Indicates whether the scheduler is currently processing work items.
        private int _delegatesQueuedOrRunning = 0;

        // Creates a new instance with the specified degree of parallelism.
        public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1) throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        // Queues a task to the scheduler.
        protected sealed override void QueueTask(Task task)
        {
            // Add the task to the list of tasks to be processed.  If there aren't enough
            // delegates currently queued or running to process tasks, schedule another.
            lock (_tasks)
            {
                _tasks.AddLast(task);
                if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                {
                    ++_delegatesQueuedOrRunning;
                    NotifyThreadPoolOfPendingWork();
                }
            }
        }

        // Inform the ThreadPool that there's work to be executed for this scheduler.
        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
                // Note that the current thread is now processing work items.
                // This is necessary to enable inlining of tasks into this thread.
                _currentThreadIsProcessingItems = true;
                try
                {
                    // Process all available items in the queue.
                    while (true)
                    {
                        Task item;
                        lock (_tasks)
                        {
                            // When there are no more items to be processed,
                            // note that we're done processing, and get out.
                            if (_tasks.Count == 0)
                            {
                                --_delegatesQueuedOrRunning;
                                break;
                            }

                            // Get the next item from the queue
                            item = _tasks.First.Value;
                            _tasks.RemoveFirst();
                        }

                        // Execute the task we pulled out of the queue
                        base.TryExecuteTask(item);
                    }
                }
                // We're done processing items on the current thread
                finally { _currentThreadIsProcessingItems = false; }
            }, null);
        }

        // Attempts to execute the specified task on the current thread.
        protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            // If this thread isn't already processing a task, we don't support inlining
            if (!_currentThreadIsProcessingItems) return false;

            // If the task was previously queued, remove it from the queue
            if (taskWasPreviouslyQueued)
                // Try to run the task.
                if (TryDequeue(task))
                    return base.TryExecuteTask(task);
                else
                    return false;
            else
                return base.TryExecuteTask(task);
        }

        // Attempt to remove a previously scheduled task from the scheduler.
        protected sealed override bool TryDequeue(Task task)
        {
            lock (_tasks) return _tasks.Remove(task);
        }

        // Gets the maximum concurrency level supported by this scheduler.
        public sealed override int MaximumConcurrencyLevel { get { return _maxDegreeOfParallelism; } }

        // Gets an enumerable of the tasks currently scheduled on this scheduler.
        protected sealed override IEnumerable<Task> GetScheduledTasks()
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(_tasks, ref lockTaken);
                if (lockTaken) return _tasks;
                else throw new NotSupportedException();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_tasks);
            }
        }
    }

}

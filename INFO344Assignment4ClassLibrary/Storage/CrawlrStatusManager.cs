using INFO344Assignment4ClassLibrary.Helpers;
using INFO344Assignment4ClassLibrary.Storage.Entities;
using Microsoft.WindowsAzure.Storage.Table;
using System.Diagnostics;

namespace INFO344Assignment4ClassLibrary.Storage
{
    /// <summary>
    ///     Helper object for managing the status of a given crawler (webCrawlr). Maintains
    ///     RAM and CPU PerformanceCounters. UpdateCrawlrStatus(...) takes a status as a string,
    ///     then posts the status with CPU counters to the crawler's status table; UpdateQueueSize(...)
    ///     takes two ints and updates the status table with the current size of the XML and URL queue.
    /// </summary>
    public class CrawlrStatusManager
    {
        private PerformanceCounter _memoryFree;
        private PerformanceCounter _cpuTime;

        public CrawlrStatusManager()
        {
            this._memoryFree = new PerformanceCounter("Memory", "Available MBytes");
            this._cpuTime = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        }

        public void UpdateCrawlrStatus(string currentStatus, CrawlrDataHelper data, CrawlrStorageManager storage)
        {
            WorkerRoleStatus currStatusEntity;
            if (currentStatus == "CLEAR")
            {
                currStatusEntity = new WorkerRoleStatus(
                    currentStatus,
                    (int)_cpuTime.NextValue(),
                    (int)_memoryFree.NextValue(),
                    0,
                    new System.Collections.Generic.Queue<string>()
                );
            }
            else
            {
                currStatusEntity = new WorkerRoleStatus(
                    currentStatus,
                    (int)_cpuTime.NextValue(),
                    (int)_memoryFree.NextValue(),
                    data.NumUrlsCrawled,
                    data.LastTenUrls
                );
            }

            TableOperation insertStatus = TableOperation.InsertOrReplace(currStatusEntity);
            storage.StatusTable.Execute(insertStatus);

        }

        public void UpdateQueueSize(CrawlrStorageManager storage, int xmlQueueSize, int urlQueueSize)
        {
            CrawlrQueueSize newSize = new CrawlrQueueSize(xmlQueueSize, urlQueueSize);
            TableOperation insertQueueSize = TableOperation.InsertOrReplace(newSize);
            storage.StatusTable.Execute(insertQueueSize);
        }
    }
}

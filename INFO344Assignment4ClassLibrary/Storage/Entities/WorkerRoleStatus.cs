using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;

namespace INFO344Assignment4ClassLibrary.Storage.Entities
{
    public class WorkerRoleStatus : TableEntity
    {
        public string CurrStatus { get; set; }
        public int CpuUsed { get; set; }
        public int RamAvailable { get; set; }
        public int NumUrlsCrawled { get; set; }
        public string LastTenUrls { get; set; }

        public WorkerRoleStatus(string currStatus, int cpuUsed, int ramAvailable, int numUrlsCrawled, Queue<string> lastTenUrls)
        {
            this.PartitionKey = "WorkerRole Status";
            this.RowKey = "WorkerRole Status";

            this.CurrStatus = currStatus;
            this.CpuUsed = cpuUsed;
            this.RamAvailable = ramAvailable;
            this.NumUrlsCrawled = numUrlsCrawled;
            this.LastTenUrls = LastTenUrlsToString(lastTenUrls);
        }

        public WorkerRoleStatus()
        {

        }

        public string LastTenUrlsToString(Queue<string> urls)
        {
            if (urls.Count == 0)
            {
                return "None";
            }
            string result = "";
            int numUrlsInStack = urls.Count;
            foreach (var url in urls)
            {
                result += url;
                result += ",";
            }

            return result.TrimEnd(',');
        }
    }

}

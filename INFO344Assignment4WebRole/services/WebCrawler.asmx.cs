using INFO344Assignment4ClassLibrary.Storage;
using INFO344Assignment4ClassLibrary.Storage.Entities;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Script.Services;
using System.Web.Services;

namespace INFO344Assignment4WebRole.services
{
    /// <summary>
    ///     ASMX Script Service for webCrawlr, a URL crawler tailored towards crawling cnn.com.
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [ScriptService]
    public class WebCrawler : WebService
    {
        // Initialize quick access to relevant Azure Storage, as well as store last given robots.txt URL. 
        private static CrawlrStorageManager _storageManager
            = new CrawlrStorageManager(ConfigurationManager.AppSettings["StorageConnectionString"]);
        private static string _robotsTxtUrl;

        /// <summary>
        ///     Give the relevant command to the worker role to start crawling the given robots.txt
        /// </summary>
        /// <param name="robotsTxtUrl">
        ///     robots.txt URL to crawl as string.
        /// </param>
        /// <returns>
        ///     Confirmation of initialization of crawl as string, or thrown exception as string.
        /// </returns>
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public string StartCrawling(string robotsTxtUrl)
        {
            try
            {
                InitializeCrawlrComponents();
                _robotsTxtUrl = robotsTxtUrl;
                InitializeCrawlrComponents();
                _storageManager.IssueCmd("START", robotsTxtUrl);

                return ("Beginning to crawl " + robotsTxtUrl);
            }
            catch (Exception ex)
            {
                return "Error: " + ex.ToString();
            }
        }

        /// <summary>
        ///     Tells the worker role to stop crawling and await further instructions.
        /// </summary>
        /// <returns>
        ///     Confirmation of stopping of crawl as string, or thrown exception as string.
        /// </returns>
        [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public string StopCrawling()
        {
            try
            {
                _storageManager.IssueCmd("CLEAR", _robotsTxtUrl);

                return ("Stopping and clearing crawl of " + _robotsTxtUrl);
            }
            catch (Exception ex)
            {
                return "Error: " + ex.ToString();
            }
        }

        /// <summary>
        ///     Query the crawler's Error Table for any found errors while crawling.
        /// </summary>
        /// <returns>
        ///     A list of formatted strings containg an URLs that threw exceptions as well as
        ///     the exception thrown, or the exception thrown from this method. If the crawler was
        ///     recently stopped, this method will throw an exception as the Error Table is deleted
        ///     and remade.
        /// </returns>
        [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public List<string> GetErrors()
        {
            List<string> results = new List<string>();
            CloudTable errorTable = _storageManager.ErrorTable;

            TableQuery<ErrorEntity> errorQuery = new TableQuery<ErrorEntity>()
                .Take(50);
            try
            {
                foreach (var error in errorTable.ExecuteQuery(errorQuery))
                {
                    results.Add(error.Url + " | " + error.Exception);
                }
            }
            catch (Exception ex)
            {

                results.Add("WebCrawler.asmx \n (if you just cleared/stopped the crawler, " +
                    "this is normal) | " + ex.ToString());
            }

            return results;
        }

        /// <summary>
        ///     Retrieves and returns a list of the last ten URLs crawled by the worker role.
        /// </summary>
        /// <returns>
        ///     A list of the last ten URLs crawled by the worker role as strings.
        /// </returns>
        [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public List<string> GetLastTenUrls()
        {
            List<string> result = new List<string>();
            CloudTable statusTable = _storageManager.StatusTable;

            TableQuery<WorkerRoleStatus> statusQuery = new TableQuery<WorkerRoleStatus>()
                .Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "WorkerRole Status")
                );

            string unparsedString = "";

            foreach (var statusItem in statusTable.ExecuteQuery(statusQuery))
            {
                unparsedString = statusItem.LastTenUrls;
            }

            string[] parsedString = unparsedString.Split(',');

            foreach (string s in parsedString)
            {
                result.Add(s);
            }
            result.Reverse();

            return result;
        }

        /// <summary>
        ///     Retrieves from azure storage the number of URLs that have been crawled by
        ///     the worker role.
        /// </summary>
        /// <returns>
        ///     The number of URLs crawled as an int.
        /// </returns>
        [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public List<int> GetNumCrawledUrls()
        {
            CloudTable urlTable = _storageManager.UrlTable;

            TableQuery<UrlTableCount> urlNumberQuery = new TableQuery<UrlTableCount>()
                .Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "UrlTableCount")
                );

            List<int> result = new List<int>();
            foreach (var statusItem in urlTable.ExecuteQuery(urlNumberQuery))
            {
                result.Add(statusItem.NumUrlsCrawled);
                result.Add(statusItem.NumUrlsInTable);
            }

            return result;
        }

        /// <summary>
        ///     Retrieves from azure storage the current size of the URL queue and
        ///     XML queue.
        /// </summary>
        /// <returns>
        ///     A list<int>, two long; first number is the XML queue size, second is
        ///     URL queue size.
        /// </returns>
        [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public List<int> GetQueueSizes()
        {
            CloudTable statusTable = _storageManager.StatusTable;

            TableQuery<CrawlrQueueSize> statusQuery = new TableQuery<CrawlrQueueSize>()
                .Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Queue Size")
                );

            List<int> result = new List<int>();
            foreach (var statusItem in statusTable.ExecuteQuery(statusQuery))
            {
                result.Add(statusItem.XmlQueueSize);
                result.Add(statusItem.UrlQueueSize);
            }

            return result;
        }

        /// <summary>
        ///     Retrieves the current status of the worker role from Azure Storage.
        /// </summary>
        /// <returns>
        ///     Worker role's various status reports as a list<string>
        /// </returns>
        [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public List<string> GetWorkerRoleStatus()
        {
            List<string> status = new List<string>();
            CloudTable statusTable = _storageManager.StatusTable;

            TableQuery<WorkerRoleStatus> statusQuery = new TableQuery<WorkerRoleStatus>()
                .Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "WorkerRole Status")
                );

            WorkerRoleStatus result;

            foreach (var statusItem in statusTable.ExecuteQuery(statusQuery))
            {
                result = statusItem;
                string currStatus = result.CurrStatus;
                if (currStatus == "CLEAR")
                {
                    currStatus = "Idle (Cleared)";
                }
                status.Add("Current status: " + currStatus);
                status.Add("CPU Utilization: " + result.CpuUsed + "%");
                status.Add("RAM Available: " + result.RamAvailable + "MBytes");
                status.Add("Number of URLs Crawled: " + result.NumUrlsCrawled);
            }

            return status;
        }

        /// <summary>
        ///     Check azure storage to see if a given URL has been crawled. If so, returns
        ///     the given URLs page title.
        /// </summary>
        /// <param name="url">
        ///     absolute URL of page, as string.
        /// </param>
        /// <returns>
        ///     List<string> containing the page title matching the given URL, if found.
        /// </returns>
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        [WebMethod]
        public List<string> SearchForUrlTitle(string url)
        {
            CloudTable urlTable = _storageManager.UrlTable;

            TableQuery<IndexedUrl> urlTitleQuery = new TableQuery<IndexedUrl>()
                .Where(
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, url.GetHashCode().ToString())
                );

            List<string> result = new List<string>();
            foreach (var statusItem in urlTable.ExecuteQuery(urlTitleQuery))
            {
                result.Add(statusItem.PageTitle);
            }

            return result;
        }

        private void InitializeCrawlrComponents()
        {
            _storageManager
                    = new CrawlrStorageManager(ConfigurationManager.AppSettings["StorageConnectionString"]);
        }
    }
}

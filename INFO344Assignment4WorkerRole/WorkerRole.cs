using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using INFO344Assignment4ClassLibrary.Crawlrs;
using INFO344Assignment4ClassLibrary.Helpers;
using INFO344Assignment4ClassLibrary.Storage;
using INFO344Assignment4ClassLibrary.Storage.Entities;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace INFO344Assignment4WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        // Helper for communicating with Azure storage & keeping track of seen URLs/XMLs
        private static CrawlrStorageManager _storageManager;
        private static CrawlrStatusManager _statusManager;
        private static CrawlrDataHelper _crawlrData;

        public override void Run()
        {
            Trace.TraceInformation("INFO344Assignment4WorkerRole is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("INFO344Assignment4WorkerRole has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("INFO344Assignment4WorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("INFO344Assignment4WorkerRole has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            _storageManager = new CrawlrStorageManager(ConfigurationManager.AppSettings["StorageConnectionString"]);

            // Get the current cmd from the cmd table;
            // Re-execute cmd query periodically until current cmd exists
            while (_storageManager.GetCurrentCmd() != "START")
            {
                Thread.Sleep(5000);
            }

            // If start cmd given, initialize download of robots.txt
            // and populate the xmlQueue and _disallowed list
            if (_storageManager.GetCurrentCmd() == "START" && _crawlrData == null)
            {
                // Set up queues, tables, data helper, status helper
                InitializeCrawlrComponents();
                Startup();
            }

            // Recurring work
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Working");

                // Do work if current cmd is still "start"
                if (_storageManager.GetCurrentCmd() == "START")
                {
                    // Process all XMLs (sitemaps) found
                    //string nextXml = "";
                    //try
                    //{
                    //    while (_crawlrData.NumXmlsQueued > 0 && _storageManager.GetCurrentCmd() == "START")
                    //    {
                    //        //CloudQueueMessage nextXmlMsg = _storageManager.XmlQueue.GetMessage();
                    //        nextXml = _crawlrData.XmlQueue.Dequeue();
                    //        _crawlrData.NumXmlsQueued--;

                    //        XmlCrawlr.CrawlXml(ref _crawlrData, ref _storageManager, nextXml);

                    //        //_storageManager.XmlQueue.DeleteMessage(nextXmlMsg);

                    //        // Update worker role status
                    //        _statusManager.UpdateCrawlrStatus(
                    //            "Loading",
                    //            _crawlrData,
                    //            _storageManager
                    //        );
                    //        _statusManager.UpdateQueueSize(_storageManager, _crawlrData.NumXmlsQueued, _crawlrData.NumUrlsQueued);

                    //        Thread.Sleep(50);
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    try
                    //    {
                    //        ErrorEntity errorUrl = new ErrorEntity(nextXml, ex.ToString());
                    //        TableOperation insertErrorUrl = TableOperation.InsertOrReplace(errorUrl);
                    //        _storageManager.ErrorTable.Execute(insertErrorUrl);
                    //    }
                    //    catch (Exception) { }
                    //}

                    // Process all URLs in queue
                    string nextUrl = "";
                    try
                    {
                        while (_storageManager.GetCurrentCmd() == "START")
                        {
                            CloudQueueMessage nextUrlMsg = _storageManager.UrlQueue.GetMessage();
                            nextUrl = nextUrlMsg.AsString;

                            UrlCrawlr.CrawlUrl(ref _crawlrData, ref _storageManager, nextUrl);

                            _storageManager.UrlQueue.DeleteMessage(nextUrlMsg);
                            _crawlrData.NumUrlsQueued--;

                            // Update worker role status
                            _statusManager.UpdateCrawlrStatus(
                                "Crawling",
                                _crawlrData,
                                _storageManager
                            );
                            _statusManager.UpdateQueueSize(_storageManager, _crawlrData.NumXmlsQueued, _crawlrData.NumUrlsQueued);

                            Thread.Sleep(50);
                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            ErrorEntity errorUrl = new ErrorEntity(nextUrl, ex.ToString());
                            TableOperation insertErrorUrl = TableOperation.InsertOrReplace(errorUrl);
                            _storageManager.ErrorTable.Execute(insertErrorUrl);
                        }
                        catch (Exception) { }
                    }
                }
                else if (_storageManager.GetCurrentCmd() == "CLEAR")
                {
                    // If the "CLEAR" command is found, update status.
                    _statusManager.UpdateCrawlrStatus(
                        "CLEAR",
                        _crawlrData,
                        _storageManager
                    );
                    _statusManager.UpdateQueueSize(_storageManager, 0, 0);
                    // Give Azure time to delete tables.
                    Thread.Sleep(20000);

                    try
                    {
                        // Idle while waiting for next command.
                        while (_storageManager.GetCurrentCmd() == "CLEAR")
                        {
                            Thread.Sleep(10000);
                        }
                    }
                    finally
                    {
                        // Reinitialize worker role.
                        InitializeCrawlrComponents();
                        Startup();
                    }
                }
                else
                {
                    // Idle worker role (for unimplemented 'pause' functionality).
                    _statusManager.UpdateCrawlrStatus(
                        "Idle",
                        _crawlrData,
                        _storageManager
                    );

                    Thread.Sleep(5000);
                }
            }

            Thread.Sleep(1000);
        }

        // Perform the initial robots.txt crawl.
        private void Startup()
        {
            // Update status
            _statusManager.UpdateCrawlrStatus(
                "Initializing",
                _crawlrData,
                _storageManager
            );

            // Crawl robots.txt
            RobotsTxtCrawlr.CrawlRobotsTxt(ref _crawlrData, ref _storageManager);

            // Initial update of queue size
            _statusManager.UpdateQueueSize(_storageManager, _crawlrData.NumXmlsQueued, _crawlrData.NumUrlsQueued);
        }

        // Refresh storage and helpers.
        private void InitializeCrawlrComponents()
        {
            _storageManager = new CrawlrStorageManager(ConfigurationManager.AppSettings["StorageConnectionString"]);
            _crawlrData = new CrawlrDataHelper();
            _statusManager = new CrawlrStatusManager();
        }
    }
}

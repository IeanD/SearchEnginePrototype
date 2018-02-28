using INFO344Assignment4ClassLibrary.Storage.Entities;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System.Linq;

namespace INFO344Assignment4ClassLibrary.Storage
{
    /// <summary>
    ///     Storage manager for a given crawler (webCrawlr). Initiates and tracks all necessary
    ///     Azure Storage items (XML Queue, URL Queue, Error Table, URL Table, Status Table, Command Table).
    ///     Can grab most recent command from the Command Table, issue a command to the command table,
    ///     grab the current robots.txt to crawl, and clear all queues & necessary tables.
    /// </summary>
    public class CrawlrStorageManager
    {
        private string _connectionString;
        private TableQuery<CrawlrCmd> _cmdQuery;
        private CloudTable _cmdTable;
        public CloudQueue XmlQueue { get; private set; }
        public CloudQueue UrlQueue { get; private set; }
        public CloudTable ErrorTable { get; private set; }
        public CloudTable UrlTable { get; private set; }
        public CloudTable StatusTable { get; private set; }

        /// <summary>
        ///     Initiates all necessary Azure Storage items (XML Queue, URL Queue, Error Table, URL Table,
        ///     Status Table, Command Table), as well as preps a query to the Command Table to get the 
        ///     most recent command.
        /// </summary>
        /// <param name="connString">
        ///     Connection string to Azure Storage account.
        /// </param>
        public CrawlrStorageManager(string connString)
        {
            this._connectionString = connString;

            this._cmdQuery = new TableQuery<CrawlrCmd>()
                .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, "workerRoleCmd"))
                .Take(1);

            this._cmdTable = new AzureTable(
                _connectionString, "devcrawlrcmdtable")
                .GetTable();

            this.XmlQueue = new AzureQueue(
                _connectionString, "devcrawlrxmlqueue")
                .GetQueue();

            this.UrlQueue = new AzureQueue(
                _connectionString, "devcrawlrurlqueue")
                .GetQueue();

            this.ErrorTable = new AzureTable(
                _connectionString, "devcrawlrerrortable")
                .GetTable();

            this.UrlTable = new AzureTable(
                _connectionString, "devcrawlrurltable")
                .GetTable();

            this.StatusTable = new AzureTable(
                _connectionString, "devcrawlrstatustable")
                .GetTable();
        }

        /// <summary>
        ///     Issues a given command to the Command Table.
        /// </summary>
        /// <param name="cmd">
        ///     Command to issue as string.
        /// </param>
        /// <param name="robotsTxt">
        ///     robots.txt to crawl / stop crawling as string.
        /// </param>
        public void IssueCmd(string cmd, string robotsTxt)
        {
            CrawlrCmd nextCmd = new CrawlrCmd(cmd, robotsTxt);
            TableOperation insertCmd = TableOperation.InsertOrReplace(nextCmd);

            _cmdTable.Execute(insertCmd);
        }

        /// <summary>
        ///     Gets the current command from the Command Table, if any.
        /// </summary>
        /// <returns>
        ///     Current command as string, or null.
        /// </returns>
        public string GetCurrentCmd()
        {
            var currentCmd = _cmdTable.ExecuteQuery(_cmdQuery);

            if (currentCmd.Any())
            {

                return currentCmd.First().Cmd;
            }
            else
            {

                return null;
            }
        }

        /// <summary>
        ///     Gets the robots.txt URI from the Command Table, if any.
        /// </summary>
        /// <returns>
        ///     Current robots.txt URI as string, or null.
        /// </returns>
        public string GetCurrentRobotsTxt()
        {
            var currentCmd = _cmdTable.ExecuteQuery(_cmdQuery);

            if (currentCmd.Any())
            {

                return currentCmd.First().Domain;
            }
            else
            {

                return null;
            }
        }

        /// <summary>
        ///     Clears / deletes all relevant crawler information from Azure Storage upon command.
        ///     Sets the XML & URL Queue sizes to 0, clears the XML & URL Queues, and deletes
        ///     the URL Table and Error Table. URL and Error Tables must be reinitialized for crawler
        ///     to function again (handled in worker role).
        /// </summary>
        public void ClearAll()
        {
            TableOperation clearQueue =
                TableOperation.InsertOrReplace(new CrawlrQueueSize(0, 0));
            StatusTable.Execute(clearQueue);
            XmlQueue.Clear();
            UrlQueue.Clear();
            UrlTable.Delete();
            ErrorTable.Delete();
        }
    }

}

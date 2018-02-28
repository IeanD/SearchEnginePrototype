using Microsoft.WindowsAzure.Storage.Table;

namespace INFO344Assignment4ClassLibrary.Storage.Entities
{
    /// <summary>
    ///     A TableEntity for a command table; stores a given crawler Worker Role command,
    ///     as well as a robots.txt URL for a given Worker Role to work with.
    /// </summary>
    public class CrawlrCmd : TableEntity
    {
        public string Cmd { get; set; }
        public string Domain { get; set; }

        public CrawlrCmd(string cmd, string domain)
        {
            this.PartitionKey = "webCrawlr";
            this.RowKey = "workerRoleCmd";

            this.Cmd = cmd;
            this.Domain = domain;
        }

        public CrawlrCmd()
        {

        }
    }
}

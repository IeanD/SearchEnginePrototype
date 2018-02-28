using Microsoft.WindowsAzure.Storage.Table;

namespace INFO344Assignment4ClassLibrary.Storage.Entities
{
    public class UrlTableCount : TableEntity
    {
        public int NumUrlsCrawled { get; set; }
        public int NumUrlsInTable { get; set; }

        public UrlTableCount(int numUrlsCrawled, int numUrlsInTable)
        {
            this.PartitionKey = "UrlTableCount";
            this.RowKey = "UrlTableCount";

            this.NumUrlsInTable = numUrlsInTable;
            this.NumUrlsCrawled = numUrlsCrawled;
        }

        public UrlTableCount()
        {

        }
    }
}

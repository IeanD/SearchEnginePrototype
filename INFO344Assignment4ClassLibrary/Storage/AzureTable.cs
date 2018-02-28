using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace INFO344Assignment4ClassLibrary.Storage
{
    public class AzureTable
    {
        private CloudTable _table;

        public AzureTable(string connString, string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(tableName);
            table.CreateIfNotExistsAsync();

            this._table = table;
        }

        public CloudTable GetTable()
        {

            return _table;
        }

    }
}

using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace INFO344Assignment4ClassLibrary.Storage.Entities
{
    public class ErrorEntity : TableEntity
    {
        public string Url { get; set; }
        public string Exception { get; set; }

        public ErrorEntity(string url, string exception)
        {
            Uri uri = new Uri(url);
            this.PartitionKey = uri.Host;
            this.RowKey = url.GetHashCode().ToString();

            this.Url = url;
            this.Exception = exception;
        }

        public ErrorEntity() { }
    }
}

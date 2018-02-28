using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace INFO344Assignment4ClassLibrary.Storage.Entities
{
    public class IndexedUrl : TableEntity
    {
        public string PageTitle { get; set; }
        public string Date { get; set; }
        public string Url { get; set; }

        public IndexedUrl(string pageTitle, string date, string url)
        {
            Uri uri = new Uri(url);
            this.PartitionKey = uri.Host;
            this.RowKey = url.GetHashCode().ToString();

            this.PageTitle = pageTitle;
            this.Date = date;
            this.Url = url;
        }

        public IndexedUrl() { }
    }
}

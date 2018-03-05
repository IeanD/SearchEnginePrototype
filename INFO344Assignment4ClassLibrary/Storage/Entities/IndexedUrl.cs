using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace INFO344Assignment4ClassLibrary.Storage.Entities
{
    public class IndexedUrl : TableEntity
    {
        public string Word { get; set; }
        public string PageTitle { get; set; }
        public string Date { get; set; }
        public string Url { get; set; }

        public IndexedUrl(string word, string pageTitle, string date, string url)
        {
            this.PartitionKey = word;
            this.RowKey = url.GetHashCode().ToString();

            this.Word = word;
            this.PageTitle = pageTitle;
            this.Date = date;
            this.Url = url;
        }

        public IndexedUrl() { }
    }
}

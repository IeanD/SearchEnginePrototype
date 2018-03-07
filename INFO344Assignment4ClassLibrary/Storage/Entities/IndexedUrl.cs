using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace INFO344Assignment4ClassLibrary.Storage.Entities
{
    /// <summary>
    ///     Entity for tracking information about a crawled URL to be searched via a reverse index. Tracks
    ///     a URL's Page Title, publish date (if any), absolute URL and an associated word (from the page title).
    /// </summary>
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

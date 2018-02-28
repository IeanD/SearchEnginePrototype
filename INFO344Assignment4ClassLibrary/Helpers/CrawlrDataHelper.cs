using System.Collections.Generic;

namespace INFO344Assignment4ClassLibrary.Helpers
{
    /// <summary>
    ///     Helper for crawler (webCrawlr).
    ///     Stores: a list of disallowed strings for a crawler, HashSets of already queued XMLs & URLs
    ///     for quick comparison, a HashSet of URLs already added to azure table storage for comparison,
    ///     and counters for number of XMLs queued, number of URLs queued, and number of URLs crawled.
    ///     Additionally contains a check (bool) method to see if a given URI is disallowed or not.
    /// </summary>
    public class CrawlrDataHelper
    {
        public List<string> DisallowedStrings { get; private set; }
        public HashSet<string> QueuedXmls { get; private set; }
        public HashSet<string> QueuedUrls { get; private set; }
        public HashSet<string> AddedUrls { get; private set; }
        public Queue<string> LastTenUrls { get; private set; }
        public int NumXmlsQueued { get; set; }
        public int NumUrlsQueued { get; set; }
        public int NumUrlsCrawled { get; set; }
        public int NumUrlsIndexed { get; set; }

        public CrawlrDataHelper()
        {
            this.DisallowedStrings = new List<string>();
            this.QueuedXmls = new HashSet<string>();
            this.QueuedUrls = new HashSet<string>();
            this.AddedUrls = new HashSet<string>();
            this.LastTenUrls = new Queue<string>(10);
            this.NumXmlsQueued = 0;
            this.NumUrlsQueued = 0;
            this.NumUrlsCrawled = 0;
            this.NumUrlsIndexed = 0;
        }

        public bool ChkIfUriAllowed(string uri)
        {
            foreach (string s in DisallowedStrings)
            {
                if (uri.Contains(s))
                {

                    return false;
                }
            }

            return true;
        }
    }

}

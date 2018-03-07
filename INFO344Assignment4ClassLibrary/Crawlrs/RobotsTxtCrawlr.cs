using INFO344Assignment4ClassLibrary.Helpers;
using INFO344Assignment4ClassLibrary.Storage;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace INFO344Assignment4ClassLibrary.Crawlrs
{
    /// <summary>
    ///     Helper class for crawling a robots.txt URL.
    ///     This class will load a robots.txt from a given URL, place all relevant sitemaps into a queue,
    ///     and build a list of disallowed strings.
    /// </summary>
    public class RobotsTxtCrawlr
    {
        /// <summary>
        ///     Crawls a given robots.txt, adding all sitemaps to queue.
        /// </summary>
        /// <param name="data">
        ///     Crawler data helper. Ref.
        /// </param>
        /// <param name="storage">
        ///     Crawler azure storage helper. Ref.
        /// </param>
        public static void CrawlRobotsTxt(ref CrawlrDataHelper data, ref CrawlrStorageManager storage)
        {
            string url = storage.GetCurrentRobotsTxt();
            CrawlSpecificRobotsTxt(url, ref data, ref storage);

            // Include bleacherreport.com (formerly cnn.com/sports) if crawling cnn.com
            if (storage.GetCurrentRobotsTxt().Contains("cnn"))
            {
                CrawlSpecificRobotsTxt("http://www.bleacherreport.com/robots.txt", ref data, ref storage);
            }
        }

        private static void CrawlSpecificRobotsTxt(string url, ref CrawlrDataHelper data, ref CrawlrStorageManager storage)
        {
            string tempPath = Path.GetTempFileName();
            WebClient wc = new WebClient();
            wc.DownloadFile(url, tempPath);
            StreamReader input = new StreamReader(tempPath);
            string currLine = "";
            string currUserAgent = "";
            List<string> sitemaps = new List<string>();
            while ((currLine = input.ReadLine()) != null)
            {
                var splitLine = currLine.Split(' ');
                if (splitLine[0].ToLower() == "sitemap:")
                {
                    bool pass = false;
                    if (url.Contains("bleacherreport"))
                    {
                        if (splitLine[1].Contains("/nba") || splitLine[1].Contains("/articles"))
                        {
                            pass = true;
                        }
                    }
                    else
                    {
                        pass = true;
                    }
                    if (pass)
                    {
                        sitemaps.Add(splitLine[1]);
                        data.QueuedXmls.Add(splitLine[1]);
                        data.XmlQueue.Enqueue(splitLine[1]);
                        data.NumXmlsQueued++;

                    }
                }
                else if (splitLine[0].ToLower() == "user-agent:")
                {
                    currUserAgent = splitLine[1];
                }
                else if (splitLine[0].ToLower() == "disallow:" && currUserAgent == "*")
                {
                    data.DisallowedStrings.Add(splitLine[1]);
                }

            }
        }
    }
}

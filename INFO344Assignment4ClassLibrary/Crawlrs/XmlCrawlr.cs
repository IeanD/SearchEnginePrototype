using INFO344Assignment4ClassLibrary.Helpers;
using INFO344Assignment4ClassLibrary.Storage;
using INFO344Assignment4ClassLibrary.Storage.Entities;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace INFO344Assignment4ClassLibrary.Crawlrs
{
    /// <summary>
    ///     Helper class for webCrawlr.
    ///     This class can crawl a given XML (sitemap), placing all found XMLs/URLs into a queue
    ///     while doing its best to check against date and filter out XMLs/URLs older than 1 January
    ///     2018.
    /// </summary>
    public class XmlCrawlr
    {
        /// <summary>
        ///     Crawls a given XML document (sitemap) by URI, checking each found URI's date if possible
        ///     and filtering out links disallowed or older than 2 months. Places found and valid XMLs/URLs
        ///     into a relevant queue for crawling.
        /// </summary>
        /// <param name="data">
        ///     Crawler data helper. Ref.
        /// </param>
        /// <param name="storage">
        ///     Crawler azure storage helper. Ref.
        /// </param>
        /// <param name="xml">
        ///     URI to XML document (sitemap) to crawl.
        /// </param>
        public static void CrawlXml(ref CrawlrDataHelper data, ref CrawlrStorageManager storage, string xml)
        {
            if (data.ChkIfUriAllowed(xml))
            {
                if (xml.EndsWith(".xml"))
                {
                    XElement sitemap = XElement.Load(xml);
                    string sitemapType = sitemap.Name.LocalName;
                    string nameSpace = sitemap.GetDefaultNamespace().ToString();
                    string dateNameSpace = null;
                    XName dateParent = null;
                    XName date = null;
                    if (sitemap.ToString().Contains(@"xmlns:news"))
                    {
                        dateNameSpace = sitemap.GetNamespaceOfPrefix("news").ToString();
                        dateParent = XName.Get("news", dateNameSpace);
                        date = XName.Get("publication_date", dateNameSpace);
                    }
                    else if (sitemap.ToString().Contains(@"xmlns:video"))
                    {
                        dateNameSpace = sitemap.GetNamespaceOfPrefix("video").ToString();
                        dateParent = XName.Get("video", dateNameSpace);
                        date = XName.Get("publication_date", dateNameSpace);
                    }
                    XName loc = XName.Get("loc", nameSpace);
                    XName lastMod = XName.Get("lastmod", nameSpace);
                    XName elementSelector;
                    if (sitemapType == "sitemapindex")
                    {
                        elementSelector = XName.Get("sitemap", nameSpace);
                    }
                    else
                    {
                        elementSelector = XName.Get("url", nameSpace);
                    }

                    List<string> xmlsToQueue = new List<string>();
                    List<string> urlsToQueue = new List<string>();

                    foreach (var element in sitemap.Elements(elementSelector))
                    {
                        bool validDateIfExists = true;
                        var currLocElement = element.Element(loc);
                        string currLocValue = currLocElement.Value;
                        var currLastModElement = element.Element(lastMod);
                        if (currLastModElement == null)
                        {
                            currLastModElement = element.Element(dateParent);
                            currLastModElement = (currLastModElement == null ? null : currLastModElement.Element(date));
                        }
                        if (currLastModElement != null)
                        {
                            validDateIfExists = DateTime.Parse(currLastModElement.Value) >= new DateTime(2018, 1, 1);
                        }
                        if (currLocValue.Contains(".xml"))
                        {
                            if (!data.QueuedXmls.Contains(currLocValue)
                                && validDateIfExists)
                            {
                                xmlsToQueue.Add(currLocValue);
                            }
                        }
                        else
                        {
                            if (!data.QueuedUrls.Contains(currLocValue)
                                && validDateIfExists)
                            {
                                urlsToQueue.Add(currLocValue);
                            }
                        }
                    }

                    foreach (string newXml in xmlsToQueue)
                    {
                        data.QueuedXmls.Add(newXml);
                        data.NumXmlsQueued++;
                        data.XmlQueue.Enqueue(newXml);
                    }

                    foreach (string newUrl in urlsToQueue)
                    {
                        UrlCrawlr.ChkAndAddUrl(newUrl, xml, null, ref data, ref storage);
                    }
                }
            }
        }
    }
}

using INFO344Assignment4ClassLibrary.Trie;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Web.Script.Services;
using System.Web.Services;

namespace INFO344Assignment4WebRole.services
{
    /// <summary>
    ///     Services for downloading data for, building, and searching a Trie data structure.
    /// 
    ///     by Iean Drew.
    /// </summary>
    [WebService(Namespace = "http://ieandrewcrawlr.cloudapp.net/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [ScriptService]
    public class GetQuerySuggestions : WebService
    {
        private static Trie _titlesTrie;
        private static string _filePath;
        private static int _numTitlesAddedToTrie = 0;
        private static string _lastTitleAddedToTrie = "";
        private static bool _trieIsBuilding = false;

        /// <summary>
        ///     Queries the VM for amount of available RAM in MB.
        /// </summary>
        /// <returns>
        ///     A string reporting the amount of RAM free in MB.
        /// </returns>
        [WebMethod]
        public string MemoryAvailable()
        {
            PerformanceCounter perf = new PerformanceCounter("Memory", "Available MBytes");

            string output = String.Format("The amount of free RAM is {0}MBytes.",
                                            perf.NextValue());

            return output;
        }

        /// <summary>
        ///     Downloads data set from Azure Blob Storage.
        /// </summary>
        /// <returns>
        ///     A string saying whether the download succeeded or failed.
        /// </returns>
        [WebMethod]
        public string DownloadWikiTitles()
        {
            string result = "Download failed.";

            _filePath = Path.GetTempFileName();
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.AppSettings["StorageConnectionString"]);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("igd");

            if (container.Exists())
            {
                foreach (IListBlobItem item in container.ListBlobs(null, false))
                {
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        CloudBlockBlob blob = (CloudBlockBlob)item;
                        CloudBlockBlob blobDl = container.GetBlockBlobReference("WikiTitles");

                        blobDl.DownloadToFile(_filePath, FileMode.Create);
                        result = "Download succeeded!";
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///     Build a List-Trie data structure from the dataset downloaded 
        ///     via DownloadWikiTitles().
        /// </summary>
        /// <returns>
        ///     A string reporting how many titles were added to the Trie, what
        ///     the last title added was, and how much free RAM is available
        ///     post operations.
        /// </returns>
        [WebMethod]
        public string BuildTrie()
        {
            _trieIsBuilding = true;
            if (_filePath == null)
            {
                DownloadWikiTitles();
            }
            _titlesTrie = new Trie();
            int counter = 0;
            string line;
            string inputPath = _filePath;
            StreamReader input = new StreamReader(inputPath);
            PerformanceCounter perf = new PerformanceCounter("Memory", "Available MBytes");

            while ((line = input.ReadLine()) != null)
            {
                if (perf.NextValue() <= 50
                    //|| line.IndexOf('B') == 0
                    )
                {
                    break;
                }

                _titlesTrie.AddWord(line.ToLower());
                counter++;
                _numTitlesAddedToTrie = counter;
                if (line != null && line != "" && line != " ")
                {
                    _lastTitleAddedToTrie = line;
                }
            }
            string output = String.Format("{0} titles were added. The last title added was {1}. The amount of free RAM is {2}MBytes.",
                                counter,
                                _lastTitleAddedToTrie,
                                perf.NextValue());

            _trieIsBuilding = false;

            return output;
        }

        /// <summary>
        ///     Searches the Trie data structure built via BuildTrie() for the
        ///     given search term.
        /// </summary>
        /// <param name="search">
        ///     A search term as a string.
        /// </param>
        /// <returns>
        ///     A list of up to 10 matching results in string format, packaged
        ///     as JSON.
        /// </returns>
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<string> SearchTrie(string search)
        {
            if (_titlesTrie == null && !_trieIsBuilding)
            {
                BuildTrie();
            }

            if (!_trieIsBuilding)
            {
                return _titlesTrie.SearchForWord(search);
            }
            else
            {
                return new List<string>();
            }
        }

        /// <summary>
        ///     Gets the number of titles added to the trie, and the last title added.
        /// </summary>
        /// <returns>
        ///     A List<string> where the first value is the number of titles added, and the
        ///     second is the last title added.
        /// </returns>
        [WebMethod]
        [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json)]
        public List<String> GetTrieStats()
        {
            List<string> results = new List<string>
            {
                _numTitlesAddedToTrie.ToString(),
                _lastTitleAddedToTrie
            };

            return results;
        }
    }
}
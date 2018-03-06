using System;
using System.Collections.Generic;

namespace INFO344Assignment4ClassLibrary.Trie
{
    /// <summary>
    ///     A hybrid List-Trie data structure for storing and rapidly retrieving
    ///     titles from a dataset. Works in conjunction with TrieNode.cs.
    /// 
    ///     By Iean Drew.
    /// </summary>
    public class Trie
    {
        public string LastStringAdded { get; set; }
        public int NumStringsAdded { get; set; }
        private TrieNode _root { get; }
        private static int MAX_LIST_SIZE = 100;
        private static int MAX_NUM_RESULTS = 10;

        /// <summary>
        ///     A basic Trie object constructor. Takes no parameters.
        /// </summary>
        public Trie()
        {
            this._root = new TrieNode('|', 0, null);
            this._root.Expanded = true;
        }

        /// <summary>
        ///     Adds a string to the Trie data structure for retrieval.
        /// </summary>
        /// <param name="word">
        ///     The string to be stored in the Trie.
        /// </param>
        public void AddWord(String word)
        {
            char firstLetter = word[0];
            TrieNode currNode;

            if (!_root.HasChild(firstLetter))
            {
                currNode = _root.AddChild(firstLetter);
            }
            else
            {
                currNode = _root.GetChild(firstLetter);
            }

            AddWord(currNode, word);
        }

        /// <summary>
        ///     Searches the current Trie structure for a given string. 
        ///     Provides up to 10 matching results that begin with the given
        ///     string.
        /// </summary>
        /// <param name="searchTerm">
        ///     The string to be searched for in the Trie.
        /// </param>
        /// <returns>
        ///     A List of strings matching the given given string.
        /// </returns>
        public List<string> SearchForWord(string searchTerm)
        {
            int level = 0;
            List<string> closestStrings = new List<string>();
            List<string> results = new List<string>();
            TrieNode currNode = _root;
            TrieNode nextNode = null;
            searchTerm = searchTerm.ToLower().TrimStart();

            // Find the closest matching node to the given search term.
            FindClosestNode(searchTerm, ref level, ref closestStrings, ref currNode, ref nextNode);

            // Add the search term itself, if in Trie.
            if (currNode.HasChild('^') && currNode.Level == searchTerm.Length)
            {
                results.Add(BuildWord(currNode));
            }

            // Return up to 10 matching strings from the trie.
            if (closestStrings != null)
            {
                foreach (string s in closestStrings)
                {
                    results.Add(s);

                    if (results.Count == MAX_NUM_RESULTS)
                    {
                        break;
                    }
                }
            }

            return results;
        }

        private void FindClosestNode(string searchTerm, ref int level, ref List<string> closestStrings, ref TrieNode currNode, ref TrieNode nextNode)
        {
            foreach (char c in searchTerm)
            {
                nextNode = currNode.GetChild(c);
                if (nextNode == null)
                {
                    closestStrings = FindClosestStrings(currNode, searchTerm);
                    break;
                }
                else
                {
                    currNode = nextNode;
                    level = currNode.Level;
                }
                if (level == searchTerm.Length)
                {
                    closestStrings = FindClosestStrings(currNode, searchTerm);
                    break;
                }
            }
        }

        private void AddWord(TrieNode currNode, string word)
        {
            int currIndex = currNode.Level;

            if (currNode.NumChildren == MAX_LIST_SIZE)
            {
                currNode.Expand();

                if (word.Length < currIndex)
                {
                    if (!currNode.HasChild(word[currIndex]))
                    {
                        currNode.AddChild(word[currIndex]);
                    }
                }
            }

            if (word.Length == currIndex)
            {
                currNode.AddChild('^');
            }
            else if (currNode.HasChild(word[currIndex]))
            {
                AddWord(currNode.GetChild(word[currIndex]), word);
            }
            else if (currNode.Expanded == false)
            {
                currNode.AddString(word.Substring(currIndex));
            }
            else
            {
                AddWord(currNode.AddChild(word[currIndex]), word);
            }
        }

        private List<string> FindClosestStrings(TrieNode currNode, string searchTerm)
        {
            List<string> closestStrings = new List<string>();
            if (currNode.StrChildren != null)
            {
                foreach (string s in currNode.StrChildren)
                {
                    string candidate = (BuildWord(currNode) + s).ToLower();
                    if (candidate.StartsWith(searchTerm.ToLower()) && !closestStrings.Contains(candidate.ToLower()))
                    {
                        closestStrings.Add(candidate);
                    }
                    if (closestStrings.Count >= MAX_NUM_RESULTS)
                    {
                        break;
                    }
                }
            }
            else
            {
                foreach (TrieNode childNode in currNode.TrieChildren)
                {
                    closestStrings.AddRange(FindClosestStrings(childNode, searchTerm));
                    if (closestStrings.Count >= MAX_NUM_RESULTS)
                    {
                        break;
                    }
                }
            }

            return closestStrings;
        }

        private string BuildWord(TrieNode currNode)
        {
            string result = "";
            if (currNode.Value == '^')
            {
                currNode = currNode.Parent;
            }
            while (currNode.Parent != null)
            {
                result = result.Insert(0, currNode.Value.ToString());
                currNode = currNode.Parent;
            }

            return result;
        }
    }

}

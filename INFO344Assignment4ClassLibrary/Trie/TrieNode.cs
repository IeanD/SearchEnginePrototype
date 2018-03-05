using System.Collections.Generic;

namespace INFO344Assignment4ClassLibrary.Trie
{
    /// <summary>
    ///     A node within a hybrid list-trie data structure for rapid retrieval
    ///     of Wikipedia titles. 
    /// 
    ///     By Iean Drew.
    /// </summary>
    public class TrieNode
    {
        public TrieNode Parent { get; set; }
        public char Value { get; }
        public int Level { get; }
        public int NumChildren { get; set; }
        public bool Expanded { get; set; }
        public List<TrieNode> TrieChildren { get; set; }
        public List<string> StrChildren { get; set; }

        /// <summary>
        ///     Basic TrieNode constructor.
        /// </summary>
        /// <param name="value">
        ///     The character value this TrieNode should represent.
        /// </param>
        /// <param name="level">
        ///     The depth of this TrieNode within the overall Trie data structure.
        /// </param>
        /// <param name="parent">
        ///     Another TrieNode to be considered this TrieNode's parent object.
        /// </param>
        public TrieNode(char value, int level, TrieNode parent)
        {
            this.Parent = parent;
            this.Value = value;
            this.Level = level;
            this.NumChildren = 0;
            this.Expanded = false;
            this.TrieChildren = new List<TrieNode>();
        }

        internal TrieNode AddChild(char c)
        {
            TrieNode newNode = new TrieNode(c, Level + 1, this);
            TrieChildren.Add(newNode);
            if (c != '^')
            {
                NumChildren++;
                Expanded = true;
            }

            return newNode;
        }

        internal TrieNode GetChild(char c)
        {

            return TrieChildren.Find
                (n => n.Value.ToString() == c.ToString());
        }

        internal void Expand()
        {
            Expanded = true;
            string[] array = StrChildren.ToArray();
            char currChar;
            int newNumChildren = 0;
            TrieNode tempNode;
            foreach (string currString in array)
            {
                if (currString != "")
                {
                    currChar = currString[0];
                    if (HasChild(currChar))
                    {
                        tempNode = GetChild(currChar);
                    }
                    else
                    {
                        tempNode = AddChild(currChar);
                        newNumChildren++;
                    }

                    tempNode.AddString(currString.Substring(1));
                }
            }

            StrChildren = null;
            NumChildren = newNumChildren;
        }

        internal void AddString(string remainsOfWord)
        {
            if (StrChildren == null)
            {
                StrChildren = new List<string>();
            }
            StrChildren.Add(remainsOfWord);

            NumChildren++;
        }

        internal bool HasChild(char c)
        {
            if (TrieChildren.Count > 0)
            {

                return TrieChildren.Exists
                    (n => n.Value.ToString() == c.ToString());
            }
            else
            {

                return false;
            }
        }
    }
}
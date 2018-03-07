# Iean Drew
# INFO 344 Programmin Assignment 4
# 6 March 2018

## Website Links
 * [Link to working website](http://igdcrawlr.cloudapp.net/)
 * [Link to crawler dashboard](http://igdcrawlr.cloudapp.net/crawlr_dashboard.html)
 * [Link to GetQuerySuggestions.asmx](http://igdcrawlr.cloudapp.net/services/GetQuerySuggestions.asmx)
 * [Link to WebCrawler.asmx](http://igdcrawlr.cloudapp.net/services/WebCrawler.asmx)
 * [Link to what's left of PA1 (to see example results, if curious)](http://ec2-35-164-128-55.us-west-2.compute.amazonaws.com/)

## Screenshots
 * [Screenshot of running PA4 instance on Azure](https://drive.google.com/file/d/1o8M8Tr4ebeKlNhmFgFgs8fuVI-SoK_ZB/view?usp=sharing)
 * [Screenshot of running modified PA1 instance on AWS EC2](https://drive.google.com/file/d/1_5ZnzV4iy-KYjs1dT5j9Gmav2J0Zi6sN/view?usp=sharing)

## Write up
As CK said, this assignment was basically wiring together my PA1, PA2 and PA3, changing what was necessary to make the three work nicely together. I broke up changes related to each assignment specifically below as well as some overall changes.

### Overall Changes
First, I created a new VS2017 Cloud Service project, then started by adding all of my PA3 code by manually (to make sure everything built). I then added my PA2's GetQuerySuggestions.asmx to the Web Role, then added my Trie.cs and TrieNode.cs to my class library for easy access.
I did not re-use my PA2's frontend, instead electing to write a new front-end. My PA2's frontend wasn't worth trying to fix up. My new frontend uses basic HTML, Bootstrap and JavaScript. Any data I get, whether it's JSONP from my PA1 or regular JSON from GetQuerySuggestions.asmx or WebCrawler.asmx is parsed into HTML and attached/detached to the DOM by my JavaScript (searchwork.js).

### PA1 Changes
For PA1, I initially had implemented the spelling correction extra credit to find close matches, and also returned multiple results. Additionally, my PA1 returned premade HTML as a string which was inserted directly by JavaScript on my client side. Subsequently, I had to adjust my match-finding class in order to return an exact match or no match at all, then create a new class which would return a string formatted like JSON with the appropriate NBA player stats. I did not encode it as a JSON on the PHP side, instead choosing to just use a string and JSON.parse() on the JavaScript side. Finally, I returned this string wrapped around a JSONP callback (e.g. ```echo $_REQUEST['callback'] . "('" . $output . "')"```). If you go to my original PA1 URL (listed above), you can actually enter in player names and preview what the response string will be. 

### PA2 Changes
I didn't change a lot about PA2. I added two props to my Trie class: an int to keep track of the number of lines added, and a string to keep track of the last line added. Additionally, I added a new WebMethod to my GetQuerySuggestions.asmx to poll those two values and return them as JSON so that my crawler dashboard can display my Trie's current status (it even live updates while it's building). I implemented the Hybrid Trie during PA2, so it should be able to hold the entire dataset. Otherwise, I added some code to GetQuerySuggestions.asmx in order to automatically build the trie if a user searches something and the Trie has gone down. My PA2's original frontend and JavaScript were both scrapped and reimplemented as parts of my new frontend (index.html and searchwork.js).

### PA3 Changes
PA3 probably had the most changes. I reworked my WorkerRole.cs a good deal, moving most of the "Clear"/"Stop" functionality out of it and into WebCrawler.asmx's StopCrawling() method. I also changed my crawler's initial build (the XML phase) to use a local queue on the machine as opposed to an Azure Queue. I changed my IndexedUrl TableEntity to store a part of the page title (e.g. "senate" or "trump") as the PartitionKey, the .getHashCode() of the actual URL as the RowKey, then added a new field to store a given page title word (in addition to the already stored Page Title/Date/URL). I rewrote my WebCrawler.asmx's search method to .Trim(), .ToLower() and .Split(' ') a user's search term, then created and executed Azure queries for each resulting word and collected the results into a List<IndexedUrl>, then use a Linq query to sort the List<IndexedUrl> first by the number of occurences descending, then by the date descending, then return the top 30 sorted results to the frontend as JSON. Additionally, I implemented a cache through a static Dictionary<string, List<string>> prop, similar to how did it in class. SearchForPageTitle(...) now checks the dictionary, returning any stored results for previous searches and adding new results as they come in. Working in tandem with a static Queue<string> prop, the dictionary-based cache should limit itself to 100 cached result lists. Finally, I updated my admin dashboard to display information about the status of my Trie, and to display how many words have been indexed vs. how many URLs have been crawled.

### New Stuff Specific to PA4
I used Chris' provided Google Ad sidebar code and some Bootstrap columns to add Google Ads to my site. I'm not sure if they're safe to click on, or if they'll mess up Chris' AdSense account :P but they do display.

## Extra credit write up
1. No extra credit this time around, too much going on. I just tried to get the basics working well.
'use strict';

var titleSearchForm = document.getElementById("searchForm");
titleSearchForm.addEventListener("input", function (e) {
    performSearch(e)
});

function performSearch(e) {
    e.preventDefault();

    let input = document.getElementById("titleSearch").value;
    if (input === "") {
        hideAndClearSearch();
    }

    const url = "services/GetQuerySuggestions.asmx/SearchTrie";
    let data = JSON.stringify({
        search: input
    });
    let init = {
        method: 'POST',
        body: data,
        headers: new Headers({
            'content-type': 'application/json'
        })
    };
    fetch(url, init)
        .then(function (response) {
            return response.json();
        }).then(function (json) {
            showResult(json.d);
        }
        );
}

function hideAndClearSearch() {
    document.getElementById("titleSuggestions").innerHTML = "";
    document.getElementById("titleSuggestions").setAttribute("style", "display:none");
}

function showResult(jsonArray) {
    if (document.getElementById("titleSearch").value === "" || jsonArray.length === 0) {
        hideAndClearSearch();
        return;
    }
    else {
        hideAndClearSearch();
        for (var i = 0; i < jsonArray.length; i++) {
            let resultWrapper = document.createElement('div');
            let result = document.createElement('a');
            result.setAttribute('href', 'javascript:;');
            result.setAttribute('onclick', 'appendNbaSearchScript("' + jsonArray[i] + '")');
            result.innerHTML = jsonArray[i];
            resultWrapper.appendChild(result);
            document.getElementById("titleSuggestions").appendChild(resultWrapper);
        }
        document.getElementById("titleSuggestions").style.display = "block";
    }
}

function appendNbaSearchScript(input) {
    let newScriptsDiv = document.getElementById('newScripts');
    while (newScriptsDiv.firstChild) {
        newScriptsDiv.removeChild(newScriptsDiv.firstChild);
    }
    let newScript = document.createElement('script');
    let src = "http://ec2-35-164-128-55.us-west-2.compute.amazonaws.com/search.php?callback=nbaSearch&search=" + input;
    newScript.setAttribute("src", src);
    newScriptsDiv.appendChild(newScript);
}

function nbaSearch(jsonString) {
    let json = JSON.parse(jsonString);
    if (json.Name === '%NOMATCH%') {
        console.log('No match found on EC2.');
        // Proceed to search crawlr table.
    }
    else {
        // Display NBA Stats results.
    }
}
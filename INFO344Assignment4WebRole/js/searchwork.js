'use strict';

var titleSearchForm = document.getElementById("searchForm");
titleSearchForm.addEventListener("input", function (e) {
    e.preventDefault();

    getSuggestions(e)
});
titleSearchForm.addEventListener('submit', function (e) {
    e.preventDefault();

    performSearch(e);
});

function performSearch(e) {
    let input = document.getElementById('titleSearch').value;
    appendNbaSearchScript(input);
}

function getSuggestions(e) {
    let input = document.getElementById("titleSearch").value;
    if (input === "") {
        hideAndClearSuggestions();
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
            showSuggestions(json.d);
        }
        );
}

function hideAndClearSuggestions() {
    document.getElementById("titleSuggestions").innerHTML = "";
    document.getElementById("titleSuggestions").setAttribute("style", "display:none");
}

function showSuggestions(jsonArray) {
    if (document.getElementById("titleSearch").value === "" || jsonArray.length === 0) {
        hideAndClearSuggestions();
        return;
    }
    else {
        hideAndClearSuggestions();
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
    let nbaResultsDiv = document.getElementById('nbaResultsWrapper');
    let urlResultsDiv = document.getElementById('urlResultsWrapper');

    if (json.Name === '%NOMATCH%') {
        nbaResultsDiv.setAttribute('style', 'display:none');
        console.log('No match found on EC2.');
        // Proceed to search crawlr table.
    }
    else {
        // Display NBA Stats results.
        //urlResultsDiv.setAttribute('style', 'display:none');
        displayNbaResults(json);
    }
    searchUrlIndex(document.getElementById('titleSearch').value);
}

function displayNbaResults(json) {
    let nbaResultsDiv = document.getElementById('nbaResults');

    // Clear results wrapper
    while (nbaResultsDiv.firstChild) {
        nbaResultsDiv.removeChild(nbaResultsDiv.firstChild);
    }

    // Create player headshot img
    let imgElement = document.createElement('img');
    imgElement.setAttribute('class', 'player-img');
    imgElement.setAttribute('onerror', 'javascript:this.src="default.png"');
    imgElement.setAttribute('src', json.HeadshotUrl);
    // Create header
    let headerElement = document.createElement('h1');
    headerElement.setAttribute('class', 'player-name');
    headerElement.innerHTML = json.Name;
    // Create table div
    let tableDiv = document.createElement('div');
    tableDiv.setAttribute('class', '[ container ] player-container');
    // Create table head
    let tableElement = document.createElement('table');
    tableElement.setAttribute('class', 'table');
    let tableHead = document.createElement('thead');
    let tableHeadRow = document.createElement('tr');
    let tableHeadCol1 = document.createElement('th');
    tableHeadCol1.setAttribute('scope', 'col');
    tableHeadCol1.innerHTML = 'GP';
    tableHeadRow.appendChild(tableHeadCol1);
    let tableHeadCol2 = document.createElement('th');
    tableHeadCol2.setAttribute('scope', 'col');
    tableHeadCol2.innerHTML = 'Team';
    tableHeadRow.appendChild(tableHeadCol2);
    let tableHeadCol3 = document.createElement('th');
    tableHeadCol3.setAttribute('scope', 'col');
    tableHeadCol3.innerHTML = 'PPG';
    tableHeadRow.appendChild(tableHeadCol3);
    let tableHeadCol4 = document.createElement('th');
    tableHeadCol4.setAttribute('scope', 'col');
    tableHeadCol4.innerHTML = '3PTM';
    tableHeadRow.appendChild(tableHeadCol4);
    let tableHeadCol5 = document.createElement('th');
    tableHeadCol5.setAttribute('scope', 'col');
    tableHeadCol5.innerHTML = 'REB';
    tableHeadRow.appendChild(tableHeadCol5);
    let tableHeadCol6 = document.createElement('th');
    tableHeadCol6.setAttribute('scope', 'col');
    tableHeadCol6.innerHTML = 'AST';
    tableHeadRow.appendChild(tableHeadCol6);
    tableHead.appendChild(tableHeadRow);
    tableElement.appendChild(tableHead);
    // Create table body
    let tableBody = document.createElement('tbody');
    let tableBodyRow = document.createElement('tr');
    let tableBodyCol1 = document.createElement('td');
    tableBodyCol1.innerHTML = json.Gp;
    tableBodyRow.appendChild(tableBodyCol1);
    let tableBodyCol2 = document.createElement('td');
    tableBodyCol2.innerHTML = json.Team;
    tableBodyRow.appendChild(tableBodyCol2);
    let tableBodyCol3 = document.createElement('td');
    tableBodyCol3.innerHTML = json.Ppg;
    tableBodyRow.appendChild(tableBodyCol3);
    let tableBodyCol4 = document.createElement('td');
    tableBodyCol4.innerHTML = json.Threeptm;
    tableBodyRow.appendChild(tableBodyCol4);
    let tableBodyCol5 = document.createElement('td');
    tableBodyCol5.innerHTML = json.Reb;
    tableBodyRow.appendChild(tableBodyCol5);
    let tableBodyCol6 = document.createElement('td');
    tableBodyCol6.innerHTML = json.Ast;
    tableBodyRow.appendChild(tableBodyCol6);
    tableBody.appendChild(tableBodyRow);
    tableElement.appendChild(tableBody);
    tableDiv.appendChild(tableElement);
    // Append img, header, table & display
    nbaResultsDiv.appendChild(imgElement);
    nbaResultsDiv.appendChild(headerElement);
    nbaResultsDiv.appendChild(tableDiv);
    document.getElementById('nbaResultsWrapper').setAttribute('style', 'display:block');
}

function searchUrlIndex(input) {
    let url = "services/WebCrawler.asmx/SearchForPageTitle";
    let data = JSON.stringify({
        userSearch: input
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
            console.log(json.d);
            showUrlResults(json.d);
        });
}

function showUrlResults(list) {
    let urlResultsDiv = document.getElementById('urlResults');

    // Clear results wrapper
    while (urlResultsDiv.firstChild) {
        urlResultsDiv.removeChild(urlResultsDiv.firstChild);
    }

    if (list.length === 0) {
        let noResult = document.createElement('p');
        noResult.innerHTML = 'No Results Found.';
        urlResultsDiv.appendChild(noResult);
    } else {
        for (var i = 0; i < list.length; i++) {
            let currResultLine = document.createElement('p');
            let currResultLink = document.createElement('a');
            let splitResult = list[i].split('|||');
            currResultLink.setAttribute('href', splitResult[2]);
            currResultLink.innerHTML = splitResult[0];
            currResultLine.appendChild(currResultLink);
            if (splitResult[1] != "NULL") {
                let currResultDate = document.createElement('span');
                currResultDate.setAttribute('class', 'url-date');
                currResultDate.innerHTML = splitResult[1];
                currResultLine.appendChild(currResultDate);
            }
            //currResult.innerHTML = list[i];
            urlResultsDiv.appendChild(document.createElement('hr'));
            urlResultsDiv.appendChild(currResultLine);
        }
    }


    document.getElementById('urlResultsWrapper').setAttribute('style', 'display:block');
}

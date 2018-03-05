'use strict';

var init = false;

var urlInputForm = document.getElementById("urlInputForm");
urlInputForm.addEventListener("submit", function (e) {
    init = true;
    startCrawl(e)
});

var searchBtn = document.getElementById("urlSearchButton");
searchBtn.addEventListener("click", function (e) {
    searchForTitleByUrl(e);
});

const interval = setInterval(refreshAll, 6000);

function startCrawl(e) {
    e.preventDefault();

    let input = document.getElementById("urlCrawl").value;
    const url = "services/WebCrawler.asmx/StartCrawling";
    let data = JSON.stringify({
        robotsTxtUrl: input
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
        }
        );
}

function stopCrawler() {
    const stopUrl = "services/WebCrawler.asmx/StopCrawling";

    let init = {
        method: 'GET',
        headers: new Headers({
            'content-type': 'application/json'
        })
    };

    fetch(stopUrl, init)
        .then(function (response) {
            return response.json();
        }).then(function (json) {
            console.log(json.d);
        }
        );
}

function searchForTitleByUrl(e) {
    if (e) {
        e.preventDefault(e);
    }

    const searchUrl = "services/WebCrawler.asmx/SearchForUrlTitle"

    let input = document.getElementById("urlSearch").value;

    let data = JSON.stringify({
        url: input
    });
    let init = {
        method: 'POST',
        body: data,
        headers: new Headers({
            'content-type': 'application/json'
        })
    };
    fetch(searchUrl, init)
        .then(function (response) {
            return response.json();
        }).then(function (json) {

            let outputDiv = document.getElementById("urlSearchResults");
            outputDiv.style = "display: block";

            let searchResult = document.createElement('p');
            while (outputDiv.firstChild) {
                outputDiv.removeChild(outputDiv.firstChild);
            }
            if (json.d.length > 0) {
                searchResult.innerHTML = "<strong>Found page title: </strong>" + json.d[0];
            }
            else {
                searchResult.innerHTML = "<strong>No results found.<strong>";
            }
            outputDiv.appendChild(searchResult);
        }
        );
}

function refreshAll(e) {
    if (e) {
        e.preventDefault(e);
    }

    const statusUrl = "services/WebCrawler.asmx/GetWorkerRoleStatus";

    let statusInit = {
        method: 'GET',
        headers: new Headers({
            'content-type': 'application/json'
        })
    };
    fetch(statusUrl, statusInit)
        .then(function (response) {
            return response.json();
        }).then(function (json) {

            let currStatusLi = document.getElementById("workerRoleStatus");
            let cpuUsedLi = document.getElementById("cpuUtilized");
            let ramAvailLi = document.getElementById("ramUsed");

            while (currStatusLi.firstChild) {
                currStatusLi.removeChild(currStatusLi.firstChild);
            }
            while (cpuUsedLi.firstChild) {
                cpuUsedLi.removeChild(cpuUsedLi.firstChild);
            }
            while (ramAvailLi.firstChild) {
                ramAvailLi.removeChild(ramAvailLi.firstChild);
            }
            let NumUrlsLi = document.getElementById("numberUrlsCrawled");
            while (NumUrlsLi.firstChild) {
                NumUrlsLi.removeChild(NumUrlsLi.firstChild);
            }

            let currStatus = document.createElement('span');
            if (json.d.length === 0) {
                currStatus.innerHTML = (init ? "Spinning up..." : "Waiting...");
                currStatusLi.appendChild(currStatus);
            }
            else {
                currStatus.innerHTML = json.d[0];
                currStatusLi.appendChild(currStatus);
                let cpuUsed = document.createElement('span');
                cpuUsed.innerHTML = json.d[1];
                cpuUsedLi.appendChild(cpuUsed);
                let ramAvailable = document.createElement('span');
                ramAvailable.innerHTML = json.d[2];
                ramAvailLi.appendChild(ramAvailable);
                let numUrlsCrawled = document.createElement('span');
                numUrlsCrawled.innerHTML = json.d[3];
                NumUrlsLi.appendChild(numUrlsCrawled);
            }
        }
        );

    const queueSizesUrl = "services/WebCrawler.asmx/GetQueueSizes";

    let queueSizesInit = {
        method: 'GET',
        headers: new Headers({
            'content-type': 'application/json'
        })
    };
    fetch(queueSizesUrl, queueSizesInit)
        .then(function (response) {
            return response.json();
        }).then(function (json) {

            let xmlSizeLi = document.getElementById("sizeOfXmlQueue");
            let urlSizeLi = document.getElementById("sizeOfUrlQueue");

            while (xmlSizeLi.firstChild) {
                xmlSizeLi.removeChild(xmlSizeLi.firstChild);
            }
            while (urlSizeLi.firstChild) {
                urlSizeLi.removeChild(urlSizeLi.firstChild);
            }

            let xmlSize = document.createElement('span');
            if (json.d.length === 0) {
                xmlSize.innerHTML = (init ? "Spinning up..." : "Waiting...");
                xmlSizeLi.appendChild(xmlSize);
            }
            else {
                xmlSize.innerHTML = "Number of items in XML Queue: " + json.d[0];
                xmlSizeLi.appendChild(xmlSize);

                let urlSize = document.createElement('span');
                urlSize.innerHTML = "Number of items in URL Queue: " + json.d[1];
                urlSizeLi.appendChild(urlSize);
            }
        }
        );

    const lastTenUrlsUrl = "services/WebCrawler.asmx/GetLastTenUrls";

    let lastTenUrlsInit = {
        method: 'GET',
        headers: new Headers({
            'content-type': 'application/json'
        })
    };
    fetch(lastTenUrlsUrl, lastTenUrlsInit)
        .then(function (response) {
            return response.json();
        }).then(function (json) {
            let lastTenUrlsUl = document.getElementById("lastTenUrls");
            while (lastTenUrlsUl.firstChild) {
                lastTenUrlsUl.removeChild(lastTenUrlsUl.firstChild);
            }
            if (json.d[0] === "") {
                var result = document.createElement('li');
                result.innerHTML = (init ? "Spinning up..." : "Waiting...");
                lastTenUrlsUl.appendChild(result);
            }
            else {
                for (var i = 0; i < json.d.length; i++) {
                    var result = document.createElement('li');
                    result.innerHTML = json.d[i];
                    lastTenUrlsUl.appendChild(result);
                }
            }
        }
        );

    const numUrlsCrawledUrl = "services/WebCrawler.asmx/GetNumCrawledUrls";

    let numUrlsCrawledInit = {
        method: 'GET',
        headers: new Headers({
            'content-type': 'application/json'
        })
    };
    fetch(numUrlsCrawledUrl, numUrlsCrawledInit)
        .then(function (response) {
            return response.json();
        }).then(function (json) {
            let NumIndexLi = document.getElementById("sizeOfIndex");
            while (NumIndexLi.firstChild) {
                NumIndexLi.removeChild(NumIndexLi.firstChild);
            }
            let numUrlsIndexed = document.createElement('span');
            numUrlsIndexed.innerHTML = "Number of URLs indexed: " + (json.d[1] ? json.d[1] : '0');
            NumIndexLi.appendChild(numUrlsIndexed);
            //console.log(json);
        }
        );

    const errorListUrl = "services/WebCrawler.asmx/GetErrors";

    let errorListInit = {
        method: 'GET',
        headers: new Headers({
            'content-type': 'application/json'
        })
    };
    fetch(errorListUrl, errorListInit)
        .then(function (response) {
            return response.json();
        }).then(function (json) {

            let errorDiv = document.getElementById("errorsWrapper");
            while (errorDiv.firstChild) {
                errorDiv.removeChild(errorDiv.firstChild);
            }
            if (json.d.length > 0) {
                let table = document.createElement('table');
                table.classList.add("table");
                table.classList.add("table-striped");
                table.classList.add("table-bordered");
                table.classList.add("table-hover");
                let tHead = document.createElement('thead');
                tHead.classList.add("thead-dark")
                let tHeadRow = document.createElement('tr');
                let th1 = document.createElement('th');
                th1.setAttribute('scope', 'col');
                th1.innerText = "URL";
                let th2 = document.createElement('th');
                th2.setAttribute('scope', 'col');
                th2.innerText = "Error / Exception";
                tHeadRow.appendChild(th1);
                tHeadRow.appendChild(th2);
                tHead.appendChild(tHeadRow);
                table.appendChild(tHead);
                let tableBody = document.createElement('tbody');
                for (var i = 0; i < json.d.length; i++) {
                    let splitItem = json.d[i].split(" | ");
                    let url = splitItem[0];
                    let err = splitItem[1];
                    if (err.length > 30) {
                        err = err.substring(0, 140) + "... (see logs)";
                    }
                    let newRow = document.createElement('tr');
                    let col1 = document.createElement('td');
                    let col2 = document.createElement('td');
                    col1.innerText = url;
                    col2.innerText = err;
                    newRow.appendChild(col1);
                    newRow.appendChild(col2);
                    tableBody.appendChild(newRow);
                }
                table.appendChild(tableBody);
                errorDiv.appendChild(table);
            }
        }
        );
}

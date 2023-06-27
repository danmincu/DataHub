// create connection to queryHub
var connectionQuery = new signalR.HubConnectionBuilder()
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .withUrl("/hubs/query", signalR.HttpTransportType.WebSockets)
    .build();

connectionQuery.on("updateTotalSuccesfullConnections", function (query) {
    document.getElementById("totalSuccesfullConnectionsCount").innerText = query;
});


connectionQuery.on("updateTotalLiveConnections", function (query) {
    document.getElementById("liveConnectionsCount").innerText = query;
});

connectionQuery.on("maxLiveConnectionCountReached", function (maxConnection) {
    disconnectConnection();
});


connectionQuery.on("connectionAccepted", function (maxConnection) {
    setLoggedInLEDColor('blue');
});


function disconnectConnection() {    
    connectionQuery.stop();
    connectionQuery = null;
    setLoggedInLEDColor('red');    
}


function newConnection() {
    connectionQuery.invoke("ConnectQuery", 1);
    setLEDColor("limegreen");
}


function fulfilled() {
    //do something on start
    console.log("Connection to User Hub Successful");
    newConnection();
}
function rejected() {
    //rejected logs
}

function setLEDColor(color) {
    var led = document.getElementById('led');
    led.className = 'led ' + color;
}

function setLoggedInLEDColor(color) {
    var led = document.getElementById('loggedInLed');
    led.className = 'led ' + color;
}


connectionQuery.onclose((error) => {
    //document.body.style.background = "red";
    setLEDColor('red');
    setLoggedInLEDColor('red');
});

connectionQuery.onreconnected((connectionId) => {
    //document.body.style.background = "green";
    setLEDColor('green');
});

connectionQuery.onreconnecting((error) => {
    //document.body.style.background = "orange";
    setLEDColor('orange');
});

connectionQuery.start().then(fulfilled, rejected);
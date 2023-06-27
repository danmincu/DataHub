// create connection to queryHub
var connectionQuery = new signalR.HubConnectionBuilder()
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .withUrl("/hubs/query")
    //.withUrl("/hubs/query", signalR.HttpTransportType.WebSockets)
    //.withUrl("/hubs/query", signalR.HttpTransportType.LongPolling)
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


connectionQuery.onreconnecting((connectionId) => {
   console.log("Connection to User Hub Connected" + connectionId);
})

connectionQuery.onreconnected((connectionId) => {
    console.log("Connection to User Hub Reconnected" + connectionId);
    setLEDColor('green');
});

connectionQuery.onreconnecting((error) => {
    console.log("Reconnecting: " + error);
    setLEDColor('orange');
});

connectionQuery.start().then(fulfilled, rejected);
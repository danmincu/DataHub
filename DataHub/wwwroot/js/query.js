
function getToken() {

    const url = `/security/createToken?user=service-account-data`;
    return fetch(url)
        .then(response => response.json())
        .then(data => data)
        .catch(error => {
            console.error('Error:', error);
            return null;
        });
}


// create connection to queryHub
var connection = new signalR.HubConnectionBuilder()
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .withUrl("/hubs/query", {
        accessTokenFactory: () => { return getToken() }
    })
    //.withUrl("/hubs/query", signalR.HttpTransportType.WebSockets)
    //.withUrl("/hubs/query", signalR.HttpTransportType.LongPolling)
    .build();

connection.on("updateTotalSuccesfullConnections", function (count) {
    document.getElementById("totalSuccesfullConnectionsCount").innerText = count;
});

connection.on("updateTotalLiveConnections", function (query) {
    document.getElementById("liveConnectionsCount").innerText = query;
});

connection.on("maxLiveConnectionCountReached", function (maxConnection) {
    document.getElementById("serverMessage").innerText = "MaxLiveConnectionCountReached: " + maxConnection;
    disconnectConnection();
});

connection.on("data", function (ackId, data) {
    receivedAck(ackId);
    document.getElementById("messages").innerText = "ACK: " + ackId + '\n' + data;
    //Auto ACK
    ack(ackId);
});

connection.on("connectionAccepted", function (maxConnection) {
    setLoggedInLEDColor('blue');
});


function disconnectConnection() {
    connection.stop();
    connection = null;
    setLoggedInLEDColor('red');
}

function connect() {
    connectAndAck();
}

function receivedAck(ackId) {
    connection.invoke("soft-ack", ackId);
}

function ack(ackId) {
    connection.invoke("hard-ack", ackId).then(function (ack) {
        console.log(ack);
    });
}

function connectAndAck(ackId) {
    //do something on start
    console.log("Connection to User Hub Successful");
    var maxEventCount = 115;
    connection.invoke("connectToHub", maxEventCount, ackId).then(function (connectionId) {
        document.getElementById("connectionId").innerText = connectionId;
        console.log('Connection to User Hub Connected using ' + connectionId);
    });
    setLEDColor("limegreen");
}

function rejected() {
    //rejected logs
}


connection.onclose((error) => {
    document.getElementById("connectionId").innerText = "";
    setLEDColor('red');
    setLoggedInLEDColor('red');
});


connection.onreconnected((connectionId) => {
    console.log("Connection to User Hub Reconnected with cid:" + connectionId);
    document.getElementById("connectionId").innerText = connectionId;
    setLEDColor('green');
});

connection.onreconnecting((error) => {
    document.getElementById("connectionId").innerText = "";
    console.log("Reconnecting: " + error);
    setLEDColor('orange');
});


//todo
connection.start().then(() => { connectAndAck("some-ACK_ID") }, rejected);
//connectionQuery.start().then(connect, rejected);


// UI updates

function setLEDColor(color) {
    var led = document.getElementById('led');
    led.className = 'led ' + color;
}

function setLoggedInLEDColor(color) {
    var led = document.getElementById('loggedInLed');
    led.className = 'led ' + color;
}

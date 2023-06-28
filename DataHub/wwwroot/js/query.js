// create connection to queryHub
var connection = new signalR.HubConnectionBuilder()
    .configureLogging(signalR.LogLevel.Information)
    .withAutomaticReconnect()
    .withUrl("/hubs/query", {
        accessTokenFactory: () => "some-access-token"
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
    //console.log("AckId:" + ackId);
    //console.log(data);
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
    connection.invoke("ReceivedAck", ackId);
}

function ack(ackId) {
    connection.invoke("Ack", ackId).then(function (ack) {
        console.log(ack);
    });
}

function connectAndAck(ackId) {
    //do something on start
    console.log("Connection to User Hub Successful");
    connection.invoke("ConnectQuery", 15, ackId).then(function (connectionId) {
        document.getElementById("connectionId").innerText = connectionId;
        console.log('Connection to User Hub Connected using ' + connectionId);
    });
    setLEDColor("limegreen");
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


connection.start().then(() => { connectAndAck("some-ACK_ID") }, rejected);
//connectionQuery.start().then(connect, rejected);

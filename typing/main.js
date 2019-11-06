'use strict'

const express = require('express');
const parser = require('body-parser');
var parseArgs = require('minimist');
var net = require('net');
const http = require('http');

var argv = parseArgs(process.argv.slice(2));
const port = argv.port || 31415;

const app = express();
app.use(express.static('./')); // Serve static files from main directory
app.use(parser.json());
app.use(parser.urlencoded({ extended: true }));

let httpserver = http.Server(app);


var tcpserver = net.createServer(function(client) {

    console.log('Client connect. Client local address : ' + client.localAddress + ':' + client.localPort + '. client remote address : ' + client.remoteAddress + ':' + client.remotePort);

    client.setEncoding('utf-8');

    //client.setTimeout(1000);

    // When receive client data.
    client.on('data', function (data) {

        // Print received client data and length.
        console.log('Receive client send data : ' + data + ', data size : ' + client.bytesRead);

        // Server send data back to client use client net.Socket object.
        //client.end('Server received data : ' + data + ', send back to client data size : ' + client.bytesWritten);
    });

    // When client send data complete.
	/*
    client.on('end', function () {
        console.log('Client disconnect.');

        // Get current connections count.
        tcpserver.getConnections(function (err, count) {
            if(!err)
            {
                // Print current connection count in server console.
                console.log("There are %d connections now. ", count);
            }else
            {
                console.error(JSON.stringify(err));
            }

        });
    });
	*/

    // When client timeout.
    client.on('timeout', function () {
        console.log('Client request time out. ');
    })
});

tcpserver.listen(27015, function () {

    // Get server address info.
    var serverInfo = tcpserver.address();

    var serverInfoJson = JSON.stringify(serverInfo);

    console.log('TCP server listen on address : ' + serverInfoJson);

    tcpserver.on('close', function () {
        console.log('TCP server socket is closed.');
    });

    tcpserver.on('error', function (error) {
        console.error(JSON.stringify(error));
    });

});

var curWS;

httpserver.listen(parseInt(port, 10), () =>
   console.log('HTTP server listening on port %d', httpserver.address().port)
);

try {
   let WebSocket = require('ws').Server;
   let wss = new WebSocket({ port: 14285 });
   let sockets = [];

   wss.on('connection', ws => {
      for (ws.index = 0; sockets[ws.index]; ws.index++);
		sockets[ws.index] = ws;
	  
	  console.log("websocket connection number:", ws.index);
      // Communicate with first connection only
      if (ws.index == 0) {
         // Initialize
//         ws.send(JSON.stringify({global: "displayListener", value: true }));

		// supposed to be related to C++
         /*
		 holojam.on('keyEvent', (flake) => {
            var type = flake.ints[1];
            type = (type == 0 ? "onkeydown" : "onkeyup");

            var e = {
               eventType: type,
               event: {
                  keyCode: flake.ints[0] + 48
            }};

            ws.send(JSON.stringify(e));
         });
		 */
	  }
	  ws.on("message", function(msg) {
         for (var index = 0 ; index < sockets.length ; index++)
            if (index != ws.index)
               sockets[index].send(msg);
      });
      // Remove this sockets
      ws.on('close', () => sockets.splice(ws.index, 1));
	  });
} catch (err) {
   console.log(
      '\x1b[31mCouldn\'t load websocket library. Disabling event broadcasting.'
      + ' Run \'npm install\'\x1b[0m'
    + err);
}
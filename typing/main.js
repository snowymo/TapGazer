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

var curWS;
var tcpserver = net.createServer(function(client) {

    console.log('Client connect. Client local address : ' + client.localAddress + ':' + client.localPort + '. client remote address : ' + client.remoteAddress + ':' + client.remotePort);

    client.setEncoding('utf-8');

    //client.setTimeout(1000);

    // When receive client data.
    client.on('data', function (data) {
		

        // Print received client data and length.
        console.log('Receive client send data : ' + data + ', data size : ' + data.length + " " + client.bytesRead);
		
		console.log("cur ws:" + curWS);
		
		if(data.length > 10){
			// it is from tobii 4c
			//var x = data.split(" ")[0];
			var words = data.split(" ");
			if(words.length == 2){
				console.log("ongaze");
				var e = {
					eventType: "ongaze",
					event:{
						x: data.split(" ")[0],
						y: data.split(" ")[1]
					}
				};
				if(curWS)
					curWS.send(JSON.stringify(e));
			}
		}
		else if(data.length == 9){
			// focus data
			var e = {
			   eventType: "onfocus",
			   event: {
				  id: data[8]
			}};
			if(curWS)
				curWS.send(JSON.stringify(e));
		}
		else if(data.length == 1){
			// it is from sensel
			var e = {
			   eventType: "onkeydown",
			   event: {
				  keyCode: data
			}};
			console.log("send through websocket:" + e);
			if(curWS)
				curWS.send(JSON.stringify(e));
		}

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

    console.log('sensel TCP server listen on address : ' + serverInfoJson);

    tcpserver.on('close', function () {
        console.log('TCP server socket is closed.');
    });

    tcpserver.on('error', function (error) {
        console.error(JSON.stringify(error));
    });

});


httpserver.listen(parseInt(port, 10), () =>
   console.log('HTTP server listening on port %d', httpserver.address().port)
);

try {
   let WebSocket = require('ws').Server
   console.log("test websocket");
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
		curWS = ws;
		console.log("cur ws:" + curWS);
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
'use strict'

const express = require('express');
const parser = require('body-parser');
var parseArgs = require('minimist');
var argv = parseArgs(process.argv.slice(2));
const port = argv.port || 31415;

const app = express();
app.use(express.static('./')); // Serve static files from main directory
app.use(parser.json());
app.use(parser.urlencoded({ extended: true }));

const http = require('http');
let server = http.Server(app);

var curWS;

server.listen(parseInt(port, 10), () =>
   console.log('HTTP server listening on port %d', server.address().port)
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
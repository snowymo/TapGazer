function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

function Server() {
      var that = this;
      this.name = name;
      this.socket = null;

      this.connectSocket = function() {

         this.socket = new WebSocket("ws://" + window.location.hostname + ":14285");
         this.socket.binaryType = "arraybuffer";

         var that = this;
		 
         this.socket.onmessage = function(event) {

            var obj = JSON.parse(event.data);

            if (obj.eventType == "onkeydown") {
			   obj.event.preventDefault = function() { };
			   //(events_canvas[obj.eventType])(obj.event);
			   var event = document.createEvent("HTMLEvents");
				event.initEvent("keydown", true, false);
				event.key = obj.event.keyCode;
				document.dispatchEvent(event);
				
				 sleep(50).then(() => {
					// Do something after the sleep!
					event.initEvent("keyup", true, false);
					event.key = obj.event.keyCode;
					document.dispatchEvent(event);
				});
				
				
			   return;
            }else if(obj.eventType == "ongaze"){
				obj.event.preventDefault = function() { };
			   //(events_canvas[obj.eventType])(obj.event);
			   var event = document.createEvent("HTMLEvents");
				event.initEvent("gaze", true, false);
				event.x = obj.event.x;
				event.y = obj.event.y;
				console.log(event);
				document.dispatchEvent(event);
				
			   return;
			}else if(obj.eventType == "onfocus"){
				obj.event.preventDefault = function() { };
				//(events_canvas[obj.eventType])(obj.event);
				var event = document.createEvent("HTMLEvents");
				event.initEvent("focus", true, false);
				event.id = obj.event.id;
				console.log(event);
				document.dispatchEvent(event);
				
			   return;
			}
         };
         return this.socket;
      };

      this.broadcastGlobal = function(name) {
         this.broadcastObject( { global: name, value: window[name] } );
      };


      this.broadcast = function(message) {
         if (this.socket == null && this.connectSocket() == null) {
            console.log("socket is null, can't broadcast");
            return;
         }

         if (this.socket.readyState != 1) {
            console.log("socket is not open, can't broadcast");
            return;
         }

         this.socket.send(message);
      };
}

wsserver = new Server();
wssocket = wsserver.connectSocket();
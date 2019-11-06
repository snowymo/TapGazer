   var startTime = Date.now() / 1000, time = startTime, g;
   function drawCanvases(canvases) {
      window.canvases = canvases;
      for (var i = 0 ; i < canvases.length ; i++)
         trackCursor(canvases[i]);
      setInterval(function() {
         var i, canvas, context;
         time = Date.now() / 1000 - startTime;
         for (i = 0 ; i < canvases.length ; i++)
            if ((canvas = canvases[i]).update) {
               g = canvas.getContext('2d');
               g.clearRect(0, 0, canvas.width, canvas.height);
               canvas.update(g);
            }
       }, 30);
   }
   function trackCursor(canvas) {
      canvas.cursor = {x:0, y:0, z:0};
      canvas.setCursor = function(x, y, z) {
         var r = this.getBoundingClientRect();
	 this.cursor.x = x - r.left;
	 this.cursor.y = y - r.top;
	 if (z !== undefined)
	    this.cursor.z = z;
      }
      canvas.onmousedown = function(e) { this.setCursor(e.clientX, e.clientY, 1); }
      canvas.onmousemove = function(e) { this.setCursor(e.clientX, e.clientY   ); }
      canvas.onmouseup   = function(e) { this.setCursor(e.clientX, e.clientY, 0); }
   }
   let rgb = (r, g, b) => 'rgb(' + r + ',' + g + ',' + b + ')';
   let rect = (color, x, y, w, h, isFill) => {
      if (isFill)
	 g.fillStyle = color;
      else
	 g.strokeStyle = color;

      g.beginPath();
      g.moveTo(x  ,y  );
      g.lineTo(x+w,y  );
      g.lineTo(x+w,y+h);
      g.lineTo(x  ,y+h);
      g.lineTo(x  ,y  );

      if (isFill)
         g.fill();
      else
         g.stroke();
   }


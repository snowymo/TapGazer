var fs = require("fs");
var textByLine = [];
var filename = "./words_alpha.txt"; // download from https://github.com/dwyl/english-words
fs.readFile(filename, 'utf8', function(err, data) {
  if (err) throw err;
  console.log('OK: ' + filename);
  textByLine = data.split("\n")
  console.log(textByLine[0]);
  
  // generate textByLine into correct format of js
	var file = fs.createWriteStream('words_alpha.js');
	file.on('error', function(err) { /* error handling */ });
	file.write("let wordList = [\n");
	textByLine.forEach(function(word) { 
		file.write("\'" + word.slice(0, -1) + '\',\n'); 
	});
	file.write("];");
	file.end();
});


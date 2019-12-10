var fs = require("fs");
var textByLine = [];
//var filename = "./words_alpha.txt"; // download from https://github.com/dwyl/english-words
//var filename = "./google-10000-english-usa-no-swears.txt"; // download from https://github.com/first20hours/google-10000-english
var filename = "./30k.txt"; // download from https://github.com/derekchuank/high-frequency-vocabulary

fs.readFile(filename, 'utf8', function(err, data) {
  if (err) throw err;
  console.log('OK: ' + filename);
  textByLine = data.split("\n")
  console.log(textByLine[0]);
  
  // generate textByLine into correct format of js
	var file = fs.createWriteStream('words_alpha.js');
	file.on('error', function(err) { /* error handling */ });
	file.write("let wordList = [\n");
	textByLine.sort(function(a, b){
	  // ASC  -> a.length - b.length
	  // DESC -> b.length - a.length
	  return a.length - b.length || a.localeCompare(b);
	});
	textByLine.forEach(function(word) { 
//		file.write("\'" + word.slice(0, -1) + '\',\n');  // for removing \r
		if(word.length > 0 && !word.startsWith("#")){
			word = word.replace("'","\'").replace(" ","").replace(/\t/g,"");
			file.write("\'" + word + '\',\n'); 
		}
			
	});
	file.write("];");
	file.end();
});


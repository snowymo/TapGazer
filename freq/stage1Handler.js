
const fs = require("fs");
const readline = require('readline');
const lineReader = require('line-reader');

stage1result = {};
  // load random results
  let loadRandomResults = fn => {
    let randomResults = JSON.parse(fs.readFileSync(fn));
    Object.keys(randomResults).forEach(function(key) {
      var entry = randomResults[key];
      var mapping = entry["mapping"];
      if(mapping.length > 0)
          stage1result[mapping.toString()] = entry["score"];
    });
  }
  loadRandomResults("result_common5.txt");
  loadRandomResults("result_common5-2.txt");
  loadRandomResults("result_common5-3.txt");
  loadRandomResults("test.json");
  loadRandomResults("test2.json");
  loadRandomResults("test3.json");
  loadRandomResults("test4.json");

  // load tobii ken result
  let tobii3 = fs.readFileSync("tobii3.txt").toString('utf-8').split("\n");
  tobii3.forEach(line => {
    var entry = line.split(" ");
    if(entry.length == 2){
      stage1result[entry[1].toString()] = parseFloat(entry[0]);
  }
  });

//   lineReader.eachLine('tobii3.txt', function(line) {
//     var entry = line.split(" ");
//     if(entry.length == 2){
//         stage1result[entry[1].toString()] = entry[0];
//     }
// });
// const readInterface = readline.createInterface({
//     input: fs.createReadStream('tobii3.txt'),
//     crlfDelay: Infinity,
//     output: process.stdout,
//     console: false
// });
// readInterface.on('line', function (line) {
//     var entry = line.split(" ");
//     if(entry.length == 2){
//         stage1result[entry[1].toString()] = entry[0];
//     }
//   });

  // load results of mappings
// let seed2result = JSON.parse(fs.readFileSync('seed2.txt'));
// Object.keys(seed2result).forEach(function(key) {
//     var entry = seed2result[key];
//     var mapping = entry["mapping"].split(",");
//     stage1result[mapping.toString()] = entry["score"];
//   });

fs.writeFileSync('stage1.json', JSON.stringify(stage1result));;
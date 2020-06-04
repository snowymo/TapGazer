
var fs = require("fs");

stage1result = {};
// load results of mappings
let seed2result = JSON.parse(fs.readFileSync('seed2.txt'));
Object.keys(seed2result).forEach(function(key) {
    var entry = seed2result[key];
    var mapping = entry["mapping"].split(",");
    stage1result[mapping.toString()] = entry["score"];
  });

  // load random results
  let randomResults = JSON.parse(fs.readFileSync('randomResults.json'));
  Object.keys(randomResults).forEach(function(key) {
    var entry = randomResults[key];
    var mapping = entry["mapping"];
    if(mapping.length > 0)
        stage1result[mapping.toString()] = entry["score"];
  });

fs.writeFileSync('stage1.json', JSON.stringify(stage1result));;
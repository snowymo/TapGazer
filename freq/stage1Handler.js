
const fs = require("fs");
const readline = require('readline');
const lineReader = require('line-reader');

let loadRandomResults = (fn, curStage) => {
  let fileContent = fs.readFileSync(fn).toString('utf-8');
  let randomResults = JSON.parse(fileContent);
  Object.keys(randomResults).forEach(function (key) {
    var entry = randomResults[key];
    var mapping = entry["mapping"];
    if (mapping.length > 0)
    curStage[mapping.toString()] = entry["score"];
  });
}

let loadStage1 = () => {
  stage1result = {};
  // load random results
  
  loadRandomResults("result_common5.txt",stage1result);
  loadRandomResults("result_common5-2.txt",stage1result);
  loadRandomResults("result_common5-3.txt",stage1result);
  loadRandomResults("test.txt",stage1result);
  loadRandomResults("test2.txt",stage1result);
  loadRandomResults("test3.json",stage1result);
  loadRandomResults("test4.json",stage1result);

  // load tobii ken result
  let tobii3 = fs.readFileSync("tobii3.txt").toString('utf-8').split("\n");
  tobii3.forEach(line => {
    var entry = line.split(" ");
    if (entry.length == 2) {
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

  fs.writeFileSync('stage1.json', JSON.stringify(stage1result));
};



// stage2 result
let loadStage2 = () => {
  stage2results = {};
  loadRandomResults("stage2-tobii.txt", stage2results);
  
  fs.writeFileSync('stage2.json', JSON.stringify(stage2results));
};

// loadStage1();
loadStage2();
var fs = require("fs");
var SortedMap = require("collections/sorted-map");
const yargs = require('yargs');

// prepare large vocabulary
var text = fs.readFileSync("./top40k.txt").toString('utf-8');
let wordList = text.split("\n")
for (var i = 0; i < wordList.length; i++) {
   wordList[i] = wordList[i].replace(/\s+/g, '');
}
let vocabulary56k = require("./vocabulary56k.js");

// load frequencies
let wordnfreq = JSON.parse(fs.readFileSync('top40k-freq.json'));
// load finger skill
// fingerSKill["diff"][0-7]
// fingerSkill["same"][prevFinger][curFinger]
let fingerSkill = JSON.parse(fs.readFileSync('fingerSkill.json'));
// tap
let tapTime = 0.277547;
let tapAltTime = 0.1053388228;

// prepare 100 common words
let getMostFrequentWords = () => {
   let f = {};

   f["the"] = 0.56271872;
   f["of"] = 0.33950064;
   f["and"] = 0.29944184;
   f["to"] = 0.25956096;
   f["in"] = 0.17420636;
   f["i"] = 0.11764797;
   f["that"] = 0.11073318;
   f["was"] = 0.10078245;
   f["his"] = 0.08799755;
   f["he"] = 0.08397205;

   f["it"] = 0.08058110;
   f["with"] = 0.07725512;
   f["is"] = 0.07557477;
   f["for"] = 0.07097981;
   f["as"] = 0.07037543;
   f["had"] = 0.06139336;
   f["you"] = 0.06048903;
   f["not"] = 0.05741803;
   f["be"] = 0.05662527;
   f["her"] = 0.05202501;

   f["on"] = 0.05113263;
   f["at"] = 0.05091841;
   f["by"] = 0.05061050;
   f["which"] = 0.04580906;
   f["have"] = 0.04346500;
   f["or"] = 0.04228287;
   f["from"] = 0.04108111;
   f["this"] = 0.04015425;
   f["him"] = 0.03971997;
   f["but"] = 0.03894211;

   f["all"] = 0.03703342;
   f["she"] = 0.03415846;
   f["they"] = 0.03340398;
   f["were"] = 0.03323884;
   f["my"] = 0.03277699;
   f["are"] = 0.03224178;
   f["me"] = 0.03027134;
   f["one"] = 0.02832569;
   f["their"] = 0.02820265;
   f["so"] = 0.02802481;

   f["an"] = 0.02641417;
   f["said"] = 0.02637136;
   f["them"] = 0.02509917;
   f["we"] = 0.02491655;
   f["who"] = 0.02472663;
   f["would"] = 0.02400858;
   f["been"] = 0.02357654;
   f["will"] = 0.02320022;
   f["no"] = 0.02241145;
   f["when"] = 0.01980046;

   f["there"] = 0.01961200;
   f["if"] = 0.01951102;
   f["more"] = 0.01899787;
   f["out"] = 0.01875351;
   f["up"] = 0.01792712;
   f["into"] = 0.01703963;
   f["do"] = 0.01680164;
   f["any"] = 0.01665366;
   f["your"] = 0.01658553;
   f["what"] = 0.01605908;

   f["has"] = 0.01602329;
   f["man"] = 0.01573117;
   f["could"] = 0.01571110;
   f["other"] = 0.01533530;
   f["than"] = 0.01508779;
   f["our"] = 0.01498473;
   f["some"] = 0.01476767;
   f["very"] = 0.01462382;
   f["time"] = 0.01449681;
   f["upon"] = 0.01424595;

   f["about"] = 0.01414687;
   f["may"] = 0.01400642;
   f["its"] = 0.01373270;
   f["only"] = 0.01318367;
   f["now"] = 0.01317723;
   f["like"] = 0.01280625;
   f["little"] = 0.01273589;
   f["then"] = 0.01255636;
   f["can"] = 0.01210074;
   f["should"] = 0.01192154;

   f["made"] = 0.01188501;
   f["did"] = 0.01185720;
   f["us"] = 0.01171742;
   f["such"] = 0.01136757;
   f["a"] = 0.01135294;
   f["great"] = 0.01120163;
   f["before"] = 0.01117089;
   f["must"] = 0.01108116;
   f["two"] = 0.01093366;
   f["these"] = 0.01090510;

   f["see"] = 0.01084286;
   f["know"] = 0.01075612;
   f["over"] = 0.01056659;
   f["much"] = 0.01021822;
   f["down"] = 0.00989808;
   f["after"] = 0.00978575;
   f["first"] = 0.00978196;
   f["good"] = 0.00966602;
   f["men"] = 0.00923053;

   return f;
}

// prepare letter frequency
let letterFrequency = {
   'e': 12.02,
   't': 9.10,
   'a': 8.12,
   'o': 7.68,
   'i': 7.31,
   'n': 6.95,
   's': 6.28,
   'r': 6.02,
   'h': 5.92,
   'd': 4.32,
   'l': 3.98,
   'u': 2.88,
   'c': 2.71,
   'm': 2.61,
   'f': 2.30,
   'y': 2.11,
   'w': 2.09,
   'g': 2.03,
   'p': 1.82,
   'b': 1.49,
   'v': 1.11,
   'k': 0.69,
   'x': 0.17,
   'q': 0.11,
   'j': 0.10,
   'z': 0.07,
};

let large = 100000000;

let random = function () {
   let seed, x, y, z;
   let init = s => {
      seed = s;
      x = (seed % 30268) + 1;
      seed = (seed - (seed % 30268)) / 30268;
      y = (seed % 30306) + 1;
      seed = (seed - (seed % 30306)) / 30306;
      z = (seed % 30322) + 1;
   }
   // seed
   init(2);
   return function (s) {
      if (s !== undefined)
         init(s);
      return (((x = (171 * x) % 30269) / 30269) +
         ((y = (172 * y) % 30307) / 30307) +
         ((z = (170 * z) % 30323) / 30323)) % 1;
   }
}();

let mostFrequentWords = getMostFrequentWords();

let randomInt = n => Math.floor(n * random());
let randomMathInt = n => Math.floor(Math.random() * n);

let wordCount = {}, mapping = [], wordComplete = {}, minInputString = {};
// wordComplete saves incomplete words for each input string, at most 5, according to the word freq

// suggestedMappings = [[ 'os', 'zjkxqap', 'efw', 'mnh', 'vicg', 'ld', 'yur', 'bt' ],
// [ 'fok', 'vbe', 'lzxquw', 'ims', 'tr', 'gya', 'pjd', 'hcn' ],
// [ 'dwfq', 'hnp', 'uzs', 'jyir', 'ckga', 'vbe', 'lt', 'xom' ],
// [ 'figm', 'vs', 'odc', 'zlyu', 'jewr', 'bqt', 'xpa', 'knh' ]];
let potentialMappings = [];
// load potential mappings
let LoadPotentialMappings = () => {
   var maptext = fs.readFileSync("./potentialMappings.txt").toString('utf-8');
   let mappings = maptext.split("\n");
   mappings.forEach(m => {
      curMapping = m.split(",");
      if (curMapping.length > 8)
         curMapping.pop();
      potentialMappings.push(curMapping);
   });
   let bestCount = Math.min(bestMappings.length);
   let entryArray = Array.from(bestMappings.entries());
   for (let i = 0; i < bestCount; i++) {
      potentialMappings.push(entryArray[i][1]);
   }
}

let createRandomMapping = n => {
   //random(n);
   // don't need to start from potential mapping
   // potentialMappings = [];
   // start from potential mapping
   LoadPotentialMappings();
   if (n < potentialMappings.length) {
      mapping = potentialMappings[n];
   } else {
      //random(n);
      mapping = ['', '', '', '', '', '', '', ''];
      for (let i = 0; i < 26; i++)
         mapping[randomMathInt(8)] += String.fromCharCode(97 + i);
   }
}

let modifyMapping = () => {
   let i, j, M = mapping;
   while (i = randomMathInt(8), M[i].length == 0);
   for (j = i; j == i; j = randomMathInt(8));
   let ki = randomMathInt(M[i].length);
   let kj = randomMathInt(M[j].length);
   let ch = M[i].substring(ki, ki + 1);
   M[i] = M[i].substring(0, ki) + M[i].substring(ki + 1, M[i].length);
   M[j] = M[j].substring(0, kj) + ch + M[j].substring(kj, M[j].length);
}

let classifyWord = word => {
   word = word.toLowerCase();
   let s = '';
   for (let n = 0; n < word.length; n++) {
      let ch = word.substring(n, n + 1);
      // validate the word first
      if (ch > 'z' || ch < 'a')
         return "";

      for (let bin = 0; bin < mapping.length; bin++)
         if (mapping[bin].indexOf(ch) >= 0) {
            s += bin;
            break;
         }
   }
   return s;
}

let addIncompleteWord = (curWord, curFingerSeq) => {
   // calculate the min freq in wordComplete[curFingerSeq]
   if (wordComplete[curFingerSeq] === undefined) {
      wordComplete[curFingerSeq] = new SortedMap();
   }
   wordComplete[curFingerSeq].add(curWord, wordnfreq[curWord]);
   if (wordComplete[curFingerSeq].length > 5) {
      wordComplete[curFingerSeq].delete(Array.from(wordComplete[curFingerSeq].entries())[0][0]);
   }
}

let classifyWords = n => {
   wordCount = {}; wordComplete = {}; minInputString = {};
   for (let i = 0; i < wordList.length; i++) {
      let word = wordList[i];
      let c = classifyWord(word);
      minInputString[word] = c;
      if (c == "")
         continue;
      if (wordCount[c] === undefined)
         wordCount[c] = 0;
      wordCount[c]++;
      // for word completion
      if(enableWordComplete){
         for (let ci = 1; ci < c.length-1; ci++) {
            addIncompleteWord(word, c.substr(0, ci));
         }
      }
   }
   // go through all the words again, assign a minimum input string to each word
   // when we compute the score, we will inquiry the minimum input string for cost calculation
   if(enableWordComplete){
      for(var curFingerSeq in wordComplete) {
         var curWordComplete = wordComplete[curFingerSeq];
         let curEntry = Array.from(curWordComplete.entries());
         for (let iwi = wordCount[curFingerSeq]; iwi < 5; iwi++) {
            // fill with incomplete words
            let startIndex = Math.max(0, curEntry.length - (5 - iwi));
            // if curFingerSeq is smaller than the one in dict, update it
            if(minInputString[curEntry[startIndex][1]].length > curFingerSeq.length){
               minInputString[curEntry[startIndex][1]] = curFingerSeq;
               // update wordCount
               wordCount[curFingerSeq]++;
            }
         }
      }
   }   
}

// Ken's computerScore
// let computeScore = i => {
//    let score = 0; // typing time for the 100 common words
//    for (let n = 0 ; n < wordList.length ; n++) {
//       let word = wordList[n];
//       let count = wordCount[classifyWord(word)];
//       if (count > 1)
//          if (count > 5)
// 	         score += 100000000;
//          else {
//             let weight = mostFrequentWords[word];
//          if (weight)
//             score += weight * count;// if we want to calculate the MT, we should use a new equation taking MTfinger and VisualSearch into consideration
//          }
//       else {
//          let weight = mostFrequentWords[word];
//       if (weight)
//          score -= 2 * weight * count;
//       }
//    }
//    return score;
// }

// y = 52x + 227.3 (ms)
let computeVisualSearch = c => {
   // take homograph count as the parameter
   // novice model
   // count = c;
   // return (52.0 * count + 227.3)/1000.0;
   // expert model
   return 0.02679;
}

let computeMT = w => {
   // mapping is global actually...
   word = w;
   moveTime = 0;
   // MT = time for each finger + visual search + tap
   // let fingerSeq = classifyWord(word);
   let fingerSeq = minInputString[word];
   if (fingerSeq == "")
      return 0;
   for (let i = 0; i < fingerSeq.length; i++) {
      if (i == 0 ||
         ((fingerSeq[i - 1] <= '3' && fingerSeq[i] > '3') || (fingerSeq[i - 1] > '3' && fingerSeq[i] <= '3'))) {
         // apply different finger skill
         moveTime += fingerSkill["diff"][fingerSeq[i]];
      } else {
         moveTime += fingerSkill["same"][fingerSeq[i - 1]][fingerSeq[i]];
      }
   }
   return moveTime;
}

let computeScore = (i, strict = true) => {
   let score = 0; // typing time for the 100 common words
   let bHomograph = true;
   for (let n = 0; n < wordList.length; n++) {
      let word = wordList[n];
      if(word.length == 0)
         continue;
      let count = wordCount[classifyWord(word)];
      if (count > 10 ||
         (strict && word in mostFrequentWords && count > 5)) {
         bHomograph = false;
         score = Number.MAX_SAFE_INTEGER;
         break;
      }
      // calculate the MT for each common word and calculate the weighted sum 
      if (count == 1) {
         // save the visual search if there is only one candidate word
         score += (computeMT(word) + tapAltTime) * wordnfreq[word];
      } else {
         score += (computeMT(word) + computeVisualSearch(count) + tapTime) * wordnfreq[word];
      }
      // console.log("score[+" + word + "]=" + score);
   }
   return score;
}

let evaluateMapping = (curMapping) => {
   let originalMapping = mapping.slice();
   mapping = curMapping;

   classifyWords();
   // score
   let curScore = computeScore(0, false);
   // F: calculate letter frequency for each finger
   let F = [0, 0, 0, 0, 0, 0, 0, 0];
   for (let m = 0; m < mapping.length; m++)
      for (let i = 0; i < mapping[m].length; i++) {
         let c = mapping[m].substring(i, i + 1);
         F[m] += letterFrequency[c];
      }
   for (let n = 0; n < F.length; n++)
      F[n] = Math.floor(100 * F[n]);

   // S: sum homograph number for 100 freq words
   let S = '';
   for (let word in mostFrequentWords) {
      let wc = wordCount[classifyWord(word)];
      if (wc > 5)
         S += wc;
   }
   S += '--';
   for (let i = 0; i < vocabulary56k.length; i++) {
      let wc = wordCount[classifyWord(vocabulary56k[i])];
      if (wc > 5)
         S += wc;
   }

   // H: calculate word distribution for homograph from 1 to 5
   let H = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
   for (let n = 0; n < wordList.length; n++) {
      let word = wordList[n];
      H[wordCount[classifyWord(word)]]++;
   }

   console.log(curScore, curMapping.toString());
   console.log("\t# homo", H.toString(), "\tletter", F.toString(), "common", S);

   mapping = originalMapping;
}

let T = 90; // temperture
let tryMapping = (m, iter, anneal) => {
   mapping = m;
   classifyWords();
   let s0 = computeScore(0);
   //console.log(mapping, s0);
   let str = s0;

   // Gradient)Descent))500#itera)ons#
   // each mapping has 500 iterations
   for (let i = 0; i < iter; i++) {
      let saveMapping = mapping.slice();
      modifyMapping();
      classifyWords();
      let s1 = computeScore(i);
      // strict compare
      if (s1 < s0) {
         s0 = s1;
         str += ' ' + s0;
      }
      else
         mapping = saveMapping;
   }

   if (s0 > large)
      return;


   return s0;
}

let annealMapping = (n, m) => {
   mapping = m;
   classifyWords();
   let s0 = computeScore(0);
   //console.log(s0);
   let str = s0;

   let saveMapping = mapping.slice();
   modifyMapping();
   classifyWords();
   let s1 = computeScore(i);

   T = Math.max(20, T);
   var pr = 1 / (1 + Math.exp((s0 - s1) / T--));
   if (Math.random() < pr) {
      s0 = s1;
   } else {
      mapping = saveMapping;
   }
}

let bestMappings = new SortedMap();
let bestScore = 0;
let FindOptimalLayout = (startingLevel) => {
   // start from 5000 random layouts
   if (startingLevel <= 1) {
      console.log("stage 1: Gradient Descent 500 iter for 5000 random layouts");
      for (let n = 0; n < 5000; n++) {
         // console.log(n);
         createRandomMapping(n);
         // Gradient descent 500 iteration
         bestScore = tryMapping(mapping, 500, false);
         // add to list
         if (bestScore < large) {
            //console.log("add to list")
            curMapCopy = JSON.parse(JSON.stringify(mapping));
            bestMappings.add(curMapCopy, bestScore);

            console.log("\"" + n + "\":" + JSON.stringify({ 'mapping': mapping, 'score': bestScore }) + ",");
            // console.log(bestScore, mapping.toString());
         } else {
            console.log("\"" + n + "\":{\"mapping\":[]},");
         }
      }
   }
   if (startingLevel <= 2) {
      // only keep the top 100
      console.log("stage 2: Anneal top 100 for 10 times and Gradient Descent 3000 iter for each layout");
      let bestCount = Math.min(bestMappings.length, 100);
      // bestMappings.sort();
      // let it = bestMappings.entries();
      // let result = it.next();
      // while (!result.done) {
      //  console.log(result.value); // 1 3 5 7 9
      //  result = it.next();
      // }

      let entryArray = Array.from(bestMappings.entries());
      for (let i = 0; i < bestCount; i++) {
         let curEntry = entryArray[i];
         mapping = curEntry[1];
         // anneal 10 times
         T = 90;
         for (let j = 0; j < 10; j++) {
            annealMapping(j, curEntry[1]);
            // then iter 3000 times
            bestScore = tryMapping(mapping, 3000, false);
            // add to list
            if (bestScore < large) {
               //console.log("add to list")
               curMapCopy = JSON.parse(JSON.stringify(mapping));
               bestMappings.add(curMapCopy, bestScore);
               // console.log(bestScore, mapping.toString());
               console.log("\"" + i + "-" + j + "\":" + JSON.stringify({ 'mapping': mapping, 'score': bestScore }) + ",");
            } else {
               console.log("\"" + i + "-" + j + "\":{\"mapping\":[]},");
            }
         }
      }
   }
   if (startingLevel <= 3) {
      // keep the best 10
      console.log("stage 3: Gradient Descent top 10 for 10000 iter");
      bestCount = Math.min(bestMappings.length, 10);

      let entryArray = Array.from(bestMappings.entries());
      for (let i = 0; i < bestCount; i++) {
         let curEntry = entryArray[i];
         mapping = curEntry[1];
         // lastly iter 10000 times
         bestScore = tryMapping(mapping, 10000, false);
         // add to list
         if (bestScore < large) {
            //console.log("add to list")
            curMapCopy = JSON.parse(JSON.stringify(mapping));
            bestMappings.add(curMapCopy, bestScore);
            // console.log(bestScore, mapping.toString());
            console.log("\"" + i + "\":" + JSON.stringify({ 'mapping': mapping, 'score': bestScore }) + ",");
         } else {
            console.log("\"" + i + "\":{\"mapping\":[]},");
         }
      }
      console.log("BEST ONE:" + Array.from(bestMappings.entries())[0]);
   }
}

// === command line parameters ===
const argv = yargs
    .option('startLevel', {
         description: 'the stage level to start',
         alias: 'sl',
         type: 'number',
         default: 1,
    })
    .option('wordComplete', {
        alias: 'wc',
        description: 'Enable word completion or not',
        type: 'boolean',
        default: false,
    })
    .help()
    .alias('help', 'h')
    .argv;

let stageStartLevel = argv.startLevel;
let enableWordComplete = argv.wordComplete;

if (stageStartLevel == 1) {
   // load stage1 result from stage1.json
   let stage1Result = JSON.parse(fs.readFileSync('stage1_wc.json'));
   Object.keys(stage1Result).forEach(function (key) {
      bestMappings.add(key.split(","), stage1Result[key]);
   });
}
else if (stageStartLevel == 2) {
   // load stage1 result from stage1.json
   let stage1Result = JSON.parse(fs.readFileSync('stage1.json'));
   Object.keys(stage1Result).forEach(function (key) {
      bestMappings.add(key.split(","), stage1Result[key]);
   });
}
else if (stageStartLevel == 3) {
   // load stage2result from stage2.json
   let stage2Result = JSON.parse(fs.readFileSync('stage2.json'));
   Object.keys(stage2Result).forEach(function (key) {
      bestMappings.add(key.split(","), stage2Result[key]);
   });
}

// main entry point
FindOptimalLayout(stageStartLevel);

// eval the results
// load stage3result from stage3.json
// let stage3Result = JSON.parse(fs.readFileSync('stage3.json'));
// Object.keys(stage3Result).forEach(function (key) {
//    bestMappings.add(key.split(","), stage3Result[key]);
// });
// let entryArray = Array.from(bestMappings.entries());
// for (let i = 0; i < entryArray.length; i++) {
//    let curEntry = entryArray[i];
//    mapping = curEntry[1];
//    evaluateMapping(mapping);
// }

// KEN
// evaluateMapping([ 'os', 'zjkxqap', 'efw', 'mnh', 'vicg', 'ld', 'yur', 'bt' ]);
//evaluateMapping([ 'fok', 'vbe', 'lzxquw', 'ims', 'tr', 'gya', 'pjd', 'hcn' ]);
//evaluateMapping([ 'dwfq', 'hnp', 'uzs', 'jyir', 'ckga', 'vbe', 'lt', 'xom' ]);
//evaluateMapping([ 'figm', 'vs', 'odc', 'zlyu', 'jewr', 'bqt', 'xpa', 'knh' ]);

// evaluateMapping(['os','zujcxgp','efw','mn','it','ahd','lbqr','vyk']);
// evaluateMapping([ 'os', 'zjkxqap', 'efw', 'mnh', 'vicg', 'ld', 'yur', 'bt' ]);
//evaluateMapping(['oyxk','fbe','jlzgu','dwi','qctr','ma','svp','hn']);

// evaluate the potential mappings
/* let entryArray = Array.from(bestMappings.entries());
for(let i = 0; i < bestMappings.length; i++){
   let curEntry = entryArray[i];
   evaluateMapping(curEntry[1]);
} */


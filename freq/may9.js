var fs = require("fs");

// prepare large vocabulary
var text = fs.readFileSync("./30k.txt").toString('utf-8');
let wordList = text.split("\n")
for(var i = 0; i < wordList.length; i++){
    wordList[i] = wordList[i].replace(/\s+/g, '');
}

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
   'e':12.02,
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

let random = function() {
   let seed, x, y, z;
   let init = s => {
      seed = s;
      x    = (seed % 30268) + 1;
      seed = (seed - (seed % 30268)) / 30268;
      y    = (seed % 30306) + 1;
      seed = (seed - (seed % 30306)) / 30306;
      z    = (seed % 30322) + 1;
   }
   init(2);
   return function(s) {
      if (s !== undefined)
         init(s);
      return ( ((x = (171 * x) % 30269) / 30269) +
               ((y = (172 * y) % 30307) / 30307) +
               ((z = (170 * z) % 30323) / 30323) ) % 1;
   }
}();

let mostFrequentWords = getMostFrequentWords();

let randomInt = n => Math.floor(n * random());

let wordCount = {}, mapping = [];

let createRandomMapping = n => {
   random(n);
   mapping = ['','','','','','','',''];
   for (let i = 0 ; i < 26 ; i++)
      mapping[randomInt(8)] += String.fromCharCode(97 + i);
}

let modifyMapping = () => {
   let i, j, M = mapping;
   while (i = randomInt(8), M[i].length == 0) ;
   for (j = i ; j == i ; j = randomInt(8)) ;
   let ki = randomInt(M[i].length);
   let kj = randomInt(M[j].length);
   let ch = M[i].substring(ki, ki+1);
   M[i] = M[i].substring(0, ki) +      M[i].substring(ki+1, M[i].length);
   M[j] = M[j].substring(0, kj) + ch + M[j].substring(kj  , M[j].length);
}

let classifyWord = word => {
   let s = '';
   for (let n = 0 ; n < word.length ; n++) {
      let ch = word.substring(n, n+1);
      for (let bin = 0 ; bin < mapping.length ; bin++)
         if (mapping[bin].indexOf(ch) >= 0) {
	    s += bin;
	    break;
	 }
   }
   return s;
}

let classifyWords = n => {
   wordCount = {};
   for (let i = 0 ; i < wordList.length ; i++) {
      let word = wordList[i];
      let c = classifyWord(word);
      if (wordCount[c] === undefined)
         wordCount[c] = 0;
      wordCount[c]++;
   }
}


let computeScore = i => {
   let score = 0; // typing time for the 100 common words
   for (let n = 0 ; n < wordList.length ; n++) {
      let word = wordList[n];
      let count = wordCount[classifyWord(word)];
      if (count > 1)
         if (count > 5)
	    score += 100000000;
         else {
            let weight = mostFrequentWords[word];
	    if (weight)
	       score += weight * count;// if we want to calculate the MT, we should use a new equation taking MTfinger and VisualSearch into consideration
         }
      else {
         let weight = mostFrequentWords[word];
	 if (weight)
	    score -= 2 * weight * count;
      }
   }
   return score;
}

let tryMapping = m => {
   mapping = m;
   classifyWords();
   let s0 = computeScore(0);
console.log(Math.floor(1000 * s0));
   let str = s0;

   for (let i = 0 ; i < 1000 ; i++) {
      let saveMapping = mapping.slice();
      modifyMapping();
      classifyWords();
      let s1 = computeScore(i);
      if (s1 < s0) {
	 s0 = s1;
         str += ' ' + s0;
      }
      else
         mapping = saveMapping;
   }

   if (s0 > large)
      return;

   let F = [0,0,0,0,0,0,0,0];
   for (let m = 0 ; m < mapping.length ; m++)
      for (let i = 0 ; i < mapping[m].length ; i++) {
         let c = mapping[m].substring(i,i+1);
         F[m] += letterFrequency[c];
      }
   for (let n = 0 ; n < F.length ; n++)
      F[n] = Math.floor(100 * F[n]);

   classifyWords();

   let S = '';
   for (let word in mostFrequentWords)
      S += wordCount[classifyWord(word)];

   let H = [0,0,0,0,0,0];
   for (let n = 0 ; n < wordList.length ; n++) {
      let word = wordList[n];
      H[wordCount[classifyWord(word)]]++;
   }

   console.log(Math.floor(1000 * s0), mapping, H, F, S);
}


for (let n = 0 ; n < 100 ; n++) {
   console.log(n);
   createRandomMapping(n);
   tryMapping(mapping);
}


//tryMapping([ 'os', 'zjkxqap', 'efw', 'mnh', 'vicg', 'ld', 'yur', 'bt' ]);
//tryMapping([ 'fok', 'vbe', 'lzxquw', 'ims', 'tr', 'gya', 'pjd', 'hcn' ]);
//tryMapping([ 'dwfq', 'hnp', 'uzs', 'jyir', 'ckga', 'vbe', 'lt', 'xom' ]);
//tryMapping([ 'figm', 'vs', 'odc', 'zlyu', 'jewr', 'bqt', 'xpa', 'knh' ]);


let wordCount = {};
for (let n = 0; n < pnp.length; n++) {
   let line = pnp[n];
   if (line.length > 0) {
      let words = line.split(" ");
      for (let i = 0; i < words.length; i++) {
         let word = words[i];
         if (wordCount[word] === undefined) wordCount[word] = 0;
         wordCount[word]++;
      }
   }
}

let keys = "q34tbnu90[";
let S = "";

let wordMap = {};
let possibleWords = [];

let keyboardOffset = 18;
let candOffset = 24;
//'qaz','wsx','edc','rfv','tg','','ujn','ikm','olh','pyb'
//"qaz,wsx,edc,rfv,tg ,ujn,ikm,olh,pyb"
let keyGroups = ["qaz","wsx","edc","rfvtgb","","","yhnuj","mik","ol","p"];
let digits = "123456789";
// this generates the input string from the word in dictionary,
// we probably don't need that anymore because we are using the candidates generation from python
// also because we need to... let me rewrite all the logic maybe
// we need to support word completion. So it is not enough to have a wordToKey, we should have a KeyToWord.
// let wordToKey = word => {
//    let key = "";
//    for (let i = 0; i < word.length; i++) {
//       let j = Math.floor(keyGroups.indexOf(word.charAt(i)) / 4);
//       key += digits.substring(j, j + 1);
//    }
//    return key;
// };
// wordMap will have the map from input string -> word, so let's have that directly from python
// for (let n = 0; n < wordList.length; n++) {
//    let word = wordList[n];
//    let mapKey = wordToKey(word);
//    if (!wordMap[mapKey]) wordMap[mapKey] = [];
//    wordMap[mapKey].push(word);
// }
// // sort by freq, we can sort that first
// for (let key in wordMap)
//    wordMap[key].sort(
//       (a, b) =>
//          (wordCount[b] === undefined ? 0 : wordCount[b]) -
//          (wordCount[a] === undefined ? 0 : wordCount[a])
//    );


   // var test = dictionary["fjd"][0];
   // console.log("TEST TEST:", test);
/*
   let count = 0, best = 0;
   for (let n = 0 ; n < pnp.length ; n++) {
      let line = pnp[n];
      if (line.length > 0) {
         let words = line.split(' ');
	 for (let i = 0 ; i < words.length ; i++) {
	    let word = words[i];
	    if (wordList.includes(word)) {
	       count++;
	       let key = wordToKey(word);
	       if (word == wordMap[key][0])
	          best++;
            }
	 }
      }
   }
   console.log(best / count);
*/
/*
   let histogram = [0,0,0,0,0,0];
   for (let key in wordMap)
      histogram[wordMap[key].length - 1]++;
   console.log(histogram);
*/
// we probably need to show them rows by rows? maybe put the center the one with the most probability
let arrangement = [
   [4],
   [4, 3],
   [4, 3, 5],
   [4, 3, 5, 1],
   [4, 3, 5, 1, 7],
   [4, 3, 5, 1, 7, 0],
   [4, 3, 5, 1, 7, 0, 2],
   [4, 3, 5, 1, 7, 0, 2, 6],
   [4, 3, 5, 1, 7, 0, 2, 6, 8]
];

var defaultIndex = 4;
let reverseArrangement = [];
for(let i = 0; i < 9; i++){
   let currow = [];
   for(let j = 0; j < 9; j++){
      currow.push(defaultIndex);
   }
   for(let j = 0; j < arrangement[i].length; j++){
      currow[arrangement[i][j]] = j;
   }
   reverseArrangement.push(currow);
};

let state = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
let preKeyDown = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

function resetPreKeyDown(){
   for(let i = 0; i < 10; i++){
      preKeyDown[i] = 0;
   }      
}

function updatePreKeyDown(){
   // update if it is not the first key
   // if(mapKey != ""){
      for(let i = 0; i < 10; i++){
         preKeyDown[i] = state[i];
      }      
   // }   
}

canvas1.width = 600;
canvas1.height = 450;
// showing progress should happen here
canvas1.update = g => {
   let w = canvas1.width,
      h = canvas1.height;
   let Y = i =>
      w / keyboardOffset +
      (w / 13) * (0.8 + 3 * Math.pow((i < 5 ? i - 1.5 : 7.5 - i) / 4, 2));
   let drawText = (text, x, y, dx) =>
      context.fillText(text, x - context.measureText(text).width * dx, y);
   let drawColorText = (text, x, y, dx, n) =>{
      context.fillStyle = "green";
      var typedText = text.substr(0,n);
      context.fillText(typedText, x - context.measureText(text).width * dx, y);
      context.fillStyle = "red";
      var guessText = text.substr(n);
      context.fillText(guessText, x - context.measureText(text).width * dx + context.measureText(typedText).width * dx*2, y);
   }
      

   var context = canvas1.getContext("2d");
   context.font = Math.floor(h / 15) + "px Courier";
   context.fillStyle = "black";
   drawText(S, w / 2, h / 19, 1);

   for (let i = 0; i < 10; i++) {
      let right = i >= 5;
      let ix = i + right;
      
      let x = (w / 13) * (i + 1);
      // need to have a dynamic column here
      rect(
         state[i] ? "rgb(200,200,200)" : "rgb(240,240,240)",
         x,
         Y(i) + w / 6,
         w / 13,
         w / 3,//height of each col
         true
      );
      // draw previous key down
      if(i != 5 && !state[i] && preKeyDown[i]){
         rect("green" , x, Y(i) + w / 6, w / 13, w / 3);
      }         
      else{
         rect("black" , x, Y(i) + w / 6, w / 13, w / 3);
      }

      context.fillStyle = "rgb(160,200,255)";
      drawText(
         keys.substring(i, i + 1).toUpperCase(),
         x,
         Y(i) + w / 6 + h / 17 + w / 3,
         -0.7
      );
   }
   context.fillStyle = "black";
   for (let col = 0; col < 10; col++) {
      // let right = col >= 5;
      let ix = col;
      let x = (w / 13) * (col + 1);
      // for (let row = 0; row < 3; row++) {
      //    let ch = keyGroups.charAt(4 * i + row);
      //    context.fillText(
      //       ch,
      //       x + w / 40 + (w / 13) * right,
      //       (row * h) / 15 + Y(ix) + h / 17 + w / 6
      //    );
      // }
      for(let row = 0; row < keyGroups[col].length; row++){
         let ch = keyGroups[col][row];
         context.fillText(
            ch,
            x + w / 40,
            (row * h) / 15 + Y(ix) + h / 17 + w / 6
         );
      }
   }
   if (possibleWords) {
      context.fillStyle = "red";
      let arr = arrangement[possibleWords.length - 1];
      for (let n = 0; n < possibleWords.length; n++) {
         let word = possibleWords[n];
         let x = (w / 4) * ((arr[n] % 3) + 1);
         let y = (h * 1) / 4 + (w / candOffset) * Math.floor(arr[n] / 3);
         // show progress here, green means the progress and red means the completion
         drawColorText(possibleWords[n], x, y, 0.5, mapKey.length);
      }
   }
};

let keyDigits = "asdf  jkl;";

let mapKey = "";
let isDeleteWord = false;
let updateText = () => (text.innerHTML = "<font size=6 face=courier>" + S);
let altKeyState = 0;
let maxKeys = 0,
   keyMap = 0;

window.addEventListener("keydown", e => {
   console.log("keydown " + e.key);
   let n = keys.indexOf(e.key);
   console.log(n);
   
   if (n >= 0) state[n] = 1;
   if (n == 5) altKeyState = 0;
   else altKeyState |= 1 << (n - (n > 5));

   updatePreKeyDown();

   let nKeys = 0;
   for (let i = 0; i < state.length; i++) nKeys += state[i];
   maxKeys = Math.max(maxKeys, nKeys);
   for (let i = 0; i < state.length; i++) keyMap |= state[i] << i;
   // update index every time type down
});

let letterMap = [
   1,
   2,
   4,
   8,
   1 | 2,
   2 | 4,
   4 | 8,
   1 | 4,
   2 | 8,
   1 | 8,
   1 | 2 | 4,
   2 | 4 | 8,
   1 | 2 | 4 | 8
];
let wordMapIndex = defaultIndex;
window.addEventListener("keyup", e => {
   console.log("keyup " + e.key);
   let n = "q34tbnu90[".indexOf(e.key);
   console.log(n);
   if (n >= 0) {
      // update if previous state
      // updatePreKeyDown();

      state[n] = 0;      

      console.log(maxKeys, keyMap);
      let nKeys = 0;
      for (let i = 0; i < state.length; i++) nKeys += state[i];
      if (nKeys == 0) maxKeys = keyMap = 0;

      // enter
      if (n == 5) {
         // zhenyi
         if (possibleWords) {
            // 0 in wordMap[mapKey][0] means the index of the candidates
            // we should receive focus coordinates from tobii 4c
            console.log(
               "wordMapIndex:" + wordMapIndex + " possibleWords:" + possibleWords
            );
            var indexOfWord =
               reverseArrangement[possibleWords.length - 1][wordMapIndex];
            if (indexOfWord > possibleWords.length - 1) {
               indexOfWord = 0;
            }
            S =
               mapKey == ""
                  ? S.substring(0, S.lastIndexOf(" "))
                  : S + (S.length ? " " : "") + dictionary[mapKey][indexOfWord];
            updateText();
         } else {
            console.log("no possibleWords");
         }

         mapKey = "";
         possibleWords = [];
         wordMapIndex = defaultIndex;
      } else if(n == 4 && mapKey.length > 1){
         // delete
         mapKey = mapKey.substr(0, mapKey.length-2);
         possibleWords = dictionary[mapKey]
               ? dictionary[mapKey].slice(
                  0,
                  Math.min(dictionary[mapKey].length, arrangement.length)
               )
               : dictionary[mapKey];
        } 
        else{
         // regular typing
         // let ch = keyDigits.charAt(n);
         // don't understand
         if (state[5]) {
            if (nKeys == 1) {
               if (altKeyState == 16) S += " ";
               else {
                  let k = -1;
                  for (let _k = 0; _k < 13 && k == -1; _k++)
                     if (altKeyState == letterMap[_k]) k = _k;
                     else if (altKeyState == letterMap[_k] << 5) k = _k + 13;
                  if (k >= 0) S += String.fromCharCode(97 + k);
               }
               altKeyState = 0;
            }
         } else {
            mapKey += keyDigits.charAt(n);
            possibleWords = dictionary[mapKey]
               ? dictionary[mapKey].slice(
                  0,
                  Math.min(dictionary[mapKey].length, arrangement.length)
               )
               : dictionary[mapKey];
         }
      }
   }
});
// zhenyi
predefined_coord = {
   0: [194, 200],
   1: [346, 227],
   2: [502, 223],
   3: [196, 275],
   4: [341, 284],
   5: [511, 277]
};

window.addEventListener("gaze", e => {
   console.log("gaze " + e.x + "," + e.y);
   // the current index layout is 1 0 2 \\ 3 4 5
   // the approximate coords are:
   // 0: 133, 396
   // 1: 293, 389
   // 2: 455 407
   // 3: 101, 477
   // 4: 281, 468
   // 5: 439, 478
   var minDis = 35;
   for (var i = 0; i < 6; i++) {
      var dis = Math.sqrt(
         Math.pow(e.x - predefined_coord[i][0], 2) +
         Math.pow(e.y - predefined_coord[i][1], 2)
      );
      console.log(i + " dis " + dis);
      if (dis < minDis) {
         minDis = dis;
         wordMapIndex = i;
      }
   }
   if (minDis == 35) {
      wordMapIndex = defaultIndex;
   }
});

window.addEventListener("focus", e => {
   wordMapIndex = parseInt(e.id);
   if (Number.isNaN(wordMapIndex)) wordMapIndex = defaultIndex;
   console.log("focus " + wordMapIndex);
});
drawCanvases([canvas1]);

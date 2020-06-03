using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

// generate random words from 100-common-word-list
// tell the user what is the next word for selection
// collect data: layout, when the list shows up, when the gaze fixate at the target, when user press with L/R thumb,
// how many words, each word, the size for each word, the target word, the length of the target word, 
// is it correct, the number of the word being gazed before the correct one

public class VSGenerator : MonoBehaviour {
  public class VSItem {
    public LAYOUT_OPTION curLayout;
    public DateTime start;
    public double gazeDuration;
    public double tapDuration;
    public string[] words;
    public string targetWord;
    public bool isCorrect;
    public int history;
    public string toString() {
      string item = curLayout.ToString() + "," + start.ToLongDateString().Replace(',', '-') + "-" + start.ToLongTimeString() + ",";
      int wc = 0;
      for (int i = 0; i < words.Length; i++) {
        item += words[i] + "\t";
        if (words[i].Length > 0)
          ++wc;
      }
      item += "," ;
      for (int i = 0; i < words.Length; i++) {
        item += words[i].Length.ToString() + ",";
      }
      for (int i = words.Length; i <5; i++) {
        item += ",";
      }
      item += wc + "," + Array.IndexOf(words,targetWord) + "," + targetWord + "," + targetWord.Length.ToString() + "," + (isCorrect ? "T" : "F") + "," + history.ToString()
        + "," + gazeDuration.ToString("F3") + "," + tapDuration.ToString("F3");
      return item;
    }

    public VSItem() { }

    public VSItem(LAYOUT_OPTION layout, DateTime startTimer, DateTime gazeTimer, DateTime tapTimer, string[] wordList, string tWord, bool bCorrect, List<GazeItem> historyOfGaze) {
      Set(layout, startTimer, gazeTimer, tapTimer, wordList, tWord, bCorrect, historyOfGaze);
      
    }

    public void Set(LAYOUT_OPTION layout, DateTime startTimer, DateTime gazeTimer, DateTime tapTimer, string[] wordList, string tWord, bool bCorrect, List<GazeItem> historyOfGaze) {
      curLayout = layout;
      start = startTimer;
      gazeDuration = (gazeTimer - startTimer).TotalMilliseconds;
      tapDuration = (tapTimer - gazeTimer).TotalMilliseconds;
      words = new string[wordList.Length];
      for (int i = 0; i < words.Length; i++)
        words[i] = wordList[i];
      targetWord = tWord;
      isCorrect = bCorrect;
      history = historyOfGaze.Count - 1;
      print("gaze " + gazeDuration.ToString("F3") + " tap " + tapDuration.ToString("F3") + " history " + history);
    }
  }

  public enum LAYOUT_OPTION { COLUMN, ROW, CIRCLE };
  public Transform columnLayout, rowLayout, circleLayout;

  public string userName;
  public LAYOUT_OPTION userLayout;
  public string commonWordPath;

  private int commonWordCount;
  private string[] commonWordList;

  private string currentTargetWord;
  private int currentTargetIdx;
  private string[] currentWordList;
  private int currentWordCount;

  public TextMeshPro countdownTimer;
  private BoxCollider countdownCollider;
  private DateTime startTimer, gazeStartTimer, gazeAtCorrect, tapTimer;
  private TimeSpan passedTime;
  [SerializeField]private List<GazeItem> gazeHistory;
  private int gazeThreshold;
  public class GazeItem {
    public int gazeIndex;
    public DateTime gazeTime;
    public GazeItem() { }
    public GazeItem(int gi, DateTime gt) {
      gazeIndex = gi;
      gazeTime = gt;
    }

  }

  private TextMeshPro hintText;

  //public GameObject candPrefab;
  public Transform areaParent;
  [SerializeField] VSCandHandler[] vscands;

  private List<VSItem> results;

  public float verticalOffset;

  private int newHistoryIndex, newHistoryFrameCount;

  // Start is called before the first frame update
  void Start() {
    currentWordList = new string[5];
    hintText = GetComponent<TextMeshPro>();
    startTimer = DateTime.MinValue;
    gazeHistory = new List<GazeItem>();
    results = new List<VSItem>();
    gazeStartTimer = DateTime.MinValue;
    countdownCollider = GetComponent<BoxCollider>();
    newHistoryIndex = -1;
    newHistoryFrameCount = 0;
    gazeThreshold = 5;
    vscands = new VSCandHandler[5];

    if (userLayout == LAYOUT_OPTION.COLUMN)
    {
      circleLayout.gameObject.SetActive(false);
      rowLayout.gameObject.SetActive(false);
      for (int i = 0; i < 5; i++)
        vscands[i] = columnLayout.Find("VSCand" + (i+1).ToString()).GetComponent<VSCandHandler>();
    } else if(userLayout == LAYOUT_OPTION.CIRCLE)
    {
      columnLayout.gameObject.SetActive(false);
      rowLayout.gameObject.SetActive(false);
      for (int i = 0; i < 5; i++)
        vscands[i] = circleLayout.Find("VSCand" + (i+1).ToString()).GetComponent<VSCandHandler>();
    } else if (userLayout == LAYOUT_OPTION.ROW)
    {
      columnLayout.gameObject.SetActive(false);
      circleLayout.gameObject.SetActive(false);
      for (int i = 0; i < 5; i++)
        vscands[i] = rowLayout.Find("VSCand" + (i+1).ToString()).GetComponent<VSCandHandler>();
    }

    loadCommonWords();

    prepare();
  }

  // Update is called once per frame
  void Update() {

    countdown();
    handle();

    adjustVerticalOffest();

    if (Input.GetKeyDown(KeyCode.H)) {
      printHistory();
    }
  }

  void printHistory() {
    print("history count:" + gazeHistory.Count);
    for(int i = 0; i < gazeHistory.Count; i++) {
      print("\tgaze at index " + gazeHistory[i].gazeIndex + " at " + gazeHistory[i].gazeTime.ToLongTimeString());
    }
  }

  void adjustVerticalOffest() {
    if (Input.GetKeyDown(KeyCode.S)) {
      verticalOffset += 0.02f;
    } else if (Input.GetKeyDown(KeyCode.W)) {
      verticalOffset -= 0.02f;
      verticalOffset = Mathf.Max(0, verticalOffset);
    }
    for (int i = 1; i < vscands.Length; i++) {
      if(userLayout == LAYOUT_OPTION.COLUMN)
        vscands[i].transform.localPosition = vscands[0].transform.localPosition + Vector3.back * verticalOffset * i;
      else if(userLayout == LAYOUT_OPTION.ROW)
      {
        // 1.64 is the constant width of word 'environmental'
        // use 60% of it which is
        vscands[i].transform.localPosition = vscands[0].transform.localPosition + Vector3.right * (verticalOffset+1.64f*0.6f) * ((int)(i+1)/2) * (i%2==0?1:-1);
      }
    }
  }

  public void finalizeHistory(string word, string name) {
    // call when obj losts gaze, we can finalize history here
    int index = Array.IndexOf(currentWordList, word);
    if (index != -1) {
      if(Time.frameCount - newHistoryFrameCount <= gazeThreshold && gazeHistory.Count > 2) {
        if(newHistoryIndex == gazeHistory[gazeHistory.Count - 1].gazeIndex) {
          // too short
          print("remove too short gaze: " + word + " " + (DateTime.Now - gazeAtCorrect).TotalMilliseconds);
          //printHistory();
          gazeHistory.RemoveAt(gazeHistory.Count-1);
          //printHistory();
          gazeAtCorrect = gazeHistory[gazeHistory.Count - 1].gazeTime;
        }
      }
    }
  }

  public void AddHistory(string word, string name) {
    int index = Array.IndexOf(currentWordList, word);
    if (index != -1) {
      newHistoryIndex = index;
      newHistoryFrameCount = Time.frameCount;
      //print("start gaze at " + word + " at " + Time.frameCount);
      gazeHistory.Add(new GazeItem(newHistoryIndex, DateTime.Now));
      gazeAtCorrect = DateTime.Now;
    }
  }

  void prepare() {
    // generate the word, 1~5
    generateWords();
    // tell which is the target word
    hintText.text = "<color=#666666>Please find word:</color>\n" + currentTargetWord;
    // instantiate the word[s] but hide them
    updateCand();
    // counting down and then hide the hint info
    countdownTimer.text = "3.0";

  }

  void handle() {
    // gaze and check if 'b' or 'n' is pressed for selection
    if (Input.GetKeyDown(KeyCode.Space)) {
      print("tap space at " + Time.frameCount + "-" + DateTime.Now.ToLongTimeString() + "." + DateTime.Now.Millisecond);
      if(gazeHistory.Count == 0) {
        Debug.LogWarning("haven't gaze at any word");
        return;
      }
      tapTimer = DateTime.Now;
      if((Time.frameCount - newHistoryFrameCount <= gazeThreshold) && (gazeHistory.Count > 2)) {
        print("[space]remove too short gaze: " + currentWordList[gazeHistory[gazeHistory.Count - 1].gazeIndex]);
        gazeHistory.RemoveAt(gazeHistory.Count - 1);
        gazeAtCorrect = gazeHistory[gazeHistory.Count - 1].gazeTime;
      }

      bool isCorrect = currentWordList[gazeHistory[gazeHistory.Count - 1].gazeIndex] == currentTargetWord;

      // add to results
      VSItem curItem = new VSItem(userLayout, gazeStartTimer, gazeAtCorrect, tapTimer, currentWordList, currentTargetWord, isCorrect, gazeHistory);
      results.Add(curItem);

      // reset
      gazeStartTimer = DateTime.MinValue;
      gazeHistory.Clear();
      newHistoryIndex = -1;
      //
      prepare();
    }

    // press Q to save and exit
    if (Input.GetKeyDown(KeyCode.Q)) {
      saveData();
    }
  }

  void saveData() {
    // save to file
    string destination = Application.dataPath + "/Resources/VisualSearch_" + userName + ".csv";
    if (!File.Exists(destination)) {
      File.WriteAllText(destination, "Layout, Timestamp, Words,  word len 0, word len 1, word len 2, word len 3, word len 4, word count,  target index, target word, target word len,correct, history, gaze duration, tap duration\n");
    }

    //Write some text to the file
    for(int i = 0; i < results.Count; i++) {
      File.AppendAllText(destination, results[i].toString()+"\n");
    }
  }

  void updateCand() {
    for (int i = 0; i < currentWordCount; i++) {
      vscands[i].SetText(currentWordList[i], "white");
      //vscands[i].SetColor("white");
    }
    for (int i = currentWordCount; i < 5; i++) {
      vscands[i].SetText("");
      //vscands[i].SetColor("white");
    }
    // show bounding box to indicate where the target word is
    vscands[currentTargetIdx].ShowBorder();
  }

  void countdown() {
    if(countdownTimer.text != "") {
      //countdownCollider.enabled = true;
      if (startTimer == DateTime.MinValue) {
        startTimer = DateTime.Now;
      } 
      passedTime = DateTime.Now - startTimer;
      countdownTimer.text = (3.0f - passedTime.TotalMilliseconds / 1000.0f).ToString("F1");
    }    
    if (passedTime.TotalMilliseconds >= 3000) {
      // hide the clock and show the words
      countdownCollider.enabled = false;
      startTimer = DateTime.MinValue;
      countdownTimer.text = "";
      for (int i = 0; i < 5; i++) {
        vscands[i].SetColor("black");
      }
      if(gazeStartTimer == DateTime.MinValue){
        gazeStartTimer = DateTime.Now;
        if (gazeStartTimer > gazeAtCorrect && gazeHistory.Count > 0) {
          gazeAtCorrect = DateTime.Now;
          gazeHistory[gazeHistory.Count - 1].gazeTime = DateTime.Now;
          Debug.LogWarning("gaze at word:" + currentWordList[gazeHistory[gazeHistory.Count - 1].gazeIndex] + " before shows up");
        }          
      }  
    }
  }

  HashSet<int> curRandIndex = new HashSet<int>();
  void generateWords() {
    currentWordCount = Mathf.Min(5, UnityEngine.Random.Range(1, 10));
    currentWordList = new string[5] { "", "", "", "", "" };
    curRandIndex = new HashSet<int>();
    for (int i = 0; i < currentWordCount; i++) {
      int newIndex = UnityEngine.Random.Range(0, commonWordCount - 1);
      while (curRandIndex.Contains(newIndex)) {
        newIndex = Mathf.Min(commonWordCount-1, UnityEngine.Random.Range(0, commonWordCount - 1));
      }
      curRandIndex.Add(newIndex);
      if (newIndex >= commonWordList.Length)
        Debug.LogWarning("wrong size " + newIndex);
      currentWordList[i] = commonWordList[newIndex];
    }
    currentTargetIdx = currentWordCount - UnityEngine.Random.Range(1, currentWordCount);
    currentTargetWord = currentWordList[currentTargetIdx];

    string debugMsg = "CHOOSE " + currentTargetWord + " FROM ";
    for (int i = 0; i < currentWordCount; i++) {
      debugMsg += currentWordList[i] + "\t";
    }
    Debug.Log(debugMsg);
  }

  void loadCommonWords() {
    if (commonWordPath == "")
      commonWordPath = Application.dataPath + "/Resources/commonword.txt";
    else
      commonWordPath = Application.dataPath + "/Resources/" + commonWordPath;
    using (StreamReader sr = new StreamReader(commonWordPath)) {
      commonWordCount = File.ReadAllLines(commonWordPath).Length;
      commonWordList = new string[commonWordCount];
      string line;
      int t = 0;
      // Read and display lines from the file until the end of the file is reached.
      while ((line = sr.ReadLine()) != null) {
        //here you can put something(like an if) that when it hits the 5th line it will stop reading
        commonWordList[t++] = line;
      }
    }
  }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Measurement : MonoBehaviour {

  public PhraseLoader phraseLoader;

  [SerializeField]
  private float C, INF, IF, F;

  [SerializeField]
  private float totalC, totalINF, totalIF, totalF, MSD, KSPC, CE, PC, NCER, CER, WPM, words;
  [SerializeField]
  private float totalGazeSelection, correctGazeSelection;

  [SerializeField]
  private float typingSeconds;

  [SerializeField]
  private float finishedSeconds;

  private DateTime startTime, endTime;
  private float passedTime;

  public bool allowInput;

  public TMPro.TextMeshPro clock;

  public CandidateHandler candidateHandler;

  public TMPro.TextMeshPro wpmText;


  // Start is called before the first frame update
  void Start() {
    startTime = DateTime.MinValue;
    allowInput = true;
    if (ProfileLoader.typingMode == ProfileLoader.TypingMode.REGULAR) {
      typingSeconds = 60;
    } else if (ProfileLoader.typingMode == ProfileLoader.TypingMode.TEST) {
      typingSeconds = 300;
    } else {
      typingSeconds = 600;
    }
    totalGazeSelection = 0;
    correctGazeSelection = 0;
    passedTime = 0;
  }

  public void AddInputStream(string inputStream) {
    // maybe used by TEST mode
  }

  // Update is called once per frame
  void Update() {
    if (ProfileLoader.typingMode == ProfileLoader.TypingMode.REGULAR) {
      // check backspace, left arrow and right arrow
      if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)) {
        F += 1;
      }
      if (Input.GetKeyDown(KeyCode.Backspace)) {
        IF += 1;
      }
    } else /*if (ProfileLoader.typingMode == ProfileLoader.TypingMode.TEST)*/ {
      if (Input.GetKeyDown(KeyCode.B)) {
        IF += 1;
        F += 1;
      }
    }
    // update the clock
    if (allowInput) {
      float curTime = passedTime + ((startTime == DateTime.MinValue) ? 0f : (float)(DateTime.Now - startTime).TotalSeconds);
      clock.text = ((int)(curTime/60)).ToString("00") + ":" + (curTime% 60).ToString("00");
    } else if (!allowInput)
      clock.text = "<color=red>" + (finishedSeconds / 60).ToString("00") + ":" + (finishedSeconds % 60).ToString("00");

    if (Input.GetKeyDown(KeyCode.Escape)) {
      saveData();
    }

    wpmText.text = "WPM:" + WPM.ToString("F3");
  }

  public void AddWPM(int curWC) {
    words += curWC + 1; // including the 'n' key, aka space
  }

  public void StartClock() {
    if (startTime == DateTime.MinValue)
      startTime = DateTime.Now;
  }

  public void PauseClock() {
    // pause the clock
    passedTime += (float)(DateTime.Now - startTime).TotalSeconds;
    startTime = DateTime.MinValue;
  }

//   public void ResumeClock() {
//     startTime = DateTime.Now;
//   }

  public void UpdateTestMeasure(string presented, string transribed, bool isGazeCorrect) {
    // handle presented, from words to inputString
    C = 0;
    bool isCurrentTypingCorrect = true;
    for (int i = 0; i < Mathf.Min(presented.Length, transribed.Length); i++) {
      if (ProfileLoader.configMap.ContainsKey(presented[i].ToString()) && transribed[i] == (ProfileLoader.configMap[presented[i].ToString()][0]))
        C += 1;
      else
        isCurrentTypingCorrect = false;
    }
    if (isCurrentTypingCorrect) {
      totalGazeSelection += 1;
      if (isGazeCorrect)
        correctGazeSelection += 1;
    }
    //Debug.LogWarning("gaze accuracy:" + correctGazeSelection + "/" + totalGazeSelection);
    INF = transribed.Length - C;
    C += 1; // count the space
    if (isGazeCorrect)
      C += 1;
    else
      INF += 1;
    // calculate C and INFtotalC += C;
    totalC += C;
    totalINF += INF;
    totalIF += IF;
    IF = 0;
    totalF += F;
    F = 0;
    calculateMetric();
    //WPM += 1;// it is possible user deleted words
    endTime = DateTime.Now;
    finishedSeconds = startTime == DateTime.MinValue ? passedTime : (float)(endTime - startTime).TotalSeconds + passedTime;
    //print("finishedSeconds:" + finishedSeconds);
    WPM = (words - 1f) / finishedSeconds * 60.0f / 5.0f;
    if (finishedSeconds > typingSeconds) {
      saveData();

      if (allowInput) {
        allowInput = false;
        Debug.Log("<color=blue>time is up</color>");
      }
    }
  }

  private void saveData() {
    // save to file
    string destination = Application.dataPath + "/Resources/Participants.csv";
    if (!File.Exists(destination)) {
      File.WriteAllText(destination, "Data,Name,C,INF,IF,F,WPM, Correct Gaze, Total Gaze\n");
    }

    //Write some text to the file
    // name should include profile (aka user name), mode (regular, or test), layout and session
    string name = ProfileLoader.profile + "-" + ProfileLoader.typingMode.ToString() + "-" + candidateHandler.candidateLayout.ToString() + "-" + ProfileLoader.session_number.ToString();
    File.AppendAllText(destination, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "," + name + "," + totalC.ToString() + "," + totalINF.ToString() + "," + totalIF.ToString() + "," + totalF.ToString() + "," + WPM.ToString() + ","
        + correctGazeSelection.ToString() + "," + totalGazeSelection.ToString() + "\n");
  }

  private void calculateMetric() {
    // calculate the measurement
    totalC -= 1; // remove the last 'space
    MSD = (totalINF / (totalC + totalINF));
    KSPC = (totalC + totalINF + totalIF + totalF) / (totalC + totalINF);
    if (totalF != 0) CE = totalIF / totalF;
    if (totalIF + totalINF != 0) PC = totalIF / (totalIF + totalINF);
    NCER = totalINF / (totalC + totalINF + totalIF);
    CER = totalIF / (totalC + totalINF + totalIF);

  }

  public void OnRegularInput(TMPro.TMP_InputField inputField) {
    StartClock();
    string curText = inputField.text;
    // we should calculate c, inf, if, f based on curText and the correct answer
    string correctString = phraseLoader.GetCurPhrase();
    // we update all the value only when user hits 'enter' and the word count of curText is equal to correctString
    if (curText.Length > 0 && curText[curText.Length - 1] == ' ') {
      if (curText.Remove(curText.Length - 1).Split(new char[] { ' ' }).Length == correctString.Split(new char[] { ' ' }).Length) {
        // calculate C and INF
        string transribed = curText;// we need to count space curText.Replace(" ", string.Empty);
        string presented = correctString;// we need to count space correctString.Replace(" ", string.Empty);
        INF = editDistance(presented, transribed);
        totalINF += INF;
        C = transribed.Length - INF;
        totalC += C;
        totalIF += IF;
        IF = 0;
        totalF += F;
        F = 0;
        calculateMetric();
        words += transribed.Length;
        endTime = DateTime.Now;
        finishedSeconds = finishedSeconds = startTime == DateTime.MinValue ? passedTime : (float)(endTime - startTime).TotalSeconds + passedTime;
        WPM = (words - 1.0f) / finishedSeconds * 60.0f / 5.0f;
        if (finishedSeconds > typingSeconds) {
          Debug.Log("time is up");
          allowInput = false;
          inputField.enabled = false;

          saveData();
        }
      }
    }
  }

  private int editDistance(string presented, string transribed) {
    // instead of calculating LCS for C, we should calculate MSD(aka edit distance) for INF, and C = transcribed - INF
    int[,] MSDTable = new int[presented.Length + 1, transribed.Length + 1];
    int result = 0;  // To store length of the longest common substring 
    for (int i = 0; i <= presented.Length; i++) {
      for (int j = 0; j <= transribed.Length; j++) {
        // The first row and first column  
        // entries have no logical meaning,  
        // they are used only for simplicity  
        // of program 
        if (i == 0 || j == 0)
          MSDTable[i, j] = 0;
        // If last characters are same, ignore last char 
        // and recur for remaining string 
        else if (presented[i - 1] == transribed[j - 1])
          MSDTable[i, j] = MSDTable[i - 1, j - 1];

        // If the last character is different, consider all 
        // possibilities and find the minimum 
        else
          MSDTable[i, j] = 1 + Mathf.Min(MSDTable[i, j - 1], // Insert 
                             MSDTable[i - 1, j], // Remove 
                             MSDTable[i - 1, j - 1]); // Replace 
      }
    }
    return MSDTable[presented.Length, transribed.Length];
  }

  private int LCSubStr(string Presented, string Transribed) {
    // Create a table to store lengths of longest 
    // common suffixes of substrings.   Note that 
    // LCSuff[i][j] contains length of longest 
    // common suffix of X[0..i-1] and Y[0..j-1].  

    int[,] LCSuff = new int[Presented.Length + 1, Transribed.Length + 1];
    int result = 0;  // To store length of the longest common substring 

    /* Following steps build LCSuff[m+1][n+1] in 
        bottom up fashion. */
    for (int i = 0; i <= Presented.Length; i++) {
      for (int j = 0; j <= Transribed.Length; j++) {
        // The first row and first column  
        // entries have no logical meaning,  
        // they are used only for simplicity  
        // of program 
        if (i == 0 || j == 0)
          LCSuff[i, j] = 0;
        else if (Presented[i - 1] == Transribed[j - 1]) {
          LCSuff[i, j] = LCSuff[i - 1, j - 1] + 1;
          result = Mathf.Max(result, LCSuff[i, j]);
        } else LCSuff[i, j] = 0;
      }
    }
    return result;
  }

  //   void OnGUI() {
  //     GUILayout.BeginArea(new Rect(20, 200, 250, 75));
  //     GUILayout.Label("WPM: " + WPM);
  //     GUILayout.EndArea();
  //     // 
  //     //     GUILayout.BeginArea(new Rect(20, 100, 250, 75));
  //     //     GUILayout.Label("gaze position: " + curgazeScreenCoord);
  //     //     GUILayout.Label("gaze3D position: " + gazeWorldCoord.ToString("F3"));
  //     //     GUILayout.EndArea();
  //   }

}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Measurement : MonoBehaviour
{

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
    public DateTime menuTime;
    private float passedTime;

    public bool allowInput, startMeasure;

    public TMPro.TextMeshPro clock;

    public CandidateHandler candidateHandler;

    public TMPro.TextMeshPro wpmText;

    // record ts for selection
    public class TimeCollection
    {
        public string fingerID;
        public string prevFingerID;
        public double duration;
        public double timestamp;
        public string type;
        public int candCount;
        public int candIndex;
        public TimeCollection(string f, string p, double d, double ts, string t, int cc, int ci)
        {
            fingerID = f;
            prevFingerID = p;
            duration = d;
            timestamp = ts;
            type = t;
            candCount = cc;
            candIndex = ci;
        }
        public string toString()
        {
            return fingerID + "," + prevFingerID + "," + type + "," + duration.ToString("F4") + "," + candCount + "," + candIndex + "," + timestamp.ToString("F4");
        }
    }
    private List<TimeCollection> selectionDuration = new List<TimeCollection>();
    private List<TimeCollection> tapDuration = new List<TimeCollection>();
    private string lastFingerID = "";
    private DateTime lastTapTime = DateTime.MinValue;

    // Start is called before the first frame update
    void Start()
    {
        startTime = DateTime.MinValue;
        menuTime = DateTime.MinValue;
        allowInput = true;
        if (ProfileLoader.typingMode == ProfileLoader.TypingMode.REGULAR || ProfileLoader.typingMode == ProfileLoader.TypingMode.TAPPING)
        {
            typingSeconds = 60;
        }
        if (TapProfileLoader.typingMode == ProfileLoader.TypingMode.REGULAR || TapProfileLoader.typingMode == ProfileLoader.TypingMode.TAPPING)
        {
            typingSeconds = 60;
        }
        else if (ProfileLoader.typingMode == ProfileLoader.TypingMode.TEST)
        {
            typingSeconds = 600;// 300;
        }
        else
        {
            typingSeconds = 600;
        }
        typingSeconds = ProfileLoader.typingSeconds;
        totalGazeSelection = 0;
        correctGazeSelection = 0;
        passedTime = 0;
    }

    public void AddInputStream(string inputStream)
    {
        // maybe used by TEST mode
    }

    // Update is called once per frame
    void Update()
    {
        if (ProfileLoader.typingMode == ProfileLoader.TypingMode.REGULAR)
        {
            // check backspace, left arrow and right arrow
            if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                F += 1;
            }
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                IF += 1;
            }
        }
        else /*if (ProfileLoader.typingMode == ProfileLoader.TypingMode.TEST)*/
        {
            if (Input.GetKeyDown(KeyCode.CapsLock))
            {
                IF += 1;
                F += 1;
            }
        }
        // update the clock
        if (allowInput)
        {
            if (ProfileLoader.typingMode == ProfileLoader.TypingMode.TEST)
            {
                float curTime = passedTime + ((startTime == DateTime.MinValue || menuTime == DateTime.MinValue) ? 0f : (float)(DateTime.Now - startTime).TotalSeconds);
                clock.text = ((int)(curTime / 60)).ToString("00") + ":" + (curTime % 60).ToString("00");
            }
            else
            {

                float curTime = passedTime + ((startTime == DateTime.MinValue) ? 0f : (float)(DateTime.Now - startTime).TotalSeconds);
                clock.text = ((int)(curTime / 60)).ToString("00") + ":" + (curTime % 60).ToString("00");
            }
        }
        else if (!allowInput)
            clock.text = "<color=red>" + (finishedSeconds / 60).ToString("00") + ":" + (finishedSeconds % 60).ToString("00");

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (allowInput)
                saveData();
            Application.Quit();
        }

        wpmText.text = "WPM:" + WPM.ToString("F2");
    }

    public void AddWPM(int curWC)
    {
        if (menuTime != DateTime.MinValue || ProfileLoader.typingMode == ProfileLoader.TypingMode.REGULAR)
            words += curWC + 1; // including the 'n' key, aka space
    }

    public void StartClock()
    {
        if (menuTime == DateTime.MinValue && ProfileLoader.typingMode == ProfileLoader.TypingMode.TEST)
            return;
        if (startTime == DateTime.MinValue)
            startTime = DateTime.Now;
    }

    public void PauseClock()
    {
        if (menuTime == DateTime.MinValue && ProfileLoader.typingMode == ProfileLoader.TypingMode.TEST)
            return;
        // pause the clock
        passedTime += (float)(DateTime.Now - startTime).TotalSeconds;
        startTime = DateTime.MinValue;
        RefreshTimeCollectionWhenNextPhrase();
    }

    //   public void ResumeClock() {
    //     startTime = DateTime.Now;
    //   }

    public void UpdateTestMeasure(string presented, string transribed, bool isGazeCorrect)
    {
        if (menuTime == DateTime.MinValue && ProfileLoader.typingMode == ProfileLoader.TypingMode.TEST)
            return;
        // handle presented, from words to inputString
        C = 0;
        bool isCurrentTypingCorrect = true;
        for (int i = 0; i < Mathf.Min(presented.Length, transribed.Length); i++)
        {
            // iterate all possible configMap
            bool curLetterCorrect = false;
            foreach (string configValue in ProfileLoader.configMap[presented[i].ToString().ToLower()])
            {
                if (ProfileLoader.configMap.ContainsKey(presented[i].ToString())
                && transribed[i] == (configValue[0]))
                {
                    C += 1;
                    curLetterCorrect = true;
                    break;
                }
            }
            isCurrentTypingCorrect = isCurrentTypingCorrect && curLetterCorrect;
            //if (ProfileLoader.configMap.ContainsKey(presented[i].ToString()) 
            //          && transribed[i] == (ProfileLoader.configMap[presented[i].ToString()][0]))
            //  C += 1;
            //else
            //  isCurrentTypingCorrect = false;
        }
        if (isCurrentTypingCorrect)
        {
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
        if (finishedSeconds > typingSeconds)
        {
            if (allowInput)
            {
                saveData();
                allowInput = false;
                Debug.Log("<color=blue>time is up</color>");
            }
        }
    }

    private void saveData()
    {
        // save to file
        string destination = "../Participants.csv";
        if (!File.Exists(destination))
        {
            File.WriteAllText(destination, "Data,Name,C,INF,IF,F,WPM, Correct Gaze, Total Gaze, Method, Gaze, Completion\n");
        }

        //Write some text to the file
        // name should include profile (aka user name), mode (regular, or test), layout and session
        string name = ProfileLoader.profile + "-" + ProfileLoader.typingMode.ToString() + "-" + (ProfileLoader.wcMode == ProfileLoader.WordCompletionMode.WC ? "WC" : "NC")
           + "-" + (ProfileLoader.selectionMode == ProfileLoader.SelectionMode.MS ? "MS" : "GS")
           + "-" + (ProfileLoader.isSimulated ? "SimulatedSetUp" : "RealSetUp")
          + "-" + ProfileLoader.candidateLayout.ToString() + "-" + ProfileLoader.session_number.ToString();
        File.AppendAllText(destination, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "," + name + "," + totalC.ToString() + "," + totalINF.ToString() + "," + totalIF.ToString() + "," + totalF.ToString() + "," + WPM.ToString() + ","
            + correctGazeSelection.ToString() + "," + totalGazeSelection.ToString() + ","
            + (ProfileLoader.typingMode == ProfileLoader.TypingMode.REGULAR ? "QWERTY,,"
            : ("TapGazer,"
              + (ProfileLoader.selectionMode == ProfileLoader.SelectionMode.MS ? "MS" : "GS") + ","
              + (ProfileLoader.wcMode == ProfileLoader.WordCompletionMode.WC ? "WC" : "NC"))) + "\n");

        // save time data
        string dest2 = "../time" + name + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".csv";
        File.WriteAllText(dest2, "participant, method, gaze, completion, cur_finger, prev_finger, type, duration_ms, cand_total, cand_index, time_since_1970_ms\n");
        //for (int i = 0; i < selectionDuration.Count; i++)
        //{
        //  File.AppendAllText(dest2, selectionDuration[i].fingerID + "," + selectionDuration[i].prevFingerID + "," + selectionDuration[i].duration.ToString() + "\n");
        //}

        // save tap collection to one file per user
        string destination3 = "../TapCollection_" + ProfileLoader.profile + ".csv";
        if (!File.Exists(destination3))
        {
            File.WriteAllText(destination3, "participant, method, gaze, completion, cur_finger, prev_finger, type, duration_ms, cand_total, cand_index, time_since_1970_ms\n");
        }
        else
        {
            File.AppendAllText(destination3, "new session\n");
        }

        for (int i = 0; i < tapDuration.Count; i++)
        {
            File.AppendAllText(dest2, ProfileLoader.profile + ","
              + (ProfileLoader.typingMode == ProfileLoader.TypingMode.REGULAR ? "QWERTY,,"
              : ("TapGazer,"
                + (ProfileLoader.selectionMode == ProfileLoader.SelectionMode.MS ? "MS" : "GS") + ","
                + (ProfileLoader.wcMode == ProfileLoader.WordCompletionMode.WC ? "WC" : "NC"))) + ","
                + tapDuration[i].toString() + "\n");
            File.AppendAllText(destination3, ProfileLoader.profile + ","
              + (ProfileLoader.typingMode == ProfileLoader.TypingMode.REGULAR ? "QWERTY,,"
              : ("TapGazer,"
                + (ProfileLoader.selectionMode == ProfileLoader.SelectionMode.MS ? "MS" : "GS") + ","
                + (ProfileLoader.wcMode == ProfileLoader.WordCompletionMode.WC ? "WC" : "NC"))) + ","
                + tapDuration[i].toString() + "\n");
        }
    }

    private void calculateMetric()
    {
        // calculate the measurement
        totalC -= 1; // remove the last 'space
        MSD = (totalINF / (totalC + totalINF));
        KSPC = (totalC + totalINF + totalIF + totalF) / (totalC + totalINF);
        if (totalF != 0) CE = totalIF / totalF;
        if (totalIF + totalINF != 0) PC = totalIF / (totalIF + totalINF);
        NCER = totalINF / (totalC + totalINF + totalIF);
        CER = totalIF / (totalC + totalINF + totalIF);

    }

    public void OnRegularInput(TMPro.TMP_InputField inputField)
    {
        StartClock();
        string curText = inputField.text;
        // we should calculate c, inf, if, f based on curText and the correct answer
        string correctString = phraseLoader.GetCurPhrase();
        // we update all the value only when user hits 'enter' and the word count of curText is equal to correctString
        if (curText.Length > 0 && curText[curText.Length - 1] == ' ')
        {
            if (curText.Remove(curText.Length - 1).Split(new char[] { ' ' }).Length == correctString.Split(new char[] { ' ' }).Length)
            {
                if (menuTime != DateTime.MinValue || ProfileLoader.typingMode == ProfileLoader.TypingMode.REGULAR)
                {
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
                    if (finishedSeconds > typingSeconds)
                    {
                        Debug.Log("time is up");
                        inputField.enabled = false;
                        if (allowInput)
                        {
                            allowInput = false;
                            saveData();
                        }
                    }
                }
            }
        }
    }

    public void OnRegularInput(string curInput)
    {
        StartClock();
        string curText = curInput;
        // we should calculate c, inf, if, f based on curText and the correct answer
        string correctString = phraseLoader.GetCurPhrase();
        // we update all the value only when user hits 'enter' and the word count of curText is equal to correctString
        if (curText.Length > 0 && curText[curText.Length - 1] == ' ')
        {
            if (curText.Remove(curText.Length - 1).Split(new char[] { ' ' }).Length == correctString.Split(new char[] { ' ' }).Length)
            {
                if (menuTime != DateTime.MinValue || ProfileLoader.typingMode == ProfileLoader.TypingMode.REGULAR)
                {
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
                    if (finishedSeconds > typingSeconds)
                    {
                        Debug.Log("time is up");
                        allowInput = false;

                        saveData();
                    }
                }
            }
        }
    }

    private int editDistance(string presented, string transribed)
    {
        // instead of calculating LCS for C, we should calculate MSD(aka edit distance) for INF, and C = transcribed - INF
        int[,] MSDTable = new int[presented.Length + 1, transribed.Length + 1];
        int result = 0;  // To store length of the longest common substring 
        for (int i = 0; i <= presented.Length; i++)
        {
            for (int j = 0; j <= transribed.Length; j++)
            {
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

    private int LCSubStr(string Presented, string Transribed)
    {
        // Create a table to store lengths of longest 
        // common suffixes of substrings.   Note that 
        // LCSuff[i][j] contains length of longest 
        // common suffix of X[0..i-1] and Y[0..j-1].  

        int[,] LCSuff = new int[Presented.Length + 1, Transribed.Length + 1];
        int result = 0;  // To store length of the longest common substring 

        /* Following steps build LCSuff[m+1][n+1] in 
            bottom up fashion. */
        for (int i = 0; i <= Presented.Length; i++)
        {
            for (int j = 0; j <= Transribed.Length; j++)
            {
                // The first row and first column  
                // entries have no logical meaning,  
                // they are used only for simplicity  
                // of program 
                if (i == 0 || j == 0)
                    LCSuff[i, j] = 0;
                else if (Presented[i - 1] == Transribed[j - 1])
                {
                    LCSuff[i, j] = LCSuff[i - 1, j - 1] + 1;
                    result = Mathf.Max(result, LCSuff[i, j]);
                }
                else LCSuff[i, j] = 0;
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
    private static DateTime JanFirst1970 = new DateTime(1970, 1, 1);
    public void AddTapItem(string fingerID, string type, int candCount = 0, int candIndex = 0)
    {
        if (lastTapTime == DateTime.MinValue)
        {
            tapDuration.Add(new TimeCollection(fingerID, lastFingerID,
              0, (DateTime.Now - JanFirst1970).TotalMilliseconds, type, candCount, candIndex));
        }
        else
        {
            tapDuration.Add(new TimeCollection(fingerID, lastFingerID,
              (DateTime.Now - lastTapTime).TotalMilliseconds, (DateTime.Now - JanFirst1970).TotalMilliseconds, type, candCount, candIndex));
        }
        if (fingerID == "menu")
        {
            tapDuration.Add(new TimeCollection(fingerID, "",
              (menuTime - JanFirst1970).TotalMilliseconds, (DateTime.Now - JanFirst1970).TotalMilliseconds, type, candCount, candIndex));
            return;
        }
        lastFingerID = fingerID;
        lastTapTime = DateTime.Now;
    }

    //public void AddSelectionDurationItem()
    //{
    //  if (lastTapTime == DateTime.MinValue)
    //  {
    //    selectionDuration.Add(new TimeCollection("n", lastFingerID, 0, "selection"));
    //  }
    //  else
    //  {
    //    selectionDuration.Add(new TimeCollection("n", lastFingerID, (DateTime.Now - lastTapTime).TotalMilliseconds, "selection"));
    //  }
    //  lastFingerID = "n";
    //  lastTapTime = DateTime.Now;
    //}

    //public void AddTapDurationItem(string fingerID)
    //{
    //  if (lastTapTime == DateTime.MinValue)
    //  {
    //    tapDuration.Add(new TimeCollection(fingerID, lastFingerID, 0, "tap"));
    //  }
    //  else
    //  {
    //    tapDuration.Add(new TimeCollection(fingerID, lastFingerID, (DateTime.Now - lastTapTime).TotalMilliseconds, "tap"));
    //  }
    //  lastFingerID = fingerID;
    //  lastTapTime = DateTime.Now;
    //}

    public void RefreshTimeCollectionWhenNextPhrase()
    {
        lastFingerID = "";
        lastTapTime = DateTime.MinValue;
    }
}

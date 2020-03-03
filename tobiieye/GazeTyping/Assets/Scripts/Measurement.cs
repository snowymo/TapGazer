using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Measurement : MonoBehaviour
{

    public PhraseLoader phraseLoader;

    [SerializeField]
    private float C, INF, IF, F;

    [SerializeField]
    private float totalC, totalINF, totalIF, totalF, MSD, KSPC, CE, PC, NCER, CER, WPM;

    [SerializeField]
    private float typingSeconds = 20;

    private DateTime startTime, endTime;

    public bool allowInput;

    // Start is called before the first frame update
    void Start()
    {
        startTime = DateTime.MinValue;
        allowInput = true;
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
        else if(ProfileLoader.typingMode == ProfileLoader.TypingMode.TEST)
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                IF += 1;
                F += 1;
            }
        }
    }

    public void AddWPM(int curWC)
    {
        WPM += curWC;
    }

    public void StartClock()
    {
        if (startTime == DateTime.MinValue)
            startTime = DateTime.Now;
    }

    public void UpdateTestMeasure(string presented, string transcript, bool isGazeCorrect)
    {
        // handle presented, from words to inputString
        C = 0;
        for(int i = 0; i < Mathf.Min(presented.Length, transcript.Length); i++)
        {
            if (transcript[i] == (ProfileLoader.configMap[presented[i].ToString()][0]))
                C += 1;
        }
        INF = transcript.Length- C;
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
        if ((endTime - startTime).Seconds > typingSeconds)
        {
            allowInput = false;
            Debug.Log("<color=blue>time is up</color>");
            WPM = WPM / ((endTime - startTime).Seconds / 60.0f);
        }
    }

    private void calculateMetric()
    {
        // calculate the measurement
        MSD = (totalINF / (totalC + totalINF));
        KSPC = (totalC + totalINF + totalIF + totalF) / (totalC + totalINF);
        if (totalF != 0) CE = totalIF / totalF;
        if (totalIF + totalINF != 0) PC = totalIF / (totalIF + totalINF);
        NCER = totalINF / (totalC + totalINF + totalIF);
        CER = totalIF / (totalC + totalINF + totalIF);
        
    }

    public void OnRegularInput(TMPro.TMP_InputField inputField)
    {
        if(startTime == DateTime.MinValue)
            startTime = DateTime.Now;
        string curText = inputField.text;
        // we should calculate c, inf, if, f based on curText and the correct answer
        string correctString = phraseLoader.GetCurPhrase();
        // we update all the value only when user hits 'enter' and the word count of curText is equal to correctString
        if (curText.Length > 0 && curText[curText.Length - 1] == ' ')
        {
            if (curText.Remove(curText.Length - 1).Split(new char[] { ' ' }).Length == correctString.Split(new char[] { ' ' }).Length)
            {
                // calculate C and INF
                string transcript = curText.Replace(" ", string.Empty);
                string presented = correctString.Replace(" ", string.Empty);
                C = LCSubStr(transcript, presented);
                totalC += C;
                INF = presented.Length - C + transcript.Length - C;
                totalINF += INF;
                totalIF += IF;
                IF = 0;
                totalF += F;
                F = 0;
                calculateMetric();
                WPM += correctString.Split(new char[] { ' ' }).Length;
                endTime = DateTime.Now;
                if ((endTime - startTime).Seconds > typingSeconds)
                {
                    inputField.enabled = false;
                    WPM = WPM / ((endTime - startTime).Seconds / 60.0f);
                }
            }
        }
        
    }

    private int LCSubStr(string Presented, string Transcript)
    {
        // Create a table to store lengths of longest 
        // common suffixes of substrings.   Note that 
        // LCSuff[i][j] contains length of longest 
        // common suffix of X[0..i-1] and Y[0..j-1].  

        int[,] LCSuff = new int[Presented.Length + 1, Transcript.Length + 1];
        int result = 0;  // To store length of the longest common substring 

        /* Following steps build LCSuff[m+1][n+1] in 
            bottom up fashion. */
        for (int i = 0; i <= Presented.Length; i++)
        {
            for (int j = 0; j <= Transcript.Length; j++)
            {
                // The first row and first column  
                // entries have no logical meaning,  
                // they are used only for simplicity  
                // of program 
                if (i == 0 || j == 0)
                    LCSuff[i, j] = 0;
                else if (Presented[i - 1] == Transcript[j - 1])
                {
                    LCSuff[i, j] = LCSuff[i - 1, j - 1] + 1;
                    result = Mathf.Max(result, LCSuff[i, j]);
                }
                else LCSuff[i, j] = 0;
            }
        }
        return result;
    }

}

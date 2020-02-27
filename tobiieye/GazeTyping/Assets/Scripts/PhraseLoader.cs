using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PhraseLoader : MonoBehaviour
{
    public string[] phrases;
    public int phraseCount;
    public TMPro.TextMeshPro textMesh;

    private int curPhraseIndex;
    [SerializeField]
    private string[] curPhrases;
    private int curTypingPhrase;
    private bool isNewPhrase;

    public string phrasePath;

    public bool IsNewPhrase()
    {
        bool temp = isNewPhrase;
        isNewPhrase = false;
        return temp;
    }

    
    // Start is called before the first frame update
    void Start()
    {
        // load phrase file into string lists, line by line
        if (phrasePath == "")
            phrasePath = Application.dataPath + "/Resources/phrases2.txt";
        else
            phrasePath = Application.dataPath + "/Resources/" + phrasePath;
        using (StreamReader sr = new StreamReader(phrasePath)) {
            phraseCount = File.ReadAllLines(phrasePath).Length;
            phrases = new string[phraseCount];
            string line;
            int t = 0;
            // Read and display lines from the file until the end of the file is reached.
            while ((line = sr.ReadLine()) != null) {
                //here you can put something(like an if) that when it hits the 5th line it will stop reading
                phrases[t++] = line;
            }
        }
        curPhraseIndex = 0;
        curTypingPhrase = 0;
        UpdatePhrase();
    }

    public void NextPhrases()
    {
        ++curPhraseIndex;
        curTypingPhrase = 0;
        UpdatePhrase();
        isNewPhrase = true;
    }

    public void PrevPhrases()
    {
        --curPhraseIndex;
        UpdatePhrase();
    }

    public void UpdatePhrase()
    {
        // update current phrases (5 phrases) to TextMesh
        curPhraseIndex = Mathf.Max(0, curPhraseIndex);
        curPhraseIndex = Mathf.Min(phraseCount-1, curPhraseIndex);
        ColorCurrentTypingPhrase();
    }

    private void ColorCurrentTypingPhrase(ProfileLoader.TypingMode typingMode = ProfileLoader.TypingMode.TRAINING)
    {
        string curText = phrases[curPhraseIndex];
        curPhrases = curText.Split(new char[] { ' ' });
        string arrowText = "";
        string newText = "";
        for(int i = 0; i < curTypingPhrase; i++) {
            arrowText += "<color=white>" + curPhrases[i] + "</color> ";
            if(typingMode == ProfileLoader.TypingMode.TRAINING)
                newText += "<color=green>" + curPhrases[i] + "</color> ";
            else
                newText += "<color=#c3c3c3>" + curPhrases[i] + "</color> ";
        }
        if(curTypingPhrase < curPhrases.Length)
        {
            arrowText += "<color=blue>";
            for(int i = 0; i < curPhrases[curTypingPhrase].Length/2; i++)
            {
                arrowText += " ";
            }
            arrowText += "↓</color>";
            newText += "<color=blue>" + curPhrases[curTypingPhrase] + "</color> ";
        }
        for (int i = curTypingPhrase+1; i<curPhrases.Length; i++) {
            newText += curPhrases[i] + " ";
        }
        textMesh.text = arrowText + "\n" + newText;
    }

    public bool IsCurrentTypingCorrect(string candidate, ProfileLoader.TypingMode typingMode)
    {
        // typingMode=training, then curPhraseIndex won't move until correct
        // typingMode=test, curPhraseIndex will move to the next one anyway
        if(typingMode == ProfileLoader.TypingMode.TRAINING) {
            if (candidate.Equals(curPhrases[curTypingPhrase], System.StringComparison.InvariantCultureIgnoreCase)) {
                // move to next word
                if (curTypingPhrase < (curPhrases.Length - 1)) {
                    ++curTypingPhrase;
                    ColorCurrentTypingPhrase();
                    Debug.Log("next word");
                }
                else {
                    // move to next phrase
                    Debug.Log("next phrase");
                    NextPhrases();
                    ColorCurrentTypingPhrase();
                }
                return true;
            }
            else {
                // just wrong
                return false;
            }
        }
        else {
            bool result = candidate.Equals(curPhrases[curTypingPhrase], System.StringComparison.InvariantCultureIgnoreCase);
            if (curTypingPhrase < (curPhrases.Length - 1)) {
                // move to next word
                ++curTypingPhrase;
                Debug.Log("next word");
            }
            else {
                // move to next phrase
                Debug.Log("next phrase");
                NextPhrases();
            }
            ColorCurrentTypingPhrase(typingMode);
            return result;
        }
    }

    private void Update()
    {
        // test NextPhrases
        if (Input.GetKeyDown(KeyCode.Space)) {
            NextPhrases();
        }else if (Input.GetKeyDown(KeyCode.Backspace)) {
            PrevPhrases();
        }
    }
}

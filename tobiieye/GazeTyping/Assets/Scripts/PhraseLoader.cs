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

    public string phrasePath;

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

    private void ColorCurrentTypingPhrase()
    {
        string curText = phrases[curPhraseIndex];
        curPhrases = curText.Split(new char[] { ' ' });
        string newText = "";
        for(int i = 0; i < curTypingPhrase; i++) {
            newText += "<color=green>" + curPhrases[i] + "</color> ";
        }
        if(curTypingPhrase < curPhrases.Length)
            newText += "<color=blue>" + curPhrases[curTypingPhrase] + "</color> ";
        for (int i = curTypingPhrase+1; i<curPhrases.Length; i++) {
            newText += curPhrases[i] + " ";
        }
        textMesh.text = newText; 
    }

    public bool IsCurrentTypingCorrect(string candidate)
    {
        if(candidate.Equals(curPhrases[curTypingPhrase], System.StringComparison.InvariantCultureIgnoreCase)) {
            // move to next word
            if(curTypingPhrase < (curPhrases.Length-1)) {
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

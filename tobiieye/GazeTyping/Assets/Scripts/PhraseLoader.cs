using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PhraseLoader : MonoBehaviour
{
    public string[] phrases;
    public int phraseCount;
    public TextMesh textMesh;

    private int curPhraseIndex;

    // Start is called before the first frame update
    void Start()
    {
        // load phrase file into string lists, line by line
        string phrasePath = "Assets/Resources/phrases2.txt";        
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
        UpdatePhrase();
    }

    public void NextPhrases()
    {
        ++curPhraseIndex;
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
        textMesh.text = phrases[curPhraseIndex];
    }

    private void Update()
    {
        // test NextPhrases
        if (Input.GetKeyDown("n")) {
            NextPhrases();
        }else if (Input.GetKeyDown("p")) {
            PrevPhrases();
        }
    }
}

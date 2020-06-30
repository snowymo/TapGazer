using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PhraseLoader : MonoBehaviour {
  public string[] phrases;
  public int phraseCount;
  public TMPro.TextMeshPro textMesh;

  private int curPhraseIndex;
  [SerializeField]
  private string[] curPhrases;
  private int curTypingPhrase;
  private bool isNewPhrase;

  public string phrasePath;

  public Measurement measurement;
  public CandidateHandler candidateHandler;

  public string GetCurPhrase() {
    return phrases[curPhraseIndex];
  }

  public string GetCurWord() {
    return curPhrases[curTypingPhrase];
  }

  public bool IsNewPhrase() {
    bool temp = isNewPhrase;
    isNewPhrase = false;
    return temp;
  }

  public void PreviousWord() {
    --curTypingPhrase;
    ColorCurrentTypingPhrase(ProfileLoader.TypingMode.TEST);
  }


  // Start is called before the first frame update
  void Start() {
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
    // curPhraseIndex is decided by mode, layout, and session
    curPhraseIndex = 0;
    if (ProfileLoader.typingMode == ProfileLoader.TypingMode.REGULAR || ProfileLoader.typingMode == ProfileLoader.TypingMode.TAPPING)
      curPhraseIndex = 0;
    else if (ProfileLoader.typingMode == ProfileLoader.TypingMode.TEST) {
      if ((candidateHandler.candidateLayout == CandidateHandler.CandLayout.ROW) || (candidateHandler.candidateLayout == CandidateHandler.CandLayout.LEXIC)) {
        curPhraseIndex = ProfileLoader.session_number * 100; // 100 or 200
      } else if (candidateHandler.candidateLayout == CandidateHandler.CandLayout.WORDCLOUD) {
        curPhraseIndex = 200 + ProfileLoader.session_number * 100; // 300 or 400
      }
    }
    curTypingPhrase = 0;
    UpdatePhrase();
  }

  public void NextPhrases() {
    ++curPhraseIndex;
    curTypingPhrase = 0;
    UpdatePhrase();
    isNewPhrase = true;
    // pause the clock
    measurement.PauseClock();
  }

  public void PrevPhrases() {
    --curPhraseIndex;
    UpdatePhrase();
    // pause the clock
    measurement.PauseClock();
  }

  public void UpdatePhrase() {
    // update current phrases to TextMesh
    curPhraseIndex = Mathf.Max(0, curPhraseIndex);
    curPhraseIndex = Mathf.Min(phraseCount - 1, curPhraseIndex);
    ColorCurrentTypingPhrase();
  }

  public bool IsCurrentTypingCorrect(string curTyping) {
    // only for regular
    // separate candidate into a string array, compare them one by one and colorize it
    string[] currentTypedWords;
    bool doCheck = false;
    if (curTyping.EndsWith(" ")) {
      doCheck = true;
      curTyping = curTyping.Remove(curTyping.Length - 1);
    }
    currentTypedWords = curTyping.Split(new char[] { ' ' });
    curTypingPhrase = currentTypedWords.Length;
    //Debug.Log("curTypingPhrase:" + curTypingPhrase);

    string curText = phrases[curPhraseIndex];
    curPhrases = curText.Split(new char[] { ' ' });

    string newText = "", arrowText = "";
    //Debug.Log("doCheck:" + doCheck);
    curTypingPhrase = doCheck ? curTypingPhrase : curTypingPhrase - 1;
    for (int wordIndex = 0; wordIndex < curTypingPhrase; wordIndex++) {
      bool curResult = currentTypedWords[wordIndex].Equals(curPhrases[wordIndex], System.StringComparison.InvariantCultureIgnoreCase);
      arrowText += "<color=white>" + curPhrases[wordIndex] + "</color> ";
      newText += (curResult ? "<color=#c3c3c3>" : "<color=red>") + curPhrases[wordIndex] + "</color> ";
    }
    if (curTypingPhrase < curPhrases.Length) {
      arrowText += "<color=blue>";
      for (int i = 0; i < curPhrases[curTypingPhrase].Length / 2; i++) {
        arrowText += " ";
      }
      arrowText += "↓</color>";
      newText += "<color=blue>" + curPhrases[curTypingPhrase] + "</color> ";
    }
    for (int i = curTypingPhrase + 1; i < curPhrases.Length; i++) {
      newText += curPhrases[i] + " ";
    }
    textMesh.text = arrowText + "\n" + newText;

    // check if next phrase
    if (doCheck && curTypingPhrase == (curPhrases.Length)) {
      NextPhrases();
      return true;
    }
    return false;
  }

  private void ColorCurrentTypingPhrase(ProfileLoader.TypingMode typingMode = ProfileLoader.TypingMode.TRAINING) {
    string curText = phrases[curPhraseIndex];
    curPhrases = curText.Split(new char[] { ' ' });
    string arrowText = "";
    string newText = "";
    for (int i = 0; i < curTypingPhrase; i++) {
      arrowText += "<color=white>" + curPhrases[i] + "</color> ";
      if (typingMode == ProfileLoader.TypingMode.TRAINING)
        newText += "<color=green>" + curPhrases[i] + "</color> ";
      else if (typingMode == ProfileLoader.TypingMode.TEST || typingMode == ProfileLoader.TypingMode.TAPPING)
        newText += "<color=#c3c3c3>" + curPhrases[i] + "</color> ";
    }
    if (curTypingPhrase < curPhrases.Length) {
      arrowText += "<color=blue>";
      for (int i = 0; i < curPhrases[curTypingPhrase].Length / 2; i++) {
        arrowText += " ";
      }
      arrowText += "↓</color>";
      newText += "<color=blue>" + curPhrases[curTypingPhrase] + "</color> ";
    }
    for (int i = curTypingPhrase + 1; i < curPhrases.Length; i++) {
      newText += curPhrases[i] + " ";
    }
    textMesh.text = arrowText + "\n" + newText;
  }

  public bool IsCurrentTypingCorrect(string candidate, ProfileLoader.TypingMode typingMode) {
    // typingMode=training, then curPhraseIndex won't move until correct
    // typingMode=test, curPhraseIndex will move to the next one anyway
    if (typingMode == ProfileLoader.TypingMode.TRAINING) {
      measurement.AddWPM(curPhrases[curTypingPhrase].Length);
      if (candidate.Equals(curPhrases[curTypingPhrase], System.StringComparison.InvariantCultureIgnoreCase)) {
        // move to next word
        if (curTypingPhrase < (curPhrases.Length - 1)) {
          ++curTypingPhrase;
          ColorCurrentTypingPhrase();
          Debug.Log("next word");
        } else {
          // move to next phrase
          Debug.Log("next phrase");
          NextPhrases();
          ColorCurrentTypingPhrase();
        }
        return true;
      } else {
        // just wrong
        return false;
      }
    } else if (typingMode == ProfileLoader.TypingMode.TEST || typingMode == ProfileLoader.TypingMode.TAPPING) {
      bool result = candidate.Equals(curPhrases[curTypingPhrase], System.StringComparison.InvariantCultureIgnoreCase);
      if (curTypingPhrase < (curPhrases.Length - 1)) {
        measurement.AddWPM(curPhrases[curTypingPhrase].Length);
        // move to next word
        ++curTypingPhrase;
        //Debug.Log("next word");
      } else {
        // move to next phrase
        //Debug.Log("next phrase");
        measurement.AddWPM(curPhrases[curTypingPhrase].Length);
        NextPhrases();
      }
      ColorCurrentTypingPhrase(typingMode);
      return result;
    } else if (typingMode == ProfileLoader.TypingMode.REGULAR) {
      // should not go to here
      return true;
    } else {
      return false;
    }
  }

  private void Update() {
    // DEBUG test NextPhrases
    if (ProfileLoader.typingMode != ProfileLoader.TypingMode.REGULAR) {
      if (Input.GetKeyDown(KeyCode.Space)) {
        Debug.Log("next phrase by pressing space");
        NextPhrases();
      } else if (Input.GetKeyDown(KeyCode.Backspace)) {
        PrevPhrases();
      }
    }
  }
}

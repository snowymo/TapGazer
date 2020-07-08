using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InputHandler : MonoBehaviour {
  public TMPro.TextMeshPro inputTextMesh;
  public WordlistLoader wordListLoader;
  public PhraseLoader phraseLoader;

  private string[] inputStringTemplate = new string[] { "q", "3", "4", "t", "b", "n", "u", "9", "0", "[" }; // to save time, we convert them to asdf del enter jkl;, and then maybe convert ; to p for json
  private Dictionary<string, string> mapInput2InputString;
  private string[] regularInputString;

  public string currentInputString; // to save current input string, refresh it when click 'enter'; in training mode, user has to type until correct
  public string currentInputLine; // refresh when go to next phrase, currentInputString should be substring(lastIndexOf('ent'))
                                  //public string currentDisplayLine; // the string to display on screen
  List<string> currentTypedWords; // inputTextMesh.text should be generated based on currentTypedWords.
  public CandidateHandler candidateHandler;

  public GameObject[] selectedFingers;

  public GameObject helpInfo;

  public HandAnimationCtrl handModel;

  public Measurement measurement;

  public Network tcpNetwork4Sensel, tcpNetwork4Gaze;
  
  // Start is called before the first frame update
  void Start() {
    inputTextMesh.text = "";
    currentInputString = "";
    currentInputLine = "";
    //currentDisplayLine = "";
    currentTypedWords = new List<string>();
    mapInput2InputString = new Dictionary<string, string>();
    mapInput2InputString.Add("q", "a");
    mapInput2InputString.Add("3", "s");
    mapInput2InputString.Add("4", "d");
    mapInput2InputString.Add("t", "f");
    mapInput2InputString.Add("u", "j");
    mapInput2InputString.Add("9", "k");
    mapInput2InputString.Add("0", "l");
    mapInput2InputString.Add("[", ";");
    //
    regularInputString = new string[26];
    regularInputString[0] = "a";
    for (int i = 1; i < regularInputString.Length; i++) {
      string prev = regularInputString[i - 1];
      char cprev = prev[0];
      ++cprev;
      regularInputString[i] = cprev.ToString();
    }
    if (ProfileLoader.inputMode == ProfileLoader.InputMode.TOUCH) {
      tcpNetwork4Sensel.enabled = true;
      tcpNetwork4Sensel.ConnectServer();
    }

    if (ProfileLoader.outputMode == ProfileLoader.OutputMode.Trackerbar) {
      tcpNetwork4Gaze.enabled = true;
      tcpNetwork4Gaze.ConnectServer();
    }
  }

  private void updateDisplayInput() {
    // udpate inputTextMesh.text with currentTypedWords
    inputTextMesh.text = "";
    if (phraseLoader.IsNewPhrase()) {
      currentTypedWords.Clear();
      currentInputLine = "";
    }

    for (int i = 0; i < currentTypedWords.Count; i++) {
      inputTextMesh.text += currentTypedWords[i] + " ";
    }
  }

  private void retrieveInputStringFromLine() {
    int index = currentInputLine.LastIndexOf('n');
    currentInputString = currentInputLine.Substring(index + 1);
  }

  public void HandleTouchInput() {
    // receive tcp package from c++ sensel
    // parsing the msg to know which is up which is down or nothing happened
    // assign it to somewhere so HandleNewKeyboard could use that
    string curMessage;
    while (tcpNetwork4Sensel.serverMessages.TryDequeue(out curMessage)) {
      // deal with this curMessage
      //Debug.Log("from msg queue:" + curMessage);
      int index = System.Array.IndexOf(inputStringTemplate, curMessage);
      if (index == -1)
        continue;
      if (index < 5)
        handModel.PressLeftFingers(index);
      else
        handModel.PressRightFingers(index - 5);
      helpInfo.SetActive(false);
      switch (index) {
        case 4:
          // b
          // delete  
          if (currentInputLine.Length > 1) {
            // if delete 'n', we need to remove the last typed word
            if (currentInputLine[currentInputLine.Length - 1] == 'n') {
              currentTypedWords.RemoveAt(currentTypedWords.Count - 1);
              // curTypingPhrase should move back to previous word too
              if (ProfileLoader.typingMode == ProfileLoader.TypingMode.TEST || ProfileLoader.typingMode == ProfileLoader.TypingMode.TAPPING)
                phraseLoader.PreviousWord();
            }
            currentInputLine = currentInputLine.Substring(0, currentInputLine.Length - 1); // b won't be put inside currentLine, n will, behave as space
            retrieveInputStringFromLine();
            if (currentInputString.Length > 0)
              wordListLoader.UpdateCandidates(currentInputString);
            else
              candidateHandler.ResetCandidates();
          } else {
            currentInputLine = "";
            currentInputString = "";
            candidateHandler.ResetCandidates();
          }
          break;
        case 5:
            // n
            {
            string presented = phraseLoader.GetCurWord().ToLower();
            // enter
            currentInputLine += 'n';
            string curWord = "null";
            if (wordListLoader.currentCandidates.Length > 0 && wordListLoader.currentCandidates[0] != null) {
              curWord = candidateHandler.CurrentGazedText == "" ? wordListLoader.currentCandidates[0] : candidateHandler.CurrentGazedText;// wordListLoader.currentCandidates[candidateHandler.GazedCandidate]; // 0 for now, 0 should be replaced by gaze result                        
            }
            // check if correct
            curWord = (phraseLoader.IsCurrentTypingCorrect(curWord, ProfileLoader.typingMode) ? "<color=green>" : "<color=red>") + curWord + "</color>";
            Debug.Log("cur word:" + curWord);
            currentTypedWords.Add(curWord);
            // pass corredsponding parameter to measurement
            // currentInputString is the input stream for current word, without 'n'
            // candidateHandler.GazedCandidate is the index of the candidates
            // the combination of the currentInputString and index is the entire input of the transribed=C+INF, presented is retrieved from PhraseLoader
            // the correct index of the candidates, we'd better find a way to get index from wordListLoader.currentCandidates
            measurement.UpdateTestMeasure(presented, currentInputString, curWord.Contains("=green"));
            // flush input
            currentInputString = "";
            candidateHandler.ResetCandidates();
          }
          break;
        default:
          // regular input
          measurement.StartClock();
          currentInputLine += mapInput2InputString[curMessage];
          retrieveInputStringFromLine();
          Debug.Log("input string:" + currentInputString);
          wordListLoader.UpdateCandidates(currentInputString);
          break;
      }
      updateDisplayInput();
    }
  }

  private string classifyWord(string word) {
    string fingerSeq = "";
    word = word.ToLower();
    for (int i = 0; i < word.Length; i++)
    {
      fingerSeq += ProfileLoader.configMap[word[i].ToString()];
    }
    return fingerSeq;
  }

  private void HandleNewKeyboard() {
    for (int i = 0; i < inputStringTemplate.Length; i++) {
      if (Input.GetKeyDown(inputStringTemplate[i])) {
        // process the key down
        //selectedFingers[i].SetActive(true);
        // hand animation
        if (i < 5)
          handModel.PressLeftFingers(i);
        else
          handModel.PressRightFingers(i - 5);
        helpInfo.SetActive(false);
        // reset candidates
        //candidateHandler.ResetCandidates();
        if (inputStringTemplate[i] == "b") {
          // delete  
          if (currentInputLine.Length > 1) {
            // if delete 'n', we need to remove the last typed word
            if (currentInputLine[currentInputLine.Length - 1] == 'n') {
              currentTypedWords.RemoveAt(currentTypedWords.Count - 1);
              // curTypingPhrase should move back to previous word too
              if (ProfileLoader.typingMode == ProfileLoader.TypingMode.TEST || ProfileLoader.typingMode == ProfileLoader.TypingMode.TAPPING)
                phraseLoader.PreviousWord();
            }
            if (candidateHandler.enableDeleteEntire)
            {
              // remove the entire word rather than one letter
              int lastN = currentInputLine.LastIndexOf('n');
              currentInputLine = lastN == -1 ? "" : currentInputLine.Substring(0, lastN+1);
              currentInputString = "";
              candidateHandler.ResetCandidates();
            }
            else if (currentInputLine[currentInputLine.Length - 1] == 'n' || 
              !candidateHandler.enableDeleteEntire)
            {
              // remove one letter
              currentInputLine = currentInputLine.Substring(0, currentInputLine.Length - 1); // b won't be put inside currentLine, n will, behave as space
              retrieveInputStringFromLine();
              if (currentInputString.Length > 0)
                wordListLoader.UpdateCandidates(currentInputString);
              else
                candidateHandler.ResetCandidates();
            }
            
          } else {
            currentInputLine = "";
            currentInputString = "";
            candidateHandler.ResetCandidates();
          }
        } else if (inputStringTemplate[i] == "n") {
          string presented = phraseLoader.GetCurWord();
          // enter
          currentInputLine += 'n';
          string curWord = "null";
          if (wordListLoader.currentCandidates.Length > 0 && wordListLoader.currentCandidates[0] != null) {
            curWord = candidateHandler.CurrentGazedText == "" ? candidateHandler.defaultWord : candidateHandler.CurrentGazedText;// wordListLoader.currentCandidates[candidateHandler.GazedCandidate]; // 0 for now, 0 should be replaced by gaze result                        
          }
          // check if correct
          if(ProfileLoader.typingMode == ProfileLoader.TypingMode.TAPPING)
          {
            // classify the word to type
            string correctFingerSeq = classifyWord(presented);
            curWord = (correctFingerSeq.Equals(currentInputString) ? presented : curWord);
            curWord = (phraseLoader.IsCurrentTypingCorrect(curWord, ProfileLoader.typingMode) ? "<color=green>" : "<color=red>") + curWord + "</color>";
          } else
          {
            curWord = (phraseLoader.IsCurrentTypingCorrect(curWord, ProfileLoader.typingMode) ? "<color=green>" : "<color=red>") + curWord + "</color>";
          }
          
          //Debug.Log("cur word:" + curWord);
          currentTypedWords.Add(curWord);
          // pass corredsponding parameter to measurement
          // currentInputString is the input stream for current word, without 'n'
          // candidateHandler.GazedCandidate is the index of the candidates
          // the combination of the currentInputString and index is the entire input of the transribed=C+INF, presented is retrieved from PhraseLoader
          // the correct index of the candidates, we'd better find a way to get index from wordListLoader.currentCandidates
          measurement.UpdateTestMeasure(presented, currentInputString, curWord.Contains("=green"));
          // flush input
          currentInputString = "";
          /*candidateHandler.ResetCandidates();*/
          wordListLoader.ResetCandidates();
          candidateHandler.defaultWord = "";
        } else {
          // regular input
          measurement.StartClock();
          currentInputLine += mapInput2InputString[inputStringTemplate[i]];
          retrieveInputStringFromLine();
          //Debug.Log("input string:" + currentInputString);
          wordListLoader.UpdateCandidates(currentInputString);
        }
        break;
      }
      if (Input.GetKeyUp(inputStringTemplate[i])) {
        // process the key up
        selectedFingers[i].SetActive(false);
        // move the finger back but keep the color changes
        if (i < 5)
          handModel.ReleaseLeftFingers(i);
        else
          handModel.ReleaseRightFingers(i - 5);
      }
    }
    updateDisplayInput();
  }

  public void HandleRegularKeyboard(TMPro.TMP_InputField inputField) {
    // check focus
    //if (!inputField.isFocused) {
    //    inputField.ActivateInputField();
    //}

    // directly typed into input section?, check when there is 'space'
    string curText = inputField.text;

    // 
    measurement.OnRegularInput(inputField);

    bool nextPhraseOrNot = phraseLoader.IsCurrentTypingCorrect(curText);
    if (nextPhraseOrNot)
      inputField.text = "";
    StartCoroutine(moveCaret(inputField));
  }

  IEnumerator moveCaret(TMPro.TMP_InputField inputField) {
    yield return new WaitForEndOfFrame();
    inputField.MoveTextEnd(true);
  }

  Vector2 screenGazeTuner;
  private void adjustScreenGaze() {
    if (Input.GetKeyDown(KeyCode.UpArrow)) {
      screenGazeTuner.y += 1;
    }else if (Input.GetKeyDown(KeyCode.DownArrow)) {
      screenGazeTuner.y -= 1;
    }
    if (Input.GetKeyDown(KeyCode.LeftArrow)) {
      screenGazeTuner.x -= 1;
    } else if (Input.GetKeyDown(KeyCode.RightArrow)) {
      screenGazeTuner.x += 1;
    }
  }

  // Update is called once per frame
  void Update() {
    if (ProfileLoader.inputMode == ProfileLoader.InputMode.KEYBOARD) {
      if (ProfileLoader.typingMode == ProfileLoader.TypingMode.REGULAR) {

      } else if (ProfileLoader.typingMode == ProfileLoader.TypingMode.TRAINING) {
        HandleNewKeyboard();
      } else if (measurement.allowInput) {
        HandleNewKeyboard();
      }
    } else {
      if (measurement.allowInput)
        HandleTouchInput();
    }

    if(ProfileLoader.outputMode == ProfileLoader.OutputMode.Trackerbar) {
      adjustScreenGaze();
      HandleScreenGaze();
    }
  }

  public void HandleScreenGaze() {
    // receive tcp package from tracker bar
    // parsing the msg to know the screen coordinates
    // assign it to somewhere so HandlerScreenGaze could use it
    string curMessage;
    Vector2 curScreenCoord = new Vector2(1000,1000);

    while (tcpNetwork4Gaze.serverMessages.TryDequeue(out curMessage)) {
      // deal with this curMessage
      //Debug.Log("from msg queue:" + curMessage);
      string[] coords = curMessage.Split(new char[] { ' ' });      
      Single.TryParse(coords[0], out curScreenCoord.x);
      Single.TryParse(coords[1], out curScreenCoord.y);
    }
    curScreenCoord += screenGazeTuner;
    candidateHandler.ScreenGaze = curScreenCoord;

    // visualize delta gaze, for fine tune
    Vector2 deltaGaze = candidateHandler.ScreenGaze - candidateHandler.ScreenGazeOffset;
    deltaGaze.y = Camera.main.pixelHeight - deltaGaze.y;
    //candidateHandler.screenGazeIndicator.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(deltaGaze.x, deltaGaze.y, Camera.main.nearClipPlane+5));
    candidateHandler.screenGazeIndicator.transform.position = deltaGaze;
  }

  public void CalibrateScreenGaze() {
    candidateHandler.ScreenGazeOffset = candidateHandler.ScreenGaze;
  }
}

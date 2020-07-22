using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InputHandler : MonoBehaviour
{
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

  public GameObject helpInfo, secondKeyHelpInfo;

  public HandAnimationCtrl handModel;

  public Measurement measurement;

  public Network tcpNetwork4Sensel, tcpNetwork4Gaze;

  private string[] selectionKeys; // ||
  private string[] deletionKeys; // &&
  public class KeyEventTime
  {
    public int down;
    public int up;
    public int duration;
    public KeyEventTime() { down = -1; duration = 0; up = 0; }
    public void setDown() { down = Time.frameCount; duration = 0; }
    public void setUp() {
      up = Time.frameCount; duration = Time.frameCount - down;
      //print("[in up] up:" + up + " duration:" + duration);
    }
    public void setHold() {
      duration = Time.frameCount - down;
      /*print(duration.TotalMilliseconds);*/
      //print("[in hold] up:" + up + " duration:" + duration);
    }
    public override string ToString() {
      return "down:" + down.ToString() + " duration:" + duration.ToString() + " up" + up.ToString();
    }
  }
  private Dictionary<string, KeyEventTime> controlKeyStatus;

  public TMPro.TextMeshPro spellingModeText;

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
    for (int i = 1; i < regularInputString.Length; i++)
    {
      string prev = regularInputString[i - 1];
      char cprev = prev[0];
      ++cprev;
      regularInputString[i] = cprev.ToString();
    }
    if (ProfileLoader.inputMode == ProfileLoader.InputMode.TOUCH)
    {
      tcpNetwork4Sensel.enabled = true;
      tcpNetwork4Sensel.ConnectServer();
    }

    if (ProfileLoader.outputMode == ProfileLoader.OutputMode.Trackerbar)
    {
      tcpNetwork4Gaze.enabled = true;
      tcpNetwork4Gaze.ConnectServer();
    }
    controlKeyStatus = new Dictionary<string, KeyEventTime>();
    controlKeyStatus["b"] = new KeyEventTime();
    controlKeyStatus["n"] = new KeyEventTime();
    if (ProfileLoader.enterMode == ProfileLoader.EnterMode.RIGHT_THUMB)
    {
      selectionKeys = new string[] { "n" };
      deletionKeys = new string[] { "b" };
    } else if (ProfileLoader.enterMode == ProfileLoader.EnterMode.BOTH_THUMB)
    {
      selectionKeys = new string[] { "n", "b" };
      deletionKeys = new string[] { "b", "n" };
    }
    spellingKeyStatus["t"] = new KeyEventTime();
    spellingKeyStatus["u"] = new KeyEventTime();
  }

  private void updateDisplayInput() {
    // udpate inputTextMesh.text with currentTypedWords
    inputTextMesh.text = "";
    if (phraseLoader.IsNewPhrase())
    {
      currentTypedWords.Clear();
      currentInputLine = "";
    }

    for (int i = 0; i < currentTypedWords.Count; i++)
    {
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
    while (tcpNetwork4Sensel.serverMessages.TryDequeue(out curMessage))
    {
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
      switch (index)
      {
        case 4:
          // b
          // delete  
          if (currentInputLine.Length > 1)
          {
            // if delete 'n', we need to remove the last typed word
            if (currentInputLine[currentInputLine.Length - 1] == 'n')
            {
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
          } else
          {
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
          if (wordListLoader.currentCandidates.Length > 0 && wordListLoader.currentCandidates[0] != null)
          {
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

  private void delete() {
    // delete  
    //print("delete");
    if (currentInputLine.Length > 1)
    {
      // if delete 'n', we need to remove the last typed word
      if (currentInputLine[currentInputLine.Length - 1] == 'n')
      {
        currentTypedWords.RemoveAt(currentTypedWords.Count - 1);
        // curTypingPhrase should move back to previous word too
        if (ProfileLoader.typingMode == ProfileLoader.TypingMode.TEST || ProfileLoader.typingMode == ProfileLoader.TypingMode.TAPPING)
          phraseLoader.PreviousWord();
      }
      int len = currentInputLine.Length;
      if (candidateHandler.enableDeleteEntire)
      {
        // remove the entire word rather than one letter
        int lastN = currentInputLine.LastIndexOf('n');
        currentInputLine = lastN == -1 ? "" : currentInputLine.Substring(0, lastN + 1);
        currentInputString = "";
        candidateHandler.ResetCandidates();
      }
      if (len == currentInputLine.Length)
      {
        if ((currentInputLine.Length > 0 && currentInputLine[currentInputLine.Length - 1] == 'n') ||
          !candidateHandler.enableDeleteEntire ||
          readyForSecondKey)
        {
          // remove one letter
          currentInputLine = currentInputLine.Substring(0, currentInputLine.Length - 1); // b won't be put inside currentLine, n will, behave as space
          retrieveInputStringFromLine();
          if (currentInputString.Length > 0)
            wordListLoader.UpdateCandidates(currentInputString);
          else
            candidateHandler.ResetCandidates();
        }
      }
    } else
    {
      currentInputLine = "";
      currentInputString = "";
      candidateHandler.ResetCandidates();
    }
  }

  private void selectUnchord() {
    string presented = phraseLoader.GetCurWord();
    // enter
    currentInputLine += 'n';
    string curWord = "null";
    if (wordListLoader.currentCandidates.Length > 0 && wordListLoader.currentCandidates[0] != null)
    {
      curWord = candidateHandler.CurrentGazedText == "" ? candidateHandler.defaultWord : candidateHandler.CurrentGazedText;// wordListLoader.currentCandidates[candidateHandler.GazedCandidate]; // 0 for now, 0 should be replaced by gaze result                        
    }
    // check if correct
    if (ProfileLoader.typingMode == ProfileLoader.TypingMode.TAPPING)
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
  }

  private bool toggleSpelling = false;
  private Dictionary<string, KeyEventTime> spellingKeyStatus = new Dictionary<string, KeyEventTime>();
  bool hitSpellKey = false;
  private bool updateSpellingMode() {
    // t and u
    foreach (KeyValuePair<string, KeyEventTime> eachKey in spellingKeyStatus)
    {
      //
      spellingKeyStatus[eachKey.Key].duration = 0;
      if (Input.GetKeyDown(eachKey.Key))
      {
        //print(eachKey.Key + " down");
        spellingKeyStatus[eachKey.Key].setDown();
      }
      if (Input.GetKey(eachKey.Key))
      {
        //print(eachKey.Key + " hold");
        spellingKeyStatus[eachKey.Key].setHold();
      }
      if (Input.GetKeyUp(eachKey.Key))
      {
        //print(eachKey.Key + " up");
        spellingKeyStatus[eachKey.Key].setUp();
      }
    }

    if (hitSpellKey)
    {
      // previously in deletion, then check if both key up
      bool bothUp = true;
      foreach (KeyValuePair<string, KeyEventTime> eachKey in spellingKeyStatus)
      {
        bothUp = bothUp && ((eachKey.Value.up >= eachKey.Value.down) && eachKey.Value.down >= 0);
        //print(eachKey.Value.ToString());
      }
      if (bothUp)
      {
        toggleSpelling = !toggleSpelling;
        //print("change spell mode:" + toggleSpelling);
        hitSpellKey = false;
        spellingModeText.text = "mode:" + (toggleSpelling ? "spelling" : "word");
        return true;
      }
    } else
    {
      // previously not in deletion, then check if current is in deletion
      bool curHitSpellKey = true;
      foreach (KeyValuePair<string, KeyEventTime> eachKey in spellingKeyStatus)
      {
        curHitSpellKey = curHitSpellKey &&
          (
          (eachKey.Value.duration > 0)
          || (
          (eachKey.Value.duration == 0)
            && (eachKey.Value.up == eachKey.Value.down)
            )
          );
      }
      hitSpellKey = curHitSpellKey;
    }
    return hitSpellKey;
  }

  bool readyForSecondKey = false;
  int[] keySelectionIndex = new int[] { 2, 7, 3, 6 };
  bool hitDeletionKey = false;
  bool justHitSelectionKey = false;

  private void typeInWordMode() {
    //print("Time.frameCount " + Time.frameCount);
    // retrieve selection and deletion status
    // it is difficult to check two-key down. So let's record the timestamp that b and n is down
    foreach (KeyValuePair<string, KeyEventTime> eachKey in controlKeyStatus)
    {
      //
      controlKeyStatus[eachKey.Key].duration = 0;
      if (Input.GetKeyDown(eachKey.Key))
      {
        //print(eachKey.Key + " down");
        controlKeyStatus[eachKey.Key].setDown();
      }
      if (Input.GetKey(eachKey.Key))
      {
        //print(eachKey.Key + " hold");
        controlKeyStatus[eachKey.Key].setHold();
      }
      if (Input.GetKeyUp(eachKey.Key))
      {
        //print(eachKey.Key + " up");
        controlKeyStatus[eachKey.Key].setUp();
        justHitSelectionKey = false;
      }
    }

    if (hitDeletionKey)
    {
      // previously in deletion, then check if both key up
      bool bothUp = true;
      // reset
      readyForSecondKey = false;
      for (int deletionKeysIndex = 0; deletionKeysIndex < deletionKeys.Length; deletionKeysIndex++)
      {
        //print("[check deletion key]" + deletionKeys[deletionKeysIndex] 
        //  + ":" + Input.GetKey(deletionKeys[deletionKeysIndex].ToString()));
        //hitDeletionKey = hitDeletionKey && (Input.GetKey(deletionKeys[deletionKeysIndex]) ||
        //  Input.GetKeyDown(deletionKeys[deletionKeysIndex]));
        // check the hold time to assign hitDeletionKey
        //print(deletionKeys[deletionKeysIndex] + ":" + controlKeyStatus[deletionKeys[deletionKeysIndex]].duration.TotalMilliseconds);
        bothUp = bothUp &&
          (controlKeyStatus[deletionKeys[deletionKeysIndex]].up
          > controlKeyStatus[deletionKeys[deletionKeysIndex]].down);
      }
      if (bothUp)
      {
        delete();
        hitDeletionKey = false;
      }
    } else
    {
      // previously not in deletion, then check if current is in deletion
      bool curHitDeletionKey = true;
      for (int deletionKeysIndex = 0; deletionKeysIndex < deletionKeys.Length; deletionKeysIndex++)
      {
        //print("[check deletion key]" + deletionKeys[deletionKeysIndex] 
        //  + ":" + Input.GetKey(deletionKeys[deletionKeysIndex].ToString()));
        //hitDeletionKey = hitDeletionKey && (Input.GetKey(deletionKeys[deletionKeysIndex]) ||
        //  Input.GetKeyDown(deletionKeys[deletionKeysIndex]));
        // check the hold time to assign hitDeletionKey
        //print(deletionKeys[deletionKeysIndex] + ":" + controlKeyStatus[deletionKeys[deletionKeysIndex]].duration.TotalMilliseconds);
        curHitDeletionKey = curHitDeletionKey &&
          (
          (controlKeyStatus[deletionKeys[deletionKeysIndex]].duration > 0)
          || (
          (controlKeyStatus[deletionKeys[deletionKeysIndex]].duration == 0)
            && (controlKeyStatus[deletionKeys[deletionKeysIndex]].up == controlKeyStatus[deletionKeys[deletionKeysIndex]].down)
            )
          );
      }
      hitDeletionKey = curHitDeletionKey;
      bool hitSelectionKey = false;
      if (!hitDeletionKey)
      {
        if (!justHitSelectionKey)
        {
          // check selection key
          if (selectionKeys.Length == 1)
          {
            hitSelectionKey = Input.GetKeyDown(selectionKeys[0]);
          } else
          {
            for (int i = 0; i < selectionKeys.Length; i++)
            {
              // one up one down
              //if(controlKeyStatus[selectionKeys[i]].duration != 0)
              //  print(controlKeyStatus[selectionKeys[i]].duration + "\t" + controlKeyStatus[selectionKeys[1 - i]].up);
              if (
                (controlKeyStatus[selectionKeys[i]].duration > 0) && (controlKeyStatus[selectionKeys[i]].up < controlKeyStatus[selectionKeys[i]].down)
              && (controlKeyStatus[selectionKeys[1 - i]].up > controlKeyStatus[selectionKeys[1 - i]].down))
              {
                hitSelectionKey = true;
              }
            }
          }
        }

        // select chord // key selection
        if (candidateHandler.enableChordSelection)
        {
          if (hitSelectionKey)
          {
            print("[key selection] ready for second key");
            readyForSecondKey = true;
          }
        }
        if (readyForSecondKey)
        {
          // assign candIndex
          int candIndex = -1;
          for (int i = 0; i < keySelectionIndex.Length; i++)
          {
            if (Input.GetKeyDown(inputStringTemplate[keySelectionIndex[i]]))
            {
              // update candIndex
              candIndex = i + 1;
              print("[key selection] via LH " + candIndex.ToString());
              justHitSelectionKey = true;
              break;
            }
          }
          if (candIndex == -1)
          {
            bool selectionKeyUp = false;
            for (int i = 0; i < selectionKeys.Length; i++)
            {
              selectionKeyUp = Input.GetKeyUp(selectionKeys[i]) || selectionKeyUp;
            }
            if (selectionKeyUp)
            {
              candIndex = 0;
              print("[key selection] via thumb " + candIndex.ToString());
            }
          }
          // update based on current candIndex
          if (candIndex != -1)
          {
            // using key selection
            string presented = phraseLoader.GetCurWord();
            // enter
            currentInputLine += 'n';  // TODO: still use n to represent selection
            string curWord = "null";
            curWord = candidateHandler.candidateObjects[candIndex].GetComponent<Candidate>().pureText;
            print("[key selection] curword " + curWord);
            curWord = (phraseLoader.IsCurrentTypingCorrect(curWord, ProfileLoader.typingMode) ? "<color=green>" : "<color=red>") + curWord + "</color>";

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
            readyForSecondKey = false;
          }
        } else
        {
          // select no chord
          if (hitSelectionKey)
          {
            selectUnchord();
            justHitSelectionKey = true;
          } else
          {
            // regular input
            for (int i = 0; i < inputStringTemplate.Length; i++)
            {
              if (Input.GetKeyUp(inputStringTemplate[i]))
              {
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
                if (mapInput2InputString.ContainsKey(inputStringTemplate[i]))
                {
                  measurement.StartClock();
                  currentInputLine += mapInput2InputString[inputStringTemplate[i]];
                  retrieveInputStringFromLine();
                  //Debug.Log("input string:" + currentInputString);
                  wordListLoader.UpdateCandidates(currentInputString);
                }
                break;
              }
              if (Input.GetKeyUp(inputStringTemplate[i]))
              {
                // process the key up
                selectedFingers[i].SetActive(false);
                // move the finger back but keep the color changes
                if (i < 5)
                  handModel.ReleaseLeftFingers(i);
                else
                  handModel.ReleaseRightFingers(i - 5);
              }
            }
          }
        }
      }
    }

    updateDisplayInput();
  }

  private void typeInSpellMode() {
    // finalize a letter via 1) a different finger 2) enter finger 3) time up
    // we may still have RT and BT differences
  }

  private void HandleNewKeyboard() {
    bool isSpellKeyDown = updateSpellingMode();
    //print("isSpellKeyDown:" + isSpellKeyDown);
    if (isSpellKeyDown)
      return;

    // type in spell mode
    if (toggleSpelling)
    {
      typeInSpellMode();
    } else
    {
      typeInWordMode();
    }
    
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
    if (Input.GetKeyDown(KeyCode.UpArrow))
    {
      screenGazeTuner.y += 1;
    } else if (Input.GetKeyDown(KeyCode.DownArrow))
    {
      screenGazeTuner.y -= 1;
    }
    if (Input.GetKeyDown(KeyCode.LeftArrow))
    {
      screenGazeTuner.x -= 1;
    } else if (Input.GetKeyDown(KeyCode.RightArrow))
    {
      screenGazeTuner.x += 1;
    }
  }

  // Update is called once per frame
  void Update() {
    if (ProfileLoader.inputMode == ProfileLoader.InputMode.KEYBOARD)
    {
      if (ProfileLoader.typingMode == ProfileLoader.TypingMode.REGULAR)
      {

      } else if (ProfileLoader.typingMode == ProfileLoader.TypingMode.TRAINING)
      {
        HandleNewKeyboard();
      } else if (measurement.allowInput)
      {
        HandleNewKeyboard();
      }
    } else
    {
      if (measurement.allowInput)
        HandleTouchInput();
    }

    if (ProfileLoader.outputMode == ProfileLoader.OutputMode.Trackerbar)
    {
      adjustScreenGaze();
      HandleScreenGaze();
    }

    // show second key finger assignment
    secondKeyHelpInfo.SetActive(readyForSecondKey);
  }

  public void HandleScreenGaze() {
    // receive tcp package from tracker bar
    // parsing the msg to know the screen coordinates
    // assign it to somewhere so HandlerScreenGaze could use it
    string curMessage;
    Vector2 curScreenCoord = new Vector2(1000, 1000);

    while (tcpNetwork4Gaze.serverMessages.TryDequeue(out curMessage))
    {
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

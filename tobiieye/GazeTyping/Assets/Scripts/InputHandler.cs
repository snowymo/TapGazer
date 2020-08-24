﻿using System.Collections;
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
    public bool isPressed;
    public KeyEventTime() { down = -1; duration = 0; up = 0; isPressed = false; }
    public void setDown() { down = Time.frameCount; duration = 0; isPressed = true; }
    public void setUp()
    {
      up = Time.frameCount; duration = Time.frameCount - down; isPressed = false;
      //print("[in up] up:" + up + " duration:" + duration);
    }
    public void setHold()
    {
      duration = Time.frameCount - down;
      /*print(duration.TotalMilliseconds);*/
      //print("[in hold] up:" + up + " duration:" + duration);
    }
    public override string ToString()
    {
      return "down:" + down.ToString() + " duration:" + duration.ToString() + " up" + up.ToString();
    }
  }
  private Dictionary<string, KeyEventTime> controlKeyStatus;

  public TMPro.TextMeshPro spellingModeText;

  // Start is called before the first frame update
  void Start()
  {
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
    }
    else if (ProfileLoader.enterMode == ProfileLoader.EnterMode.BOTH_THUMB)
    {
      selectionKeys = new string[] { "n", "b" };
      deletionKeys = new string[] { "b", "n" };
    }
    // for MS, GS, GSR, they are all BOTH_THUMB
    selectionKeys = new string[] { "n", "b" };
    deletionKeys = new string[] { "b", "n" };
    spellingKeyStatus["t"] = new KeyEventTime();
    spellingKeyStatus["u"] = new KeyEventTime();
  }

  private void updateDisplayInput()
  {
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
    if (toggleSpelling)
    {
      // add letter for spelling mode
      int lastN = currentInputLine.LastIndexOf(' ');
      if (lastN + 1 < currentInputLine.Length)
      {
        print("[spelling mode] display:" + lastN + " " + currentInputLine.Substring(lastN + 1));
        inputTextMesh.text += currentInputLine.Substring(lastN + 1);
      }
    }
  }

  private string retrieveInputStringFromLine()
  {
    int index = currentInputLine.LastIndexOf('n');
    currentInputString = currentInputLine.Substring(index + 1);
    return currentInputString;
  }

  public void HandleTouchInput()
  {
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
          }
          else
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
            measurement.AddTapItem("n", "selection");
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
          //measurement.AddTapDurationItem(mapInput2InputString[curMessage]);
          measurement.AddTapItem(mapInput2InputString[curMessage], "tap");
          retrieveInputStringFromLine();
          Debug.Log("input string:" + currentInputString);
          wordListLoader.UpdateCandidates(currentInputString);
          break;
      }
      updateDisplayInput();
    }
  }

  private string classifyWord(string word)
  {
    string fingerSeq = "";
    word = word.ToLower();
    for (int i = 0; i < word.Length; i++)
    {
      fingerSeq += ProfileLoader.configMap[word[i].ToString()];
    }
    return fingerSeq;
  }

  private void delete()
  {
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
    }
    else
    {
      currentInputLine = "";
      currentInputString = "";
      candidateHandler.ResetCandidates();
    }
  }

  private void selectUnchord()
  {
    string presented = phraseLoader.GetCurWord();
    // enter
    currentInputLine += 'n';
    measurement.AddTapItem("n", "selection");
    string curWord = "null";
    if (wordListLoader.currentCandidates.Length > 0 && wordListLoader.currentCandidates[0] != null)
    {
      curWord = candidateHandler.CurrentGazedText == "" ? candidateHandler.defaultWord : candidateHandler.CurrentGazedText;// wordListLoader.currentCandidates[candidateHandler.GazedCandidate]; // 0 for now, 0 should be replaced by gaze result                        
    }
    // check if correct
    if (ProfileLoader.typingMode == ProfileLoader.TypingMode.TAPPING)
    {
      // classify the word to type
      // TODO: once we support paging, we need to know if the correct word is at the current page
      string correctFingerSeq = classifyWord(presented);
      curWord = (correctFingerSeq.Equals(currentInputString) ? presented : curWord);
      curWord = (phraseLoader.IsCurrentTypingCorrect(curWord, ProfileLoader.typingMode) ? "<color=green>" : "<color=red>") + curWord + "</color>";
    }
    else
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
  private bool updateSpellingMode()
  {
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

    bool curHitSpellKey = true;
    foreach (KeyValuePair<string, KeyEventTime> eachKey in spellingKeyStatus)
    {
      curHitSpellKey = curHitSpellKey && eachKey.Value.isPressed;
      //print(eachKey.Value.ToString());
    }
    if (curHitSpellKey)
    {
      toggleSpelling = !toggleSpelling;
      print("change spell mode:" + toggleSpelling);
      spellingModeText.text = "mode:" + (toggleSpelling ? "spelling" : "word");
    }

    return hitSpellKey;
  }

  bool readyForSecondKey = false;
  int[] keySelectionIndex = new int[] { 5, 2, 7, 3, 6 };
  bool hitDeletionKey = false;
  bool justHitSelectionKey = false;
  bool controlKeyUpHit = false;

  private void handleSelection(int candIndex)
  {
    // using key selection
    string presented = phraseLoader.GetCurWord();
    // enter
    currentInputLine += 'n';  // TODO: still use n to represent selection
                              // record selection time
    string curWord = "null";
    if(candIndex == -1)
    {
      // means could be anything on the same page
      // TODO, update once we have page index
      string correctFingerSeq = classifyWord(presented);
      curWord = (correctFingerSeq.Equals(currentInputString) ? presented : curWord);
    }
    else
    {
      curWord = candidateHandler.candidateObjects[candIndex].GetComponent<Candidate>().pureText;
      //print("[key selection] curword " + curWord);
    }
    curWord = (phraseLoader.IsCurrentTypingCorrect(curWord, ProfileLoader.typingMode) ? "<color=green>" : "<color=red>") + curWord + "</color>";
    //Debug.Log("cur word:" + curWord);
    currentTypedWords.Add(curWord);
    measurement.UpdateTestMeasure(presented, currentInputString, curWord.Contains("=green"));
    // flush input
    currentInputString = "";
    /*candidateHandler.ResetCandidates();*/
    wordListLoader.ResetCandidates();
    candidateHandler.defaultWord = "";
    readyForSecondKey = false;
  }

  private void typeInWordModeNoChord()
  {
    // update all control key status
    //foreach (KeyValuePair<string, KeyEventTime> eachKey in controlKeyStatus)
    //{
    //  // update control key's state
    //  controlKeyStatus[eachKey.Key].duration = 0;
    //  if (Input.GetKeyDown(eachKey.Key))
    //  {
    //    //print(eachKey.Key + " down");
    //    controlKeyStatus[eachKey.Key].setDown();
    //  }
    //  if (Input.GetKey(eachKey.Key))
    //  {
    //    //print(eachKey.Key + " hold");
    //    controlKeyStatus[eachKey.Key].setHold();
    //  }
    //  if (Input.GetKeyUp(eachKey.Key))
    //  {
    //    //print(eachKey.Key + " up");
    //    controlKeyStatus[eachKey.Key].setUp();
    //  }
    //}

    if (!Input.anyKeyDown)
    {
      return;
    }

    // only another key is followed by control key 'b'
    if (readyForSecondKey && ProfileLoader.selectionMode == ProfileLoader.SelectionMode.MS)
    {
      // could selection could be deletion
      if (Input.GetKeyDown("b"))
      {
        print("LT LT deletion");
        delete();
        measurement.AddTapItem("b", "deletion");
        readyForSecondKey = false;
        updateDisplayInput();
      }
      else
      {
        // assign candIndex          
        int candIndex = 0;
        for (int i = 0; i < keySelectionIndex.Length; i++)
        {
          if (Input.GetKeyDown(inputStringTemplate[keySelectionIndex[i]]))
          {
            // update candIndex
            candIndex = i;
            print("[key selection] via LH " + candIndex.ToString());
            // handle word selection
            handleSelection(candIndex);
            updateDisplayInput();
            measurement.AddTapItem(inputStringTemplate[keySelectionIndex[i]], "selection");
            readyForSecondKey = false;
            break;
          }
        }
        // thinking if hit another key when waiting for second key?
      }
      return;
    }

    if(readyForSecondKey && ProfileLoader.selectionMode != ProfileLoader.SelectionMode.MS)
    {
      // allow to select with either thumb
      if (Input.GetKeyDown("b") )
      {
        print("LT LT deletion");
        delete();
        measurement.AddTapItem("b", "deletion");
        readyForSecondKey = false;
        updateDisplayInput();
      }
    }

    if (!readyForSecondKey)
    {
      // either regular input or control key for the first time press
      if (Input.GetKeyDown("n"))
      {
        switch (ProfileLoader.selectionMode)
        {
          case ProfileLoader.SelectionMode.MS:
            // select default word
            readyForSecondKey = false; // selectionKeys[0] is 'n' aka right thumb.
                                       //print("RT selection");
            handleSelection(0);
            break;
          case ProfileLoader.SelectionMode.GSE:
          case ProfileLoader.SelectionMode.GSR:
            handleSelection(-1);
            break;
          default:
            break;
        }
        
        measurement.AddTapItem("n", "selection");
        helpInfo.SetActive(false);
      }
      else if (Input.GetKeyDown("b"))
      {
        switch (ProfileLoader.selectionMode)
        {
          case ProfileLoader.SelectionMode.MS:
            // waiting for second key in MS mode
            //print("LT waiting");
            readyForSecondKey = true; // selectionKeys[0] is 'n' aka right thumb.
            measurement.AddTapItem("b", "selection");
            break;
          case ProfileLoader.SelectionMode.GSE:
          case ProfileLoader.SelectionMode.GSR:
            // deletion
            print("LT deletion");
            delete();
            measurement.AddTapItem("b", "deletion");
            readyForSecondKey = false;
            updateDisplayInput();
            break;
          default:
            break;
        }        
        helpInfo.SetActive(false);
      }
      else
      {
        // regular input
        for (int i = 0; i < inputStringTemplate.Length; i++)
        {
          if (Input.GetKeyDown(inputStringTemplate[i]))
          {
            // process the key down
            //selectedFingers[i].SetActive(true);
            // hand animation
            if (i < 5)
              handModel.PressLeftFingers(i);
            else
              handModel.PressRightFingers(i - 5);
            helpInfo.SetActive(false);
            // reset candidates            //candidateHandler.ResetCandidates();    
            if (mapInput2InputString.ContainsKey(inputStringTemplate[i]))
            {
              measurement.StartClock();
              currentInputLine += mapInput2InputString[inputStringTemplate[i]];
              measurement.AddTapItem(mapInput2InputString[inputStringTemplate[i]], "tap");
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
            break;
          }
        }
      }
      updateDisplayInput();
    }
  }

  private void typeInWordMode()
  {
    if (readyForSecondKey && ProfileLoader.selectionMode == ProfileLoader.SelectionMode.MS)
    {
      if (Input.GetKeyDown("b"))
      {
        delete();
        //reset isPressed
        //for (int deletionKeysIndex = 0; deletionKeysIndex < deletionKeys.Length; deletionKeysIndex++)
        //{
        //  controlKeyStatus[deletionKeys[deletionKeysIndex]].isPressed = false;
        //}
        updateDisplayInput();
        measurement.AddTapItem("b", "deletion");
        
      }
      else
      {
        // assign candIndex          
        int candIndex = 0;
        for (int i = 0; i < keySelectionIndex.Length; i++)
        {
          if (Input.GetKeyDown(inputStringTemplate[keySelectionIndex[i]]))
          {
            // update candIndex
            candIndex = i;
            print("[key selection] via LH " + candIndex.ToString());
            justHitSelectionKey = true;
            // handle word selection
            handleSelection(candIndex);
            measurement.AddTapItem(inputStringTemplate[keySelectionIndex[i]], "selection");
            break;
          }
        }
      }      
      updateDisplayInput();
      return;
    }
    //print("Time.frameCount " + Time.frameCount);
    // retrieve selection and deletion status
    // it is difficult to check two-key down. So let's record the timestamp that b and n is down
    controlKeyUpHit = false;
    foreach (KeyValuePair<string, KeyEventTime> eachKey in controlKeyStatus)
    {
      // update control key's state
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
        //
        hitDeletionKey = false;
        //
        controlKeyUpHit = true;
      }
    }

    //if (!hitDeletionKey)
    //{
      // previously not in deletion, then check if current is in deletion
      //bool curHitDeletionKey = true;
      //for (int deletionKeysIndex = 0; deletionKeysIndex < deletionKeys.Length; deletionKeysIndex++)
      //{
      //  curHitDeletionKey = curHitDeletionKey && controlKeyStatus[deletionKeys[deletionKeysIndex]].isPressed;
      //}
      //hitDeletionKey = curHitDeletionKey;
      //// TODO TEST: delete when both down
      //if (hitDeletionKey)
      //{
      //  delete();
      //  //reset isPressed
      //  for (int deletionKeysIndex = 0; deletionKeysIndex < deletionKeys.Length; deletionKeysIndex++)
      //  {
      //    controlKeyStatus[deletionKeys[deletionKeysIndex]].isPressed = false;
      //  }
      //  updateDisplayInput();
      //  return;
      //}

      // considering selection
      // we can only know when the control key is up
      // once it is up, let us check if another key has been pressed during this key is hold
      bool hitSelectionKey = false;
      readyForSecondKey = false;
      if (controlKeyUpHit)
      {
        for (int i = 0; i < selectionKeys.Length; i++)
        {
          if ((controlKeyStatus[selectionKeys[i]].up > controlKeyStatus[selectionKeys[i]].down)
            && (controlKeyStatus[selectionKeys[i]].down > controlKeyStatus[selectionKeys[1 - i]].up)
            )
          {
            if (retrieveInputStringFromLine().Length > 0)
            {
              hitSelectionKey = true;
              readyForSecondKey = i == 0 ? false : true; // selectionKeys[0] is 'n' aka right thumb.
              measurement.AddTapItem(selectionKeys[i], "selection");
            }
          }
        }
      }
      if (hitSelectionKey)
      {
        int candIndex = 0;
        if (readyForSecondKey && ProfileLoader.selectionMode == ProfileLoader.SelectionMode.MS)
        {
          print("[key selection] ready for the second key");
        }
        else
        {
          print("RT selection");
          // handle word selection
          handleSelection(candIndex);
        }
        updateDisplayInput();
        return;
      }

      // regular input
      for (int i = 0; i < inputStringTemplate.Length; i++)
      {
        if (Input.GetKeyDown(inputStringTemplate[i]))
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
            //measurement.AddTapDurationItem(mapInput2InputString[inputStringTemplate[i]]);
            measurement.AddTapItem(mapInput2InputString[inputStringTemplate[i]], "tap");
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
    //}
    updateDisplayInput();
  }

  int lastSpellTime = 0;
  string lastSpellFinger = "";
  const int spellTimeUp = 40;
  int curLetterIndex = 0;
  private bool newLetter(string curSpellFinger)
  {
    if (curSpellFinger != lastSpellFinger)
    {
      //lastSpellTime = Time.frameCount - spellTimeUp * 2;
      print("[new letter] different finger");
      return true;
    }
    if (Time.frameCount - lastSpellTime >= spellTimeUp)
    {
      print("[new letter] time is up:" + (Time.frameCount - lastSpellTime));
      return true;
    }
    return false;
  }
  private void typeInSpellMode()
  {
    // finalize a letter via 1) a different finger 2) enter finger 3) time up
    // we may still have RT and BT differences

    // deal with select and delete keys
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
        hitDeletionKey = false;
      }
    }

    //if (hitDeletionKey)
    //{
    //  // previously in deletion, then check if both key up
    //  bool bothUp = true;
    //  // reset
    //  readyForSecondKey = false;
    //  for (int deletionKeysIndex = 0; deletionKeysIndex < deletionKeys.Length; deletionKeysIndex++)
    //  {
    //    bothUp = bothUp &&
    //      (controlKeyStatus[deletionKeys[deletionKeysIndex]].up
    //      > controlKeyStatus[deletionKeys[deletionKeysIndex]].down);
    //  }
    //  if (bothUp)
    //  {
    //    print("[spelling mode] delete: " + currentInputLine);
    //    lastSpellFinger = "b";
    //    if (currentInputLine.Length > 1)
    //    {
    //      // if delete 'n', we need to remove the last typed word
    //      if (currentInputLine[currentInputLine.Length - 1] == ' ')
    //      {
    //        print("[spelling mode] delete: currentTypedWords.count " + currentTypedWords.Count);
    //        currentTypedWords.RemoveAt(currentTypedWords.Count - 1);            
    //        // curTypingPhrase should move back to previous word too
    //        if (ProfileLoader.typingMode == ProfileLoader.TypingMode.TEST || ProfileLoader.typingMode == ProfileLoader.TypingMode.TAPPING)
    //          phraseLoader.PreviousWord();
    //          currentInputLine = currentInputLine.Substring(0, currentInputLine.Length - 2);
    //      } else
    //      {
    //        currentInputLine = currentInputLine.Substring(0, currentInputLine.Length - 1);
    //      }
    //    } else
    //    {
    //      currentInputLine = "";
    //    }
    //    hitDeletionKey = false;
    //  }
    //}
    if (!hitDeletionKey)
    {
      // previously not in deletion, then check if current is in deletion
      bool curHitDeletionKey = true;
      for (int deletionKeysIndex = 0; deletionKeysIndex < deletionKeys.Length; deletionKeysIndex++)
      {
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
      if (hitDeletionKey)
      {
        //TODO TEST: apply deletion when both down
        print("[spelling mode] delete: " + currentInputLine);
        lastSpellFinger = "b";
        if (currentInputLine.Length > 1)
        {
          // if delete 'n', we need to remove the last typed word
          if (currentInputLine[currentInputLine.Length - 1] == ' ')
          {
            print("[spelling mode] delete: currentTypedWords.count " + currentTypedWords.Count);
            currentTypedWords.RemoveAt(currentTypedWords.Count - 1);
            // curTypingPhrase should move back to previous word too
            if (ProfileLoader.typingMode == ProfileLoader.TypingMode.TEST || ProfileLoader.typingMode == ProfileLoader.TypingMode.TAPPING)
              phraseLoader.PreviousWord();
            currentInputLine = currentInputLine.Substring(0, currentInputLine.Length - 2);
          }
          else
          {
            currentInputLine = currentInputLine.Substring(0, currentInputLine.Length - 1);
          }
        }
        else
        {
          currentInputLine = "";
        }
      }

      bool hitSelectionKey = false;
      if (!hitDeletionKey)
      {
        if (!justHitSelectionKey)
        {
          // check selection key
          if (selectionKeys.Length == 1)
          {
            hitSelectionKey = Input.GetKeyDown(selectionKeys[0]);
          }
          else
          {
            for (int i = 0; i < selectionKeys.Length; i++)
            {
              // one up one down
              if (
                (controlKeyStatus[selectionKeys[i]].duration > 0) && (controlKeyStatus[selectionKeys[i]].up < controlKeyStatus[selectionKeys[i]].down)
              && (controlKeyStatus[selectionKeys[1 - i]].up > controlKeyStatus[selectionKeys[1 - i]].down))
              {
                hitSelectionKey = true;
              }
            }
          }
        }
      }

      if (hitSelectionKey)
      {
        lastSpellFinger = "n";
        // select current letter or type space
        if (Time.frameCount - lastSpellTime >= spellTimeUp)
        {
          // type space
          currentInputString = currentInputLine.Substring(currentInputLine.LastIndexOf(" ") + 1);
          currentInputLine += " ";

          // check if correct
          string curWord = (phraseLoader.IsCurrentTypingCorrect(currentInputString, ProfileLoader.typingMode) ?
            "<color=green>" : "<color=red>") + currentInputString + "</color>";
          Debug.Log("[spelling mode] select:" + currentInputString + " cur word:" + curWord);
          currentTypedWords.Add(curWord);
          // update measurement if necessary
          string presented = phraseLoader.GetCurWord();
          measurement.UpdateTestMeasure(presented, currentInputString, curWord.Contains("=green"));
          // flush input
          currentInputString = "";
        }
      }
      else
      {
        // regular input
        //print("[spelling mode] regular input");
        for (int i = 0; i < inputStringTemplate.Length; i++)
        {
          if (Input.GetKeyDown(inputStringTemplate[i]))
          {
            // process the key down
            // hand animation
            if (i < 5)
              handModel.PressLeftFingers(i);
            else
              handModel.PressRightFingers(i - 5);

            if (mapInput2InputString.ContainsKey(inputStringTemplate[i]))
            {
              measurement.StartClock();
              // check if it is a new letter or updating current letter
              if (newLetter(inputStringTemplate[i]))
              {
                curLetterIndex = 0;
                currentInputLine += ProfileLoader.letterMap[mapInput2InputString[inputStringTemplate[i]]][curLetterIndex];
              }
              else
              {
                currentInputLine = currentInputLine.Substring(0, currentInputLine.Length - 1);
                curLetterIndex = curLetterIndex + 1;
                currentInputLine += ProfileLoader.letterMap[mapInput2InputString[inputStringTemplate[i]]][curLetterIndex];
              }
              lastSpellTime = Time.frameCount;
              lastSpellFinger = inputStringTemplate[i];
              print("[spelling mode] type: finger - " + lastSpellFinger + " at " + lastSpellTime);
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
    // update currentInputLine to textMesh
    updateDisplayInput();
    //inputTextMesh.text = currentInputLine;
  }

  private void HandleNewKeyboard()
  {
    //TODO: use non chord for spelling mode later
    //bool isSpellKeyDown = updateSpellingMode();
    ////print("isSpellKeyDown:" + isSpellKeyDown);
    //if (isSpellKeyDown)
    //  return;

    // type in spell mode
    if (toggleSpelling)
    {
      typeInSpellMode();
    }
    else
    {
      typeInWordModeNoChord();
      //typeInWordMode();
    }

  }

  public void HandleRegularKeyboard(TMPro.TMP_InputField inputField)
  {
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

  IEnumerator moveCaret(TMPro.TMP_InputField inputField)
  {
    yield return new WaitForEndOfFrame();
    inputField.MoveTextEnd(true);
  }

  Vector2 screenGazeTuner;
  private void adjustScreenGaze()
  {
    if (Input.GetKeyDown(KeyCode.UpArrow))
    {
      screenGazeTuner.y += 1;
    }
    else if (Input.GetKeyDown(KeyCode.DownArrow))
    {
      screenGazeTuner.y -= 1;
    }
    if (Input.GetKeyDown(KeyCode.LeftArrow))
    {
      screenGazeTuner.x -= 1;
    }
    else if (Input.GetKeyDown(KeyCode.RightArrow))
    {
      screenGazeTuner.x += 1;
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (ProfileLoader.inputMode == ProfileLoader.InputMode.KEYBOARD)
    {
      if (ProfileLoader.typingMode == ProfileLoader.TypingMode.REGULAR)
      {

      }
      else if (ProfileLoader.typingMode == ProfileLoader.TypingMode.TRAINING)
      {
        HandleNewKeyboard();
      }
      else if (measurement.allowInput)
      {
        HandleNewKeyboard();
      }
    }
    else
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

  public void HandleScreenGaze()
  {
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

  public void CalibrateScreenGaze()
  {
    candidateHandler.ScreenGazeOffset = candidateHandler.ScreenGaze;
  }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public TMPro.TextMeshPro inputTextMesh;
    public WordlistLoader wordListLoader;
    public PhraseLoader phraseLoader;

    private string[] inputStringTemplate = new string[] { "q", "3", "4", "t", "b", "n", "u", "9", "0", "[" }; // to save time, we convert them to asdf del enter jkl;, and then maybe convert ; to p for json
    private Dictionary<string, string> mapInput2InputString;

    public string currentInputString; // to save current input string, refresh it when click 'enter'; in training mode, user has to type until correct
    public string currentInputLine; // refresh when go to next phrase, currentInputString should be substring(lastIndexOf('ent'))
    //public string currentDisplayLine; // the string to display on screen
    List<string> currentTypedWords; // inputTextMesh.text should be generated based on currentTypedWords.
    public CandidateHandler candidateHandler;

    public GameObject[] selectedFingers;

    public GameObject helpInfo;

    public HandAnimationCtrl handModel;

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
    }

    private void updateDisplayInput()
    {
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

    private void retrieveInputStringFromLine()
    {
        int index = currentInputLine.LastIndexOf('n');
        currentInputString = currentInputLine.Substring(index+1);
    }

    // Update is called once per frame
    void Update()
    {
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
                candidateHandler.ResetCandidates();
                if (inputStringTemplate[i] == "b")
                {
                    // delete  
                    if(currentInputLine.Length > 1) {
                        // if delete 'n', we need to remove the last typed word
                        if (currentInputLine[currentInputLine.Length - 1] == 'n') {
                            currentTypedWords.RemoveAt(currentTypedWords.Count - 1);
                        }
                        currentInputLine = currentInputLine.Substring(0, currentInputLine.Length - 1); // b won't be put inside currentLine, n will, behave as space
                        retrieveInputStringFromLine();
                        if(currentInputString.Length > 0)
                            wordListLoader.UpdateCandidates(currentInputString);
                    }
                    else {
                        currentInputLine = "";
                        currentInputString = "";
                    }
                }
                else if (inputStringTemplate[i] == "n")
                {
                    // enter
                    currentInputLine += 'n';
                    // flush input
                    currentInputString = "";
                    string curWord = "null";
                    if (wordListLoader.currentCandidates.Length > 0 && wordListLoader.currentCandidates[0] != null)
                    {
                        curWord = candidateHandler.CurrentGazedText == "" ? wordListLoader.currentCandidates[0] : candidateHandler.CurrentGazedText;// wordListLoader.currentCandidates[candidateHandler.GazedCandidate]; // 0 for now, 0 should be replaced by gaze result                        
                    }
                    // check if correct
                    if (phraseLoader.IsCurrentTypingCorrect(curWord, ProfileLoader.typingMode))
                    {
                        curWord = "<color=green>" + curWord + "</color>";
                    }
                    else
                    {
                        // mark the current typing to red and tell the users
                        curWord = "<color=red>" + curWord + "</color>";
                    }
                    Debug.Log("cur word:" + curWord);
                    currentTypedWords.Add(curWord);
                }
                else
                {
                    // regular input
                    currentInputLine += mapInput2InputString[inputStringTemplate[i]];
                    retrieveInputStringFromLine();
                    Debug.Log("input string:" + currentInputString);
                    wordListLoader.UpdateCandidates(currentInputString);
                }
                break;
            }
            if (Input.GetKeyUp(inputStringTemplate[i]))
            {
                // process the key down
                selectedFingers[i].SetActive(false);
                // move the finger back but keep the color changes
                if(i < 5)
                    handModel.ReleaseLeftFingers(i);
                else
                    handModel.ReleaseRightFingers(i-5);
            }
        }
        updateDisplayInput();
    }
}

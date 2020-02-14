﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public TMPro.TextMeshPro inputTextMesh;
    public WordlistLoader wordListLoader;
    public PhraseLoader phraseLoader;

    private string[] inputStringTemplate = new string[] { "q", "3", "4", "t", "b", "n", "u", "9", "0", "[" }; // to save time, we convert them to asdf del enter jkl;, and then maybe convert ; to p for json
    private Dictionary<string, string> mapInput2InputString;

    public string currentInputString; // to save current input string, refresh it when click 'enter'
    public CandidateHandler candidateHandler;

    // Start is called before the first frame update
    void Start()
    {
        inputTextMesh.text = "";
        currentInputString = "";
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

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < inputStringTemplate.Length; i++) {
            if (Input.GetKeyDown(inputStringTemplate[i])) {
                // process the key down
                if(inputStringTemplate[i] == "b") {
                    // delete
                    candidateHandler.ResetCandidates();
                    if (currentInputString.Length > 1) {
                        currentInputString = currentInputString.Substring(0, currentInputString.Length - 1);
                        wordListLoader.UpdateCandidates(currentInputString);
                    }
                    else {
                        // reset candidates
                        currentInputString = "";
                        //wordListLoader.ResetCandidates();
                    }                    
                }
                else if(inputStringTemplate[i] == "n") {
                    // enter
                    inputTextMesh.text = wordListLoader.currentCandidates[candidateHandler.GazedCandidate]; // 0 for now, 0 should be replaced by gaze result
                    // check if correct
                    phraseLoader.IsCurrentTypingCorrect(wordListLoader.currentCandidates[candidateHandler.GazedCandidate]);
                    // flush input
                    currentInputString = "";
                    // flush candidates
                    candidateHandler.ResetCandidates();
                }
                else {
                    // regular input
                    currentInputString += mapInput2InputString[inputStringTemplate[i]];
                    candidateHandler.ResetCandidates();
                    wordListLoader.UpdateCandidates(currentInputString);
                }
                break;
            }
        }
    }
}

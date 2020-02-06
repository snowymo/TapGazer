using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public TextMesh inputTextMesh;
    public WordlistLoader wordListLoader;

    private string[] inputStringTemplate = new string[] { "q", "3", "4", "t", "b", "n", "u", "9", "0", "[" }; // to save time, we convert them to asdf del enter jkl;, and then maybe convert ; to p for json
    private Dictionary<string, string> mapInput2InputString;

    public string currentInputString; // to save current input string, refresh it when click 'enter'

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

                    currentInputString = currentInputString.Substring(0, currentInputString.Length - 1);
                }
                else if(inputStringTemplate[i] == "n") {
                    // enter
                    inputTextMesh.text += wordListLoader.currentCandidates[0] + " ";
                    currentInputString = "";
                }
                else {
                    // regular input
                    currentInputString += mapInput2InputString[inputStringTemplate[i]];
                    wordListLoader.UpdateCandidates(currentInputString);
                }
                break;
            }
        }
    }
}

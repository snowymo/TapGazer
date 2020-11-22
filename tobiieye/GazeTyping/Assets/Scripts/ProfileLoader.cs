using Codeplex.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// User is supposed to run python script to generate customized config (key mapping) first
// If the user is willing to use the default one, the profile is empty ("")
// Otherwise, we need the name of the user
// And populate it to all related files: wordlistLoader and hand

public class ProfileLoader : MonoBehaviour {
  public TypingMode curTypingMode;
  public static TypingMode typingMode;
  public static string profile = "";
  public string curProfile;
  public static int session_number;
  public int curSessionNumber;
  public WordlistLoader wordlistLoader;
  /// <summary>
  /// configMap: 26 -> 8
  /// letterMap: 8 -> 26
  /// </summary>
  [SerializeField]
  public static Dictionary<string, string> configMap;
  [SerializeField]
  public static Dictionary<string, List<char>> letterMap;
  int[] renderTextureIndices;
  public DynamicHandKey dynamicHandKey;
  public Keyboard dynamicKeyColor;
  public GridHintKeyboard dynamicGridHint;
  public Transform keyboardHint, handmodelHint, gridHint;
  public bool showKeyboard, showHandModel, showGridHint;

  public GameObject regularInput, newKeyboardInput;
  public OutputMode curOutputMode;
  public static OutputMode outputMode;

  // training is new keyboard + type correct then continue
  // test is new keyboard + keep typing
  // regular is regular keyboard + keep typing
  public enum TypingMode { TRAINING, TEST, REGULAR, TAPPING };
  public enum OutputMode { Devkit, Trackerbar, Screen };
  public enum InputMode { KEYBOARD, TOUCH };
  public static InputMode inputMode;
  public InputMode curInputMode;
  /// <summary>
  /// enterMode
  /// right thumb: only use right thumb for enter, left thumb is reserved for deletion
  /// both thumb: both thumbs could be used for enter, two thumbs pressing together are for deletion
  /// </summary>
  public enum EnterMode { RIGHT_THUMB, BOTH_THUMB};
  public static EnterMode enterMode;
  [SerializeField] private EnterMode curEnterMode;
  /// <summary>
  /// wordCompletion mode
  /// WC: with word completion: show completed word firsts, then from most frequent to not frequent
  /// NC: no word completion: show one word of which the length is shortest when no complete candidate; show only complete candidates when exist
  /// </summary>
  public enum WordCompletionMode { WC, NC};
  public static WordCompletionMode wcMode;
  public WordCompletionMode curWcMode;
  /// <summary>
  /// selection mode
  /// MS: manual selection: select the most common word by right thumb; left thumb + extra tap could select any one of 5 candidates
  /// GS: gaze selection:
  /// GSE: select the correct word any either thumb
  /// GSR: select the most common word by right thumb and all other available words by left thumb
  /// </summary>
  public enum SelectionMode { MS, GSE, GSR};
  public static SelectionMode selectionMode;
  public SelectionMode curSelectionMode;

  public enum CandLayout { ROW, FAN, BYCOL, LEXIC, WORDCLOUD, DIVISION, DIVISION_END, ONE };
  public CandLayout curCandidateLayout;
  public static CandLayout candidateLayout;

  public static int typingSeconds = 60;
  public static int typingPhraseCount = 5;
  public static Dictionary<string, string> mapInput2InputString;
  public static Dictionary<string, List<string>> mapInputString2Letter;
  public static bool enableDeletion = true;

    public bool curIsSimulated;
    public static bool isSimulated;

  // profile.name includes
  // name
  // typing seconds
  // number of phrase in this session
  // next 10 lines show the letters that could be considered as from LPinky to RPinky
  private void loadFromProfileName()
  {
    //profile = curProfile;
    string fileConfig = File.ReadAllText(Application.streamingAssetsPath + "/profile.name");
    string[] fileConfigDetails = fileConfig.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
    if(fileConfigDetails.Length < 13)
    {
      Debug.LogError("Failed to find 13 lines in profile.name");
    }
    curProfile = profile = fileConfigDetails[0];
    typingSeconds = int.Parse(fileConfigDetails[1]);
    typingPhraseCount = int.Parse(fileConfigDetails[2]);
    mapInput2InputString = new Dictionary<string, string>();
    mapInputString2Letter = new Dictionary<string, List<string>>();
    string[] fingerIDs = new string[] { "a", "s", "d", "f", "b", "n", "j", "k", "l", ";" }; // "b" should be never used and n means space
    if (fileConfigDetails[7] == fileConfigDetails[8])
    {
      enableDeletion = false;
    }
    for (int i = 3; i < 13; i++)
    {
      // seperate by ","
      string[] curLetters = fileConfigDetails[i].Split(new char[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
      for(int letterIdx = 0; letterIdx < curLetters.Length; letterIdx++)
      {
        if (!mapInput2InputString.ContainsKey(curLetters[letterIdx]))
        {
          mapInput2InputString.Add(curLetters[letterIdx], fingerIDs[i - 3]);
        }        
        List<string> temp;
        if(mapInputString2Letter.TryGetValue(fingerIDs[i - 3], out temp))
        {
          // add to list
          mapInputString2Letter[fingerIDs[i - 3]].Add(curLetters[letterIdx]);
        }
        else
        {
          mapInputString2Letter.Add(fingerIDs[i - 3], new List<string>() { curLetters[letterIdx] });
        }
      }
    }
    
  }

  // Start is called before the first frame update
  void Awake() {
    loadFromProfileName();

    wordlistLoader.wordlistPath = "top0.9-result" + profile + ".json";
    typingMode = curTypingMode;
    inputMode = curInputMode;
    session_number = curSessionNumber;
    outputMode = curOutputMode;
    //curEnterMode = EnterMode.BOTH_THUMB;
    enterMode = curEnterMode;
    wcMode = curWcMode;
    selectionMode = curSelectionMode;
    candidateLayout = curCandidateLayout;
        isSimulated = curIsSimulated;
    Debug.Log("typing mode:" + typingMode);
    loadConfigFile();
    if (showHandModel) {
      handmodelHint.gameObject.SetActive(true);
      updateRenderTexture();
    }
    if (showKeyboard) {
      keyboardHint.gameObject.SetActive(true);
      dynamicKeyColor.SetFinger(configMap);
    }
    if (showGridHint)
    {
      gridHint.gameObject.SetActive(true);
      dynamicGridHint.SetFinger(configMap);
    }

    //if (typingMode == TypingMode.REGULAR)
    //  newKeyboardInput.SetActive(false);
    //else
    // use newKeyboardInput all the time I guess
      regularInput.SetActive(false);

    if(typingMode == TypingMode.REGULAR || typingMode == TypingMode.TAPPING)
      // screen mode
      UnityEngine.XR.XRSettings.enabled = false;
  }

  private void loadEnvironemnt() {
    // apply corresponding environment
    if(outputMode == OutputMode.Devkit) {
      // vr mode
      UnityEngine.XR.XRSettings.enabled = true;
    } else if(outputMode == OutputMode.Trackerbar) {
      // Trackerbar mode
      UnityEngine.XR.XRSettings.enabled = false;
    }else if(outputMode == OutputMode.Screen)
    {
      // Screen mode
      UnityEngine.XR.XRSettings.enabled = false;
    }
  }

  protected void loadConfigFile() {
    configMap = new Dictionary<string, string>();
    letterMap = new Dictionary<string, List<char>>();
    string configPath = Application.streamingAssetsPath + "/config" + profile + ".json";
    string configContent = "";
    if (File.Exists(configPath))
      configContent = File.ReadAllText(configPath);
    else
    {
      Debug.LogError("File " + configPath + " not exists");
    }

    dynamic configJson = DynamicJson.Parse(configContent);
    foreach (KeyValuePair<string, dynamic> item in configJson) {
      if (configMap.ContainsKey(item.Key)) {
        Debug.LogWarning("key: " + item.Key + " already exists.");
      } else {
        configMap.Add(item.Key, item.Value.ToString());
      }
      if (letterMap.ContainsKey(item.Value))
      {
        letterMap[item.Value].Add(item.Key[0]);
      } else
      {
        letterMap[item.Value] = new List<char>();
        letterMap[item.Value].Add(item.Key[0]);
      }
    }
  }

  protected void updateRenderTexture() {
    renderTextureIndices = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
    // iterate all the keys in configMap and put them in the correct place
    foreach (string key in configMap.Keys) {
      char curKey = key[0];
      if (curKey == 'l')
        curKey = 'L';
      if (curKey == 'r')
        curKey = 'R';
      if (curKey == 'y')
        curKey = 'Y';
      if (configMap[key] == "a") {
        // put it in the first finger
        // for the first finger, we need to skip two \n
        dynamicHandKey.SetFingerKey(0, 1, renderTextureIndices[0], curKey);
        renderTextureIndices[0] = renderTextureIndices[0] + 1;
      } else if (configMap[key] == "s") {
        // put it in the second finger
        // for the first finger, we need to skip two \n
        dynamicHandKey.SetFingerKey(1, 1, renderTextureIndices[1], curKey);
        renderTextureIndices[1] = renderTextureIndices[1] + 1;
      } else if (configMap[key] == "d") {
        // put it in the third finger
        // for the first finger, we need to skip two \n
        dynamicHandKey.SetFingerKey(2, 0, renderTextureIndices[2], curKey);
        renderTextureIndices[2] = renderTextureIndices[2] + 1;
      } else if (configMap[key] == "f") {
        // put it in the fourth finger
        // for the first finger, we need to skip two \n
        dynamicHandKey.SetFingerKey(3, 0, renderTextureIndices[3], curKey);
        renderTextureIndices[3] = renderTextureIndices[3] + 1;
      } else if (configMap[key] == "j") {

        // for the first finger, we need to skip two \n
        dynamicHandKey.SetFingerKey(4, 0, renderTextureIndices[4], curKey);
        renderTextureIndices[4] = renderTextureIndices[4] + 1;
      } else if (configMap[key] == "k") {

        // for the first finger, we need to skip two \n
        dynamicHandKey.SetFingerKey(5, 0, renderTextureIndices[5], curKey);
        renderTextureIndices[5] = renderTextureIndices[5] + 1;
      } else if (configMap[key] == "l") {

        // for the first finger, we need to skip two \n
        dynamicHandKey.SetFingerKey(6, 1, renderTextureIndices[6], curKey);
        renderTextureIndices[6] = renderTextureIndices[6] + 1;
      } else if (configMap[key] == ";") {

        // for the first finger, we need to skip two \n
        dynamicHandKey.SetFingerKey(7, 0, renderTextureIndices[7], curKey);
        renderTextureIndices[7] = renderTextureIndices[7] + 1;
      }
    }

    dynamicHandKey.UpdateText();
  }
}

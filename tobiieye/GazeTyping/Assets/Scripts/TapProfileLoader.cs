using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ProfileLoader;

public class TapProfileLoader : ProfileLoader
{
  void Awake() {
    profile = curProfile;
    wordlistLoader.wordlistPath = "30k-result" + profile + ".json";
    typingMode = curTypingMode;
    inputMode = curInputMode;
    session_number = curSessionNumber;
    outputMode = curOutputMode;

    Debug.Log("typing mode:" + typingMode);

    loadConfigFile();

    if (showHandModel)
    {
      handmodelHint.gameObject.SetActive(true);
      updateRenderTexture();
    }
    if (showKeyboard)
    {
      keyboardHint.gameObject.SetActive(true);
      dynamicKeyColor.SetFinger(configMap);
    }
    if (showGridHint)
    {
      gridHint.gameObject.SetActive(true);
      dynamicGridHint.SetFinger(configMap);
    }

    if (typingMode == TypingMode.REGULAR)
      newKeyboardInput.SetActive(false);
    else
      regularInput.SetActive(false);

    // screen mode
    UnityEngine.XR.XRSettings.enabled = false;
  }
}

using Codeplex.Data;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// User is supposed to run python script to generate customized config (key mapping) first
// If the user is willing to use the default one, the profile is empty ("")
// Otherwise, we need the name of the user
// And populate it to all related files: wordlistLoader and hand

public class ProfileLoader : MonoBehaviour
{
    public TypingMode curTypingMode;
    public static TypingMode typingMode;
    public static string profile = "";
    public WordlistLoader wordlistLoader;
    [SerializeField]
    public static Dictionary<string, string> configMap;
    int[] renderTextureIndices;
    public DynamicHandKey dynamicHandKey;
    public Keyboard dynamicKeyColor;

    public GameObject regularInput, newKeyboardInput;

    // training is new keyboard + type correct then continue
    // test is new keyboard + keep typing
    // regular is regular keyboard + keep typing
    public enum TypingMode { TRAINING, TEST, REGULAR}; 

    // Start is called before the first frame update
    void Awake()
    {
        wordlistLoader.wordlistPath = "30k-result" + profile + ".json";
        typingMode = curTypingMode;
        Debug.Log("typing mode:" + typingMode);
        loadConfigFile();
        updateRenderTexture();
        if (typingMode == TypingMode.REGULAR)
            newKeyboardInput.SetActive(false);
        else
            regularInput.SetActive(false);
    }

    private void loadConfigFile()
    {
        configMap = new Dictionary<string, string>();
        string configPath = Application.dataPath + "/Resources/config" + profile+ ".json";
        string configContent = File.ReadAllText(configPath);


        dynamic configJson = DynamicJson.Parse(configContent);
        foreach (KeyValuePair<string, dynamic> item in configJson)
        {      
            if (configMap.ContainsKey(item.Key))
            {
                Debug.LogWarning("key: " + item.Key + " already exists.");
            }
            else
            {
                configMap.Add(item.Key, item.Value.ToString());
            }
        }
    }

    private void updateRenderTexture()
    {
        renderTextureIndices = new int[8] { 0,0,0,0,0,0,0,0 };
        // iterate all the keys in configMap and put them in the correct place
        foreach (string key in configMap.Keys)
        {
            if(configMap[key] == "a")
            {
                // put it in the first finger
                // for the first finger, we need to skip two \n
                dynamicHandKey.SetFingerKey(0, 2, renderTextureIndices[0], key[0]);
                renderTextureIndices[0] = renderTextureIndices[0] + 1;
            }else if (configMap[key] == "s")
            {
                // put it in the second finger
                // for the first finger, we need to skip two \n
                dynamicHandKey.SetFingerKey(1, 1, renderTextureIndices[1], key[0]);
                renderTextureIndices[1] = renderTextureIndices[1] + 1;
            }
            else if (configMap[key] == "d")
            {
                // put it in the third finger
                // for the first finger, we need to skip two \n
                dynamicHandKey.SetFingerKey(2, 0, renderTextureIndices[2], key[0]);
                renderTextureIndices[2] = renderTextureIndices[2] + 1;
            }
            else if (configMap[key] == "f")
            {
                // put it in the fourth finger
                // for the first finger, we need to skip two \n
                dynamicHandKey.SetFingerKey(3, 0, renderTextureIndices[3], key[0]);
                renderTextureIndices[3] = renderTextureIndices[3] + 1;
            }else if (configMap[key] == "j")
            {
                
                // for the first finger, we need to skip two \n
                dynamicHandKey.SetFingerKey(4, 0, renderTextureIndices[4], key[0]);
                renderTextureIndices[4] = renderTextureIndices[4] + 1;
            }
            else if (configMap[key] == "k")
            {
                
                // for the first finger, we need to skip two \n
                dynamicHandKey.SetFingerKey(5, 0, renderTextureIndices[5], key[0]);
                renderTextureIndices[5] = renderTextureIndices[5] + 1;
            }
            else if (configMap[key] == "l")
            {
                
                // for the first finger, we need to skip two \n
                dynamicHandKey.SetFingerKey(6, 1, renderTextureIndices[6], key[0]);
                renderTextureIndices[6] = renderTextureIndices[6] + 1;
            }
            else if (configMap[key] == ";")
            {
                
                // for the first finger, we need to skip two \n
                dynamicHandKey.SetFingerKey(7, 2, renderTextureIndices[7], key[0]);
                renderTextureIndices[7] = renderTextureIndices[7] + 1;
            }
        }

        dynamicHandKey.UpdateText();
        dynamicKeyColor.SetFinger(configMap);
    }
}

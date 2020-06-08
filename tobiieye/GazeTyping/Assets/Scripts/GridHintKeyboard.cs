using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// update current keyboard (finger to key mapping) to grid hint keyboard

public class GridHintKeyboard : MonoBehaviour
{
  public TMPro.TextMeshPro[] gridKeyObjects;

  private Dictionary<string, string> finger2key;
  private Dictionary<string, int> input2index = new Dictionary<string, int>() { { "a", 0 }, { "s", 1 }, { "d", 2 }, { "f", 3 }, { "j", 4 }, { "k", 5 }, { "l", 6 }, { ";", 7 } };

  // Start is called before the first frame update
  void setup() {
    finger2key = new Dictionary<string, string>();
    foreach (TMPro.TextMeshPro tm in gridKeyObjects)
      tm.text = "";
  }

  public void SetFinger(Dictionary<string, string> configMap) {
    setup();
    int charA = 'a';
    for (int i = 0; i < 26; i++)
    {
      int objIndex = i / 9;
      char curChar = System.Convert.ToChar(charA + i);
      string curStr = "" + curChar;
      string curFinger = input2index[configMap[curStr]] < 4 ? "L" + (input2index[configMap[curStr]] + 1).ToString() : "R" + (input2index[configMap[curStr]] - 3).ToString();
      finger2key.Add(curStr, curFinger+"  ");
      // update to textMeshPro
      gridKeyObjects[objIndex].text += "<color=yellow>" + curStr + "</color>:<color=orange>" + curFinger + "</color>  ";
      if (i % 3 == 2)
        gridKeyObjects[objIndex].text += "\n";
    }


  }
}

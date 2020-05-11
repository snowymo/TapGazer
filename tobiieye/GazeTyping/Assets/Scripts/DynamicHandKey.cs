using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DynamicHandKey : MonoBehaviour
{
    public TMPro.TextMeshPro[] fingerText;

    public void SetFingerKey(int fingerIndex, int skipCount, int keyIndex, char key)
    {
        string curText = fingerText[fingerIndex].text;
        int tobeModifiedIndex = 0;
        int times = skipCount + keyIndex;
        while (times-- >= 0)
        {
            tobeModifiedIndex = curText.IndexOf("\\n", tobeModifiedIndex)+2;
        }
        StringBuilder sb = new StringBuilder(curText);
    
    if(tobeModifiedIndex < 3) {
      Debug.LogWarning("curText:" + curText + "\ttobe modified index:" + tobeModifiedIndex + "\tkey:" + key);
    } else {
      sb[tobeModifiedIndex - 3] = key;
      curText = sb.ToString();
      fingerText[fingerIndex].text = curText;
    }        
    }

    public void UpdateText()
    {
        for(int i = 0; i < fingerText.Length; i++)
        {
            fingerText[i].text = fingerText[i].text.Replace("\\n", "\n");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keyboard : MonoBehaviour
{
    // generate quads for later configuration
    Vector3 qpos = new Vector3(3.6f, 0.1f, -1.873f);
    Vector3 apos = new Vector3(3.44f, 0.1f, 0.043f);
    Vector3 zpos = new Vector3(3.12f, 0.1f, 1.954f);
    float deltax = 0.65f;
    float deltaz = 2.3f;
    public GameObject keyPrefab;
    string[] firstLine = { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p" };
    string[] secondLine = { "a", "s", "d", "f", "g", "h", "j", "k", "l" };
    string[] thirdLine = { "z", "x", "c", "v", "b", "n", "m" };
    Dictionary<string, int> input2index = new Dictionary<string, int>() { { "a", 0 }, { "s", 1 }, { "d", 2 }, { "f", 3 }, { "j", 4 }, { "k", 5 }, { "l", 6 }, { ";", 7 } };
    List<GameObject> keyObjects;
    public Material[] keyColors;

    private void Setup()
    {
        keyObjects = new List<GameObject>();
        Vector3 theScale = keyPrefab.transform.localScale;
        // create a to z
        for (int i = 0; i < 10; i++)
        {
            // first line
            GameObject go = Instantiate(keyPrefab);
            go.name = firstLine[i];
            go.transform.parent = transform;
            go.transform.localPosition = qpos + new Vector3(-deltax * i, 0, 0);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = theScale;
            keyObjects.Add(go);
        }
        for (int i = 0; i < 9; i++)
        {
            // seconde line
            GameObject go = Instantiate(keyPrefab);
            go.name = secondLine[i];
            go.transform.parent = transform;
            go.transform.localPosition = apos + new Vector3(-deltax * i, 0, 0);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = theScale;
            keyObjects.Add(go);
        }
        for (int i = 0; i < 7; i++)
        {
            // seconde line
            GameObject go = Instantiate(keyPrefab);
            go.name = thirdLine[i];
            go.transform.parent = transform;
            go.transform.localPosition = zpos + new Vector3(-deltax * i, 0, 0);
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = theScale;
            keyObjects.Add(go);
        }
        // apply configuration
    }

    public void SetFinger(Dictionary<string,string> configMap)
    {
        Setup();
        for (int i = 0; i < keyObjects.Count; i++)
        {
            keyObjects[i].GetComponent<Renderer>().material = keyColors[input2index[configMap[keyObjects[i].name]]];
        }
    }
}

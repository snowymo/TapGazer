﻿using Codeplex.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
// load the json file generated by python
public class WordlistLoader : MonoBehaviour {
    //public class CustomerInvoice {
    //    // everything else gets stored here
    //    [JsonExtensionData]
    //    private IDictionary<string, JToken> _additionalData;
    //}

    private Dictionary<string, string[]> wordDict;
    private string wordlistContent;
    private dynamic wordlistJson;
    public int wordDictCount;
    public string testInputString;
    //public Candidate candText0;
    public string[] currentCandidates;
    public CandidateHandler candidateHandler;
    private int currentProgress; // the number of the string was typed
    public string wordlistPath;
    [SerializeField]
    private int preloadedCandidates; // 20 for regular option, but actually 11 is enough lol. 48 for by-column option, in case we want it to be 6*8

    // Start is called before the first frame update
    void Start()
    {
        preloadedCandidates = 54;
        wordDict = new Dictionary<string, string[]>();
        currentCandidates = new string[preloadedCandidates];

        if (wordlistPath == "")
            wordlistPath = Application.dataPath + "/Resources/noswear10k-result.json";
        else
            wordlistPath = Application.dataPath + "/Resources/" + wordlistPath;
        wordlistContent = File.ReadAllText(wordlistPath);
        //candText0.SetCandidateText("");

        // test
        wordlistContent = wordlistContent.Replace(";", "p");

        //var data = JsonUtility.Parse(wordlistPath);
        // Parse (from JsonString to DynamicJson)

        wordlistJson = DynamicJson.Parse(wordlistContent);
        foreach (KeyValuePair<string, dynamic> item in wordlistJson) {
            string temp = item.Value.ToString();
            temp = temp.Replace("\"", "");
            temp = temp.Replace("[", "");
            temp = temp.Replace("]", "");
            if (wordDict.ContainsKey(item.Key)) {
                Debug.LogWarning("key: " + item.Key + " already exists.");
            }
            else {
                string[] cands = temp.Split(new char[] { ',' });
                string[] first20cand = new string[Mathf.Min(cands.Length, preloadedCandidates)];
                Array.Copy(cands, first20cand, first20cand.Length);
                wordDict.Add(item.Key, first20cand);
            }                
        }
        wordDictCount = wordDict.Count;

        //var r1 = json.foo; // "json" - dynamic(string)
        //var r2 = json.bar; // 100 - dynamic(double)
        //var r3 = json.nest.foobar; // true - dynamic(bool)
        //var r4 = json["nest"]["foobar"]; // can access indexer
    }

    public string[] GetCandidates(string inputString)
    {
        // turn ";" to "p"
        inputString = inputString.Replace(";", "p");
        if (wordDict.ContainsKey(inputString)) {
            return wordDict[inputString];
        }
        else {
            Debug.LogWarning("no candidates for " + inputString);
            return new string[0];
        }            
    }

    public void ResetCandidates()
    {
        currentProgress = 0;
        //candText0.SetCandidateText("");
        candidateHandler.ResetCandidates();
    }

    public void UpdateCandidates(string inputString)
    {
        currentProgress = inputString.Length;
        // turn ";" to "p"
        if (!wordDict.ContainsKey(inputString)) {
            Debug.LogWarning("no candidates for " + inputString);
        }
        inputString = inputString.Replace(";", "p");
        if (!wordDict.ContainsKey(inputString)) {
            Debug.LogWarning("no candidates for " + inputString);
            return;
        }
        //candText0.SetCandidateText(wordDict[inputString][0], currentProgress); // for now
        int i = 0;
        for (; i < Mathf.Min(currentCandidates.Length, wordDict[inputString].Length); i++) {
            currentCandidates[i] = wordDict[inputString][i];
        }
        for (; i < Mathf.Max(currentCandidates.Length, wordDict[inputString].Length); i++)
        {
            currentCandidates[i] = "";
        }
        candidateHandler.UpdateCandidates(currentCandidates, currentProgress);
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space)) {
        //    candTextMesh.text = GetCandidates(testInputString)[0]; // for now
        //}
    }
}

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

    private Dictionary<string, string[]> wordDict, completeCandDict;
    private string wordlistContent, completeCandContent;
    private dynamic wordlistJson, completeCandJson;
    public int wordDictCount;
    public string testInputString;
    //public Candidate candText0;
    public string[] currentCandidates;
    public CandidateHandler candidateHandler;
    private int currentProgress; // the number of the string was typed
    public string wordlistPath;
    private string completeCandPath;
    [SerializeField]
    private int preloadedCandidates; // 20 for regular option, but actually 11 is enough lol. 48 for by-column option, in case we want it to be 6*8
    [SerializeField]
    private int preloadedCompleteCandidates;    // 13 complete candidate
    public GameObject helpInfo;

    // Start is called before the first frame update
    void Start()
    {
        preloadedCandidates = 54;
        preloadedCompleteCandidates = 12;
        wordDict = new Dictionary<string, string[]>();
        completeCandDict = new Dictionary<string, string[]>();
        currentCandidates = new string[preloadedCandidates];

        if (wordlistPath == "")
            wordlistPath = Application.dataPath + "/Resources/noswear10k-result.json";
        else
            wordlistPath = Application.dataPath + "/Resources/" + wordlistPath;
        completeCandPath = wordlistPath.Replace("result", "cand");
        wordlistContent = File.ReadAllText(wordlistPath);
        completeCandContent = File.ReadAllText(completeCandPath);
        //candText0.SetCandidateText("");

        // test
        wordlistContent = wordlistContent.Replace(";", "p");
        completeCandContent = completeCandContent.Replace(";", "p");

        //var data = JsonUtility.Parse(wordlistPath);
        // Parse (from JsonString to DynamicJson)
        completeCandJson = DynamicJson.Parse(completeCandContent);
        foreach (KeyValuePair<string, dynamic> item in completeCandJson)
        {
            string temp = item.Value.ToString();
            temp = temp.Replace("\"", "");
            temp = temp.Replace("[", "");
            temp = temp.Replace("]", "");
            if (completeCandDict.ContainsKey(item.Key))
            {
                Debug.LogWarning("key: " + item.Key + " already exists.");
            }
            else
            {
                string[] cands = temp.Split(new char[] { ',' });
                completeCandDict.Add(item.Key, cands);
            }
        }

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
                // load at most #preloadedCandidates candidate, and include at least #preloadedCompleteCandidates complete candidates, except there are not that many
                string[] first20cand = new string[Mathf.Min(cands.Length, preloadedCandidates)];
                int curCompletedCand = 0;
                if (completeCandDict.ContainsKey(item.Key))
                {
                    curCompletedCand = Mathf.Min(completeCandDict[item.Key].Length, preloadedCompleteCandidates);
                }                
                int totalCandCount = 0, completeCandCount = 0;
                while(totalCandCount < first20cand.Length && ( totalCandCount < preloadedCandidates || completeCandCount < curCompletedCand))
                {
                    if (cands[totalCandCount].Length == item.Key.Length)
                    {
                        ++completeCandCount;
                        first20cand[totalCandCount] = cands[totalCandCount++];
                    }
                    else if(totalCandCount + (curCompletedCand - completeCandCount) == first20cand.Length)
                    {
                        // reaches the max of the incompleted candidates capacity
                        // no more incompleted candidates
                        //Debug.Log("no more incompleted candidates");
                        // append the rest completed candidates to the array
                        Array.Copy(completeCandDict[item.Key], completeCandCount, first20cand, totalCandCount, curCompletedCand - completeCandCount);
                        break;
                    }
                    else
                    {
                        first20cand[totalCandCount] = cands[totalCandCount++];
                    }
                    
                }
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
        //if (!wordDict.ContainsKey(inputString)) {
            //Debug.LogWarning("no candidates for " + inputString);
        //}
        inputString = inputString.Replace(";", "p");
        if (!wordDict.ContainsKey(inputString)) {
            //Debug.LogWarning("no candidates for " + inputString);
            // tell the users there are no candidates in the dictionary
            currentCandidates = new string[preloadedCandidates];
            candidateHandler.ResetCandidates();
            helpInfo.SetActive(true);
            return;
        }
        //candText0.SetCandidateText(wordDict[inputString][0], currentProgress); // for now
        // make sure currentCandidates loaded all the complete candidates
        if (preloadedCandidates < wordDict[inputString].Length)
        {
            currentCandidates = new string[preloadedCandidates];
        }
        currentCandidates = wordDict[inputString];
        //Debug.Log("input string:" + inputString + " candidates length " + currentCandidates.Length);
        //for(int i = 0; i < currentCandidates.Length; i++) {
        //    Debug.Log("currentCandidates[" + i + "]:" + currentCandidates[i]);
        //}
        if (completeCandDict.ContainsKey(inputString))
        {
            candidateHandler.UpdateCandidates(currentCandidates, currentProgress, completeCandDict[inputString]);
        }
        else
        {
            candidateHandler.UpdateCandidates(currentCandidates, currentProgress, new string[0]);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space)) {
        //    candTextMesh.text = GetCandidates(testInputString)[0]; // for now
        //}
    }
}

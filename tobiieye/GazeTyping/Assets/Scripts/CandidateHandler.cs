using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CandidateHandler : MonoBehaviour
{

    public GameObject CandidatePrefab, FanLayoutCandidatePrefab;

    [SerializeField]
    int CandidateCount = 11;
    float CandidateWidth; // roughly it fits for 5-character word
    float CandidateHeight = -1.5f;
    int CandidatePerRow;
    public int GazedCandidate = 0; // index of the candidate being gazed
    float perWidth;
    public float fanRadius = 4f;
    public float verticalScale = 0.5f;
    public float fanAngle = 30f;
    public float textHeight = 0.8f;
    Dictionary<int, int> fanHorizontalMap;
    List<List<string>> candidateColumns;

    public enum CandLayout { ROW, FAN, BYCOL, LEXIC, WORDCLOUD};
    public CandLayout candidateLayout;

    private List<GameObject> candidateObjects;
    public string CurrentGazedText;
    [SerializeField]
    private float kSizeScale;

    public bool isEllipsis;
    
    // map btw regular keys and input string
    Dictionary<char, int> mapKey2Column = new Dictionary<char, int>()
    {
        {'q',0}, { 'a',0},{'z',0}, {'w',1}, {'s',1}, {'x',1}, {'e',2}, {'d', 2}, {'c', 2}, {'r', 3}, {'f', 3}, {'v', 3}, {'t',3}, {'g', 3}, {'b',3},
        {'y',4}, { 'h',4},{'n',4}, {'u',4}, {'j',4}, {'i',5}, {'k',5}, {'m', 5}, {'o', 6}, {'l', 6}, {'p', 7}
    };

    // show the prefab at the corresponding place
    // step 1: show the candidates directly, maybe 10 for now,
    // Question: maybe candidates section should not be attached to camera
    // step 2: thinking about the width of the candidates and put it correctly

    // Start is called before the first frame update
    void Start()
    {
        isEllipsis = false;
        candidateObjects = new List<GameObject>();
        perWidth = 0.73f;
        kSizeScale = 1f;
        if (candidateLayout == CandLayout.ROW)
            CreateRowLayout();
        else if(candidateLayout == CandLayout.FAN)
            updateFanLayout();
        else if(candidateLayout == CandLayout.LEXIC)
        {
            CreateRowLayout();
            // order the candidates by lexical order
        }else if(candidateLayout == CandLayout.BYCOL)
        {
            CreateColumnLayout();
        }else if(candidateLayout == CandLayout.WORDCLOUD) {
            CreateWordCloudLayout();
        }        
    }

    private void CreateWordCloudLayout()
    {
        // let's use a 4x4 here
        // first, we can show some candidates with high freq, but let's do it next
        //
        isEllipsis = true;
        CandidateCount = 16;
        int maxLength = 10;
        CandidatePerRow = 4;
        perWidth = 0.75f;
        CandidateWidth = perWidth * maxLength;
        for (int i = 0; i < CandidateCount; i++) {
            GameObject go = Instantiate(CandidatePrefab, transform);
            go.name = "Cand" + i.ToString();
            go.transform.localPosition = new Vector3(-CandidateWidth * (CandidatePerRow - 1) / 2 + (i % CandidatePerRow) * CandidateWidth, i / CandidatePerRow * CandidateHeight - 2.5f, 0);
            go.GetComponent<Candidate>().SetCandidateText("");
            go.GetComponent<Candidate>().candidateIndex = i;
            go.GetComponent<Candidate>().candidateHandler = this;
            candidateObjects.Add(go);
        }
    }

    private int[] DoCandidateShowPreviously(string[] candidates, ref List<int> availableIndices)
    {
        int[] result = new int[candidates.Length];
        for (int i = 0; i < candidateObjects.Count; i++)
            availableIndices.Add(i);

        for (int i = 0; i < candidates.Length; i++) {
            result[i] = -1;
            for (int j = 0; j < candidateObjects.Count; j++) {
                if(candidates[i].Equals(candidateObjects[j].GetComponent<Candidate>().pureText, StringComparison.CurrentCultureIgnoreCase)) {
                    result[i] = j;
                    break;
                }
            }
            if (result[i] > -1)
                availableIndices.Remove(result[i]);
        }
        return result;
    }

    float MapIndex2Size(int index)
    {
        // definitely it won't be x -> x
        // let's use 16-x for now?
        index = Mathf.Min(15, index);
        //float answer = (54.0f - index) / 54.0f * 10.0f;
        float answer = 200.0f - index * 8.5f;
        return answer;
    }

    int[] priorityIndices = new int[] { 5, 10, 9, 6, 0, 15, 12, 3, 1, 14, 8, 7, 2, 13, 4, 11 };
    private int findAvailableIndex(List<int> availableIndices)
    {
        
        // let's try put it in the center and then go to the side
        // a manual order for 0-15: 5,10, 9, 6, 0, 15, 12, 3, 1,14,8,7,2,13,4,11
        for (int i = 0; i < priorityIndices.Length; i++)
        {
            if(availableIndices.IndexOf(priorityIndices[i]) > -1)
            {
                return priorityIndices[i];
            }
        }
        return -1;
    }

    private void UpdateWordCloudLayout(string[] candidates, string[] allCandidates, int progress)
    {
        // we need to update the position at the same time
        // instead of calculate=ing the word length for both lines, and finding the longer one, let's predefine a LONG number and apply
        int maxLength = 9;
        CandidateWidth = perWidth * maxLength;
        int candNum = Mathf.Min(candidates.Length, CandidateCount);

        // grid layout
        List<int> availableIndices = new List<int>();
        int[] showPreviously = DoCandidateShowPreviously(candidates, ref availableIndices);
        ResetCandidates();

        for (int i = 0; i < candNum; i++) {
            // update the text, check if the candidate shows up last time, we don't apply any change
            // 'candidates' was updated after typing, 'candidateObjects' wasn't
            if (showPreviously[i] > -1) {
                // shows before, only apply the new progress
                float candSize = MapIndex2Size(Array.IndexOf(allCandidates, candidates[i])) * kSizeScale;
                candidateObjects[showPreviously[i]].GetComponent<Candidate>().SetCandidateText(candidates[i], progress, candSize, isEllipsis);
            }
            else {
                // find an available index in candidateObjects, find from availableIndices
                // instead of doing it randomly, we should have a consitent order for the index
                int randIndex = findAvailableIndex(availableIndices);
                availableIndices.Remove(randIndex);                
                // the position is decided during creation
                // the size is related to where it is in allCandidates
                float candSize = MapIndex2Size(Array.IndexOf(allCandidates, candidates[i])) * kSizeScale;
                candidateObjects[randIndex].GetComponent<Candidate>().SetCandidateText(candidates[i], progress, candSize, isEllipsis);
            }
        }        
    }

    void CreateRowLayout()
    {
        CandidateCount = 13;
        // the first one is placed in the center
        GameObject go = Instantiate(CandidatePrefab, transform);
        go.name = "Cand0";
        go.GetComponent<Candidate>().SetCandidateText("");
        go.GetComponent<Candidate>().candidateIndex = 0;
        go.GetComponent<Candidate>().candidateHandler = this;
        candidateObjects.Add(go);
        CandidatePerRow = 4;
        // the rest are placed in two rows
        for (int i = 0; i < CandidateCount-1; i++)
        {
            go = Instantiate(CandidatePrefab, transform);
            go.name = "Cand" + (i + 1).ToString();
            go.transform.localPosition = new Vector3(-CandidateWidth * (CandidatePerRow - 1) / 2 + (i % CandidatePerRow) * CandidateWidth, i / CandidatePerRow * CandidateHeight - 1.5f, 0);
            go.GetComponent<Candidate>().SetCandidateText("");
            go.GetComponent<Candidate>().candidateIndex = i + 1;
            go.GetComponent<Candidate>().candidateHandler = this;
            candidateObjects.Add(go);
        }
    }

    void CreateColumnLayout()
    {
        // 8 columns + 1
        CandidatePerRow = 9;
        CandidateCount = CandidatePerRow * candidateNumberPerColumn;
        
        candidateColumns = new List<List<string>>();
        for(int i = 0; i < CandidatePerRow; i++)
        {
            candidateColumns.Add(new List<string>());
        }

        // the rest are placed in two rows
        for (int i = 0; i < CandidateCount; i++)
        {
            GameObject go = Instantiate(CandidatePrefab, transform);
            go.name = "Cand" + i.ToString();
            go.transform.localPosition = new Vector3(-CandidateWidth * (CandidatePerRow-1) / 2 + (i % CandidatePerRow) * CandidateWidth, i / CandidatePerRow * CandidateHeight - 1.5f, 0);
            go.GetComponent<Candidate>().SetCandidateText("");
            go.GetComponent<Candidate>().candidateIndex = i;
            go.GetComponent<Candidate>().candidateHandler = this;
            candidateObjects.Add(go);
        }
    }

    void updateFanLayout()
    {
        CandidateCount = 11;
        // the first one is placed in the center
        GameObject fanLayout = Instantiate(FanLayoutCandidatePrefab, transform);
        // fill the list with FanLayoutCandidatePrefab
        for (int i = 0; i < CandidateCount; i++)
        {
            GameObject go = fanLayout.transform.Find("Cand" + i.ToString()).gameObject;
            go.GetComponent<Candidate>().SetCandidateText("");
            go.GetComponent<Candidate>().candidateIndex = i;
            go.GetComponent<Candidate>().candidateHandler = this;
            candidateObjects.Add(go);
        }
        // gun = player.transform.Find("Gun").gameObject;
    }

    void ConfigFanLayout()
    {
        // well let's make it 11 too
        //candidateObjects.Clear();
        fanHorizontalMap = new Dictionary<int, int>();
        fanHorizontalMap.Add(0, 0);
        fanHorizontalMap.Add(1, -2); fanHorizontalMap.Add(2, -1); fanHorizontalMap.Add(3, 1); fanHorizontalMap.Add(4, 2);
        fanHorizontalMap.Add(5, -3); fanHorizontalMap.Add(6, -2); fanHorizontalMap.Add(7, -1); fanHorizontalMap.Add(8, 1); fanHorizontalMap.Add(9, 2); fanHorizontalMap.Add(10, 3);
        // so it is 1 // 4 // 6
        for (int i = 0; i < CandidateCount; i++)
        {
            GameObject go = candidateObjects[i];// Instantiate(CandidatePrefab, transform);
            go.name = "Cand" + i.ToString();
            // which circle
            int circleNumber = (i > 4) ? 2 : ((i == 0) ? 0 : 1);
            int horizontalIndex = fanHorizontalMap[i];
            // times the radius
            float distance = circleNumber * fanRadius;
            go.transform.localPosition = new Vector3( distance * Mathf.Sin(horizontalIndex * fanAngle * Mathf.PI / 180), - distance * verticalScale + Mathf.Abs(horizontalIndex) * textHeight, 0);
            go.GetComponent<Candidate>().SetCandidateText("test");
            go.GetComponent<Candidate>().candidateIndex = i;
            go.GetComponent<Candidate>().candidateHandler = this;
            //candidateObjects.Add(go);
            candidateObjects[i] = go;
        }
    }

    void UpdateFanLayoutCandidate(string[] candidates, int progress)
    {
        // the candidates are placed like a fan
        int candNum = Mathf.Min(candidates.Length, CandidateCount);
        for (int i = 0; i < candNum; i++)
        {
            candidateObjects[i].GetComponent<Candidate>().SetCandidateText(candidates[i], progress);
        }
        for (int i = candNum; i < CandidateCount; i++)
        {
            candidateObjects[i].GetComponent<Candidate>().SetCandidateText("");
        }
    }

    void UpdateRowLayoutCandidate(string[] candidates, int progress)
    {
        // we need to update the position at the same time
        // calculate the word length for both lines, find the longer one
        // or find the longest candidate, use that as the template, and re-calculate the width and place them
        int maxLength = candidates[0].Length;
        for (int i = 1; i < Mathf.Min(11, candidates.Length); i++)
        {
            if (candidates[i].Length > maxLength)
                maxLength = candidates[i].Length;
        }
        CandidateWidth = perWidth * maxLength;
        int candNum = Mathf.Min(candidates.Length, CandidateCount);

        // row layout

        for (int i = 0; i < candNum; i++)
        {
            if (i == 0)
            {
                candidateObjects[0].GetComponent<Candidate>().SetCandidateText(candidates[0], progress);
            }
            else
            {
                candidateObjects[i].GetComponent<Candidate>().SetCandidateText(candidates[i], progress);
                candidateObjects[i].transform.localPosition = new Vector3(-2f * CandidateWidth + ((i - 1) % CandidatePerRow) * CandidateWidth, (i - 1) / CandidatePerRow * CandidateHeight - 1.5f, 0);
            }
        }
        for (int i = candNum; i < CandidateCount; i++)
        {
            candidateObjects[i].GetComponent<Candidate>().SetCandidateText("");
            candidateObjects[i].transform.localPosition = new Vector3(-2f * CandidateWidth + ((i - 1) % CandidatePerRow) * CandidateWidth, (i - 1) / CandidatePerRow * CandidateHeight - 1.5f, 0);
        }
    }

    List<string> sortHelp = new List<string>();
    void UpdateLexicalCandidate(string[] candidates, int progress)
    {
        // we need to update the position at the same time
        // calculate the word length for both lines, find the longer one
        // or find the longest candidate, use that as the template, and re-calculate the width and place them
        int maxLength = candidates[0].Length;
        CandidateCount = 13;
        CandidatePerRow = 4;
        for (int i = 1; i < Mathf.Min(CandidateCount, candidates.Length); i++)
        {
            if (candidates[i].Length > maxLength)
                maxLength = candidates[i].Length;
        }
        CandidateWidth = perWidth * maxLength;
        int candNum = Mathf.Min(candidates.Length, CandidateCount);

        // reorder the candidates except the first one
        sortHelp = new List<string>(candidates);
        List<string> restList = sortHelp.GetRange(1, candNum-1);
        restList.Sort();
        //Array.Copy(restList, 0, candidates, 1, restList.Count);
        for (int i = 0; i < restList.Count; i++)
        {
            candidates[i+1] = restList[i];
        }

        // row layout
        for (int i = 0; i < candNum; i++)
        {
            if (i == 0)
            {
                candidateObjects[0].GetComponent<Candidate>().SetCandidateText(candidates[0], progress);
            }
            else
            {
                candidateObjects[i].GetComponent<Candidate>().SetCandidateText(candidates[i], progress);
                candidateObjects[i].transform.localPosition = new Vector3(-CandidateWidth * (CandidatePerRow - 1) / 2 + ((i - 1) % CandidatePerRow) * CandidateWidth, (i - 1) / CandidatePerRow * CandidateHeight - 1.5f, 0);
            }
        }
        for (int i = candNum; i < CandidateCount; i++)
        {
            candidateObjects[i].GetComponent<Candidate>().SetCandidateText("");
            candidateObjects[i].transform.localPosition = new Vector3(-CandidateWidth * (CandidatePerRow - 1) / 2 + ((i - 1) % CandidatePerRow) * CandidateWidth, (i - 1) / CandidatePerRow * CandidateHeight - 1.5f, 0);
        }
    }

    int candidateNumberPerColumn = 6;
    int[] maxLength;
    
    void UpdateByColumnCandidate(string[] candidates, int progress)
    {
        // we need to update the position at the same time
        // step 1, feed the candidates column by column,
        // step 2, record the maximum length for each column
        int candNum = Mathf.Min(candidates.Length, CandidateCount);
        maxLength = new int[CandidatePerRow];
        int longestCand = 0;
        // reset the candidateColumn
        for (int i = 0; i < CandidatePerRow; i++)
        {
            candidateColumns[i].Clear();
        }

        // by column layout
        for(int i = 0; i < candNum; i++)
        {
            // check the next character for each candidate, and then put it into the correct bucket
            // turn regular char to input string
            // if it is completed candidates, put it in the center lol
            int columnIndex = 4;
            if (candidates[i].Length > progress)
            {
                columnIndex = mapKey2Column[candidates[i][progress]];
                columnIndex = columnIndex > 3 ? columnIndex + 1 : columnIndex;
            }
            
            if (candidateColumns[columnIndex].Count < candidateNumberPerColumn)
            {
                if(candidates[i].Length > 0)
                {
                    candidateColumns[columnIndex].Add(candidates[i]);
                    maxLength[columnIndex] = Mathf.Max(maxLength[columnIndex], candidates[i].Length);
                    longestCand = Mathf.Max(longestCand, candidates[i].Length);
                }                
            }                        
        }

        // calculate the longest word per column and change the position
        CandidateWidth = perWidth * longestCand;
        float hoffset = (CandidatePerRow - 1.0f) / 2f * -CandidateWidth;
        for (int i = 0; i < candidateColumns.Count; i++)
        {
            candidateColumns[i].Sort();
            for (int j = 0; j < candidateColumns[i].Count; j++)
            {
                candidateObjects[i + j * CandidatePerRow].GetComponent<Candidate>().SetCandidateText(candidateColumns[i][j], progress);
//                CandidateWidth = perWidth * longestCand;
                candidateObjects[i + j * CandidatePerRow].transform.localPosition = 
                    new Vector3(-CandidateWidth * (CandidatePerRow - 1) / 2 + i * CandidateWidth, j * CandidateHeight, 0);
            }
            for (int j = candidateColumns[i].Count; j < candidateNumberPerColumn; j++)
            {
                candidateObjects[i + j * CandidatePerRow].transform.localPosition =
                    new Vector3(hoffset + i * CandidateWidth, j * CandidateHeight, 0);
            }
        }
    }

    public void UpdateCandidates(string[] candidates, int progress, string[] completedCand)
    {
        if (candidateLayout == CandLayout.ROW)
            UpdateRowLayoutCandidate(candidates, progress);
        else if(candidateLayout == CandLayout.FAN)
            UpdateFanLayoutCandidate(candidates, progress);
        else if(candidateLayout == CandLayout.LEXIC)
        {
            int totalNumber = 13;
            string[] newCand = ReorgCandidates(candidates, totalNumber, completedCand);
            //for (int i = 0; i < newCand.Length; i++) {
            //    Debug.Log("newCand[" + i + "]:" + newCand[i]);
            //}
            UpdateLexicalCandidate(newCand, progress);
        }else if(candidateLayout == CandLayout.BYCOL)
        {
            UpdateByColumnCandidate(candidates, progress);
        }else if(candidateLayout == CandLayout.WORDCLOUD) {
            int totalNumber = 16;
            string[] newCand = ReorgCandidates(candidates, totalNumber, completedCand, false);
            UpdateWordCloudLayout(newCand, candidates, progress);
        }
    }

    private string[] ReorgCandidates(string[] candidates, int totalNumber, string[] completedCand, bool remainFirst = true)
    {
        // make sure the complete candidates are placed in candidates before totalNumber
        int completeCandNumber = completedCand.Length;
        string[] newCand = new string[totalNumber];
        if (completeCandNumber == 0)
        {
            // no completed candidates then we just return candidates directly            
            if (totalNumber > candidates.Length)
                return candidates;
            Array.Copy(candidates, newCand, totalNumber);
            return newCand;
        }
        // a simple trick: because all the candidates will be sorted via lexcial order later, we just need to put all the completed candidates first, and then the top (n-m) incompleted candidates
        int copyNumber = Mathf.Min(completedCand.Length, totalNumber);
        
        int completeNotFirst = -1;
        if (remainFirst)
        {
            newCand[0] = candidates[0];
            copyNumber = Mathf.Min(completedCand.Length, totalNumber - 1);
            // if candidates[0] is in completedCand, we need to copy the 13th one
            for (int i = 0; i < copyNumber; i++)
            {
                if (completedCand[i] == newCand[0])
                {
                    completeNotFirst = i;
                    break;
                }
            }
        }
        int toCopy = copyNumber;
        if (completeNotFirst != -1)
        {
            Array.Copy(completedCand, 0, newCand, 1, completeNotFirst);
            Debug.LogWarning("completedCand.length:" + completedCand.Length);
            Debug.LogWarning("completeNotFirst:" + completeNotFirst);
            Debug.LogWarning("newCand.length:" + newCand.Length);
            Debug.LogWarning("copyNumber - completeNotFirst:" + (copyNumber - completeNotFirst));
            toCopy = Mathf.Min(copyNumber - completeNotFirst, completedCand.Length - (completeNotFirst + 1));
            //if (completedCand.Length > (completeNotFirst + 1) && newCand.Length > (completeNotFirst + 1)) {
            Array.Copy(completedCand, completeNotFirst + 1, newCand, completeNotFirst + 1, toCopy);
            //foreach (string nc in newCand) {
            //    Debug.Log(nc);
            //}
            //}            
        }
        else
        {
            Array.Copy(completedCand, 0, newCand, remainFirst?1:0, copyNumber);
            toCopy = remainFirst ? copyNumber : (copyNumber - 1);
        }
            
        for (int i = toCopy + 1, j = (remainFirst? 1:0); i < totalNumber; i++)
        {
            while(j < candidates.Length && candidates[j].Length == completedCand[0].Length)
            {
                // completed cand
                ++j;
            }
            // not completed cand
            if(j < candidates.Length)
                newCand[i] = candidates[j++];
            else
            {
                // j reaches the end of candidates, no more could be assigned to newCand
                string[] newCand2 = new string[i];
                Array.Copy(newCand, newCand2, i);
                //foreach (string nc in newCand2) {
                //    Debug.Log(nc);
                //}
                return newCand2;
            }
        }
        return newCand;
    }

    public void ResetCandidates()
    {
        for (int i = 0; i < CandidateCount; i++) {
            candidateObjects[i].GetComponent<Candidate>().SetCandidateText("");
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown("c"))
        //{
            //CreateFanLayout();
        //}        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandidateHandler : MonoBehaviour
{

    public GameObject CandidatePrefab, FanLayoutCandidatePrefab;

    [SerializeField]
    int CandidateCount = 11;
    float CandidateWidth = 4.0f; // roughly it fits for 5-character word
    float CandidateHeight = -1.5f;
    int CandidatePerRow = 5;
    public int GazedCandidate = 0; // index of the candidate being gazed
    float perWidth = 0.57f;
    public float fanRadius = 4f;
    public float verticalScale = 0.5f;
    public float fanAngle = 30f;
    public float textHeight = 0.8f;
    Dictionary<int, int> fanHorizontalMap;
    List<List<string>> candidateColumns;

    public enum CandLayout { ROW, FAN, BYCOL, LEXIC};
    public CandLayout candidateLayout;

    private List<GameObject> candidateObjects;
    public string CurrentGazedText;

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
        candidateObjects = new List<GameObject>();
        if(candidateLayout == CandLayout.ROW)
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
        }
    }

    void CreateRowLayout()
    {
        CandidateCount = 11;
        // the first one is placed in the center
        GameObject go = Instantiate(CandidatePrefab, transform);
        go.name = "Cand0";
        go.GetComponent<Candidate>().SetCandidateText("");
        go.GetComponent<Candidate>().candidateIndex = 0;
        go.GetComponent<Candidate>().candidateHandler = this;
        candidateObjects.Add(go);
        // the rest are placed in two rows
        for (int i = 0; i < CandidateCount-1; i++)
        {
            go = Instantiate(CandidatePrefab, transform);
            go.name = "Cand" + (i + 1).ToString();
            go.transform.localPosition = new Vector3(-8f + (i % CandidatePerRow) * CandidateWidth, i / CandidatePerRow * CandidateHeight - 1.5f, 0);
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
        for (int i = 1; i < Mathf.Min(11, candidates.Length); i++)
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
                candidateObjects[i].transform.localPosition = new Vector3(-2f * CandidateWidth + ((i - 1) % CandidatePerRow) * CandidateWidth, (i - 1) / CandidatePerRow * CandidateHeight - 1.5f, 0);
            }
        }
        for (int i = candNum; i < CandidateCount; i++)
        {
            candidateObjects[i].GetComponent<Candidate>().SetCandidateText("");
            candidateObjects[i].transform.localPosition = new Vector3(-2f * CandidateWidth + ((i - 1) % CandidatePerRow) * CandidateWidth, (i - 1) / CandidatePerRow * CandidateHeight - 1.5f, 0);
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
        for (int i = 0; i < candidateColumns.Count; i++)
        {
            candidateColumns[i].Sort();
            for (int j = 0; j < candidateColumns[i].Count; j++)
            {
                candidateObjects[i + j * candidateNumberPerColumn].GetComponent<Candidate>().SetCandidateText(candidateColumns[i][j], progress);
//                CandidateWidth = perWidth * longestCand;
                candidateObjects[i + j * candidateNumberPerColumn].transform.localPosition = 
                    new Vector3(-CandidateWidth * (CandidatePerRow - 1) / 2 + i * CandidateWidth, j * CandidateHeight, 0);
            }
            for (int j = candidateColumns[i].Count; j < candidateNumberPerColumn; j++)
            {
                candidateObjects[i + j * candidateNumberPerColumn].transform.localPosition =
                    new Vector3(-CandidateWidth * (CandidatePerRow - 1) / 2 + i * CandidateWidth, j * CandidateHeight, 0);
            }
        }
    }

    public void UpdateCandidates(string[] candidates, int progress)
    {
        if (candidateLayout == CandLayout.ROW)
            UpdateRowLayoutCandidate(candidates, progress);
        else if(candidateLayout == CandLayout.FAN)
            UpdateFanLayoutCandidate(candidates, progress);
        else if(candidateLayout == CandLayout.LEXIC)
        {
            UpdateLexicalCandidate(candidates, progress);
        }else if(candidateLayout == CandLayout.BYCOL)
        {
            UpdateByColumnCandidate(candidates, progress);
        }
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

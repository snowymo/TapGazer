using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandidateHandler : MonoBehaviour
{

    public GameObject CandidatePrefab, FanLayoutCandidatePrefab;

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

    public enum CandLayout { ROW, FAN};
    public CandLayout candidateLayout;

    private List<GameObject> candidateObjects;

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
        else
            updateFanLayout();
    }

    void CreateRowLayout()
    {
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

    void updateFanLayout()
    {
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

    public void UpdateCandidates(string[] candidates, int progress)
    {
        if (candidateLayout == CandLayout.ROW)
            UpdateRowLayoutCandidate(candidates, progress);
        else
            UpdateFanLayoutCandidate(candidates, progress);
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

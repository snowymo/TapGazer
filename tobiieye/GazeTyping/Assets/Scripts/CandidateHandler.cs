using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandidateHandler : MonoBehaviour
{

    public GameObject CandidatePrefab;

    int CandidateCount = 11;
    float CandidateWidth = 4.0f; // roughly it fits for 5-character word
    float CandidateHeight = -1.5f;
    int CandidatePerRow = 5;
    public int GazedCandidate = 0; // index of the candidate being gazed
    float perWidth = 0.57f;

    private List<GameObject> candidateObjects;

    // show the prefab at the corresponding place
    // step 1: show the candidates directly, maybe 10 for now,
    // Question: maybe candidates section should not be attached to camera
    // step 2: thinking about the width of the candidates and put it correctly

    // Start is called before the first frame update
    void Start()
    {
        candidateObjects = new List<GameObject>();
        CreateRowLayout();
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

    public void UpdateCandidates(string[] candidates, int progress)
    {
        // we need to update the position at the same time
        // calculate the word length for both lines, find the longer one
        // or find the longest candidate, use that as the template, and re-calculate the width and place them
        int maxLength = candidates[0].Length;
        for(int i = 1; i < Mathf.Min(11,candidates.Length); i++)
        {
            if (candidates[i].Length > maxLength)
                maxLength = candidates[i].Length;
        }
        CandidateWidth = perWidth * maxLength;
        int candNum = Mathf.Min(candidates.Length, CandidateCount);

        // row layout
        
        for (int i = 0; i < candNum; i++) {
            if(i == 0)
            {
                candidateObjects[0].GetComponent<Candidate>().SetCandidateText(candidates[0], progress);
            }
            else
            {
                candidateObjects[i].GetComponent<Candidate>().SetCandidateText(candidates[i], progress);
                candidateObjects[i].transform.localPosition = new Vector3(-2f * CandidateWidth + ((i - 1) % CandidatePerRow) * CandidateWidth, (i - 1) / CandidatePerRow * CandidateHeight - 1.5f, 0);
            }            
        }
        for(int i = candNum; i < CandidateCount; i++) {
            candidateObjects[i].GetComponent<Candidate>().SetCandidateText("");
            candidateObjects[i].transform.localPosition = new Vector3(-2f * CandidateWidth + ((i - 1) % CandidatePerRow) * CandidateWidth, (i - 1) / CandidatePerRow * CandidateHeight - 1.5f, 0);
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

    }
}

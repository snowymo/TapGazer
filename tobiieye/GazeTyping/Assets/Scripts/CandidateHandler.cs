using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandidateHandler : MonoBehaviour
{

    public GameObject CandidatePrefab;

    int CandidateCount = 10;
    float CandidateWidth = 4.0f;
    float CandidateHeight = -1.5f;
    int CandidatePerRow = 5;
    public int GazedCandidate = 0; // index of the candidate being gazed

    private List<GameObject> candidateObjects;

    // show the prefab at the corresponding place
    // step 1: show the candidates directly, maybe 10 for now,
    // Question: maybe candidates section should not be attached to camera
    // step 2: thinking about the width of the candidates and put it correctly

    // Start is called before the first frame update
    void Start()
    {
        candidateObjects = new List<GameObject>();
        for(int i = 0; i < CandidateCount; i++) {
            GameObject go = Instantiate(CandidatePrefab, transform);
            go.name = "Cand" + (i+1).ToString();
            go.transform.localPosition = new Vector3(-8f+(i % CandidatePerRow) * CandidateWidth, i / CandidatePerRow * CandidateHeight-1.5f, 0);
            go.GetComponent<Candidate>().SetCandidateText("");
            go.GetComponent<Candidate>().candidateIndex = i+1;
            go.GetComponent<Candidate>().candidateHandler = this;
            candidateObjects.Add(go);
        }
    }

    public void UpdateCandidates(string[] candidates, int progress)
    {
        int candNum = Mathf.Min(candidates.Length-1, CandidateCount);
        for (int i = 0; i < candNum; i++) {
            candidateObjects[i].GetComponent<Candidate>().SetCandidateText(candidates[i+1], progress);
        }
        for(int i = candNum; i < CandidateCount; i++) {
            candidateObjects[i].GetComponent<Candidate>().SetCandidateText("");
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

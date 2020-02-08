using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Candidate : MonoBehaviour
{
    public TextMesh CandText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCandidateText(string text)
    {
        CandText.text = text;
    }
}

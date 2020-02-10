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

    public void SetCandidateText(string text, int progress = 0)
    {
        // update the color of first #progress# characters to blue and the rest to red
        CandText.text = "<color=blue>" + text.Substring(0, Mathf.Min(text.Length, progress)) + "</color>";
        if(progress < text.Length)
            CandText.text += "<color=red>" + text.Substring(progress) + "</color>";
    }
}

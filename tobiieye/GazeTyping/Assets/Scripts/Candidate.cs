using System.Collections;
using System.Collections.Generic;
using Tobii.XR.Examples;
using UnityEngine;

public class Candidate : MonoBehaviour
{
    public TMPro.TextMeshPro CandText;
    public HandlerFocusAtGaze handlerFocusAtGaze;
    public int candidateIndex = 0;
    public CandidateHandler candidateHandler;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (handlerFocusAtGaze.GetGaze())
        {
            CandText.fontStyle = TMPro.FontStyles.Underline;
            candidateHandler.GazedCandidate = candidateIndex;
        }
        else
        {
            CandText.fontStyle = TMPro.FontStyles.Normal;
            //candidateHandler.GazedCandidate = 0;
        }
    }

    public void SetCandidateText(string text, int progress = 0)
    {
        // update the color of first #progress# characters to blue and the rest to red
        CandText.text = "<color=blue>" + text.Substring(0, Mathf.Min(text.Length, progress)) + "</color>";
        if(progress < text.Length)
            CandText.text += "<color=orange>" + text.Substring(progress) + "</color>";
    }
}

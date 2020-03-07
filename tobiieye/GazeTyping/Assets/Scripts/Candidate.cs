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
    public GameObject planeCollider;
    public string pureText;

    float kWidthScale;

    string removeFormat(string richtext)
    {
        // there should be several '<' and '>' in the string
        while(richtext.IndexOf('<') != -1)
        {
            int start = richtext.IndexOf('<');
            int end = richtext.IndexOf('>');
            richtext = richtext.Remove(start, end - start+1);
        }
        return richtext;
    }

    // Update is called once per frame
    void Update()
    {
        if (handlerFocusAtGaze.GetGaze())
        {
            CandText.fontStyle = TMPro.FontStyles.Underline;
            candidateHandler.GazedCandidate = candidateIndex;
            candidateHandler.CurrentGazedText = removeFormat(CandText.text);
        }
        else
        {
            CandText.fontStyle = TMPro.FontStyles.Normal;
            //candidateHandler.GazedCandidate = 0;
        }
    }

    private const float kOriginalSize = 120f;
    private const float kOriginalZScale = 0.08f;

    public void SetCandidateText(string text, int progress = 0, float fontSize = kOriginalSize)
    {
        // update the color of first #progress# characters to blue and the rest to red
        pureText = text;
        CandText.text = "<color=blue>" + text.Substring(0, Mathf.Min(text.Length, progress)) + "</color>";
        if(progress < text.Length)
            CandText.text += "<color=orange>" + text.Substring(progress) + "</color>";
        kWidthScale = 0.12f;
        CandText.fontSize = fontSize;
        planeCollider.transform.localScale = new Vector3(0.51f * text.Length * kWidthScale * fontSize/ kOriginalSize, planeCollider.transform.localScale.y, fontSize / kOriginalSize * kOriginalZScale);
    }
    
}

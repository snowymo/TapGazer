using System.Collections;
using System.Collections.Generic;
using Tobii.XR.Examples;
using UnityEngine;

public class Candidate : MonoBehaviour {
  public TMPro.TextMeshPro CandText;
  public HandlerFocusAtGaze handlerFocusAtGaze;
  public HandlerScreenGaze handlerScreenGaze;
  public int candidateIndex = 0;
  public CandidateHandler candidateHandler;
  public GameObject planeCollider;
  public string pureText;
  public BoxCollider textCollider;

  float kWidthScale;

  string removeFormat(string richtext) {
    // there should be several '<' and '>' in the string
    while (richtext.IndexOf('<') != -1) {
      int start = richtext.IndexOf('<');
      int end = richtext.IndexOf('>');
      richtext = richtext.Remove(start, end - start + 1);
    }
    return richtext;
  }

  // Update is called once per frame
  void Update() {
    if (ProfileLoader.outputMode == ProfileLoader.OutputMode.Devkit) {
      if (handlerFocusAtGaze.GetGaze()) {
        CandText.fontStyle = TMPro.FontStyles.Underline;
        candidateHandler.GazedCandidate = candidateIndex;
        candidateHandler.CurrentGazedText = pureText;// TODO: TESTING removeFormat(CandText.text);
      } else {
        CandText.fontStyle = TMPro.FontStyles.Normal;
        //candidateHandler.GazedCandidate = 0;
      }
    } else {
      // shoot a ray vertical to the screen
      // turn gaze into correct x,y space
      Vector2 deltaGaze = candidateHandler.ScreenGaze - candidateHandler.ScreenGazeOffset;
      deltaGaze.y = Camera.main.pixelHeight - deltaGaze.y;
      
      handlerScreenGaze.GazeFocusChanged(deltaGaze);
      if (handlerScreenGaze.GetGaze()) {
        CandText.fontStyle = TMPro.FontStyles.Underline;
        candidateHandler.GazedCandidate = candidateIndex;
        candidateHandler.CurrentGazedText = pureText;// TODO: TESTING removeFormat(CandText.text);
      } else {
        CandText.fontStyle = TMPro.FontStyles.Normal;
        //candidateHandler.GazedCandidate = 0;
      }
    }

  }

  public void Start() {
    kOriginalZScale = ProfileLoader.outputMode == ProfileLoader.OutputMode.Devkit ? 0.1f : 0.18f;
  }

  private const float kOriginalSize = 4f;
  private float kOriginalZScale = 0.08f;
  public float firstOverflowCharacterIndex;

  public void SetCandidateText(string text, int progress = 0, int maxLength=0, float fontSize = kOriginalSize, bool isEllipsis = false) {
    // update the color of first #progress# characters to blue and the rest to red
    pureText = text;
    CandText.text = "<color=blue>" + text.Substring(0, Mathf.Min(text.Length, progress)) + "</color>";
    if (progress < text.Length)
      CandText.text += "<color=orange>" + text.Substring(progress) + "</color>";
    kWidthScale = 0.12f;
    CandText.fontSize = fontSize;
    float minLength = Mathf.Max(maxLength, Mathf.Max(5, text.Length));
    planeCollider.transform.localScale = new Vector3(0.51f * minLength * kWidthScale * fontSize / kOriginalSize, planeCollider.transform.localScale.y, fontSize / kOriginalSize * kOriginalZScale);
    firstOverflowCharacterIndex = CandText.characterWidthAdjustment;
    if (isEllipsis) {
      CandText.enableWordWrapping = true;
      StartCoroutine(checkReverseEllipsis(text, progress));
    } else
      CandText.enableWordWrapping = false;

    //textCollider.enabled = false;
    StartCoroutine(renewTextCollider());
  }

  float minColliderWidth = 0.85f, colliderHeight = 0.35f;
  IEnumerator renewTextCollider() {
    yield return new WaitForEndOfFrame();
    //textCollider.enabled = true;
    textCollider.size = new Vector3(Mathf.Max(minColliderWidth, CandText.preferredWidth), colliderHeight);
  }

  IEnumerator checkReverseEllipsis(string text, int progress) {
    yield return new WaitForEndOfFrame();
    if (CandText.textInfo.lineCount > 1) {
      // get the count of how many words for the first line
      int toremove = text.Length - CandText.textInfo.lineInfo[0].characterCount + 2;
      //string newtext = "..." + text.Substring(text.Length - CandText.textInfo.lineInfo[0].characterCount+2, CandText.textInfo.lineInfo[0].characterCount-2);
      // udpate the text
      int remainingTypedWords = Mathf.Max(0, Mathf.Min(text.Length - toremove, progress - toremove));
      //Debug.Log("toremove " + toremove + " remain typed words " + Mathf.Min(text.Length - toremove, progress - toremove));
      CandText.text = "<color=blue>.." + text.Substring(toremove, remainingTypedWords) + "</color>";
      if (progress < text.Length) {
        CandText.text += "<color=orange>" + text.Substring(toremove + remainingTypedWords) + "</color>";
      }
    }
    //Debug.Log(CandText.text + ":" + CandText.textInfo.lineCount + ":" + CandText.textInfo.lineInfo[0].characterCount);
    //if (CandText.textInfo.lineCount > 1) {
    //    Debug.Log(CandText.text + CandText.textInfo.lineInfo[1].firstCharacterIndex);
    //}
  }

  // XXX operating in no overflow (Horizonal|Vertical)
  //private void SetTextWithEllipsis()
  //{
  //    // create generator with value and current Rect
  //    var generator = new TextGenerator();
  //    var rectTransform = CandText.GetComponent<RectTransform>();
  //    var settings = CandText.text.GetGenerationSettings(rectTransform.rect.size);
  //    generator.Populate(value, settings);

  //    // trncate visible value and add ellipsis
  //    var characterCountVisible = generator.characterCountVisible;
  //    var updatedText = value;
  //    if (value.Length > characterCountVisible) {
  //        updatedText = value.Substring(0, characterCountVisible - 1);
  //        updatedText += "…";
  //    }

  //    // update text
  //    textComponent.text = updatedText;
  //}

}

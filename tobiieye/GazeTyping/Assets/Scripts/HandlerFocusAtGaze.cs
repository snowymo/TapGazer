using Tobii.G2OM;
using UnityEngine;

namespace Tobii.XR.Examples
{
  //Monobehaviour which implements the "IGazeFocusable" interface, meaning it will be called on when the object receives focus
  public class HandlerFocusAtGaze : MonoBehaviour, IGazeFocusable
  {
    public Color HighlightColor = Color.red;
    public float AnimationTime = 0.1f;

    private Renderer _renderer;
    private Color _originalColor;
    private Color _targetColor;

    bool isGazed = false;
    public bool GetGaze() {
      return isGazed;
    }

    public void SetGaze(bool hasFocus) {
      //If this object received focus, fade the object's color to highlight color
      if (hasFocus)
      {
//        _targetColor = HighlightColor;
        // underline?
        isGazed = true;
      }
      //If this object lost focus, fade the object's color to it's original color
      else
      {
//        _targetColor = _originalColor;
        isGazed = false;
      }
    }

    //The method of the "IGazeFocusable" interface, which will be called when this object receives or loses focus
    public void GazeFocusChanged(bool hasFocus) {
      SetGaze(hasFocus);
    }

    private void Start() {
      if (ProfileLoader.outputMode == ProfileLoader.OutputMode.Trackerbar)
      {
        // disable it
        transform.gameObject.GetComponent<HandlerFocusAtGaze>().enabled = false;
      }
//      _renderer = GetComponent<Renderer>();
//       _originalColor = _renderer.material.color;
//       _targetColor = _originalColor;
    }

//     private void Update() {
//       //This lerp will fade the color of the object
//       if (_renderer.material.HasProperty(Shader.PropertyToID("_BaseColor"))) // new rendering pipeline (lightweight, hd, universal...)
//       {
//         _renderer.material.SetColor("_BaseColor", Color.Lerp(_renderer.material.GetColor("_BaseColor"), _targetColor, Time.deltaTime * (1 / AnimationTime)));
//       } else // old standard rendering pipline
//       {
//         _renderer.material.color = Color.Lerp(_renderer.material.color, _targetColor, Time.deltaTime * (1 / AnimationTime));
//       }
//     }
  }
}

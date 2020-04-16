using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandlerScreenGaze : MonoBehaviour {
  // retrieve the gaze from tracker bar
  // form a ray shoot perpendicular to the screen
  // change the attribute of the candidate
  // Start is called before the first frame update

  public Color HighlightColor = Color.red;
  public float AnimationTime = 0.1f;

  private Renderer _renderer;
  private Color _originalColor;
  private Color _targetColor;

  bool isGazed = false;
  public bool GetGaze() {
    return isGazed;
  }

  void Start() {
    if (ProfileLoader.outputMode == ProfileLoader.OutputMode.Devkit) {
      // disable it
      transform.gameObject.GetComponent<HandlerScreenGaze>().enabled = false;
    }
    _renderer = GetComponent<Renderer>();
    _originalColor = _renderer.material.color;
    _targetColor = _originalColor;
  }

  // Update is called once per frame
  void Update() {
    //This lerp will fade the color of the object
    if (_renderer.material.HasProperty(Shader.PropertyToID("_BaseColor"))) // new rendering pipeline (lightweight, hd, universal...)
    {
      _renderer.material.SetColor("_BaseColor", Color.Lerp(_renderer.material.GetColor("_BaseColor"), _targetColor, Time.deltaTime * (1 / AnimationTime)));
    } else // old standard rendering pipline
      {
      _renderer.material.color = Color.Lerp(_renderer.material.color, _targetColor, Time.deltaTime * (1 / AnimationTime));
    }

  }

  Vector3 gazeWorldCoord;
  Vector2 curgazeScreenCoord;
  public void GazeFocusChanged(Vector2 gazeScreenCoord) {
    curgazeScreenCoord = gazeScreenCoord;
    //If this object received focus, fade the object's color to highlight color
    RaycastHit hit;
    // Bit shift the index of the layer (8) to get a bit mask
    int layerMask = 1 << 10;
    // TODO: Does the ray intersect any objects excluding the player layer
    //Debug.Log("screen point:" + gazeScreenCoord.ToString("F3"));
    gazeWorldCoord = Camera.main.ScreenToWorldPoint(new Vector3(gazeScreenCoord.x, gazeScreenCoord.y, Camera.main.nearClipPlane));
    //Debug.Log("world coord:" + gazeWorldCoord.ToString("F3"));
    
    //gazeWorldCoord.z = Camera.main.nearClipPlane;
    if(Physics.Raycast(Camera.main.ScreenPointToRay(gazeScreenCoord), out hit, layerMask)) {
      //Debug.DrawRay(gazeWorldCoord, (Vector3.forward) * hit.distance, Color.yellow);
      Debug.Log("Did Hit");
      //hit.collider.gameObject.GetComponent<Tobii.XR.Examples.HandlerFocusAtGaze>().SetGaze(true);
      if (hit.collider.transform.parent.name.Equals(transform.parent.name)) {
        _targetColor = HighlightColor;
        // underline?
        isGazed = true;
      } else {
        _targetColor = _originalColor;
        isGazed = false;
      }
    } else {
      //Debug.DrawRay(gazeWorldCoord, Vector3.forward * 1000, Color.white);
      Debug.Log("Did not Hit");
      _targetColor = _originalColor;
      isGazed = false;
    }
  }

  // test with mouse
  void OnGUI() {
    Vector3 point = new Vector3();
    Event currentEvent = Event.current;
    Vector2 mousePos = new Vector2();

    // Get the mouse position from Event.
    // Note that the y position from Event is inverted.
    mousePos.x = currentEvent.mousePosition.x;
    mousePos.y = Camera.main.pixelHeight - currentEvent.mousePosition.y;

    point = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));

    GUILayout.BeginArea(new Rect(20, 20, 250, 75));
    GUILayout.Label("Screen pixels: " + Camera.main.pixelWidth + ":" + Camera.main.pixelHeight);
    GUILayout.Label("Mouse position: " + mousePos);
    GUILayout.Label("World position: " + point.ToString("F3"));
    GUILayout.EndArea();

    GUILayout.BeginArea(new Rect(20, 100, 250, 75));
    GUILayout.Label("gaze position: " + curgazeScreenCoord);
    GUILayout.Label("gaze3D position: " + gazeWorldCoord.ToString("F3"));
    GUILayout.EndArea();
  }
}

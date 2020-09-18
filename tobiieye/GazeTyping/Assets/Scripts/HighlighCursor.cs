using System.Collections;
using System.Collections.Generic;
using Tobii.G2OM;
using UnityEngine;

public class HighlighCursor : MonoBehaviour, IGazeFocusable
{
    public Color HighlightColor = Color.red;
    public float AnimationTime = 0.1f;

    private Renderer _renderer;
    private Color _originalColor;
    private Color _targetColor;

    public GameObject cursorButton;

    //The method of the "IGazeFocusable" interface, which will be called when this object receives or loses focus
    public void GazeFocusChanged(bool hasFocus)
    {
        //If this object received focus, fade the object's color to highlight color
        if (hasFocus)
        {
            _targetColor = HighlightColor;
            cursorButton.GetComponent<MeshRenderer>().enabled = true;
            print("enabling renderer");
        }
        //If this object lost focus, fade the object's color to it's original color
        else
        {
            _targetColor = _originalColor;
            cursorButton.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _originalColor = _renderer.material.color;
        _targetColor = _originalColor;
    }

    private void Update()
    {
        //This lerp will fade the color of the object
        if (_renderer.material.HasProperty(Shader.PropertyToID("_BaseColor"))) // new rendering pipeline (lightweight, hd, universal...)
        {
            _renderer.material.SetColor("_BaseColor", Color.Lerp(_renderer.material.GetColor("_BaseColor"), _targetColor, Time.deltaTime * (1 / AnimationTime)));
        }
        else // old standard rendering pipline
        {
            _renderer.material.color = Color.Lerp(_renderer.material.color, _targetColor, Time.deltaTime * (1 / AnimationTime));
        }
    }
}

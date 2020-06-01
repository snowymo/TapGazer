using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Tobii.G2OM;

public class VSCandHandler : MonoBehaviour, IGazeFocusable {

  [SerializeField] Vector3 boundSize, textBoundSize;
  [SerializeField] float fw, fh, pw, ph, rw, rh;
  public VSGenerator vsGenerator;
  BoxCollider bc;
  int hasBox;
  private string pureText;
  private MeshRenderer mr;

  //
  float minColliderWidth = 0.5f;
  public float colliderHeight = 0.25f;

  TextMeshPro textComp;
  // Start is called before the first frame update
  void Awake() {
    textComp = GetComponent<TextMeshPro>();
    
    boundSize = textComp.bounds.size;
    fw = textComp.flexibleWidth;
    fh = textComp.flexibleHeight;
    pw = textComp.preferredWidth;
    ph = textComp.preferredHeight;
    rh = textComp.renderedHeight;
    rw = textComp.renderedWidth;
    textBoundSize = textComp.textBounds.size;
    //gameObject.AddComponent<Tobii.XR.Examples.UIGazeCollider>();
    hasBox = -1;

    mr = GetComponent<MeshRenderer>();
    bc = GetComponent<BoxCollider>();
  }

  public void SetText(string candText, string colorName = "white") {
    pureText = candText;
    textComp.text = pureText;// "<color=" + colorName + ">" + pureText + "</color>";
    pw = textComp.preferredWidth;
    ph = textComp.preferredHeight;
    hasBox = 0;
    mr.enabled = false;
    bc.enabled = false;
    //if (bc != null) {
    //  Destroy(bc);
    //}
  }

  public void SetColor(Color color) {
    textComp.color = color;
  }

  public void SetColor(string colorName) {
    mr.enabled = true;
    bc.enabled = true;
    //textComp.text = "<color=" + colorName + ">" + pureText + "</color>";
    //textComp.fontStyle = FontStyles.Normal;
  }

  void adjustColliderHeight() {
    if (Input.GetKeyDown(KeyCode.E)) {
      colliderHeight -= 0.01f;
    }else if (Input.GetKeyDown(KeyCode.D)) {
      colliderHeight += 0.01f;
    }
    bc.size = new Vector3(Mathf.Max(minColliderWidth, pw), colliderHeight/*(ph - colliderHeight)*/);
  }

  // Update is called once per frame
  void Update() {
    if (hasBox == 10) {      
      hasBox += 1;
      if (bc.transform.parent.name.Contains("penta"))
      {
        //circle
        if(bc.transform.name.Contains("1") || bc.transform.name.Contains("3") || bc.transform.name.Contains("5"))
        {
          bc.center = new Vector3(pw/2, 0, bc.center.z);
        } else
        {
          bc.center = new Vector3(-pw/2, 0, bc.center.z);
        }
      } else
      {
        bc.center = new Vector3(bc.center.x, 0, bc.center.z);
      }
      
      bc.size = new Vector3(Mathf.Max(minColliderWidth, pw), colliderHeight/*(ph- colliderHeight)*/);
    } else if (hasBox >=0 && hasBox < 10) {
      hasBox += 1;
    }
    adjustColliderHeight();
  }

  public void GazeFocusChanged(bool hasFocus) {
    //If this object received focus, fade the object's color to highlight color
    if (hasFocus) {
      print("glance at " + pureText + " at " + Time.frameCount);
      //textComp.fontStyle = TMPro.FontStyles.Underline;
      if (pureText == "")
        return;
      vsGenerator.AddHistory(pureText, gameObject.name);
    }
    //If this object lost focus, fade the object's color to it's original color
    else {
      //print("lost glance at " + pureText + " at " + Time.frameCount);
      textComp.fontStyle = TMPro.FontStyles.Normal;
      if (pureText == "")
        return;
      vsGenerator.finalizeHistory(pureText, gameObject.name);
    }
  }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandAnimationCtrl : MonoBehaviour
{

  public Transform[] leftFingers, rightFingers;
  public Material regularFinger, selectedFinger;
  Dictionary<int, int> mapSkinned2Index, mapIndex2Skinned; // first int is the index for skinned material, the second is the index for fingers
  public SkinnedMeshRenderer handMeshRenderer;
  [SerializeField]
  Material[] fingerMaterials;

  [SerializeField]
  Quaternion[] leftFingerAngles, rightFingerAngles;

  // Start is called before the first frame update
  void Start() {
    leftFingerAngles = new Quaternion[5];
    for (int i = 0; i < leftFingerAngles.Length; i++)
    {
      leftFingerAngles[i] = leftFingers[i].localRotation;
    }
    rightFingerAngles = new Quaternion[5];
    for (int i = 0; i < rightFingerAngles.Length; i++)
    {
      rightFingerAngles[i] = rightFingers[i].localRotation;
    }
    mapSkinned2Index = new Dictionary<int, int>
        {
            {0,7 },{2,6},{3,8},{4,9},{5,5},{6,2},{7,3},{8,1},{9,0},{10,4}
        };
    mapIndex2Skinned = new Dictionary<int, int>
        {
            {7,0 },{6,2},{8,3},{9,4},{5,5},{2,6},{3,7},{1,8},{0,9},{4,10}
        };
    fingerMaterials = handMeshRenderer.materials;

    ResetAllFingers();
  }

  // Update is called once per frame
  void Update() {
    if (Input.GetKeyDown("a"))
    {
      // simulate left pinky
      PressLeftFingers(0);
    } else if (Input.GetKeyDown("s"))
    {
      PressLeftFingers(1);
    } else if (Input.GetKeyDown("d"))
    {
      PressLeftFingers(2);
    } else if (Input.GetKeyDown("f"))
    {
      PressLeftFingers(3);
    } else if (Input.GetKeyDown("g"))
    {
      PressLeftFingers(4);
    } else if (Input.GetKeyDown("h"))
    {
      // simulate left pinky
      PressRightFingers(0);
    } else if (Input.GetKeyDown("j"))
    {
      PressRightFingers(1);
    } else if (Input.GetKeyDown("k"))
    {
      PressRightFingers(2);
    } else if (Input.GetKeyDown("l"))
    {
      PressRightFingers(3);
    } else if (Input.GetKeyDown(";"))
    {
      PressRightFingers(4);
    } else if (Input.GetKeyDown("z"))
    {
      ResetAllFingers();
    }
  }

  public void PressLeftFingers(int fingerIndex) {
    if (mapIndex2Skinned == null)
      return;
    ResetAllFingers();
    fingerMaterials[mapIndex2Skinned[fingerIndex]] = selectedFinger;
    handMeshRenderer.materials = fingerMaterials;
    if (fingerIndex == 4)
      leftFingers[fingerIndex].Rotate(new Vector3(0, -30, 0));
    else
      leftFingers[fingerIndex].Rotate(new Vector3(0, 0, 65));

  }

  public void ReleaseLeftFingers(int fingerIndex) {
    if (mapIndex2Skinned == null)
      return;
    leftFingers[fingerIndex].localRotation = leftFingerAngles[fingerIndex];
  }
  public void PressRightFingers(int fingerIndex) {
    if (mapIndex2Skinned == null)
      return;
    ResetAllFingers();
    fingerMaterials[mapIndex2Skinned[fingerIndex + 5]] = selectedFinger;
    handMeshRenderer.materials = fingerMaterials;
    if (fingerIndex == 0)
      rightFingers[fingerIndex].Rotate(new Vector3(0, 30, 0));
    else
      rightFingers[fingerIndex].Rotate(new Vector3(0, 0, 65));
  }

  public void ReleaseRightFingers(int fingerIndex) {
    if (mapIndex2Skinned == null)
      return;
    rightFingers[fingerIndex].localRotation = rightFingerAngles[fingerIndex];
  }

  public void ResetAllFingers() {
    if (mapIndex2Skinned == null)
      return;
    for (int i = 0; i < 5; i++)
    {
      fingerMaterials[mapIndex2Skinned[i]] = regularFinger;
      fingerMaterials[mapIndex2Skinned[i + 5]] = regularFinger;
      ReleaseLeftFingers(i);
      ReleaseRightFingers(i);
    }
    handMeshRenderer.materials = fingerMaterials;
  }
}

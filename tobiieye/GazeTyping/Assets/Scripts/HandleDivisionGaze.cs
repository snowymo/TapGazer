using System.Collections;
using System.Collections.Generic;
using Tobii.G2OM;
using UnityEngine;

public class HandleDivisionGaze : MonoBehaviour, IGazeFocusable
{
  public int DivisionNumber;
  public CandidateHandler candHandler;
  private string[] divisionName = { "left", "middle", "right" };

  public void GazeFocusChanged(bool hasFocus) {
    if (hasFocus)
    {
      candHandler.UpdateDivisionGaze(DivisionNumber);
      //print("gaze at " + divisionName[DivisionNumber] + " division");
    }
  }
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MTrepeatCollection : MonoBehaviour
{

  [SerializeField] int[] times;
  [SerializeField] float[] results, alterResults;
  [SerializeField] string[] fingerKeys;
  [SerializeField] int currentProgress;
  DateTime startTime;
  float[] alterDuration;
  DateTime[] alterStart;
  [SerializeField] int[] alterTimes;
  public TextMeshPro textMesh;
    // Start is called before the first frame update
    void Start()
    {
      times = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    results = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    alterDuration = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    alterStart = new DateTime[10];
    alterTimes = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    alterResults = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    fingerKeys = new string[] { "A", "S", "D", "F", "Space", "K", "J", "L", "Semicolon", "Space" };
    currentProgress = 0;
    }

  void collectAlternativeTap() {
    int alterProgress = currentProgress % 10;
    string curKey = fingerKeys[alterProgress];
    if (alterProgress < 5) {
      textMesh.text = "please press with RH index finger then press " + fingerKeys[alterProgress];
    } else {
      textMesh.text = "please press with LH index finger then press " + fingerKeys[alterProgress];
    }
    
    
    if (Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), curKey))) {  
        TimeSpan deltaTime = DateTime.Now - alterStart[alterProgress];
      alterDuration[alterProgress] += (float)deltaTime.TotalMilliseconds;
        if (alterDuration[alterProgress] > 2500) {
        alterResults[alterProgress] = alterDuration[alterProgress] / 1000.0f / (float)alterTimes[alterProgress];
          ++currentProgress;
          return;
        }
       // print("now " + DateTime.Now.ToString());
      
      ++alterTimes[alterProgress];
    } else {
      if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.F)) {
          alterStart[alterProgress] = DateTime.Now;
      }
    }
  }

  void collectMTrepeat() {
    textMesh.text = "please press " + fingerKeys[currentProgress];
    if (Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), fingerKeys[currentProgress]))) {
      if (times[currentProgress] == 0) {
        startTime = DateTime.Now;
        print("startTime " + startTime.ToString());
      } else {
        TimeSpan deltaTime = DateTime.Now - startTime;
        if ((deltaTime).TotalMilliseconds > 2500) {
          results[currentProgress] = (float)(deltaTime.TotalMilliseconds) / 1000.0f / (float)times[currentProgress];
          ++currentProgress;
          return;
        }
        print("now " + DateTime.Now.ToString());
      }
      ++times[currentProgress];
    }
  }

    // Update is called once per frame
    void Update()
    {
//     if(currentProgress < 10) {
//       collectMTrepeat();
//     }
    collectAlternativeTap();
  }
}

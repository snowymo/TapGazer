using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class MTrepeatCollection : MonoBehaviour
{

  [SerializeField] int[] times, sameTimes;
  [SerializeField] double[] results, alterResults, samesideResults;
  [SerializeField] string[] fingerKeys;
  [SerializeField] int currentProgress;
  DateTime startTime,sameStart,sameEnd;
  float[] alterDuration;
  DateTime[] alterStart;
  [SerializeField] int[] alterTimes;
  string[] sameSequence;
  int[] sameKeys;
  public TextMeshPro textMesh;
    // Start is called before the first frame update
    void init() {
    times = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    results = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    alterDuration = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    alterStart = new DateTime[10];
    alterTimes = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    alterResults = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    samesideResults = new double[24];
    sameTimes = new int[24];
    for (int i = 0; i < samesideResults.Length; i++) {
      sameTimes[i] = 0;
      samesideResults[i] = 0;
    }
      
    sameStart = DateTime.MaxValue;
    sameEnd = DateTime.MaxValue;
    currentProgress = 0;
  }
    void Start()
    {
    init();
    fingerKeys = new string[] { "A", "S", "D", "F", "Space", "K", "J", "L", "Semicolon", "Space" };
    sameSequence = new string[] {  "as", ";l", "af", ";j", "sd", "lk", "ad",  ";k", "sf", "lj", "df", "kj" };
    sameKeys = new int[] {  0, 1, 8, 7, 0, 3, 8, 6, 1, 2, 7, 5,
                            0, 2, 8, 5, 1, 3, 7, 6, 2, 3, 5, 6 };
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
        //print("startTime " + startTime.ToString());
      } else {
        TimeSpan deltaTime = DateTime.Now - startTime;
        if ((deltaTime).TotalMilliseconds > 2500) {
          results[currentProgress] = (float)(deltaTime.TotalMilliseconds) / 1000.0f / (float)times[currentProgress];
          ++currentProgress;
          return;
        }
        //print("now " + DateTime.Now.ToString());
      }
      ++times[currentProgress];
    }
  }

  void collectSameSide() {
    // six for each side, not a lot actually
    // let's use space to separate each pair
    int sameProgress = currentProgress % 20;
    if (sameProgress >= 12)
      return;
    textMesh.text = "press pair " + sameSequence[sameProgress] + " in order";
    if(Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), fingerKeys[sameKeys[sameProgress * 2]]))){
      sameStart = DateTime.Now;
      if ((sameStart - sameEnd).TotalMilliseconds > 0) {
        samesideResults[sameProgress * 2 + 1] += (sameStart - sameEnd).TotalMilliseconds;
        ++sameTimes[sameProgress * 2 + 1];
      }        
    } else if(Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), fingerKeys[sameKeys[sameProgress * 2 + 1]]))) {
      sameEnd = DateTime.Now;
      if ((sameEnd- sameStart).TotalMilliseconds > 0) {
        ++sameTimes[sameProgress * 2];
        samesideResults[sameProgress * 2] += (sameEnd- sameStart).TotalMilliseconds;
      }        
    }
    if (samesideResults[sameProgress * 2] + samesideResults[sameProgress * 2+1] > 2500) {
      samesideResults[sameProgress * 2] /= sameTimes[sameProgress * 2];
      samesideResults[sameProgress * 2+1] /= sameTimes[sameProgress * 2+1];
      sameStart = DateTime.MaxValue;
      sameEnd = DateTime.MaxValue;
      ++currentProgress;
    }
  }

  private void saveData() {
    // save to file
    string destination = Application.dataPath + "/Resources/MTcollection.csv";
    if (!File.Exists(destination)) {
      File.WriteAllText(destination, ",MTrepeat,MTalter\n");
    }

    //Write some text to the file
    // name should include profile (aka user name), mode (regular, or test), layout and session
    for(int i = 0; i < fingerKeys.Length && results[0]!=0; i++) {
      File.AppendAllText(destination, fingerKeys[i] + "," + results[i] + "," + alterResults[i] + "\n");
    }

    File.AppendAllText(destination, "pair, MTsameside\n");
    for (int i = 0; i < sameSequence.Length && sameTimes[0] != 0; i++) {
      File.AppendAllText(destination, sameSequence[i] + "," + samesideResults[i*2]+ ",\nrev-" + sameSequence[i] + "," + samesideResults[i * 2+1] + "\n");
    }
    
  }

  // Update is called once per frame
  void Update()
    {
    if (currentProgress < 10) {
      collectMTrepeat();
    } else if (currentProgress < 20)
      collectAlternativeTap();
    else if (currentProgress < 32) {
      collectSameSide();
    } else {
      // write down the answer
      saveData();
      init();
    }
    //     collectSameSide();
    //     if(currentProgress >= 12) {
    //       saveData();
    //       init();
    //     }

  }
}

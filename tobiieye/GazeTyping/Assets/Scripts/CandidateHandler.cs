using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CandidateHandler : MonoBehaviour
{

  public GameObject CandidatePrefab, FanLayoutCandidatePrefab;

  WordlistLoader wordListLoader;
  InputHandler inputHandler;

  private Transform pentagonArea;

  [SerializeField]
  int CandidateCount = 11;
  float CandidateWidth; // roughly it fits for 5-character word
  float CandidateStartHeight = -2.1f;
  float CandidateHeight = -0.5f;
  int CandidatePerRow;
  public int GazedCandidate = 0; // index of the candidate being gazed
  float perWidth;
  public float fanRadius = 4f;
  public float verticalScale = 0.5f;
  public float fanAngle = 30f;
  public float textHeight = 0.8f;
  Dictionary<int, int> fanHorizontalMap;
  List<List<string>> candidateColumns;

  [SerializeField] int pageIndex;
  [SerializeField] int pageTotal;
  public TMPro.TextMeshPro pageTextMesh;
  public int GetPageTotal() { return pageTotal; }

  //public enum CandLayout { ROW, FAN, BYCOL, LEXIC, WORDCLOUD, DIVISION, DIVISION_END, ONE };
  private ProfileLoader.CandLayout candidateLayout;

  [SerializeField] private bool enableWordCompletion;
  public bool enableDeleteEntire;
  public bool enableChordSelection;// if enable key selection, (which is the other way of gaze selection), user press 'b' to display the number for candidates, then press with u90[ for candidate 1,2,3,4

  public List<GameObject> candidateObjects;
  public string defaultWord;
  public string CurrentGazedText;
  [SerializeField]
  private float kSizeScale;

  public TMPro.TextMeshPro[] secondFingerAss;

  public bool isEllipsis;
  public GameObject screenGazeIndicator;

  private Vector2 screenGaze;
  public Vector2 ScreenGaze
  {
    get { return screenGaze; }
    set { screenGaze = value; }
  }

  private Vector2 screenGazeOffset;
  public Vector2 ScreenGazeOffset
  {
    get { return screenGazeOffset; }
    set { screenGazeOffset = value; }
  }

  public int curGazedDivision;

  // map btw regular keys and input string
  Dictionary<char, int> mapKey2Column = new Dictionary<char, int>()
    {
        {'q',0}, { 'a',0},{'z',0}, {'w',1}, {'s',1}, {'x',1}, {'e',2}, {'d', 2}, {'c', 2}, {'r', 3}, {'f', 3}, {'v', 3}, {'t',3}, {'g', 3}, {'b',3},
        {'y',4}, { 'h',4},{'n',4}, {'u',4}, {'j',4}, {'i',5}, {'k',5}, {'m', 5}, {'o', 6}, {'l', 6}, {'p', 7}
    };

  // show the prefab at the corresponding place
  // step 1: show the candidates directly, maybe 10 for now,
  // Question: maybe candidates section should not be attached to camera
  // step 2: thinking about the width of the candidates and put it correctly

  // Start is called before the first frame update
  void Start()
  {
    //
    secondFingerAss[1].text = ProfileLoader.mapInputString2Letter["d"][0] + "-LMid";
    secondFingerAss[2].text = ProfileLoader.mapInputString2Letter["k"][0] + "-RMid";
    secondFingerAss[3].text = ProfileLoader.mapInputString2Letter["f"][0] + "-LInd";
    secondFingerAss[4].text = ProfileLoader.mapInputString2Letter["j"][0] + "-RInd";

    pageTextMesh.enabled = ProfileLoader.typingMode == ProfileLoader.TypingMode.TEST && ProfileLoader.selectionMode == ProfileLoader.SelectionMode.MS;

    wordListLoader = GetComponent<WordlistLoader>();
    inputHandler = GameObject.Find("InputSection").GetComponent<InputHandler>();
    enableWordCompletion = ProfileLoader.wcMode == ProfileLoader.WordCompletionMode.WC;
    // only support non VR for now to save gaze efforts. Support VR later
    if (enableChordSelection)
      UnityEngine.XR.XRSettings.enabled = false;

    isEllipsis = false;
    candidateObjects = new List<GameObject>();
    perWidth = 0.25f;// 0.73f;
    kSizeScale = 1f;
    CandidateHeight = ProfileLoader.outputMode == ProfileLoader.OutputMode.Trackerbar ? -2.5f : -0.36f;
    CandidateStartHeight = ProfileLoader.outputMode == ProfileLoader.OutputMode.Trackerbar ? -2.5f : -2.1f;
    if (screenGazeIndicator != null)
      screenGazeIndicator.SetActive(ProfileLoader.outputMode == ProfileLoader.OutputMode.Trackerbar);
    curGazedDivision = 1; // gaze at the middle division by default

    pentagonArea = transform.Find("pentagonArea");

    pageIndex = 1;
    pageTotal = 1;

    candidateLayout = ProfileLoader.candidateLayout;
    if (candidateLayout == ProfileLoader.CandLayout.ROW)
      CreateRowLayout();
    else if (candidateLayout == ProfileLoader.CandLayout.FAN)
      updateFanLayout();
    else if (candidateLayout == ProfileLoader.CandLayout.LEXIC)
    {
      CandidateCount = 13;
      CandidatePerRow = 4;

      //CandidateCount = 7;
      //CandidatePerRow = 3;
      CreateRowLayout();
      // order the candidates by lexical order
    }
    else if (candidateLayout == ProfileLoader.CandLayout.BYCOL)
    {
      CreateColumnLayout();
    }
    else if (candidateLayout == ProfileLoader.CandLayout.WORDCLOUD)
    {
      CreateWordCloudLayout();
    }
    else if (candidateLayout == ProfileLoader.CandLayout.DIVISION || candidateLayout == ProfileLoader.CandLayout.DIVISION_END)
    {
      CreateDivisionLayout();
    }
    else if (candidateLayout == ProfileLoader.CandLayout.ONE)
    {
      // for now, and circle later
      CandidateCount = 5;
      CandidatePerRow = 2;
      //CreateRowLayout();
      CreateCircleLayout();
    }
  }

  private void CreateDivisionLayout()
  {
    CandidateCount = 15;
    CandidatePerRow = 3;
    CandidateWidth = perWidth * 8;

    for (int i = 0; i < CandidateCount; i++)
    {
      GameObject go = Instantiate(CandidatePrefab, transform);
      go.name = "Cand" + (i).ToString();
      go.transform.localPosition = new Vector3(
        -CandidateWidth * (CandidatePerRow - 1) / 2 + (i % CandidatePerRow) * CandidateWidth,
        i / CandidatePerRow * CandidateHeight + CandidateStartHeight, 0);
      go.GetComponent<Candidate>().SetCandidateText("");
      go.GetComponent<Candidate>().candidateIndex = i;
      go.GetComponent<Candidate>().candidateHandler = this;
      candidateObjects.Add(go);
    }
  }

  public void UpdateDivisionGaze(int divIndex)
  {
    if (candidateLayout == ProfileLoader.CandLayout.DIVISION || candidateLayout == ProfileLoader.CandLayout.DIVISION_END)
    {
      curGazedDivision = divIndex;
      if (cachedCandidates != null)
        UpdateDivisionLayout(cachedCandidates, cachedProgress, candidateLayout == ProfileLoader.CandLayout.DIVISION ? 0 : 1);
    }
  }

  private string[] leftDivision = new string[5], middleDivision = new string[5], rightDivision = new string[5];
  private char[] firstLetterDivSep1 = { 'h', 'e' }, firstLetterDivSep2 = { 'q', 'r' };
  private void UpdateDivisionLayout(string[] candidates, int progress, int divideLatter)
  {
    int maxLength = 8;// Mathf.Max(4, candidates[0].Length);
    CandidateWidth = perWidth * maxLength;

    // retrieve the candidates for each division, at most 5
    leftDivision = new string[5] { "", "", "", "", "" };
    middleDivision = new string[5] { "", "", "", "", "" };
    rightDivision = new string[5] { "", "", "", "", "" };
    int leftDivIndex = 0, midDivIndex = 0, rightDivIndex = 0;
    bool leftDone = false, midDone = false, rightDone = false;


    for (int i = 0; i < candidates.Length; i++)
    {
      if (leftDone && rightDone && midDone)
        break;

      string curWord = candidates[i].ToLower();
      char letter = divideLatter == 0 ? curWord[0] : curWord[curWord.Length - 1];

      if (!leftDone && letter < firstLetterDivSep1[divideLatter])
      {
        if ((i < 5) || (curGazedDivision == 0 && leftDivIndex < 5))
        {
          leftDivision[leftDivIndex] = candidates[i];
          candidateObjects[leftDivIndex * 3].GetComponent<Candidate>().SetCandidateText(candidates[i], progress, maxLength - 1);
          candidateObjects[leftDivIndex * 3].transform.localPosition = new Vector3(
            -CandidateWidth, leftDivIndex * CandidateHeight + CandidateStartHeight, 0);
          ++leftDivIndex;
        }
        else
          leftDone = true;
      }
      else if (!midDone && letter < firstLetterDivSep2[divideLatter] && letter >= firstLetterDivSep1[divideLatter])
      {
        if ((i < 5) || ((curGazedDivision == 1) && (midDivIndex < 5)))
        {
          middleDivision[midDivIndex] = candidates[i];
          candidateObjects[midDivIndex * 3 + 1].GetComponent<Candidate>().SetCandidateText(candidates[i], progress, maxLength - 1);
          candidateObjects[midDivIndex * 3 + 1].transform.localPosition = new Vector3(
            0, midDivIndex * CandidateHeight + CandidateStartHeight, 0);
          ++midDivIndex;
        }
        else
          midDone = true;
      }
      else if (!rightDone && letter >= firstLetterDivSep2[divideLatter])
      {
        if ((i < 5) || ((curGazedDivision == 2) && (rightDivIndex < 5)))
        {
          rightDivision[rightDivIndex] = candidates[i];
          candidateObjects[rightDivIndex * 3 + 2].GetComponent<Candidate>().SetCandidateText(candidates[i], progress, maxLength - 1);
          candidateObjects[rightDivIndex * 3 + 2].transform.localPosition = new Vector3(
            CandidateWidth, rightDivIndex * CandidateHeight + CandidateStartHeight, 0);
          ++rightDivIndex;
        }
        else
          rightDone = true;
      }
    }
    // TODO
    defaultWord = candidateObjects[curGazedDivision].GetComponent<Candidate>().pureText;
    // now we can reset the rest
    for (int i = leftDivIndex; i < 5; i++)
    {
      candidateObjects[i * 3].GetComponent<Candidate>().SetCandidateText("");
      candidateObjects[i * 3].transform.localPosition = new Vector3(
            -CandidateWidth, i * CandidateHeight + CandidateStartHeight, 0);
    }
    for (int i = midDivIndex; i < 5; i++)
    {
      candidateObjects[i * 3 + 1].GetComponent<Candidate>().SetCandidateText("");
      candidateObjects[i * 3 + 1].transform.localPosition = new Vector3(
            0, i * CandidateHeight + CandidateStartHeight, 0);
    }
    for (int i = rightDivIndex; i < 5; i++)
    {
      candidateObjects[i * 3 + 2].GetComponent<Candidate>().SetCandidateText("");
      candidateObjects[i * 3 + 2].transform.localPosition = new Vector3(
            CandidateWidth, i * CandidateHeight + CandidateStartHeight, 0);
    }
  }

  private void CreateWordCloudLayout()
  {
        // let's use a 4x4 here
        // first, we can show some candidates with high freq, but let's do it next
        //
        // hack
        //isEllipsis = true;
    CandidateCount = 16;
    int maxLength = 10;
    CandidatePerRow = 4;
        perWidth = 0.75f / 4f;
    CandidateWidth = perWidth * maxLength;
    for (int i = 0; i < CandidateCount; i++)
    {
      GameObject go = Instantiate(CandidatePrefab, transform);
      go.name = "Cand" + i.ToString();
      go.transform.localPosition = new Vector3(-CandidateWidth * (CandidatePerRow - 1) / 2 + (i % CandidatePerRow) * CandidateWidth, i / CandidatePerRow * (CandidateHeight - 0.5f) - 2.5f, 0);
      go.GetComponent<Candidate>().SetCandidateText("");
      go.GetComponent<Candidate>().candidateIndex = i;
      go.GetComponent<Candidate>().candidateHandler = this;
      candidateObjects.Add(go);
    }
  }

  private int[] DoCandidateShowPreviously(string[] candidates, ref List<int> availableIndices)
  {
    int[] result = new int[candidates.Length];
    for (int i = 0; i < candidateObjects.Count; i++)
      availableIndices.Add(i);

    for (int i = 0; i < candidates.Length; i++)
    {
      result[i] = -1;
      for (int j = 0; j < candidateObjects.Count; j++)
      {
        if (candidates[i].Equals(candidateObjects[j].GetComponent<Candidate>().pureText, StringComparison.CurrentCultureIgnoreCase))
        {
          result[i] = j;
          break;
        }
      }
      if (result[i] > -1)
        availableIndices.Remove(result[i]);
    }
    return result;
  }

  float MapIndex2Size(int index)
  {
    // definitely it won't be x -> x
    // let's use 16-x for now?
    index = Mathf.Min(15, index);
    //float answer = (54.0f - index) / 54.0f * 10.0f;
    float answer = (200.0f - index * 8.5f);
        answer = (7 - index * 0.35f);
        return answer;
  }

  int[] priorityIndices = new int[] { 5, 10, 9, 6, 0, 15, 12, 3, 1, 14, 8, 7, 2, 13, 4, 11 };
  private int findAvailableIndex(List<int> availableIndices)
  {

    // let's try put it in the center and then go to the side
    // a manual order for 0-15: 5,10, 9, 6, 0, 15, 12, 3, 1,14,8,7,2,13,4,11
    for (int i = 0; i < priorityIndices.Length; i++)
    {
      if (availableIndices.IndexOf(priorityIndices[i]) > -1)
      {
        return priorityIndices[i];
      }
    }
    return -1;
  }

  private void UpdateWordCloudLayout(string[] candidates, string[] allCandidates, int progress)
  {
    // we need to update the position at the same time
    // instead of calculate=ing the word length for both lines, and finding the longer one, let's predefine a LONG number and apply
    int maxLength = 9;
    CandidateWidth = perWidth * maxLength;
    int candNum = Mathf.Min(candidates.Length, CandidateCount);

    // grid layout
    List<int> availableIndices = new List<int>();
    int[] showPreviously = DoCandidateShowPreviously(candidates, ref availableIndices);
    ResetCandidates();

    for (int i = 0; i < candNum; i++)
    {
      // update the text, check if the candidate shows up last time, we don't apply any change
      // 'candidates' was updated after typing, 'candidateObjects' wasn't
      if (showPreviously[i] > -1)
      {
        // shows before, only apply the new progress
        float candSize = MapIndex2Size(Array.IndexOf(allCandidates, candidates[i])) * kSizeScale;
        candidateObjects[showPreviously[i]].GetComponent<Candidate>().SetCandidateText(candidates[i], progress, maxLength - 1, candSize, isEllipsis);
      }
      else
      {
        // find an available index in candidateObjects, find from availableIndices
        // instead of doing it randomly, we should have a consitent order for the index
        int randIndex = findAvailableIndex(availableIndices);
        availableIndices.Remove(randIndex);
        // the position is decided during creation
        // the size is related to where it is in allCandidates
        float candSize = MapIndex2Size(Array.IndexOf(allCandidates, candidates[i])) * kSizeScale;
        candidateObjects[randIndex].GetComponent<Candidate>().SetCandidateText(candidates[i], progress, maxLength - 1, candSize, isEllipsis);
      }
    }
    defaultWord = candidates[0];
  }

  void CreateRowLayout()
  {
    // the first one is placed in the center
    GameObject go = Instantiate(CandidatePrefab, transform);
    go.name = "Cand0";
    go.transform.localPosition = new Vector3(0, CandidateStartHeight, 0);
    go.GetComponent<Candidate>().SetCandidateText("");
    go.GetComponent<Candidate>().candidateIndex = 0;
    go.GetComponent<Candidate>().candidateHandler = this;
    candidateObjects.Add(go);

    // the rest are placed in two rows
    for (int i = 0; i < CandidateCount - 1; i++)
    {
      go = Instantiate(CandidatePrefab, transform);
      go.name = "Cand" + (i + 1).ToString();
      go.transform.localPosition = new Vector3(
        -CandidateWidth * (CandidatePerRow - 1) / 2 + (i % CandidatePerRow) * CandidateWidth,
        i / CandidatePerRow * CandidateHeight + CandidateStartHeight, 0);
      go.GetComponent<Candidate>().SetCandidateText("");
      go.GetComponent<Candidate>().candidateIndex = i + 1;
      go.GetComponent<Candidate>().candidateHandler = this;
      candidateObjects.Add(go);
    }
  }

  void CreateCircleLayout()
  {
    pentagonArea.gameObject.SetActive(true);
    string prefix = "VSCand";
    for (int i = 1; i <= 5; i++)
    {
      GameObject go = pentagonArea.Find(prefix + i.ToString()).gameObject;
      go.GetComponent<Candidate>().SetCandidateText("");
      go.GetComponent<Candidate>().candidateIndex = i;
      go.GetComponent<Candidate>().candidateHandler = this;
      candidateObjects.Add(go);
    }
    if(ProfileLoader.selectionMode != ProfileLoader.SelectionMode.MS || ProfileLoader.typingMode == ProfileLoader.TypingMode.REGULAR)
    {
      // load 10 candidates
      for (int i = 6; i <= 10; i++)
      {
        GameObject go = pentagonArea.Find(prefix + i.ToString()).gameObject;
        go.SetActive(true);
        go.GetComponent<Candidate>().SetCandidateText("");
        go.GetComponent<Candidate>().candidateIndex = i;
        go.GetComponent<Candidate>().candidateHandler = this;
        candidateObjects.Add(go);
      }
      CandidateCount = 10;
    }
  }

  void CreateColumnLayout()
  {
    // 8 columns + 1
    CandidatePerRow = 9;
    CandidateCount = CandidatePerRow * candidateNumberPerColumn;

    candidateColumns = new List<List<string>>();
    for (int i = 0; i < CandidatePerRow; i++)
    {
      candidateColumns.Add(new List<string>());
    }

    // the rest are placed in two rows
    for (int i = 0; i < CandidateCount; i++)
    {
      GameObject go = Instantiate(CandidatePrefab, transform);
      go.name = "Cand" + i.ToString();
      go.transform.localPosition = new Vector3(
        -CandidateWidth * (CandidatePerRow - 1) / 2 + (i % CandidatePerRow) * CandidateWidth,
        (i / CandidatePerRow + 1) * CandidateHeight + CandidateStartHeight, 0);
      go.GetComponent<Candidate>().SetCandidateText("");
      go.GetComponent<Candidate>().candidateIndex = i;
      go.GetComponent<Candidate>().candidateHandler = this;
      candidateObjects.Add(go);
    }
  }

  void updateFanLayout()
  {
    CandidateCount = 11;
    // the first one is placed in the center
    GameObject fanLayout = Instantiate(FanLayoutCandidatePrefab, transform);
    // fill the list with FanLayoutCandidatePrefab
    for (int i = 0; i < CandidateCount; i++)
    {
      GameObject go = fanLayout.transform.Find("Cand" + i.ToString()).gameObject;
      go.GetComponent<Candidate>().SetCandidateText("");
      go.GetComponent<Candidate>().candidateIndex = i;
      go.GetComponent<Candidate>().candidateHandler = this;
      candidateObjects.Add(go);
    }
    // gun = player.transform.Find("Gun").gameObject;
  }

  void ConfigFanLayout()
  {
    // well let's make it 11 too
    //candidateObjects.Clear();
    fanHorizontalMap = new Dictionary<int, int>();
    fanHorizontalMap.Add(0, 0);
    fanHorizontalMap.Add(1, -2); fanHorizontalMap.Add(2, -1); fanHorizontalMap.Add(3, 1); fanHorizontalMap.Add(4, 2);
    fanHorizontalMap.Add(5, -3); fanHorizontalMap.Add(6, -2); fanHorizontalMap.Add(7, -1); fanHorizontalMap.Add(8, 1); fanHorizontalMap.Add(9, 2); fanHorizontalMap.Add(10, 3);
    // so it is 1 // 4 // 6
    for (int i = 0; i < CandidateCount; i++)
    {
      GameObject go = candidateObjects[i];// Instantiate(CandidatePrefab, transform);
      go.name = "Cand" + i.ToString();
      // which circle
      int circleNumber = (i > 4) ? 2 : ((i == 0) ? 0 : 1);
      int horizontalIndex = fanHorizontalMap[i];
      // times the radius
      float distance = circleNumber * fanRadius;
      go.transform.localPosition = new Vector3(distance * Mathf.Sin(horizontalIndex * fanAngle * Mathf.PI / 180), -distance * verticalScale + Mathf.Abs(horizontalIndex) * textHeight, 0);
      go.GetComponent<Candidate>().SetCandidateText("test");
      go.GetComponent<Candidate>().candidateIndex = i;
      go.GetComponent<Candidate>().candidateHandler = this;
      //candidateObjects.Add(go);
      candidateObjects[i] = go;
    }
  }

  void UpdateFanLayoutCandidate(string[] candidates, int progress)
  {
    // the candidates are placed like a fan
    int candNum = Mathf.Min(candidates.Length, CandidateCount);
    for (int i = 0; i < candNum; i++)
    {
      candidateObjects[i].GetComponent<Candidate>().SetCandidateText(candidates[i], progress);
    }
    for (int i = candNum; i < CandidateCount; i++)
    {
      candidateObjects[i].GetComponent<Candidate>().SetCandidateText("");
    }
  }

    void UpdateRowLayoutCandidate(string[] candidates, int progress)
  {
    // we need to update the position at the same time
    // calculate the word length for both lines, find the longer one
    // or find the longest candidate, use that as the template, and re-calculate the width and place them
    int maxLength = candidates[0].Length;
    for (int i = 1; i < Mathf.Min(11, candidates.Length); i++)
    {
      if (candidates[i].Length > maxLength)
        maxLength = candidates[i].Length;
    }
    CandidateWidth = perWidth * maxLength;
    int candNum = Mathf.Min(candidates.Length, CandidateCount);

    // row layout
    defaultWord = candidates[0];
    for (int i = 0; i < candNum; i++)
    {
      if (i == 0)
      {
        candidateObjects[0].GetComponent<Candidate>().SetCandidateText(candidates[0], progress, maxLength - 1);
      }
      else
      {
        candidateObjects[i].GetComponent<Candidate>().SetCandidateText(candidates[i], progress, maxLength - 1);
        candidateObjects[i].transform.localPosition = new Vector3(-2f * CandidateWidth + ((i - 1) % CandidatePerRow) * CandidateWidth, (i - 1) / CandidatePerRow * CandidateHeight + CandidateStartHeight, 0);
      }
    }
    for (int i = candNum; i < CandidateCount; i++)
    {
      candidateObjects[i].GetComponent<Candidate>().SetCandidateText("");
      candidateObjects[i].transform.localPosition = new Vector3(-2f * CandidateWidth + ((i - 1) % CandidatePerRow) * CandidateWidth, (i - 1) / CandidatePerRow * CandidateHeight + CandidateStartHeight, 0);
    }
  }

  List<string> sortHelp = new List<string>();
  void UpdateLexicalCandidate(string[] candidates, int progress)
  {
    // we need to update the position at the same time
    // calculate the word length for both lines, find the longer one
    // or find the longest candidate, use that as the template, and re-calculate the width and place them

    int maxLength = Mathf.Max(4, candidates.Length > 0 ? candidates[0].Length : 0);
    //         CandidateCount = 13;
    //         CandidatePerRow = 4;
    for (int i = 1; i < Mathf.Min(CandidateCount, candidates.Length); i++)
    {
      if (candidates[i].Length > maxLength)
        maxLength = candidates[i].Length;
    }
    CandidateWidth = perWidth * maxLength;
    int candNum = Mathf.Min(candidates.Length, CandidateCount);

    // reorder the candidates except the first one
    sortHelp = new List<string>(candidates);
    List<string> restList = sortHelp.GetRange(1, candNum - 1);
    restList.Sort();
    //Array.Copy(restList, 0, candidates, 1, restList.Count);
    for (int i = 0; i < restList.Count; i++)
    {
      candidates[i + 1] = restList[i];
    }

    // row layout
    for (int i = 0; i < candNum; i++)
    {
      if (i == 0)
      {
        candidateObjects[0].GetComponent<Candidate>().SetCandidateText(candidates[0], progress, maxLength - 1);
      }
      else
      {
        candidateObjects[i].GetComponent<Candidate>().SetCandidateText(candidates[i], progress, maxLength - 1);
        candidateObjects[i].transform.localPosition = new Vector3(
          -CandidateWidth * (CandidatePerRow - 1) / 2 + ((i - 1) % CandidatePerRow) * CandidateWidth,
          ((i - 1) / CandidatePerRow + 1) * CandidateHeight + CandidateStartHeight, 0);//(i / CandidatePerRow + 1)
      }
    }
    for (int i = candNum; i < CandidateCount; i++)
    {
      candidateObjects[i].GetComponent<Candidate>().SetCandidateText("");
      candidateObjects[i].transform.localPosition = new Vector3(
        -CandidateWidth * (CandidatePerRow - 1) / 2 + ((i - 1) % CandidatePerRow) * CandidateWidth,
        ((i - 1) / CandidatePerRow + 1) * CandidateHeight + CandidateStartHeight, 0);
    }
    defaultWord = candidates[0];
  }

  int candidateNumberPerColumn = 6;
  int[] maxLength;

  void UpdateByColumnCandidate(string[] candidates, int progress)
  {
    // we need to update the position at the same time
    // step 1, feed the candidates column by column,
    // step 2, record the maximum length for each column
    int candNum = Mathf.Min(candidates.Length, CandidateCount);
    maxLength = new int[CandidatePerRow];
    int longestCand = 0;
    // reset the candidateColumn
    for (int i = 0; i < CandidatePerRow; i++)
    {
      candidateColumns[i].Clear();
    }

    // by column layout
    for (int i = 0; i < candNum; i++)
    {
      // check the next character for each candidate, and then put it into the correct bucket
      // turn regular char to input string
      // if it is completed candidates, put it in the center lol
      int columnIndex = 4;
      if (candidates[i].Length > progress)
      {
        columnIndex = mapKey2Column[candidates[i][progress]];
        columnIndex = columnIndex > 3 ? columnIndex + 1 : columnIndex;
      }

      if (candidateColumns[columnIndex].Count < candidateNumberPerColumn)
      {
        if (candidates[i].Length > 0)
        {
          candidateColumns[columnIndex].Add(candidates[i]);
          maxLength[columnIndex] = Mathf.Max(maxLength[columnIndex], candidates[i].Length);
          longestCand = Mathf.Max(longestCand, candidates[i].Length);
        }
      }
    }

    // calculate the longest word per column and change the position
    CandidateWidth = perWidth * longestCand;
    float hoffset = (CandidatePerRow - 1.0f) / 2f * -CandidateWidth;
    for (int i = 0; i < candidateColumns.Count; i++)
    {
      candidateColumns[i].Sort();
      for (int j = 0; j < candidateColumns[i].Count; j++)
      {
        candidateObjects[i + j * CandidatePerRow].GetComponent<Candidate>().SetCandidateText(candidateColumns[i][j], progress);
        //                CandidateWidth = perWidth * longestCand;
        candidateObjects[i + j * CandidatePerRow].transform.localPosition =
            new Vector3(-CandidateWidth * (CandidatePerRow - 1) / 2 + i * CandidateWidth, j * CandidateHeight, 0);
      }
      for (int j = candidateColumns[i].Count; j < candidateNumberPerColumn; j++)
      {
        candidateObjects[i + j * CandidatePerRow].transform.localPosition =
            new Vector3(hoffset + i * CandidateWidth, j * CandidateHeight, 0);
      }
    }
  }

  private string[] cachedFullCandidates = new string[54];
  private string[] cachedCandidates = new string[54];
  private int cachedProgress;
  private string[] cachedCompleteCand = new string[54];
  private int cacheProgress;
  public void UpdateCandidates(string[] candidates, int progress, string[] completedCand)
  {
    // updateCandidates should be called only when has regular input
    pageIndex = 1;

    cachedProgress = progress;
    //cachedCompleteCand = new string[completedCand.Length];
    //completedCand.CopyTo(cachedCompleteCand, 0);
    int completedCandCount = completedCand.Length;
    //Array.Copy(completedCand, cachedCompleteCand, completedCandCount);

    int candidatesCount = candidates.Length;
    //Array.Copy(candidates, cachedFullCandidates, candidatesCount);
    //cachedFullCandidates = new string[candidates.Length];
    //candidates.CopyTo(cachedFullCandidates, 0);

    pageTotal = (completedCandCount + 4) / 5;// +4 is for rounding

    int cachedCandidateCount = 0;
    if (enableWordCompletion)
    {
      //Array.Copy(candidates, cachedCandidates, candidatesCount);
      cachedCandidateCount = candidatesCount;
      //cachedCandidates = new string[candidates.Length];
      //candidates.CopyTo(cachedCandidates, 0);
      pageTotal = (candidatesCount + 4) / 5;// +4 is for rounding
    }

    if (candidateLayout == ProfileLoader.CandLayout.ROW)
      UpdateRowLayoutCandidate(cachedCandidates, progress);
    else if (candidateLayout == ProfileLoader.CandLayout.FAN)
      UpdateFanLayoutCandidate(cachedCandidates, progress);
    else if (candidateLayout == ProfileLoader.CandLayout.LEXIC)
    {
            CandidateCount = 13;
            PrepareCandidates(candidates, CandidateCount, completedCand);
            string[] newCand = ReorgCandidates(candidates, CandidateCount, completedCand);
            //for (int i = 0; i < newCand.Length; i++) {
            //    Debug.Log("newCand[" + i + "]:" + newCand[i]);
            //}
            UpdateLexicalCandidate(newCand, progress);
    }
    else if (candidateLayout == ProfileLoader.CandLayout.BYCOL)
    {
      UpdateByColumnCandidate(cachedCandidates, progress);
    }
    else if (candidateLayout == ProfileLoader.CandLayout.WORDCLOUD)
    {
            CandidateCount = 16;
            PrepareCandidates(candidates, CandidateCount, completedCand);
            string[] newCand = ReorgCandidates(candidates, CandidateCount, completedCand, false);
      UpdateWordCloudLayout(newCand, candidates, progress);
    }
    else if (candidateLayout == ProfileLoader.CandLayout.DIVISION)
    {
            PrepareCandidates(candidates, CandidateCount, completedCand);
            UpdateDivisionLayout(candidates, progress, 0);
    }
    else if (candidateLayout == ProfileLoader.CandLayout.DIVISION_END)
    {
            PrepareCandidates(candidates, CandidateCount, completedCand);
            UpdateDivisionLayout(candidates, progress, 1);
    }
    else if (candidateLayout == ProfileLoader.CandLayout.ONE)
    {
      cacheProgress = progress;
      //string[] newCand;
      if (enableWordCompletion)
      {
        PrepareCandidates(candidates, CandidateCount, completedCand);
      }      
      else
      {
        PrepareCandidates(new string[0], CandidateCount, completedCand);
      }
      // below has pageIndex involved, so we should call below when nextPage
      UpdateOneLayout(newCand, candidates, progress);
    }
    UpdatePageNumber();
  }

  private void UpdatePageNumber()
  {
    pageTextMesh.SetText("Page " + pageIndex.ToString() + "/" + pageTotal.ToString());
  }

  public void ResetPage()
  {
    pageTextMesh.SetText("");
  }

  public void NextPage()
  {
    if (pageTotal == 0)
      return;
    // "b" + "["
    pageIndex = (pageIndex + 1) % pageTotal;
    if (pageIndex == 0) pageIndex = pageTotal;
    UpdatePageNumber();
    //string[] newCand = ReorgCandidates(wordListLoader.currentCandidates, 5, wordListLoader.completeCandDict[inputHandler.currentInputString], false);
    UpdateOneLayout(newCand, wordListLoader.currentCandidates, cacheProgress);
  }

  public void PrevPage()
  {
    if (pageTotal == 0)
      return;
    // "b" + "0"
    pageIndex = (pageIndex + pageTotal - 1) % pageTotal;
    if (pageIndex == 0) pageIndex = pageTotal;
    UpdatePageNumber();
    //string[] newCand = ReorgCandidates(wordListLoader.currentCandidates, 5, wordListLoader.completeCandDict[inputHandler.currentInputString], false);
    UpdateOneLayout(newCand, wordListLoader.currentCandidates, cacheProgress);
  }

  private void UpdateOneLayout(string[] cachedReorgCand, string[] candidates, int progress)
  {
    // show at most 5 complete candidates; If there are more than 5, show the most frequent ones
    // later we will filter the words that do not meet this requirement if we are using qwerty      
    if (cachedReorgCand.Length > 0) {
      // show the word according to the page index
      int startIndex = (pageIndex - 1) * CandidateCount;
      defaultWord = cachedReorgCand[startIndex];

      int maxLength = Mathf.Max(4, defaultWord.Length);
      int candNum = Mathf.Min(cachedReorgCand.Length, CandidateCount);
      for (int i = 1; i < Math.Min(CandidateCount, cachedReorgCand.Length - startIndex); i++) {
        if (cachedReorgCand[i + startIndex].Length > maxLength)
          maxLength = cachedReorgCand[i + startIndex].Length;
      }
      CandidateWidth = perWidth * maxLength;

      for (int i = 0; i < CandidateCount; i++) {
        candidateObjects[i].GetComponent<Candidate>().SetCandidateText(i < (cachedReorgCand.Length - startIndex) ? cachedReorgCand[i+ startIndex] : "", progress, maxLength - 1);
        //candidateObjects[i].GetComponent<Candidate>().SetCandidateText(i < candNum ? cachedReorgCand[i]
        //  : (enableWordCompletion && i < cachedReorgCand.Length ? cachedReorgCand[i] : ""), progress, maxLength - 1);
      }
    } else {
      // show one word when there is no complete candidates
      // pick the shortest one for now
      int minLengthCand = candidates[0].Length;
      string oneCand = candidates[0];
      for (int i = 1; i < candidates.Length; i++) {
        if (candidates[i].Length == progress + 1) {
          oneCand = candidates[i];
          break;
        }
        else if (minLengthCand > candidates[i].Length) {
          minLengthCand = candidates[i].Length;
          oneCand = candidates[i];
        }
      }
      defaultWord = oneCand;
      candidateObjects[0].GetComponent<Candidate>().SetCandidateText(oneCand, progress, Mathf.Max(4, oneCand.Length));
      for (int i = 1; i < CandidateCount; i++)
      {
        candidateObjects[i].GetComponent<Candidate>().SetCandidateText("");
      }
    }  
  }

  // if no completion, return all complete cand, even 0
  // if with completion, return all complete cand + incomplete as many as we can
  public int GetCandCount() {
    if (ProfileLoader.selectionMode == ProfileLoader.SelectionMode.MS)
      return Math.Min(5, Math.Max(1, newCand.Length));
    else
    {
      return Math.Min(10, Math.Max(1, newCand.Length));
    }
  }
  private string[] newCand = new string[] { };
  private string[] PrepareCandidates(string[] candidates, int totalNumber, string[] completedCand)
  {
    // instead of getting only totalNumber candidates ready for current page, we should get all of them ready for next/prev page to access
    int newCandLen = completedCand.Length;
    if (enableWordCompletion)
      newCandLen = candidates.Length;

    newCand = new string[newCandLen];
    for (int i = 0; i < newCandLen; i++)
      newCand[i] = "";

    // put complete cand first, then following with incomplete candidates
    // there still exists completed candidates in candidates array so we need to skip them
    //// a simple trick: because all the candidates will be sorted via lexcial order later, we just need to put all the completed candidates first, and then the top (n-m) incompleted candidates

    // copy completed cand first and then copy incompleted cand
    if(completedCand.Length > 0)
      Array.Copy(completedCand, 0, newCand, 0, completedCand.Length);
    int curIndex = completedCand.Length;
    for (int j = 0; curIndex < newCandLen && j < candidates.Length; curIndex++, j++)
    {
      while (candidates[j].Length == inputHandler.currentInputString.Length)
      {
        ++j;
      }
      newCand[curIndex] = candidates[j];
    }
    
    return newCand;
  }
  private string[] ReorgCandidates(string[] candidates, int totalNumber, string[] completedCand, bool remainFirst = true)
  {
        // instead of getting only totalNumber candidates ready for current page, we should get all of them ready for next/prev page to access
        int newCandLen = Math.Min(totalNumber, candidates.Length);
    //if (newCandLen > totalNumber)      newCandLen = totalNumber;
    //Math.Max(totalNumber, completedCand.Length);

    //int startIndex = (pageIndex - 1) * totalNumber;
    remainFirst = remainFirst && enableWordCompletion;
    // make sure the complete candidates are placed in candidates before totalNumber
    int completeCandNumber = completedCand.Length;
    
    newCand = new string[newCandLen];
    for (int i = 0; i < newCandLen; i++)
      newCand[i] = "";

    //if (completeCandNumber == 0)
    //{
    //  // no completed candidates then we just return candidates directly            
    //  return newCand;
    //}
    // put complete cand first, then following with incomplete candidates
    // there still exists completed candidates in candidates array so we need to skip them
    //// a simple trick: because all the candidates will be sorted via lexcial order later, we just need to put all the completed candidates first, and then the top (n-m) incompleted candidates
    ///

    if (remainFirst)
    {      
      newCand[0] = candidates[0];
      int curIndex = 1;
      if (completedCand.Length > 0 && candidates[0].Length == completedCand[0].Length)
      {
        int mostCommonIdx = Array.IndexOf(completedCand, candidates[0]);
        // need to skip the most frequent word if it is not complete cand
        Array.Copy(completedCand, 0, newCand, 1, mostCommonIdx);
        Array.Copy(completedCand, mostCommonIdx + 1, newCand, 1 + mostCommonIdx, completedCand.Length - mostCommonIdx - 1);
        curIndex += completedCand.Length - 1;
      }
      else
      {
        Array.Copy(completedCand, 0, newCand, 1, completedCand.Length);
        curIndex += completedCand.Length;
      }
      // copy the rest of candidates that are not completed
      for(int j = 1; curIndex < newCandLen && j < candidates.Length; curIndex++, j++)
      {
        while(candidates[j].Length == inputHandler.currentInputString.Length)
        {
          ++j;
        }
        newCand[curIndex] = candidates[j];
      }
    }
    else
    {
      // copy completed cand first and then copy incompleted cand
      Array.Copy(completedCand, 0, newCand, 0, Math.Min(newCandLen, completedCand.Length));
      int curIndex = Math.Min(newCandLen, completedCand.Length);
      for (int j = 0; curIndex < newCandLen && j < candidates.Length; curIndex++, j++)
      {
        while (candidates[j].Length == inputHandler.currentInputString.Length)
        {
          ++j;
        }
        newCand[curIndex] = candidates[j];
      }
    }
    return newCand;

    //int copyNumber = Mathf.Max(0, Mathf.Min(completedCand.Length, startIndex + totalNumber) - startIndex);

    //int completeNotFirst = -1;
    //if (remainFirst && candidates.Length > startIndex)
    //{
    //  newCand[0] = candidates[0];
    //  copyNumber -= 1;
    //  // if candidates[0] is in completedCand, we need to copy the 13th one
    //  for (int i = 0; i < copyNumber; i++)
    //  {
    //    if (completedCand[i] == newCand[0])
    //    {
    //      completeNotFirst = i;
    //      break;
    //    }
    //  }
    //}
    //int toCopy = copyNumber;
    //if (completeNotFirst != -1)
    //{
    //  Array.Copy(completedCand, 0, newCand, 1, completeNotFirst);
    //  Debug.LogWarning("completedCand.length:" + completedCand.Length);
    //  Debug.LogWarning("completeNotFirst:" + completeNotFirst);
    //  Debug.LogWarning("newCand.length:" + newCand.Length);
    //  Debug.LogWarning("copyNumber - completeNotFirst:" + (copyNumber - completeNotFirst));
    //  toCopy = Mathf.Min(copyNumber - completeNotFirst, completedCand.Length - (completeNotFirst + 1));
    //  //if (completedCand.Length > (completeNotFirst + 1) && newCand.Length > (completeNotFirst + 1)) {
    //  Array.Copy(completedCand, completeNotFirst + 1, newCand, completeNotFirst + 1, toCopy);
    //  //foreach (string nc in newCand) {
    //  //    Debug.Log(nc);
    //  //}
    //  //}            
    //}
    //else 
    //{
    //  if (copyNumber > 0)
    //    Array.Copy(completedCand, startIndex, newCand, remainFirst ? 1 : 0, copyNumber);
    //  toCopy = remainFirst ? copyNumber : (copyNumber - 1);
    //}

    //for (int i = toCopy + 1, j = (remainFirst ? 1 : 0); i < totalNumber && completedCand.Length > 0; i++)
    //{
    //  while (j < candidates.Length && candidates[j].Length == completedCand[0].Length && remainFirst)
    //  {
    //    // completed cand
    //    ++j;
    //  }
    //  // not completed cand
    //  if (j < candidates.Length)
    //  {
    //    newCand[i] = candidates[(startIndex + j)];
    //    ++j;
    //  }
    //  else
    //  {
    //    // j reaches the end of candidates, no more could be assigned to newCand
    //    string[] newCand2 = new string[i];
    //    Array.Copy(newCand, newCand2, i);
    //    //foreach (string nc in newCand2) {
    //    //    Debug.Log(nc);
    //    //}
    //    return newCand2;
    //  }
    //}
    //return newCand;
  }

  public void ResetCandidates()
  {
    for (int i = 0; i < CandidateCount; i++)
    {
      candidateObjects[i].GetComponent<Candidate>().SetCandidateText("");
    }
    cachedCandidates = null;
    cachedCompleteCand = null;
  }

  // Update is called once per frame
  //void Update() {
  //if (Input.GetKeyDown("c"))
  //{
  //CreateFanLayout();
  //}        
  //}
}

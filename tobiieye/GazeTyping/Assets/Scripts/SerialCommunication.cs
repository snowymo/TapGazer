using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;
using System.Collections.Concurrent;

public class SerialCommunication : MonoBehaviour
{
    public ConcurrentQueue<int> serialMessage;

    private SerialPort handStream = new SerialPort("COM7", 9600);

    public string prevFingers;

//    private string[] inputString = { "q", "3", "4", "t", "b", "n", "u", "9", "0", "[" };

    // Start is called before the first frame update
    void Start()
    {
        //handStream.Open();
        serialMessage = new ConcurrentQueue<int>();
        prevFingers = "10000000000";
        //rightHandStream.ReadTimeout = 1;
    }

    public void Open()
    {
        handStream.Open();
    }

    // Update is called once per frame
    void Update()
    {             
        if (handStream.IsOpen)
        {
            try
            {
                // should be different and has an extra 1 => press
                string curHand = handStream.ReadLine();
                if(!prevFingers.Equals(curHand))
                {
                    int count = 0;
                    int curPressedFinger = -1;
                    if(curHand.Length != 11)
                    {
                        Debug.LogWarning("serial length wrong: " + curHand);
                        return;
                    }
                    for(int i = 1; i <= 10; i++)
                    {
                        if(curHand[i] == '1' && prevFingers[i] == '0')
                        {
                            ++count;
                            curPressedFinger = i-1;
                        }
                    }
                    if(count == 1)
                    {
                        serialMessage.Enqueue(curPressedFinger);
                    }
                    prevFingers = curHand;
                }
                //print("hands: " + curRightHand);
            }
            catch (Exception e)
            {
                print(e);
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            handStream.Close();
        }
    }
}

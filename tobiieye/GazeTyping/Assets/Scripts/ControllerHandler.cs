using System.Collections;
using System.Collections.Generic;
using Tobii.XR.Examples;
using UnityEngine;

// take care of the vive controller action

public class ControllerHandler : MonoBehaviour
{

    public Transform TypingSection;

    // Start is called before the first frame update
    void Start()
    {
        TypingSection.position = Camera.main.gameObject.transform.position;
        TypingSection.rotation = Camera.main.gameObject.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (ControllerManager.Instance.GetButtonPressDown(ControllerButton.Trigger))
        {
            // reset the input area, put it in front of the current camera
            TypingSection.position = Camera.main.gameObject.transform.position;
            TypingSection.rotation = Camera.main.gameObject.transform.rotation;
        }
    }
}

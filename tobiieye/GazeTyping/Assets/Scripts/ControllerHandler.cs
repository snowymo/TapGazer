using System.Collections;
using System.Collections.Generic;
using Tobii.XR.Examples;
using UnityEngine;

// take care of the vive controller action

public class ControllerHandler : MonoBehaviour
{

    public Transform TypingSection;

    void updateTypingSection()
    {
    // reset the input area, put it in front of the current camera
    Quaternion quat = Camera.main.gameObject.transform.rotation;
    TypingSection.position = Camera.main.gameObject.transform.position + quat * Vector3.forward * 10;
        
        Vector3 angle = quat.eulerAngles;
        angle.z = 0;
        TypingSection.rotation = Quaternion.Euler(angle);
    }

    // Start is called before the first frame update
    void Start()
    {
        updateTypingSection();
    }

    // Update is called once per frame
    void Update()
    {
        if (UnityEngine.XR.XRSettings.enabled && ControllerManager.Instance.GetButtonPressDown(ControllerButton.Trigger))
        {
            updateTypingSection();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class walkInPlace : MonoBehaviour
{
    InputDevice tracker1;
    InputDevice tracker2;
    // Start is called before the first frame update
    void Start()
    {
        var allDevices = new List<InputDevice>();
        //InputDevices.GetDevices(allDevices);
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.TrackedDevice, allDevices);
        int count = 0;

        foreach (InputDevice d in allDevices)
        {
            Debug.Log(d.name);
        }

        /*foreach (InputDevice device in allDevices){
            if (device.role == InputDeviceRole.HardwareTracker)
            {
                if (count == 0)
                {
                    tracker1 = device;
                }
                else
                {
                    tracker2 = device;
                }
            }
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        //tracker1.TryGetFeatureValue(CommonUsages.devicePosition, out var pos1);
        //tracker2.TryGetFeatureValue(CommonUsages.devicePosition, out var pos2);
    }
}

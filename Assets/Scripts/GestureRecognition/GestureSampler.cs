using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureSampler : MonoBehaviour
{
    public List<GestureData> gestureData = new List<GestureData>();
    
    private void Update()
    {
        if (Input.GetKey(KeyCode.Alpha1))
        {
            gestureData[0].AddSample(HandWorldLandmarkProcesser.instance.HandAngles);
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            gestureData[1].AddSample(HandWorldLandmarkProcesser.instance.HandAngles);
        }
        else if (Input.GetKey(KeyCode.Alpha3))
        {
            gestureData[2].AddSample(HandWorldLandmarkProcesser.instance.HandAngles);
        }
    }
}

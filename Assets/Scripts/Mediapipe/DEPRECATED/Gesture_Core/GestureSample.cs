using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GestureSample
{
    public float[] angles;

    public GestureSample(float[] newAngles)
    {
        angles = newAngles;
    }
}

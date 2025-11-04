using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGesture", menuName = "Gesture/GestureData")]
public class GestureData : ScriptableObject
{
    public GestureType gestureLabel;
    
    public List<GestureSample> samples = new List<GestureSample>();

    public void AddSample(float[] currentAngles)
    {
        samples.Add(new GestureSample(currentAngles));
        Debug.Log($"GestureLabel : {gestureLabel} Sample Added / Current Sample Count : {samples.Count}");
    }
}

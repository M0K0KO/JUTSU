using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestureSolver : MonoBehaviour
{
    public List<GestureData> gestureDatabase;

    [SerializeField] private float confidenceThreshold = 20f;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GestureType type = SolveGesture(HandWorldLandmarkProcesser.instance.HandAngles);

            string indicatorText = "";
            switch (type)
            {
                case GestureType.Open:
                    indicatorText = "Open";
                    break;
                case GestureType.Close:
                    indicatorText = "Close";
                    break;
                case GestureType.Kon:
                    indicatorText = "Kon";
                    break;
                case GestureType.None:
                    indicatorText = "None";
                    break;
            }
            
            GestureIndicator.instance.ChangeGestureIndicatorText(indicatorText);
        }
    }

    public GestureType SolveGesture(float[] currentAngles)
    {
        float minDistance = float.MaxValue;
        GestureType bestGesture = GestureType.None;

        foreach (GestureData gesture in gestureDatabase)
        {
            foreach (GestureSample sample in gesture.samples)
            {
                float distance = CalculateEuclideanDistance(currentAngles, sample.angles);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestGesture = gesture.gestureLabel;
                }
            }
        }

        Debug.Log(minDistance);
        
        if (minDistance < confidenceThreshold)
        {
            return bestGesture;
        }
        else
        {
            return GestureType.None;
        }
        
    }
    
    private float CalculateEuclideanDistance(float[] a, float[] b)
    {
        float distance = 0;
        
        for (int i = 0; i < a.Length; i++)
        {
            distance += (a[i] - b[i]) * (a[i] - b[i]);
        }
        
        return distance;
    }
}

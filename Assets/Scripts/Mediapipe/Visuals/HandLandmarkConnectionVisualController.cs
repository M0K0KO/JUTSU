using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandLandmarkConnectionVisualController : MonoBehaviour
{
    LineRenderer lineRenderer;

    private int lengthOfLine = 2;
    
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        lineRenderer.positionCount = lengthOfLine;
    }

    public void SetPoints(Vector3 startPoint, Vector3 endPoint)
    {
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
    }
}

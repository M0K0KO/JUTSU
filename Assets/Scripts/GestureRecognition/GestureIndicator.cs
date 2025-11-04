using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GestureIndicator : MonoBehaviour
{
    public static GestureIndicator instance;
    
    [SerializeField] private TextMeshProUGUI gestureIndicatorText;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void ChangeGestureIndicatorText(string text)
    {
        gestureIndicatorText.text = text;
    }
}

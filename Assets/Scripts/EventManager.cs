using System;
using UnityEngine;

public static class EventManager
{
    public static Action<PlayerCameraState, Transform> OnCameraStateChange;
    public static void TriggerOnCameraStateChange(PlayerCameraState state, Transform target)
    {
        OnCameraStateChange?.Invoke(state, target);
    }


    public delegate void GestureRecognition(GestureType gestureType);
    public static GestureRecognition OnGestureRecognition;
    public static void TriggerOnGestureRecognition(GestureType gestureType)
    {
        if (gestureType == GestureType.None) return;
        
        Debug.Log($"TriggerOnGestureRecognition {gestureType.ToString()}");
        OnGestureRecognition?.Invoke(gestureType);
    }
}

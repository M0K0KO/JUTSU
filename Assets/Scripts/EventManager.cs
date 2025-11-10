using System;
using UnityEngine;

public static class EventManager
{
    public static Action<PlayerCameraState, Transform> OnCameraStateChange;

    public static void TriggerOnCameraStateChange(PlayerCameraState state, Transform target)
    {
        OnCameraStateChange?.Invoke(state, target);
    }
}

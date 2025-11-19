using System;
using UnityEngine;

public static class EventManager
{
    public static Action<PlayerCameraState, Transform> OnCameraStateChange;
    public static void TriggerOnCameraStateChange(PlayerCameraState state, Transform target)
    {
        OnCameraStateChange?.Invoke(state, target);
    }

    public static Action OnJustuModeEnter;
    public static void TriggerOnJustuModeEnter()
    {
        OnJustuModeEnter?.Invoke();
    }

    public static Action<GestureType> OnJutsuActivation;

    public static void TriggerOnJutsuActivation(GestureType type)
    {
        OnJutsuActivation?.Invoke(type);
    }
    
    public static Action OnJustuModeExit;

    public static void TriggerOnJustuModeExit()
    {
        OnJustuModeExit?.Invoke();
    }

    public static Action<Vector3, float> OnAkaHit;

    public static void TriggerOnAkaHit(Vector3 initialDirection, float pushDuration)
    {
        OnAkaHit?.Invoke(initialDirection, pushDuration);
    }
}

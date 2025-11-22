using System;
using UnityEngine;

public static class EventManager
{
    public static Action<PlayerCameraState, Transform> OnCameraStateChange;
    public static void TriggerOnCameraStateChange(PlayerCameraState state, Transform target)
    {
        OnCameraStateChange?.Invoke(state, target);
    }

    public static Action OnJutsuModeEnter;
    public static void TriggerOnJutsuModeEnter()
    {
        OnJutsuModeEnter?.Invoke();
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

    public static Action<Vector3, float, float> OnAkaHit;
    public static void TriggerOnAkaHit(Vector3 initialDirection, float pushDuration, float projectileSpeed)
    {
        OnAkaHit?.Invoke(initialDirection, pushDuration, projectileSpeed);
    }

    public static Action OnMuryokushoStart;
    public static void TriggerOnMuryokushoStart()
    {
        OnMuryokushoStart?.Invoke();
    }

    public static Action OnMuryokushoEnd;
    public static void TriggerOnMuryokushoEnd()
    {
        OnMuryokushoEnd?.Invoke();
    }
}

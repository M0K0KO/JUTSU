using System;
using System.Collections;
using System.Collections.Generic;
using Mediapipe.Unity.HandWorldLandmarkDetection;
using UnityEngine;

public class PlayerJutsuManager : MonoBehaviour
{
    private PlayerManager player;

    [Header("Sequence Settings")]
    [SerializeField] private float sequenceMaxDuration;
    [SerializeField, Range(0.1f, 0.9f)] private float slowedTimeScale;

    private bool jutsuGestureTrigger = false;
    private bool jutsuVoiceTrigger = false;

    public bool isUsingJutsu { get; private set; } = false;

    private void Awake()
    {
        player = GetComponent<PlayerManager>();
    }

    private void Update()
    {
        if (player.playerInput.JutsuInput)
        {
            player.playerInput.ClearJutsuInput();
            
            // if (cooldown~~)
            if (!isUsingJutsu)
                StartCoroutine(JutsuMode(GestureType.Kon));
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            GestureTrigger();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            VoiceTrigger();
        }
    }

    public IEnumerator JutsuMode(GestureType gestureType)
    {
        isUsingJutsu = true;
        
        player.stateMachine.CheckNearbyEnemies(out GameObject target, false);
        PlayerCameraStateHandler.instance.UpdateCameraState(PlayerCameraState.Jutsu, target.transform);

        jutsuGestureTrigger = false;
        jutsuVoiceTrigger = false;
        float elapsedTime = 0f;
        Time.timeScale = slowedTimeScale;
        
        Action jutsu = GetJutsu(gestureType);

        bool isTriggered = false;

        while (elapsedTime < sequenceMaxDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            
            if (gestureType == HandWorldLandmarkVisualizer.instance.currentGesture)
            {
                jutsuGestureTrigger = true;
                // Start Recording
            }

            if (jutsuGestureTrigger)
            {
                if (jutsuVoiceTrigger)
                {
                    jutsuVoiceTrigger = false;
                    Time.timeScale = 1;
                    isTriggered = true;
                    break;
                }
            }
            yield return null;
        }

        
        HandWorldLandmarkVisualizer.instance.DeactivateVisuals();
        
        
        if (isTriggered)
        {
            Debug.Log("Jutsu Sequence Ended (Triggered)");
            jutsu();
        }
        else
        {
            Debug.Log("Jutsu Sequence Ended (Out of duration)");
            Time.timeScale = 1;
        }
        
        PlayerCameraStateHandler.instance.UpdateCameraState(PlayerCameraState.Strafe, target.transform);

        jutsuGestureTrigger = false;
        jutsuVoiceTrigger = false;
        isUsingJutsu = false;
        yield return null;
    }

    private Action GetJutsu(GestureType gestureType)
    {
        switch (gestureType)
        {
            case GestureType.Kon:
                return Kon;
            default:
                return null;
        }
    }

    // Exposed API for Jutsu voice Recognition
    public void VoiceTrigger()
    {
        jutsuVoiceTrigger = true;
        Debug.Log("Voice Recognized!");
    }

    public void GestureTrigger()
    {
        jutsuGestureTrigger = true;
        Debug.Log("Gesture Recognized!");
    }
    
    private void Kon()
    {
        Debug.Log("SUMMONING FOX DEVIL");
    }
}

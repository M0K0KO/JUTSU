using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mediapipe.Unity.HandWorldLandmarkDetection;
using UnityEngine;
using Whisper;
using Whisper.Utils;

public class PlayerJutsuManager : MonoBehaviour
{
    private PlayerManager player;

    [Header("Sequence Settings")]
    [SerializeField] private float sequenceMaxDuration;
    [SerializeField, Range(0.1f, 0.9f)] private float slowedTimeScale;

    [Header("Jutsu List")]
    [SerializeField] private List<Jutsu> jutsuList;

    public bool isUsingJutsu { get; private set; } = false;

    private Dictionary<GestureType, Jutsu> jutsuDict = new Dictionary<GestureType, Jutsu>();

    private void Awake()
    {
        player = GetComponent<PlayerManager>();

        //VoiceRecognitionManager.instance.microphoneRecord.OnRecordStop += OnRecordStop;
    }

    private void Start()
    {
        foreach (var jutsu in jutsuList)
        {
            jutsuDict.Add(jutsu.gestureType, jutsu);
        }
    }

    private void OnDestroy()
    {
    }

    private void Update()
    {
        if (player.playerInput.JutsuInput)
        {
            player.playerInput.ClearJutsuInput();
            
            // if (cooldown~~)
            if (!isUsingJutsu)
                StartCoroutine(JutsuMode());
        }
    }

    public IEnumerator JutsuMode()
    {
        
        isUsingJutsu = true;
        
        player.stateMachine.CheckNearbyEnemies(out GameObject target, false);
        PlayerCameraStateHandler.instance.UpdateCameraState(PlayerCameraState.Jutsu, target.transform);

        float elapsedTime = 0f;
        Time.timeScale = slowedTimeScale;

        bool isTriggered = false;
        bool jutsuGestureTrigger = false;

        Action jutsu = null;
        string expectedVoiceCommand = "";
        GestureType detectedGesture = GestureType.None;

        Task<string> voiceTask = null;
        CancellationTokenSource cts = new CancellationTokenSource();
        
        Debug.Log("Jutsu Mode: Started. Waiting for gesture...");

        try
        {
            while (elapsedTime < sequenceMaxDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;

                if (!jutsuGestureTrigger)
                {
                    detectedGesture = HandWorldLandmarkVisualizer.instance.currentGesture;
                    if (detectedGesture != GestureType.None)
                    {
                        jutsu = GetJutsu(detectedGesture);
                        expectedVoiceCommand = GetJutsuVoiceCommand(detectedGesture);

                        jutsuGestureTrigger = true;
                        Debug.Log(
                            $"[Phase 1] Gesture '{detectedGesture}' detected. Listening for '{expectedVoiceCommand}'...");
                        voiceTask = RecognizeVoiceAsync(cts.Token);
                    }
                }
                else
                {
                    if (HandWorldLandmarkVisualizer.instance.currentGesture != detectedGesture)
                    {
                        Debug.Log("[Canceled] Gesture was not held. Cancelling Jutsu.");
                        break; 
                    }

                    if (voiceTask != null && voiceTask.IsCompleted)
                    {
                        if (voiceTask.Status == TaskStatus.RanToCompletion)
                        {
                            string voiceResult = voiceTask.Result;
                            Debug.Log($"[Phase 2] Voice task completed. Heard: '{voiceResult}'");

                            if (StringSimilarity.IsSimilar(voiceResult, expectedVoiceCommand)) isTriggered = true;
                            else Debug.Log("Wrong voice command.");
                        }
                        else if (voiceTask.Status == TaskStatus.Canceled)
                        {
                            Debug.Log("[Phase 2] Voice task was canceled.");
                        }
                        else if (voiceTask.Status == TaskStatus.Faulted)
                        {
                            Debug.LogWarning($"[Phase 2] Voice task faulted: {voiceTask.Exception}");
                        }
                        break; 
                    }
                }

                yield return null;
            }
        }
        finally
        {
            Debug.Log("Jutsu sequence ending. Cleaning up...");
            cts.Cancel();
            cts.Dispose();

            Time.timeScale = 1f;
            if (HandWorldLandmarkVisualizer.instance != null)
            {
                HandWorldLandmarkVisualizer.instance.DeactivateVisuals();
            }
            if (VoiceRecognitionManager.instance != null && VoiceRecognitionManager.instance.microphoneRecord != null)
            {
                VoiceRecognitionManager.instance.microphoneRecord.StopRecord();
            }

            if (isTriggered && jutsu != null)
            {
                Debug.Log("Jutsu Sequence Ended (Triggered)");
                jutsu();
            }
            else
            {
                Debug.Log("Jutsu Sequence Ended (Timeout or Failed)");
            }
            
            if (PlayerCameraStateHandler.instance != null && target != null)
            {
                PlayerCameraStateHandler.instance.UpdateCameraState(PlayerCameraState.Strafe, target.transform);
            }
            else if (PlayerCameraStateHandler.instance != null)
            {
                PlayerCameraStateHandler.instance.UpdateCameraState(PlayerCameraState.Strafe, null); 
            }
            
            isUsingJutsu = false;
        }
    }
    
    private async Task<string> RecognizeVoiceAsync(CancellationToken ct)
    {
        TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();

        OnVoiceRecognizedDelegate onResult = null;
        onResult = (result) =>
        {
            tcs.TrySetResult(result);
        };

        VoiceRecognitionManager.instance.whipserManager.OnVoiceRecognized += onResult;
        VoiceRecognitionManager.instance.microphoneRecord.StartRecord();
        
        using (ct.Register(() => tcs.TrySetCanceled(ct)))
        {
            VoiceRecognitionManager.instance.whipserManager.OnVoiceRecognized += onResult;
            VoiceRecognitionManager.instance.microphoneRecord.StartRecord();
        
            try
            {
                return await tcs.Task;
            }
            catch (OperationCanceledException)
            {
                Debug.Log("RecognizeVoiceAsync was canceled by CancellationToken.");
                return string.Empty; 
            }
            finally
            {
                VoiceRecognitionManager.instance.whipserManager.OnVoiceRecognized -= onResult;
                VoiceRecognitionManager.instance.microphoneRecord.StopRecord();
            }
        }
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

    private string GetJutsuVoiceCommand(GestureType gestureType) => jutsuDict[gestureType].targetCommand;

    private void Kon()
    {
        Debug.Log("SUMMONING FOX DEVIL");
    }
}

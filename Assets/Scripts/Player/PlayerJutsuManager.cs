using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Mediapipe.Unity.HandWorldLandmarkDetection;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using Whisper;
using Whisper.Utils;

public class PlayerJutsuManager : MonoBehaviour
{
    private PlayerManager player;

    [Header("Sequence Settings")]
    [SerializeField]
    private float sequenceMaxDuration;

    [SerializeField, Range(0.1f, 0.9f)] private float slowedTimeScale;

    [Header("Jutsu List")]
    [SerializeField]
    private List<Jutsu> jutsuList;

    [Header("Muryokusho")]
    [SerializeField] private Material bloomQuadMaterial;
    [SerializeField] private Material dissolveMaterial;
    [SerializeField] private Transform intersectionSphereTransform;
    [SerializeField] private MuryokushoSequenceData muryokushoSequenceData;

    [Header("Aka")]
    [SerializeField] private GameObject AkaObject;
    [SerializeField] private AkaSequenceData akaSequenceData;
    private Transform akaSpawnHandLandmark;

    [Header("Kon")]
    [SerializeField] private GameObject konWolfInstance;
    [SerializeField] private Animator konWolfAnimator;
    [SerializeField] private Material konWolfMaterial;
    [SerializeField] private KonSequenceData konSequenceData;
    

public bool isUsingJutsu { get; private set; } = false;

    private Dictionary<GestureType, Jutsu> jutsuDict = new Dictionary<GestureType, Jutsu>();

    private const int gestureQueueCapacity = 20;
    private Queue<GestureType> gestureQueue = new Queue<GestureType>(gestureQueueCapacity);

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

    private void Update()
    {
        if (player.playerInput.JutsuInput)
        {
            player.playerInput.ClearJutsuInput();

            // if (cooldown~~)
            if (!isUsingJutsu)
                StartCoroutine(JutsuMode());
        }


        if (Input.GetKeyDown(KeyCode.Q))
        {
            //Muryokusho();
            //Aka();
            Kon();
        }
    }

    public IEnumerator JutsuMode()
    {
        isUsingJutsu = true;
        EventManager.TriggerOnJustuModeEnter();

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
                    if (detectedGesture != GestureType.None && jutsuDict.ContainsKey(detectedGesture))
                    {
                        jutsu = GetJutsu(detectedGesture);
                        expectedVoiceCommand = GetJutsuVoiceCommand(detectedGesture);

                        for (int i = 0; i < gestureQueueCapacity; i++)
                        {
                            gestureQueue.Enqueue(detectedGesture);
                        }

                        jutsuGestureTrigger = true;
                        Debug.Log(
                            $"[Phase 1] Gesture '{detectedGesture}' detected. Listening for '{expectedVoiceCommand}'...");
                        voiceTask = RecognizeVoiceAsync(cts.Token);
                    }
                }
                else
                {

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

                    gestureQueue.CapacitySafeEnqueue(HandWorldLandmarkVisualizer.instance.currentGesture, gestureQueueCapacity);
                    
                    if (gestureQueue.GetCount(detectedGesture) == 0)
                    {
                        Debug.Log("[Canceled] Gesture was not held. Cancelling Jutsu.");
                        cts.Cancel();
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

            if (isTriggered && jutsu != null)
            {
                Debug.Log("Jutsu Sequence Ended (Triggered)");
                EventManager.TriggerOnJutsuActivation(detectedGesture);
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

            gestureQueue.Clear();
            
            isUsingJutsu = false;
            EventManager.TriggerOnJustuModeExit();
        }
    }

    private async Task<string> RecognizeVoiceAsync(CancellationToken ct)
    {
        var mic = VoiceRecognitionManager.instance.microphoneRecord;
        var whisper = VoiceRecognitionManager.instance.whipserManager;
        
        TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
        OnRecordStopDelegate onStop = null;
        bool isCompleted = false;

        try
        {
            Debug.Log("[Voice] Starting voice recognition...");
            
            onStop = async (chunk) =>
            {
                if (isCompleted) return; 
                
                try
                {
                    Debug.Log($"[Voice] Recording stopped. Audio length: {chunk.Length}s, Voice detected: {chunk.IsVoiceDetected}");
                    Debug.Log($"[Voice] Audio data size: {chunk.Data.Length} samples, Frequency: {chunk.Frequency}Hz");
                    
                    Debug.Log("[Voice] Processing audio with Whisper...");
                    var result = await whisper.GetTextAsync(chunk.Data, chunk.Frequency, chunk.Channels);
                    
                    if (result != null && !string.IsNullOrEmpty(result.Result))
                    {
                        var transcription = result.Result.Trim();
                        Debug.Log($"[Voice] ✓ Transcription complete: '{transcription}'");
                        tcs.TrySetResult(transcription);
                    }
                    else
                    {
                        Debug.Log("[Voice] ✗ Transcription returned empty");
                        tcs.TrySetResult(string.Empty);
                    }
                    
                    isCompleted = true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[Voice] Error in OnRecordStop handler: {e}");
                    tcs.TrySetException(e);
                    isCompleted = true;
                }
            };

           mic.OnRecordStop += onStop;
           
            
            if (!mic.IsRecording)
            {
                mic.StartRecord();
                Debug.Log("[Voice] 🎤 Microphone recording started (waiting for speech...)");
            }
            else
            {
                Debug.LogWarning("[Voice] Microphone is already recording!");
            }
            
            using (ct.Register(() => 
            {
                if (isCompleted) return;
                
                Debug.Log("[Voice] Voice recognition cancelled by token");
                if (mic != null && mic.IsRecording)
                {
                    mic.StopRecord(); 
                }
                else
                {
                    tcs.TrySetCanceled(ct);
                    isCompleted = true;
                }
            }))
            {
                var result = await tcs.Task;
                
                if (onStop != null)
                {
                    mic.OnRecordStop -= onStop;
                }
                
                return result;
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("[Voice] Voice recognition was canceled");
            return string.Empty;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Voice] Voice recognition failed: {e.Message}\n{e.StackTrace}");
            return string.Empty;
        }
        finally
        {
            if (onStop != null && mic != null)
            {
                try
                {
                    mic.OnRecordStop -= onStop;
                }
                catch { }
            }
            {
                mic.StopRecord();
                Debug.Log("[Voice] Microphone recording stopped (cleanup)");
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
        StartCoroutine(KonSequence());
    }

    private IEnumerator KonSequence()
    {
        var targetTransform = player.stateMachine.currentTargetHitTarget.transform.root;
        Vector3 playerPos = player.transform.position;
        Vector3 bossPos = targetTransform.position;
        
        Vector3 dirToBoss = bossPos - playerPos;
        dirToBoss.y = 0;
        dirToBoss.Normalize();

        Vector3 rightDir = Vector3.Cross(Vector3.up, dirToBoss);

        Vector3 spawnPos = bossPos + (rightDir * konSequenceData.spawnOffset.x) + Vector3.up * konSequenceData.spawnOffset.y;
        Vector3 lookDir = bossPos - spawnPos;
        lookDir.y = 0;

        Quaternion lookRotation = Quaternion.LookRotation(lookDir);
        Quaternion finalRot = Quaternion.Euler(-55f, lookRotation.eulerAngles.y, 0f);
        
        konWolfInstance.transform.position = spawnPos;
        konWolfInstance.transform.rotation = finalRot;
        
        
        player.impulseManager.KonRumbleImpulse();
        yield return new WaitForSeconds(konSequenceData.rumbleDuration);
        

        float animationSpeed = konSequenceData.animationPlaybackSpeedCurve.Evaluate(0);
        konWolfAnimator.SetFloat("AttackSpeed", animationSpeed);
        konWolfAnimator.Play("Attack8");

        yield return null;
        
        AnimatorStateInfo stateInfo = konWolfAnimator.GetCurrentAnimatorStateInfo(0);
        
        while (stateInfo.IsTag("Attack"))
        {
            animationSpeed = konSequenceData.animationPlaybackSpeedCurve.Evaluate(stateInfo.normalizedTime);
            Debug.Log($"{animationSpeed}");
            konWolfAnimator.SetFloat("AttackSpeed", animationSpeed);
            
            stateInfo = konWolfAnimator.GetCurrentAnimatorStateInfo(0);

            yield return null;
        }

        yield return null;
    }
    
    
    private void Muryokusho()
    {
        StartCoroutine(MuryokushoSequence());
    }

    private IEnumerator MuryokushoSequence()
    {
        bool isSkyboxChanged = false;
        
        float elapsedTime = 0f;
        while (elapsedTime < muryokushoSequenceData.quadBloomDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float alphaValue =
                muryokushoSequenceData.quadBloomCurve.Evaluate(elapsedTime / muryokushoSequenceData.quadBloomDuration);
            bloomQuadMaterial.SetFloat("_Alpha", alphaValue);

            if (elapsedTime > 0.2f)
            {
                isSkyboxChanged = true;
                RenderSettings.skybox = muryokushoSequenceData.spaceSkyboxMaterial;
                dissolveMaterial.SetFloat("_Cutoff_Height", muryokushoSequenceData.maxCutoffHeight);
            }

            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < muryokushoSequenceData.intersectionDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            
            float curveValue = 
                muryokushoSequenceData.intersectionSphereScaleCurve.Evaluate(elapsedTime / muryokushoSequenceData.intersectionDuration);
            float scaleValue = muryokushoSequenceData.minIntersectionSphereScale + muryokushoSequenceData.intersectionSphereScaleRange * curveValue;
            
            intersectionSphereTransform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
            
            yield return null;
        }
        

        elapsedTime = 0f;
        while (elapsedTime < muryokushoSequenceData.dissolveDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            
            float curveValue = muryokushoSequenceData.cutoffCurve.Evaluate(
                elapsedTime / muryokushoSequenceData.dissolveDuration);
            dissolveMaterial.SetFloat("_Cutoff_Height",
                muryokushoSequenceData.maxCutoffHeight - curveValue * muryokushoSequenceData.cutOffHeightRange);
            
            yield return null;
        }
        
        //if (isSkyboxChanged) RenderSettings.skybox = muryokushoSequenceData.originalSkyboxMaterial;
        
        bloomQuadMaterial.SetFloat("_Alpha", 0f);
        dissolveMaterial.SetFloat("_Cutoff_Height", muryokushoSequenceData.minCutoffHeight);
        intersectionSphereTransform.localScale = Vector3.zero;
    }


    private void Aka()
    {
        StartCoroutine(AkaSequence());
    }

    private IEnumerator AkaSequence()
    {
        Vector3 spawnPos = akaSpawnHandLandmark.position + Vector3.up * 0.3f;
        
        GameObject aka = Instantiate(AkaObject, spawnPos, Quaternion.identity);
        AkaManager akaManager = aka.GetComponent<AkaManager>();

        
        float elapsedTime = 0f;
        while (elapsedTime < akaSequenceData.waitDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            aka.transform.position = Vector3.Lerp(aka.transform.position, akaSpawnHandLandmark.position + Vector3.up * 0.3f,
                akaSequenceData.akaLerpSpeed * Time.unscaledDeltaTime);
            yield return null;
        }
        
        aka.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
        
        Vector3 initialDirection = player.stateMachine.currentTargetHitTarget.position - aka.transform.position;
        
        while (true)
        {
            Vector3 direction = player.stateMachine.currentTargetHitTarget.position - aka.transform.position;
            direction.Normalize();

            aka.transform.position += direction * (akaSequenceData.akaSpeed * Time.unscaledDeltaTime);

            if (akaManager.isHit)
            {
                EventManager.TriggerOnAkaHit(initialDirection, akaSequenceData.pushDuration);
                break;
            }
            
            yield return null;
        }

        yield return null;
    }

    public void RegisterAkaSpawnPoint(Transform fingertip) => akaSpawnHandLandmark = fingertip; 
}
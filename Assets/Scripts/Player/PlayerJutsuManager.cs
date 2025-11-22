using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mediapipe.Unity.HandWorldLandmarkDetection;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using Whisper;
using Whisper.Utils;
using Debug = UnityEngine.Debug;

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
    [SerializeField]
    private Material bloomQuadMaterial;

    [SerializeField] private Material dissolveMaterial;
    [SerializeField] private Transform intersectionSphereTransform;
    [SerializeField] private MuryokushoSequenceData muryokushoSequenceData;

    [Header("Aka")] [SerializeField] private GameObject AkaObject;
    [SerializeField] private AkaSequenceData akaSequenceData;
    private Transform akaSpawnHandLandmark;

    [Header("Kon")] [SerializeField] private GameObject konWolfInstance;
    [SerializeField] private Animator konWolfAnimator;
    [SerializeField] private Material konWolfMaterial;
    [SerializeField] private KonSequenceData konSequenceData;


    public bool isInMuryokusho = false;
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
    }

    public IEnumerator JutsuMode()
    {
        isUsingJutsu = true;
        EventManager.TriggerOnJutsuModeEnter();
        
        gestureQueue.Clear();
        ResetInitialPrompt();

        player.stateMachine.CheckNearbyEnemies(out GameObject target, false);
        PlayerCameraStateHandler.instance.UpdateCameraState(PlayerCameraState.Jutsu, target.transform);

        float elapsedTime = 0f;
        Time.timeScale = slowedTimeScale;

        bool isTriggered = false;
        bool jutsuGestureTrigger = false;
        bool stopRequested = false;

        Action jutsu = null;
        string expectedVoiceCommand = "";
        GestureType detectedGesture = GestureType.None;

        Task<string> voiceTask = null;
        CancellationTokenSource cts = new CancellationTokenSource();
        var mic = VoiceRecognitionManager.instance.microphoneRecord;

        Debug.Log("Jutsu Mode: Started. Waiting for gesture...");

        try
        {
            while (elapsedTime < sequenceMaxDuration || (voiceTask != null && !voiceTask.IsCompleted))
            {
                elapsedTime += Time.unscaledDeltaTime;

                if (elapsedTime >= sequenceMaxDuration && !stopRequested && voiceTask != null && !voiceTask.IsCompleted)
                {
                    Debug.Log("[Jutsu] Time out! Forcing stop microphone to check result...");
                    if (mic.IsRecording) mic.StopRecord();
                    stopRequested = true;
                }

                if (elapsedTime >= sequenceMaxDuration + 3.0f)
                {
                    Debug.Log("[Jutsu] Hard Timeout. Aborting.");
                    break;
                }

                if (!jutsuGestureTrigger)
                {
                    gestureQueue.CapacitySafeEnqueue(HandWorldLandmarkVisualizer.instance.currentGesture,
                        gestureQueueCapacity);
                    
                    detectedGesture = HandWorldLandmarkVisualizer.instance.currentGesture;
                    if (detectedGesture != GestureType.None && 
                        jutsuDict.ContainsKey(detectedGesture) && 
                        gestureQueue.GetCount(detectedGesture) == gestureQueueCapacity)
                    {
                        jutsu = GetJutsu(detectedGesture);
                        expectedVoiceCommand = GetJutsuVoiceCommand(detectedGesture);

                        for (int i = 0; i < gestureQueueCapacity; i++) gestureQueue.Enqueue(detectedGesture);

                        jutsuGestureTrigger = true;
                        Debug.Log(
                            $"[Phase 1] Gesture '{detectedGesture}' detected. Listening for '{expectedVoiceCommand}'...");

                        UpdateInitialPrompt(expectedVoiceCommand);
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

                            if (voiceResult.Length != 0 &&
                                StringSimilarity.IsSimilar(voiceResult, expectedVoiceCommand))
                            {
                                isTriggered = true;
                                break;
                            }
                            else
                            {
                                voiceTask = RecognizeVoiceAsync(cts.Token);
                                continue;
                            }
                        }
                        else if (voiceTask.Status == TaskStatus.Canceled || voiceTask.Status == TaskStatus.Faulted)
                        {
                            Debug.Log("[Phase 2] Voice task canceled or faulted. Retrying...");
                            voiceTask = RecognizeVoiceAsync(cts.Token);
                            continue;
                        }
                    }
                    
                    gestureQueue.CapacitySafeEnqueue(HandWorldLandmarkVisualizer.instance.currentGesture,
                        gestureQueueCapacity);

                    if (gestureQueue.GetCount(detectedGesture) == 0 && !stopRequested)
                    {
                        Debug.Log("[Canceled] Gesture lost during casting.");

                        if (mic.IsRecording) mic.StopRecord();
                        stopRequested = true;
                    }
                }

                yield return null;
            }

            if (!isTriggered && voiceTask != null && voiceTask.Status == TaskStatus.RanToCompletion)
            {
                string voiceResult = voiceTask.Result;
                Debug.Log($"[Post-Loop Check] Voice task finished. Heard: '{voiceResult}'");

                if (StringSimilarity.IsSimilar(voiceResult, expectedVoiceCommand))
                {
                    isTriggered = true;
                    Debug.Log("[Post-Loop Check] Success! Triggering Jutsu.");
                }
            }
        }
        finally
        {
            Debug.Log("Jutsu sequence ending. Cleaning up...");
            cts.Cancel();
            cts.Dispose();

            if (detectedGesture != GestureType.Aka)
            {
                Time.timeScale = 1f;
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
                PlayerCameraStateHandler.instance.UpdateCameraState(PlayerCameraState.Strafe,
                    player.stateMachine.currentTargetHitTarget.transform);
                Time.timeScale = 1f;
                HandWorldLandmarkVisualizer.instance.DeactivateVisuals();
                Debug.Log("Jutsu Sequence Ended (Timeout or Failed)");
            }

            ResetInitialPrompt();
            gestureQueue.Clear();

            isUsingJutsu = false;
            EventManager.TriggerOnJustuModeExit();
        }
    }

    private async Task<string> RecognizeVoiceAsync(CancellationToken ct)
    {
        var mic = VoiceRecognitionManager.instance.microphoneRecord;
        var whisper = VoiceRecognitionManager.instance.whisperManager;

        TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
        OnRecordStopDelegate onStop = null;
        bool isCompleted = false;

        try
        {
            onStop = async (chunk) =>
            {
                if (tcs.Task.IsCompleted) return;

                try
                {
                    var result = await whisper.GetTextAsync(chunk.Data, chunk.Frequency, chunk.Channels);

                    string transcription = result != null ? result.Result.Trim() : string.Empty;
                    tcs.TrySetResult(transcription);
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                }
            };

            mic.OnRecordStop += onStop;

            if (!mic.IsRecording) mic.StartRecord();

            using (ct.Register(() =>
                   {
                       if (mic != null && mic.IsRecording)
                       {
                           Debug.Log("[Voice] Token cancelled, stopping mic to finalize transcription...");
                           mic.StopRecord();
                       }
                       else
                       {
                           tcs.TrySetCanceled();
                       }
                   }))
            {
                return await tcs.Task;
            }
        }
        catch (TaskCanceledException)
        {
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
                catch
                {
                }
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
            case GestureType.Aka:
                return Aka;
            case GestureType.Muryokusho:
                return Muryokusho;
            case GestureType.Punch:
                Debug.Log("Punch is not implemented");
                return null;
            default:
                return null;
        }
    }

    private string GetJutsuVoiceCommand(GestureType gestureType) => jutsuDict[gestureType].targetCommand;

    
    private void Kon()
    {
        Debug.Log("KON has been called");
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

        Vector3 spawnPos = bossPos + (rightDir * konSequenceData.spawnOffset.x) +
                           Vector3.up * konSequenceData.spawnOffset.y;
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

            if (stateInfo.normalizedTime > 0.5f)
            {
                PlayerCameraStateHandler.instance.UpdateCameraState(PlayerCameraState.Strafe,
                    player.stateMachine.currentTargetHitTarget.transform);
            }

            yield return null;
        }

        yield return null;
    }


    
    private void Muryokusho()
    {
        Debug.Log("MURYOKUSHO has been called");
        PlayerCameraStateHandler.instance.UpdateCameraState(PlayerCameraState.Strafe,
            player.stateMachine.currentTargetHitTarget.transform);
        StartCoroutine(MuryokushoSequence());
    }

    private IEnumerator MuryokushoSequence()
    {
        EventManager.TriggerOnMuryokushoStart();
        bool isSkyboxChanged = false;

        isInMuryokusho = true;

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
                muryokushoSequenceData.intersectionSphereScaleCurve.Evaluate(elapsedTime /
                                                                             muryokushoSequenceData
                                                                                 .intersectionDuration);
            float scaleValue = muryokushoSequenceData.minIntersectionSphereScale +
                               muryokushoSequenceData.intersectionSphereScaleRange * curveValue;

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

        bloomQuadMaterial.SetFloat("_Alpha", 0f);
        dissolveMaterial.SetFloat("_Cutoff_Height", muryokushoSequenceData.minCutoffHeight);
        intersectionSphereTransform.localScale = Vector3.zero;


        yield return new WaitForSeconds(muryokushoSequenceData.muryokushoDuration);


        elapsedTime = 0f;
        while (elapsedTime < muryokushoSequenceData.quadBloomDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float alphaValue =
                muryokushoSequenceData.quadBloomCurve.Evaluate(elapsedTime / muryokushoSequenceData.quadBloomDuration);
            bloomQuadMaterial.SetFloat("_Alpha", alphaValue);

            if (elapsedTime > 0.2f)
            {
                RenderSettings.skybox = muryokushoSequenceData.originalSkyboxMaterial;
                dissolveMaterial.SetFloat("_Cutoff_Height", muryokushoSequenceData.maxCutoffHeight);
            }

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

        EventManager.TriggerOnMuryokushoEnd();
        isInMuryokusho = false;
    }


    
    private void Aka()
    {
        Debug.Log("AKA has been called");
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
            aka.transform.position = Vector3.Lerp(aka.transform.position,
                akaSpawnHandLandmark.position + Vector3.up * 0.3f,
                akaSequenceData.akaLerpSpeed * Time.unscaledDeltaTime);
            yield return null;
        }

        aka.GetComponent<CinemachineImpulseSource>().GenerateImpulse();

        Time.timeScale = 1f;
        Vector3 initialDirection = player.stateMachine.currentTargetHitTarget.position - aka.transform.position;
        HandWorldLandmarkVisualizer.instance.DeactivateVisuals();
        PlayerCameraStateHandler.instance.UpdateCameraState(PlayerCameraState.Strafe,
            player.stateMachine.currentTargetHitTarget.transform);

        while (true)
        {
            Vector3 direction = player.stateMachine.currentTargetHitTarget.position - aka.transform.position;
            direction.Normalize();

            aka.transform.position += direction * (akaSequenceData.akaSpeed * Time.unscaledDeltaTime);

            if (akaManager.isHit)
            {
                EventManager.TriggerOnAkaHit(initialDirection, akaSequenceData.pushDuration, akaSequenceData.akaSpeed);
                break;
            }

            yield return null;
        }

        yield return null;
    }

    public void RegisterAkaSpawnPoint(Transform fingertip) => akaSpawnHandLandmark = fingertip;



    private void UpdateInitialPrompt(string expectedCommand)
    {
        VoiceRecognitionManager.instance.whisperManager.initialPrompt = $"skill command : \"{expectedCommand}\"";
    }

    private void ResetInitialPrompt() => VoiceRecognitionManager.instance.whisperManager.initialPrompt = "";
}
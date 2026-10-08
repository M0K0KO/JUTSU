using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Whisper.Utils;
using Debug = UnityEngine.Debug;

public class PlayerJutsuManager : MonoBehaviour
{
    private PlayerManager player;

    [SerializeField] private ScriptableRendererData rendererData;
    private const string EnemyFeatureName = "Enemy";
    private const string PlayerFeatureName = "Player";
    private ScriptableRendererFeature enemyFeature;
    private ScriptableRendererFeature playerFeature;


    [Header("Sequence Settings")]
    [SerializeField]
    private float sequenceMaxDuration;

    [SerializeField, Min(1f)] private float voiceProcessingTimeout = 15f;

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
    private Transform akaSpawnBaseHandLandmark;

    [Header("Kon")] [SerializeField] private GameObject konWolfInstance;
    [SerializeField] private Animator konWolfAnimator;
    [SerializeField] private Material konWolfMaterial;
    [SerializeField] private KonSequenceData konSequenceData;
    private BaseAudioSourceHolder konWolfAudioSourceHolder;


    public bool isInMuryokusho = false;
    public bool isUsingJutsu { get; private set; } = false;

    private Dictionary<GestureType, Jutsu> jutsuDict = new Dictionary<GestureType, Jutsu>();

    private const int gestureQueueCapacity = 20;
    private Queue<GestureType> gestureQueue = new Queue<GestureType>(gestureQueueCapacity);
    private CancellationTokenSource _voiceSessionCancellation;

    private void OnDestroy()
    {
        // Unity can stop the casting coroutine on scene unload without running
        // its finally block. Detach the voice session before the next scene starts.
        _voiceSessionCancellation?.Cancel();
        _voiceSessionCancellation = null;
        ResetMuryokushoVisuals(muryokushoSequenceData != null &&
                              RenderSettings.skybox == muryokushoSequenceData.spaceSkyboxMaterial);
    }

    private void Awake()
    {
        player = GetComponent<PlayerManager>();
        //VoiceRecognitionManager.instance.microphoneRecord.OnRecordStop += OnRecordStop;

        konWolfAudioSourceHolder = konWolfInstance.GetComponentInChildren<BaseAudioSourceHolder>();

        playerFeature = rendererData.rendererFeatures.Find(f => f.name == PlayerFeatureName);
        enemyFeature = rendererData.rendererFeatures.Find(f => f.name == EnemyFeatureName);
        ResetMuryokushoVisuals(true);
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

    internal sealed class VoiceAttempt
    {
        internal Task<string> Task;
        internal bool IsProcessing;
    }

    public IEnumerator JutsuMode() => RunJutsuMode(StartVoiceRecognition,
        () => HandWorldLandmarkVisualizer.instance.currentGesture, GetJutsu);

    // The same casting flow is exercised by the opt-in player diagnostics with
    // scripted voice/gesture input, without opening the user's microphone.
    internal IEnumerator RunJutsuMode(Func<CancellationToken, VoiceAttempt> recognize,
        Func<GestureType> readGesture, Func<GestureType, Action> resolveJutsu)
    {
        if (!player.stateMachine.CheckNearbyEnemies(out GameObject target, false) || target == null)
        {
            Debug.Log("[Jutsu] No valid target. Skipping concentration mode.");
            yield break;
        }
        isUsingJutsu = true;
        EventManager.TriggerOnJutsuModeEnter();

        HandWorldLandmarkVisualizer.instance.ResetIntensity();

        gestureQueue.Clear();
        ResetInitialPrompt();

        PlayerCameraStateHandler.instance.UpdateCameraState(PlayerCameraState.Jutsu, target.transform);

        float elapsedTime = 0f;
        Time.timeScale = slowedTimeScale;

        bool isTriggered = false;
        bool jutsuGestureTrigger = false;
        bool stopRequested = false;

        Action jutsu = null;
        string expectedVoiceCommand = "";
        GestureType detectedGesture = GestureType.None;

        VoiceAttempt voice = null;
        float processingElapsed = 0f;
        CancellationTokenSource cts = new CancellationTokenSource();
        _voiceSessionCancellation = cts;
        var mic = VoiceRecognitionManager.instance.microphoneRecord;

        Debug.Log("Jutsu Mode: Started. Waiting for gesture...");

        try
        {
            while (true)
            {
                float delta = PauseMenuController.Instance.IsPaused ? 0f : Time.unscaledDeltaTime;
                elapsedTime += delta;

                if (elapsedTime >= sequenceMaxDuration && !stopRequested)
                {
                    stopRequested = true;
                    Debug.Log("[Jutsu] Input window closed. Waiting for the pending voice result...");
                    if (voice != null && !voice.Task.IsCompleted && mic.IsRecording) mic.StopRecord();
                }

                if (voice != null && voice.Task.IsCompleted)
                {
                    string voiceResult = voice.Task.Status == TaskStatus.RanToCompletion
                        ? voice.Task.Result : string.Empty;
                    Debug.Log($"[Phase 2] Voice task completed. Heard: '{voiceResult}'");
                    if (!string.IsNullOrEmpty(voiceResult) &&
                        StringSimilarity.IsSimilar(voiceResult, expectedVoiceCommand, levenshteinThreshold: 0.6f))
                    {
                        isTriggered = true;
                        break;
                    }

                    // A failed result can retry only while the original input
                    // window is open. Never start another recording after it closes.
                    if (stopRequested) break;
                    voice = recognize(cts.Token);
                    processingElapsed = 0f;
                    yield return null;
                    continue;
                }

                if (voice != null && voice.IsProcessing)
                {
                    processingElapsed += delta;
                    if (processingElapsed >= voiceProcessingTimeout)
                    {
                        Debug.Log("[Jutsu] Voice processing timeout. Aborting.");
                        break;
                    }
                }
                else if (stopRequested)
                {
                    // No inference to wait for (including a microphone start failure).
                    break;
                }

                var currentGesture = readGesture();
                if (!jutsuGestureTrigger)
                {
                    gestureQueue.CapacitySafeEnqueue(currentGesture,
                        gestureQueueCapacity);

                    detectedGesture = currentGesture;
                    if (detectedGesture != GestureType.None &&
                        jutsuDict.ContainsKey(detectedGesture) &&
                        gestureQueue.GetCount(detectedGesture) == gestureQueueCapacity)
                    {
                        jutsu = resolveJutsu(detectedGesture);
                        expectedVoiceCommand = GetJutsuVoiceCommand(detectedGesture);

                        for (int i = 0; i < gestureQueueCapacity; i++) gestureQueue.Enqueue(detectedGesture);

                        jutsuGestureTrigger = true;
                        Debug.Log(
                            $"[Phase 1] Gesture '{detectedGesture}' detected. Listening for '{expectedVoiceCommand}'...");

                        StartCoroutine(HandWorldLandmarkVisualizer.instance.Glow());

                        UpdateInitialPrompt(expectedVoiceCommand);
                        voice = recognize(cts.Token);
                        processingElapsed = 0f;
                    }
                }
                else
                {
                    gestureQueue.CapacitySafeEnqueue(currentGesture,
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

        }
        finally
        {
            Debug.Log("Jutsu sequence ending. Cleaning up...");
            if (_voiceSessionCancellation == cts) _voiceSessionCancellation = null;
            cts.Cancel();
            cts.Dispose();

            if (detectedGesture != GestureType.Aka)
            {
                Time.timeScale = 1f;
                HandWorldLandmarkVisualizer.instance.DeactivateVisuals();
                HandWorldLandmarkVisualizer.instance.ResetIntensity();
            }

            if (isTriggered && jutsu != null)
            {
                Debug.Log("Jutsu Sequence Ended (Triggered)");
                EventManager.TriggerOnJutsuActivation(detectedGesture);
                jutsu();
            }
            else
            {
                var hitTarget = player.stateMachine.currentTargetHitTarget;
                PlayerCameraStateHandler.instance.UpdateCameraState(hitTarget != null
                    ? PlayerCameraState.Strafe : PlayerCameraState.Base, hitTarget);
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

    private VoiceAttempt StartVoiceRecognition(CancellationToken ct)
    {
        var voice = new VoiceAttempt();
        voice.Task = RecognizeVoiceSessionAsync(ct, voice);
        return voice;
    }

    private Task<string> RecognizeVoiceAsync(CancellationToken ct) => StartVoiceRecognition(ct).Task;

    private async Task<string> RecognizeVoiceSessionAsync(CancellationToken ct, VoiceAttempt voice)
    {
        if (ct.IsCancellationRequested) return string.Empty;
        var mic = VoiceRecognitionManager.instance.microphoneRecord;
        var whisper = VoiceRecognitionManager.instance.whisperManager;

        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        OnRecordStopDelegate onStop = null;

        try
        {
            onStop = async (chunk) =>
            {
                if (tcs.Task.IsCompleted || ct.IsCancellationRequested || voice.IsProcessing) return;
                voice.IsProcessing = true;

                try
                {
                    var result = await whisper.GetTextAsync(chunk.Data, chunk.Frequency, chunk.Channels);

                    string transcription = result != null ? result.Result.Trim() : string.Empty;
                    tcs.TrySetResult(transcription);
                }
                catch (Exception e)
                {
                    if (ct.IsCancellationRequested) tcs.TrySetResult(string.Empty);
                    else tcs.TrySetException(e);
                }
            };

            mic.OnRecordStop += onStop;

            if (!mic.IsRecording) mic.StartRecord();

            using (ct.Register(() =>
                   {
                       // Complete this session immediately. A late native result
                       // must not keep an old mic subscription alive for the next cast.
                       // Cancellation is a normal gameplay outcome. Avoid throwing
                       // through cascading async catch blocks (Unity UUM-114402).
                       tcs.TrySetResult(string.Empty);
                       if (mic != null && mic.IsRecording)
                       {
                           mic.StopRecord();
                       }
                   }))
            {
                return await tcs.Task;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Voice] Voice recognition failed: {e.Message}\n{e.StackTrace}");
            return string.Empty;
        }
        finally
        {
            voice.IsProcessing = false;
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

            if (mic != null && mic.IsRecording)
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


        konWolfAudioSourceHolder.sfxDict["Rumble"].PlayAudioClip();
        yield return new WaitForSeconds(0.2f);
        konWolfAudioSourceHolder.sfxDict["Emerge"].PlayAudioClip();


        player.impulseManager.KonRumbleImpulse();
        yield return new WaitForSeconds(konSequenceData.rumbleDuration);


        float animationSpeed = konSequenceData.animationPlaybackSpeedCurve.Evaluate(0);
        konWolfAnimator.SetFloat("AttackSpeed", animationSpeed);
        konWolfAnimator.Play("Attack8");

        yield return new WaitForEndOfFrame();

        AnimatorStateInfo stateInfo = konWolfAnimator.GetCurrentAnimatorStateInfo(0);

        bool impact = false;
        bool cameraUpdated = false;

        while (stateInfo.IsTag("Attack"))
        {
            animationSpeed = konSequenceData.animationPlaybackSpeedCurve.Evaluate(stateInfo.normalizedTime);
            konWolfAnimator.SetFloat("AttackSpeed", animationSpeed);

            stateInfo = konWolfAnimator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.normalizedTime > 0.2f && !impact)
            {
                impact = true;
                konWolfAudioSourceHolder.sfxDict["Impact"].PlayAudioClip();
            }


            if (stateInfo.normalizedTime > 0.5f && !cameraUpdated)
            {
                cameraUpdated = true;
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
        try
        {
            yield return MuryokushoSequenceCore();
        }
        finally
        {
            ResetMuryokushoVisuals(true);
        }
    }

    private IEnumerator MuryokushoSequenceCore()
    {
        EventManager.TriggerOnMuryokushoStart();
        isInMuryokusho = true;

        SetFeatureActive(true);

        player.audioSourceHolder.sfxDict["MuryokushoStart"].PlayAudioClip();
        
        float elapsedTime = 0f;
        while (elapsedTime < muryokushoSequenceData.quadBloomDuration)
        {
            if (!PauseMenuController.Instance.IsPaused) elapsedTime += Time.unscaledDeltaTime;
            float alphaValue =
                muryokushoSequenceData.quadBloomCurve.Evaluate(elapsedTime / muryokushoSequenceData.quadBloomDuration);
            bloomQuadMaterial.SetFloat("_Alpha", alphaValue);

            if (elapsedTime > 0.2f)
            {
                RenderSettings.skybox = muryokushoSequenceData.spaceSkyboxMaterial;
                dissolveMaterial.SetFloat("_Cutoff_Height", muryokushoSequenceData.maxCutoffHeight);
            }

            yield return null;
        }

        SetFeatureActive(false);

        elapsedTime = 0f;
        while (elapsedTime < muryokushoSequenceData.intersectionDuration)
        {
            if (!PauseMenuController.Instance.IsPaused) elapsedTime += Time.unscaledDeltaTime;

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
            if (!PauseMenuController.Instance.IsPaused) elapsedTime += Time.unscaledDeltaTime;

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


        SetFeatureActive(true);

        elapsedTime = 0f;
        while (elapsedTime < muryokushoSequenceData.quadBloomDuration)
        {
            if (!PauseMenuController.Instance.IsPaused) elapsedTime += Time.unscaledDeltaTime;
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

        SetFeatureActive(false);

        EventManager.TriggerOnMuryokushoEnd();
        player.audioSourceHolder.sfxDict["MuryokushoEnd"].PlayAudioClip();

        elapsedTime = 0f;
        while (elapsedTime < muryokushoSequenceData.dissolveDuration)
        {
            if (!PauseMenuController.Instance.IsPaused) elapsedTime += Time.unscaledDeltaTime;

            float curveValue = muryokushoSequenceData.cutoffCurve.Evaluate(
                elapsedTime / muryokushoSequenceData.dissolveDuration);
            dissolveMaterial.SetFloat("_Cutoff_Height",
                muryokushoSequenceData.maxCutoffHeight - curveValue * muryokushoSequenceData.cutOffHeightRange);

            yield return null;
        }

        isInMuryokusho = false;
    }


    private void Aka()
    {
        Debug.Log("AKA has been called");
        StartCoroutine(AkaSequence());
    }

    private IEnumerator AkaSequence()
    {
        Vector3 spawnPos = akaSpawnHandLandmark.position + (akaSpawnHandLandmark.position - akaSpawnBaseHandLandmark.position) * 1.5f;

        GameObject aka = Instantiate(AkaObject, spawnPos, Quaternion.identity);
        AkaManager akaManager = aka.GetComponent<AkaManager>();


        float elapsedTime = 0f;
        while (elapsedTime < akaSequenceData.waitDuration)
        {
            if (!PauseMenuController.Instance.IsPaused) elapsedTime += Time.unscaledDeltaTime;
            aka.transform.position = Vector3.Lerp(aka.transform.position,
                akaSpawnHandLandmark.position + (akaSpawnHandLandmark.position - akaSpawnBaseHandLandmark.position) * 1.5f,
                akaSequenceData.akaLerpSpeed * Time.unscaledDeltaTime *
                (PauseMenuController.Instance.IsPaused ? 0f : 1f));
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

            aka.transform.position += direction * (akaSequenceData.akaSpeed * Time.unscaledDeltaTime *
                                                   (PauseMenuController.Instance.IsPaused ? 0f : 1f));

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
    
    public void RegisterAkaSpawnPointBase(Transform fingertip) => akaSpawnBaseHandLandmark = fingertip;


    private void UpdateInitialPrompt(string expectedCommand)
    {
        VoiceRecognitionManager.instance.whisperManager.initialPrompt = $"skill command : \"{expectedCommand}\"";
    }

    private void ResetInitialPrompt() => VoiceRecognitionManager.instance.whisperManager.initialPrompt = "";


    private void SetFeatureActive(bool isActive)
    {
        playerFeature?.SetActive(isActive);
        enemyFeature?.SetActive(isActive);
        if (rendererData != null) rendererData.SetDirty();
    }

    private void ResetMuryokushoVisuals(bool restoreSkybox)
    {
        SetFeatureActive(false);
        if (bloomQuadMaterial != null) bloomQuadMaterial.SetFloat("_Alpha", 0f);
        if (dissolveMaterial != null && muryokushoSequenceData != null)
            dissolveMaterial.SetFloat("_Cutoff_Height", muryokushoSequenceData.minCutoffHeight);
        if (intersectionSphereTransform != null) intersectionSphereTransform.localScale = Vector3.zero;
        if (restoreSkybox && muryokushoSequenceData != null)
            RenderSettings.skybox = muryokushoSequenceData.originalSkyboxMaterial;
        isInMuryokusho = false;
    }
}

using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlobalVolumeManager : MonoBehaviour
{
    public static GlobalVolumeManager instance;

    public Volume volume { get; private set; }
    
    private Vignette vignette;

    [SerializeField] private float originalVignetteIntensity = 0.2f;
    [SerializeField] private float jutsuModeVignetteIntensity = 0.4f;
    // Intensity units per real-time second; reach the target instead of approaching it asymptotically.
    [SerializeField] private float vignetteSmoothSpeed = 1.5f;
    private float targetVignetteIntensity = 0.2f;
    
    private ChromaticAberration _chromaticAberration;

    private void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        volume = GetComponent<Volume>();

        if (!volume.profile.TryGet(out vignette)) vignette = volume.profile.Add<Vignette>(true);
        volume.profile.TryGet(out _chromaticAberration);
        targetVignetteIntensity = originalVignetteIntensity;
        vignette.intensity.Override(originalVignetteIntensity);
        EnsureVignetteVisible();
    }

    private void Update()
    {
        if (vignette == null) return;
        vignette.intensity.value = Mathf.MoveTowards(
            vignette.intensity.value,
            targetVignetteIntensity,
            vignetteSmoothSpeed * Time.unscaledDeltaTime *
            (PauseMenuController.Instance != null && PauseMenuController.Instance.IsPaused ? 0f : 1f));
    }

    private void OnEnable()
    {
        if (instance != this) return;
        EventManager.OnJutsuModeEnter += OnJutsuModeEnter;
        EventManager.OnJustuModeExit += OnJutsuModeExit;
    }

    private void OnDisable()
    {
        EventManager.OnJutsuModeEnter -= OnJutsuModeEnter;
        EventManager.OnJustuModeExit -= OnJutsuModeExit;
    }

    private void OnJutsuModeEnter() => SetVignette(true);
    private void OnJutsuModeExit() => SetVignette(false);

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private void EnsureVignetteVisible()
    {
        vignette.active = true;
        vignette.intensity.overrideState = true;
        volume.enabled = true;
        volume.isGlobal = true;
        volume.weight = 1f;

        var camera = Camera.main;
        if (camera == null) return;
        var cameraData = camera.GetUniversalAdditionalCameraData();
        cameraData.renderPostProcessing = true;
        cameraData.volumeLayerMask |= 1 << volume.gameObject.layer;
        camera.SetVolumeFrameworkUpdateMode(VolumeFrameworkUpdateMode.EveryFrame);
    }

    public void SetVignette(bool isJutsuMode)
    {
        if (isJutsuMode) EnsureVignetteVisible();
        if (isJutsuMode) targetVignetteIntensity = jutsuModeVignetteIntensity;
        else targetVignetteIntensity = originalVignetteIntensity;
    }
    
}

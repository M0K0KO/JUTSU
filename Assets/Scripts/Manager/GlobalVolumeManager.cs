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
    [SerializeField] private float jutsuModeVignetteIntensity = 0.35f;
    [SerializeField] private float vignetteSmoothSpeed = 3f;
    private float targetVignetteIntensity = 0.2f;
    
    private ChromaticAberration _chromaticAberration;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        volume = GetComponent<Volume>();

        volume.profile.TryGet(out vignette);
        volume.profile.TryGet(out _chromaticAberration);
    }

    private void Update()
    {
        vignette.intensity.value = Mathf.Lerp(
            vignette.intensity.value,
            targetVignetteIntensity,
            vignetteSmoothSpeed * Time.unscaledDeltaTime * (PauseMenuController.Instance.IsPaused ? 0f : 1f));
    }

    private void OnEnable()
    {
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

    public void SetVignette(bool isJutsuMode)
    {
        if (isJutsuMode) targetVignetteIntensity = jutsuModeVignetteIntensity;
        else targetVignetteIntensity = originalVignetteIntensity;
    }
    
}

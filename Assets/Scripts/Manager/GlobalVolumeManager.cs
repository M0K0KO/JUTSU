using System;
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


    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        volume = GetComponent<Volume>();

        volume.profile.TryGet(out vignette);
    }

    private void Update()
    {
        vignette.intensity.value = Mathf.Lerp(
            vignette.intensity.value,
            targetVignetteIntensity,
            vignetteSmoothSpeed * Time.unscaledDeltaTime);
    }

    private void OnEnable()
    {
        EventManager.OnJutsuModeEnter += () => SetVignette(true);
        EventManager.OnJustuModeExit += () => SetVignette(false);
    }

    private void OnDisable()
    {
        EventManager.OnJutsuModeEnter -= () => SetVignette(true);
        EventManager.OnJustuModeExit -= () => SetVignette(false);
    }

    public void SetVignette(bool isJutsuMode)
    {
        if (isJutsuMode) targetVignetteIntensity = jutsuModeVignetteIntensity;
        else targetVignetteIntensity = originalVignetteIntensity;
    }
}

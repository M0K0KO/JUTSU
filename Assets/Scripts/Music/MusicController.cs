using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Audio;

public class MusicController : MonoBehaviour
{
    [SerializeField] private AudioClip musicClip;
    
    private AudioSource _musicAudioSource;
    [SerializeField] private AudioMixer audioMixer;
    
    private void Awake()
    {
        _musicAudioSource = GetComponent<AudioSource>();
        _musicAudioSource.clip = musicClip;
        _musicAudioSource.loop = true;
        _musicAudioSource.Play();

        audioMixer.SetFloat("CutoffBGM", 22000f);
        audioMixer.SetFloat("ResonanceBGM", 1f);
        audioMixer.SetFloat("CutoffSFX", 22000f);
        audioMixer.SetFloat("ResonanceSFX", 1f);

        EventManager.OnCameraStateChange += OnCameraStateChange;


    }

    private void OnDestroy()
    {
        EventManager.OnCameraStateChange -= OnCameraStateChange;
    }

    private void OnCameraStateChange(PlayerCameraState state, Transform target)
    {
        switch (state)
        {
            case PlayerCameraState.Jutsu:
            {
                ModifyLowpassFilter(1000f, 4f, 0.25f);
                break;
            }
            default:
            {
                ModifyLowpassFilter(22000f, 1f, 0.25f);
                break;
            }
        }
    }

    private void ModifyLowpassFilter(float cutoffFrequency, float resonance, float transitionDuration)
    {
        audioMixer.DOSetFloat("CutoffBGM", cutoffFrequency, transitionDuration);
        audioMixer.DOSetFloat("CutoffSFX", cutoffFrequency, transitionDuration);
        audioMixer.DOSetFloat("ResonanceBGM", resonance, transitionDuration);
        audioMixer.DOSetFloat("ResonanceSFX", resonance, transitionDuration);
    }
    
    
}

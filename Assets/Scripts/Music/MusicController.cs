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

        EventManager.OnJustuModeEnter += OnJutsuModeEnter;
        EventManager.OnJustuModeExit += OnJutsuModeExit;


    }

    private void OnDestroy()
    {
        EventManager.OnJustuModeEnter -= OnJutsuModeEnter;
        EventManager.OnJustuModeExit -= OnJutsuModeExit;
    }

    private void OnJutsuModeEnter()
    {
        ModifyLowpassFilter(1200f, 1f);
        _musicAudioSource.volume = 0.9f;
    }

    private void OnJutsuModeExit()
    {
        ModifyLowpassFilter(22000f, 1f);
        _musicAudioSource.pitch = 1f;
        _musicAudioSource.volume = 1f;
    }

    private void ModifyLowpassFilter(float cutoffFrequency, float resonance)
    {
        audioMixer.SetFloat("CutoffBGM", cutoffFrequency);
        audioMixer.SetFloat("CutoffSFX", cutoffFrequency);
        audioMixer.SetFloat("ResonanceBGM", resonance);
        audioMixer.SetFloat("ResonanceSFX", resonance);
    }
    
    
}

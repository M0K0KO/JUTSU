using System;
using UnityEngine;
using DG.Tweening;

public class MusicController : MonoBehaviour
{
    [SerializeField] private AudioClip battleIntroClip;
    [SerializeField] private AudioClip battleLoopClip;
    
    private AudioSource _musicAudioSource;
    private AudioLowPassFilter _lowPassFilter;

    private bool _isSlowMotion = false;
    
    private MusicState _musicState;

    public enum MusicState
    {
        None,
        Intro,
        Loop,
    }
    
    private void Awake()
    {
        _musicState = MusicState.None;
        _musicAudioSource = GetComponent<AudioSource>();
        _musicAudioSource.playOnAwake = false;
        
        _lowPassFilter = GetComponent<AudioLowPassFilter>();
        _lowPassFilter.cutoffFrequency = 22000f;
    }

    private void Start()
    {
        _musicAudioSource.clip = battleIntroClip;
        _musicAudioSource.loop = false;
        _musicAudioSource.Play();
        _musicState = MusicState.Intro;
    }

    private void Update()
    {
        // TODO: instead of changing in update, make it event driven using delegates.
        
        if (!_isSlowMotion && Time.timeScale < 0.95f)
        {
            _isSlowMotion = true;
            DOTween.To(() => _lowPassFilter.cutoffFrequency, x => _lowPassFilter.cutoffFrequency = x, 800, 0.3f);
            DOTween.To(() => _musicAudioSource.volume, x => _musicAudioSource.volume = x, 0.8f, 0.3f);
            
        }
        else if (_isSlowMotion && Time.timeScale >= 0.95f)
        {
            _isSlowMotion = false;
            DOTween.To(() => _lowPassFilter.cutoffFrequency, x => _lowPassFilter.cutoffFrequency = x, 22000, 0.3f);
            DOTween.To(() => _musicAudioSource.volume, x => _musicAudioSource.volume = x, 1f, 0.3f);
        }

        

        switch (_musicState)
        {
            case MusicState.Intro:
            {
                if (!_musicAudioSource.isPlaying)
                {
                    _musicState = MusicState.Loop;
                    _musicAudioSource.clip = battleLoopClip;
                    _musicAudioSource.loop = true;
                    _musicAudioSource.Play();
                }

                break;
            }
            case MusicState.Loop:
            {
                if (!_musicAudioSource.isPlaying)
                {
                    _musicState = MusicState.None;
                    _musicAudioSource.Stop();
                }
                break;
            }
        }
    }
    
    
}

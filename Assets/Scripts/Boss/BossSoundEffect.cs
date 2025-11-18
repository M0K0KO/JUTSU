using System;
using System.Collections.Generic;
using UnityEngine;

public class BossSoundEffect : MonoBehaviour
{
    [Header("Footstep")]
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private List<AudioClip> footstepAudioClips;
    [Header("Shockwave")]
    [SerializeField] private AudioSource shockwaveAudioSource;
    [SerializeField] private AudioClip shockwaveAudioClip;
    private int _lastFootstepClipIndex = -1;
    
    private BossStateMachine _stateMachine;

    private void Awake()
    {
        footstepAudioSource.playOnAwake = false;
        _stateMachine = GetComponent<BossStateMachine>();
    }

    public void PlayFootstepSound()
    {
        int shuffleIndex;
        do
        {
            shuffleIndex = UnityEngine.Random.Range(0, footstepAudioClips.Count);
        }
        while (shuffleIndex == _lastFootstepClipIndex);
        
        _lastFootstepClipIndex = shuffleIndex;
        
        AudioClip randomFootstep = footstepAudioClips[shuffleIndex];
        
        footstepAudioSource.pitch = UnityEngine.Random.Range(0.8f, 1f);
        footstepAudioSource.volume = UnityEngine.Random.Range(0.9f, 1f);
        footstepAudioSource.PlayOneShot(randomFootstep);
    }

    public void PlayShockwaveSound()
    {
        shockwaveAudioSource.pitch = UnityEngine.Random.Range(0.8f, 1f);
        shockwaveAudioSource.PlayOneShot(shockwaveAudioClip);
    }

}

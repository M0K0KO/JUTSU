using System;
using System.Collections.Generic;
using UnityEngine;

public class BossSoundEffect : MonoBehaviour
{
    [SerializeField] private AudioSource footstepAudioSource;
    [SerializeField] private List<AudioClip> footstepAudioClips;
    private int _lastFootstepClipIndex = -1;

    private void Awake()
    {
        footstepAudioSource.playOnAwake = false;
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
        
        footstepAudioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        footstepAudioSource.volume = UnityEngine.Random.Range(0.9f, 1f);
        footstepAudioSource.PlayOneShot(randomFootstep);
    }

}

using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SFX
{
    public string key;
    public BaseSFX sfx;
}

public class BaseAudioSourceHolder : MonoBehaviour
{
    [SerializeField] private SFX[] SFXs;
    
    public Dictionary<string, BaseSFX> sfxDict = new Dictionary<string, BaseSFX>();

    private void Awake()
    {
        foreach (SFX sfx in SFXs)
        {
            sfxDict.Add(sfx.key, sfx.sfx);
        }
    }
}

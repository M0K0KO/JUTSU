using System;
using UnityEngine;
using UnityEngine.UI;
using Whisper.Utils;

public class MicrophoneDropdownInitializer : MonoBehaviour
{
    [SerializeField] private Dropdown microphoneDropdown;
    private void Awake()
    {
        MicrophoneRecord microphoneRecord = FindFirstObjectByType<MicrophoneRecord>(FindObjectsInactive.Include);
        if (microphoneRecord != null)
        {
            microphoneRecord.microphoneDropdown = this.microphoneDropdown;
        }
    }
    

    
}

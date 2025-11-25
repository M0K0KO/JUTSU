using System;
using UnityEngine;
using UnityEngine.UI;
using Whisper.Utils;

public class MicrophoneDropdownInitializer : MonoBehaviour
{
    [SerializeField] private Dropdown microphoneDropdown;
    private void Start()
    {
        VoiceRecognitionManager.instance.microphoneRecord.UpdateMicrophoneDropdown(microphoneDropdown);
    }
    

    
}

using System;
using UnityEngine;
using Whisper;
using Whisper.Utils;

public class VoiceRecognitionManager : MonoBehaviour
{
    public static VoiceRecognitionManager instance;
    
    public MicrophoneRecord microphoneRecord { get; private set; }
    public WhisperManager whipserManager { get; private set; }

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else Destroy(gameObject);
        
        if (microphoneRecord == null) microphoneRecord = GetComponent<MicrophoneRecord>();
        if (whipserManager == null) whipserManager = GetComponent<WhisperManager>();
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}

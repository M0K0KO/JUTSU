using System;
using UnityEngine;
using Whisper;
using Whisper.Utils;

public class VoiceRecognitionManager : MonoBehaviour
{
    public static VoiceRecognitionManager instance;

    public MicrophoneRecord microphoneRecord;
    public WhisperManager whipserManager;

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else Destroy(gameObject);
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}

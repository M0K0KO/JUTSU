using System;
using UnityEngine;
using Whisper;
using Whisper.Utils;

[DefaultExecutionOrder(-1000)]
public class VoiceRecognitionManager : MonoBehaviour
{
    public static VoiceRecognitionManager instance;

    public MicrophoneRecord microphoneRecord;
    public WhisperManager whisperManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Prevent the duplicate Whisper component from loading another model.
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}

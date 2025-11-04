using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Whisper;
using Whisper.Utils;

public class VoiceTest : MonoBehaviour
{
    private WhisperManager _whisperManager;
    private MicrophoneRecord _microphoneRecord;
    
    [SerializeField] private Button microphoneButton;
    private TMP_Text _microphoneButtonText;

    [SerializeField] private TMP_Text outputText;
    [SerializeField] private TMP_Text similarityInfoText;
    [SerializeField] private string targetCommand;
    [SerializeField] private float levenshteinDistanceRatioThreshold = 0.3f;

    private void Awake()
    {
        _whisperManager = FindFirstObjectByType<WhisperManager>();
        _microphoneRecord = FindFirstObjectByType<MicrophoneRecord>();

        _microphoneButtonText = microphoneButton.GetComponentInChildren<TMP_Text>();

        _microphoneRecord.OnRecordStop += OnRecordStop;
        
        microphoneButton.onClick.AddListener(OnMicrophoneButtonClicked);
    }

    private void OnDisable()
    {
        microphoneButton.onClick.RemoveListener(OnMicrophoneButtonClicked);
    }

    private void OnMicrophoneButtonClicked()
    {
        if (!_microphoneRecord.IsRecording)
        {
            _microphoneRecord.StartRecord();
            _microphoneButtonText.text = "Stop";
        }
        else
        {
            _microphoneRecord.StopRecord();
            _microphoneButtonText.text = "Record";
        }
    }

    private async void OnRecordStop(AudioChunk recordedAudio)
    {
        _microphoneButtonText.text = "Record";
        
        var res = await _whisperManager.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, 
            recordedAudio.Channels);

        if (res == null || !outputText) return;

        var text = res.Result;
        outputText.text = text;
        similarityInfoText.text = $"Is Similar: {StringSimilarity.IsSimilar(text, targetCommand, levenshteinDistanceRatioThreshold)}\n" +
                                  $"Levenshtein Dist Ratio: {StringSimilarity.GetLevenshteinDistanceRatio(text, targetCommand)}\n" +
                                  $"Levenshtein Dist Ratio Threshold: {levenshteinDistanceRatioThreshold}\n" +
                                  $"Normalized String: {StringSimilarity.Normalize(text)}, {StringSimilarity.Normalize(targetCommand)}\n";
    }
    
    

}

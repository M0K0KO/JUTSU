using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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

    // 이거는 지금 버튼 클릭인데 이 안에 있는 함수인
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
        similarityInfoText.text =
            $"Is Similar: {StringSimilarity.IsSimilar(text, targetCommand)}\n";
        
        var normalizedInput = StringSimilarity.Normalize(text);
        var normalizedTarget = StringSimilarity.Normalize(targetCommand);

        if (StringSimilarity.IsTargetStringSingleWord(targetCommand))
        {
            similarityInfoText.text += 
                $"Jaro-Winkler: {StringSimilarity.GetJaroWinklerSimilarity(normalizedInput, normalizedTarget)}";
        }
        else
        {
            similarityInfoText.text +=
                $"Levenshtein: {StringSimilarity.GetLevenshteinDistanceSimilarity(normalizedInput, normalizedTarget)}";
        }

    }
    
    

}

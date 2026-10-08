using System;
using System.Text;
using Whisper;
using Whisper.Utils;

public static class JutsuVoiceValidation
{
    // This is an amplitude/duration gate, not a speech classifier. Whisper's
    // no-speech probability is checked separately before accepting any text.
    public static bool HasAudibleInput(AudioChunk chunk, float minimumRms = 0.003f)
    {
        if (chunk.Data == null || chunk.Frequency <= 0 || chunk.Channels <= 0 ||
            chunk.Data.Length % chunk.Channels != 0 || minimumRms <= 0f) return false;

        int frameSamples = Math.Max(1, chunk.Frequency / 50); // 20 ms
        int samplesPerChannel = chunk.Data.Length / chunk.Channels;
        int activeSamples = 0;
        for (int start = 0; start < samplesPerChannel; start += frameSamples)
        {
            int count = Math.Min(frameSamples, samplesPerChannel - start);
            double sum = 0, squareSum = 0;
            for (int i = start; i < start + count; i++)
            {
                double mono = 0;
                for (int channel = 0; channel < chunk.Channels; channel++)
                {
                    float sample = chunk.Data[i * chunk.Channels + channel];
                    if (float.IsNaN(sample) || float.IsInfinity(sample)) return false;
                    mono += sample;
                }
                mono /= chunk.Channels;
                sum += mono;
                squareSum += mono * mono;
            }
            // Remove DC offset so an inactive microphone's bias is not sound.
            double mean = sum / count;
            double variance = Math.Max(0, squareSum / count - mean * mean);
            if (variance >= minimumRms * minimumRms) activeSamples += count;
        }
        return activeSamples >= chunk.Frequency * 0.12f;
    }

    public static string GetSpeechText(WhisperResult result, float maximumNoSpeechProbability = 0.6f)
    {
        if (result?.Segments == null) return string.Empty;
        var text = new StringBuilder();
        foreach (var segment in result.Segments)
        {
            float probability = segment.NoSpeechProbability;
            if (float.IsNaN(probability) || float.IsInfinity(probability) || probability < 0f ||
                probability >= maximumNoSpeechProbability ||
                string.IsNullOrWhiteSpace(StringSimilarity.Normalize(segment.Text))) continue;
            text.Append(segment.Text);
        }
        return text.ToString().Trim();
    }
}

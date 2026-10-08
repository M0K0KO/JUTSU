using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Mediapipe;
using UnityEngine;
using Whisper;
using Whisper.Utils;

public static class JutsuSafetyChecks
{
    public static unsafe void Run()
    {
        var silence = new AudioChunk { Data = new float[48000], Frequency = 16000, Channels = 1 };
        Require(!JutsuVoiceValidation.HasAudibleInput(silence), "Silence passed the voice amplitude gate.");
        silence.Data[8000] = 1f;
        Require(!JutsuVoiceValidation.HasAudibleInput(silence), "A single click passed the voice duration gate.");
        for (int i = 0; i < silence.Data.Length; i++) silence.Data[i] = 0.1f;
        Require(!JutsuVoiceValidation.HasAudibleInput(silence), "Microphone DC offset passed the voice gate.");
        for (int i = 0; i < silence.Data.Length; i++)
            silence.Data[i] = 0.02f * (float)Math.Sin(i * 2 * Math.PI * 440 / 16000);
        Require(JutsuVoiceValidation.HasAudibleInput(silence), "Sustained audible input failed the amplitude gate.");
        var nonSpeech = new WhisperResult(new List<WhisperSegment> { new WhisperSegment(0, "Kon", 0, 20, 0.9f) }, 0);
        var speech = new WhisperResult(new List<WhisperSegment> { new WhisperSegment(0, "Kon", 0, 20, 0.1f) }, 0);
        var unknown = new WhisperResult(new List<WhisperSegment> { new WhisperSegment(0, "Kon", 0, 20) }, 0);
        Require(JutsuVoiceValidation.GetSpeechText(nonSpeech) == string.Empty &&
                JutsuVoiceValidation.GetSpeechText(unknown) == string.Empty &&
                JutsuVoiceValidation.GetSpeechText(speech) == "Kon",
            "No-speech confidence gate accepted invalid evidence or rejected valid speech.");
        Require(!StringSimilarity.IsSimilar("[Kon]", "Kon"), "An audio annotation was accepted as a command.");
        Debug.Log("JUTSU_VOICE_VALIDATION_CHECKS: passed (silence, click, DC offset, audible input, no-speech probability, annotations)");

        var go = new GameObject("JUTSU safety checks");
        go.SetActive(false);
        try
        {
            var manager = go.AddComponent<WhisperManager>();
            var update = typeof(WhisperManager).GetMethod("UpdateParams", BindingFlags.NonPublic | BindingFlags.Instance);
            var field = typeof(WhisperManager).GetField("_params", BindingFlags.NonPublic | BindingFlags.Instance);
            manager.initialPrompt = "old request";
            update.Invoke(manager, null);
            var first = (WhisperParams)field.GetValue(manager);
            manager.initialPrompt = "new request";
            update.Invoke(manager, null);
            var second = (WhisperParams)field.GetValue(manager);
            Require(!ReferenceEquals(first, second), "Whisper parameters were shared across requests.");
            Require(Marshal.PtrToStringAnsi((IntPtr)first.NativeParams.initial_prompt) == "old request",
                "The previous request's prompt was changed or released.");
            second.InitialPrompt = null;
            second.Language = null;
            Require(second.NativeParams.initial_prompt == null && second.NativeParams.language == null,
                "Cleared Whisper strings left dangling native pointers.");

            // Exercise the real runner copy, overwrite its source, and read the native image.
            var runner = go.AddComponent<HandGestureRecognizerRunner>();
            var frame = new Mediapipe.Unity.Experimental.TextureFrame(2, 2, TextureFormat.RGBA32);
            try
            {
                var pixels = frame.GetRawTextureData<byte>();
                for (var i = 0; i < pixels.Length; ++i) pixels[i] = 17;
                var buildImage = typeof(HandGestureRecognizerRunner).GetMethod("BuildOwnedImage", BindingFlags.NonPublic | BindingFlags.Instance);
                using var image = (Image)buildImage.Invoke(runner, new object[] { frame });
                for (var i = 0; i < pixels.Length; ++i) pixels[i] = 255;
                using var pixelLock = new PixelWriteLock(image);
                Require(Marshal.ReadByte(pixelLock.Pixels()) == 17, "MediaPipe image still aliases the reusable texture.");
            }
            finally
            {
                // This test frame has never acquired a GL pointer or sync token.
                // Unregister it and destroy immediately: the package's runtime
                // Dispose calls Destroy, which is invalid in edit mode.
                frame.RemoveAllReleaseListeners();
                var table = (GlobalInstanceTable<Guid, Mediapipe.Unity.Experimental.TextureFrame>)
                    typeof(Mediapipe.Unity.Experimental.TextureFrame).GetField("_InstanceTable", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                table.Remove(frame.GetInstanceID());
                UnityEngine.Object.DestroyImmediate(frame.texture);
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }

        // Exercise exception containment through Whisper's actual native inference callback.
        var context = WhisperContextParams.GetDefaultParams();
        context.UseGpu = false;
        var wrapper = WhisperWrapper.InitFromFile(Path.Combine(Application.streamingAssetsPath, "ggml-small.en.bin"), context);
        Require(wrapper != null, "Whisper model failed to load.");
        var callbackCalled = false;
        wrapper.OnProgress += _ =>
        {
            callbackCalled = true;
            throw new InvalidOperationException("Intentional callback safety test");
        };
        var callbackContained = false;
        try
        {
            var samples = new float[WhisperWrapper.WhisperSampleRate * 3];
            for (var i = 0; i < samples.Length; ++i)
                samples[i] = 0.2f * (float)Math.Sin(i * 2 * Math.PI * 440 / WhisperWrapper.WhisperSampleRate);
            wrapper.GetText(samples, WhisperWrapper.WhisperSampleRate, 1,
                WhisperParams.GetDefaultParams());
        }
        catch (InvalidOperationException exception)
        {
            callbackContained = exception.InnerException?.Message == "Intentional callback safety test";
        }
        Require(callbackCalled && callbackContained, "Whisper callback exception was not safely reported after inference.");
        Debug.Log("JUTSU_SAFETY_CHECKS: passed (parameter isolation, cleared pointers, image ownership, native callback exception)");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

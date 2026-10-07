using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using Mediapipe.Tasks.Core;
using Mediapipe.Tasks.Vision.GestureRecognizer;
using Mediapipe;
using Mediapipe.Unity;
using Mediapipe.Unity.Sample;
using UnityEngine;
using UnityEngine.Rendering;
using RunningMode = Mediapipe.Tasks.Vision.Core.RunningMode;

public class HandGestureRecognizerRunner : HandGestureRecognizeVisionTaskApiRunner<GestureRecognizer>
{
    private Mediapipe.Unity.Experimental.TextureFramePool _textureFramePool;
    private GestureRecognizerOptions options;
    private readonly object _resultLock = new object();
    private GestureRecognizerResult _pendingResult;
    private bool _hasPendingResult;
    private Exception _callbackException;
    private volatile bool _acceptResults;
    private AsyncGPUReadbackRequest _readbackRequest;
    private bool _readbackPending;
    private RenderTexture _readbackTexture;
    private bool _useAsyncReadback;
    private byte[] _pixelBuffer;
    private long _lastTimestamp = -1;
    private static readonly ImageFrame.Deleter _freeImagePixels = FreeImagePixels;

    private void OnDestroy()
    {
        Stop();
    }

    public override void Stop()
    {
        _acceptResults = false;
        // A readback must finish before its destination texture is disposed.
        if (_readbackPending)
        {
            _readbackRequest.WaitForCompletion();
            _readbackPending = false;
        }
        ReleaseReadbackTexture();
        base.Stop();
        lock (_resultLock)
        {
            _hasPendingResult = false;
            _pendingResult = default;
            _callbackException = null;
        }
        _textureFramePool?.Dispose();
        _textureFramePool = null;
        _pixelBuffer = null;
    }

    protected override IEnumerator Run()
    {
        _lastTimestamp = -1;
        _useAsyncReadback = SystemInfo.supportsAsyncGPUReadback;
        string modelPath = Path.Combine(
            Application.streamingAssetsPath, 
            "gesture_recognizer_v2.bytes");

        BaseOptions baseOptions = new BaseOptions(
            BaseOptions.Delegate.CPU,
            modelAssetPath: modelPath
        );

        options = new GestureRecognizerOptions(
            baseOptions,
            RunningMode.LIVE_STREAM,
            1,
            0.5f,
            0.5f,
            0.5f,
            null,
            null,
            OnHandGestureRecognizerOutput);

        yield return AssetLoader.PrepareAssetAsync(modelPath);

        _acceptResults = true;
        taskApi = GestureRecognizer.CreateFromOptions(options, GpuManager.GpuResources);
        Debug.Log($"using model from {modelPath}");

        var imageSource = ImageSourceProvider.ImageSource;

        yield return imageSource.Play();

        if (!imageSource.isPrepared)
        {
            Debug.LogError("Failed to start ImageSource, exiting...");
            Stop();
            yield break;
        }

        _textureFramePool = new Mediapipe.Unity.Experimental.TextureFramePool(
            imageSource.textureWidth,
            imageSource.textureHeight,
            TextureFormat.RGBA32,
            10);

        var transformationOptions = imageSource.GetTransformationOptions();
        var flipHorizontally = transformationOptions.flipHorizontally;
        var flipVertically = transformationOptions.flipVertically;
        var imageProcessingOptions =
            new Mediapipe.Tasks.Vision.Core.ImageProcessingOptions(
                rotationDegrees: (int)transformationOptions.rotationAngle);

        var waitUntilReqDone = new WaitUntil(() => _readbackRequest.done);

        while (true)
        {
            DeliverPendingResult();
            if (isPaused)
            {
                yield return new WaitWhile(() => isPaused);
            }

            if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
            {
                yield return new WaitForEndOfFrame();
                continue;
            }

            // Always return the borrowed frame, including readback/build failures.
            try
            {
                if (_useAsyncReadback)
                {
                    _readbackRequest = ReadTextureAsync(textureFrame,
                        imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
                    _readbackPending = true;
                    yield return waitUntilReqDone;
                    _readbackPending = false;

                    if (_readbackRequest.hasError)
                    {
                        Debug.LogWarning("GPU readback failed; switching to CPU texture reads.");
                        _useAsyncReadback = false;
                        continue;
                    }
                }
                else
                {
                    textureFrame.ReadTextureOnCPU(
                        imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
                }

                // BuildCPUImage aliases Unity's pixel buffer. The asynchronous task
                // must own a copy until MediaPipe releases its last image reference.
                using var image = BuildOwnedImage(textureFrame);
                var timestamp = Math.Max(GetCurrentTimestampMillisec(), _lastTimestamp + 1);
                taskApi.RecognizeAsync(image, timestamp, imageProcessingOptions);
                _lastTimestamp = timestamp;
            }
            finally
            {
                ReleaseReadbackTexture();
                textureFrame.Release();
            }
            yield return null;
        }
    }

    private Image BuildOwnedImage(Mediapipe.Unity.Experimental.TextureFrame frame)
    {
        var pixels = _useAsyncReadback ? _readbackRequest.GetData<byte>() : frame.GetRawTextureData<byte>();
        if (_pixelBuffer == null || _pixelBuffer.Length != pixels.Length)
        {
            _pixelBuffer = new byte[pixels.Length];
        }
        pixels.CopyTo(_pixelBuffer);
        var memory = Marshal.AllocHGlobal(_pixelBuffer.Length);
        try
        {
            Marshal.Copy(_pixelBuffer, 0, memory, _pixelBuffer.Length);
            return new Image(frame.imageFormat, frame.width, frame.height,
                frame.width * 4, memory, _freeImagePixels);
        }
        catch
        {
            Marshal.FreeHGlobal(memory);
            throw;
        }
    }

    private AsyncGPUReadbackRequest ReadTextureAsync(
        Mediapipe.Unity.Experimental.TextureFrame frame, Texture source, bool flipX, bool flipY)
    {
        _readbackTexture = RenderTexture.GetTemporary(frame.width, frame.height, 0, RenderTextureFormat.ARGB32);
        var previous = RenderTexture.active;
        try
        {
            Graphics.Blit(source, _readbackTexture,
                new Vector2(flipX ? -1f : 1f, flipY ? -1f : 1f),
                new Vector2(flipX ? 1f : 0f, flipY ? 1f : 0f));
            // Avoid the package callback, which reads failed requests without
            // checking hasError and can leak its temporary render texture.
            return AsyncGPUReadback.Request(_readbackTexture, 0, TextureFormat.RGBA32);
        }
        finally
        {
            RenderTexture.active = previous;
        }
    }

    private void ReleaseReadbackTexture()
    {
        if (_readbackTexture == null) return;
        RenderTexture.ReleaseTemporary(_readbackTexture);
        _readbackTexture = null;
    }

    [AOT.MonoPInvokeCallback(typeof(ImageFrame.Deleter))]
    private static void FreeImagePixels(IntPtr memory)
    {
        Marshal.FreeHGlobal(memory);
    }

    private void OnHandGestureRecognizerOutput(GestureRecognizerResult result, Image image, long timestamp)
    {
        // MediaPipe calls this on a worker thread. Do not access Unity objects here,
        // and never allow a managed exception to escape the native callback.
        try
        {
            lock (_resultLock)
            {
                if (!_acceptResults) return;
                result.CloneTo(ref _pendingResult);
                _hasPendingResult = true;
            }
        }
        catch (Exception exception)
        {
            lock (_resultLock)
            {
                _callbackException = exception;
            }
        }
    }

    private void DeliverPendingResult()
    {
        lock (_resultLock)
        {
            if (_callbackException != null)
            {
                Debug.LogException(_callbackException);
                _callbackException = null;
            }

            if (!_hasPendingResult) return;
            _hasPendingResult = false;
            var visualizer = HandWorldLandmarkVisualizer.instance;
            if (visualizer != null)
            {
                visualizer.DrawLater(_pendingResult, GetRecognizedGestureType(_pendingResult));
            }
        }
    }

    private GestureType GetRecognizedGestureType(GestureRecognizerResult result)
    {
        if (result.gestures == null || result.gestures.Count == 0 ||
            result.gestures[0].categories == null || result.gestures[0].categories.Count == 0)
        {
            return GestureType.None;
        }

        string bestGesture = result.gestures[0].categories[0].categoryName;

        if (bestGesture == "none") return GestureType.None;
        
        switch (bestGesture)
        {
            case "kon":
                return GestureType.Kon;     
            case "aka":
                return GestureType.Aka;
            case "muryokusho":
                return GestureType.Muryokusho;
            case "punch":
                return GestureType.Punch;
            default:
                return GestureType.None;
        }
    }
}

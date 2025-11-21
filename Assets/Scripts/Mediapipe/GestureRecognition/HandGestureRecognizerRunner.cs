using System.Collections;
using System.IO;
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

    public override void Stop()
    {
        base.Stop();
        _textureFramePool?.Dispose();
        _textureFramePool = null;
    }

    protected override IEnumerator Run()
    {
        string modelPath = Path.Combine(
            Application.streamingAssetsPath, 
            "gesture_recognizer_v1.bytes");

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

        taskApi = GestureRecognizer.CreateFromOptions(options, GpuManager.GpuResources);
        Debug.Log($"using model from {modelPath}");

        var imageSource = ImageSourceProvider.ImageSource;

        yield return imageSource.Play();

        if (!imageSource.isPrepared)
        {
            Debug.LogError("Failed to start ImageSource, exiting...");
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

        AsyncGPUReadbackRequest req = default;
        var waitUntilReqDone = new WaitUntil(() => req.done);

        var canUseGpuImage = SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 &&
                             GpuManager.GpuResources != null;
        using var glContext = canUseGpuImage ? GpuManager.GetGlContext() : null;

        while (true)
        {
            if (isPaused)
            {
                yield return new WaitWhile(() => isPaused);
            }

            if (!_textureFramePool.TryGetTextureFrame(out var textureFrame))
            {
                yield return new WaitForEndOfFrame();
                continue;
            }

            Image image;
            req = textureFrame.ReadTextureAsync(imageSource.GetCurrentTexture(), flipHorizontally, flipVertically);
            yield return waitUntilReqDone;

            if (req.hasError)
            {
                Debug.LogWarning($"Failed to read texture from the image source");
                continue;
            }

            image = textureFrame.BuildCPUImage();
            textureFrame.Release();

            taskApi.RecognizeAsync(image, GetCurrentTimestampMillisec(), imageProcessingOptions);
        }
    }

    private void OnHandGestureRecognizerOutput(GestureRecognizerResult result, Image image, long timestamp)
    {
        if (result.gestures != null)
        {
            //Debug.Log(result.handWorldLandmarks.Count);

            HandWorldLandmarkVisualizer.instance.DrawLater(result, GetRecognizedGestureType(result));

            //Debug.Log(result.handWorldLandmarks[0].landmarks[0].z);
            Debug.Log(result.gestures[0].categories[0].categoryName);
        }
    }

    private GestureType GetRecognizedGestureType(GestureRecognizerResult result)
    {
        string bestGesture = result.gestures[0].categories[0].categoryName;

        if (bestGesture == "none") return GestureType.None;
        
        switch (bestGesture)
        {
            case "kon":
                return GestureType.Kon;       
            default:
                return GestureType.None;
        }
    }
}
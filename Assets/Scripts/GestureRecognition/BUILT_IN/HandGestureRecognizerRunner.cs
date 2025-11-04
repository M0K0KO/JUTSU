using System.Collections;
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
        BaseOptions baseOptions = new BaseOptions(
            BaseOptions.Delegate.CPU, 
            modelAssetPath: "gesture_recognizer.bytes"
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

        yield return AssetLoader.PrepareAssetAsync(
            $"gesture_recognizer.bytes");

        taskApi = GestureRecognizer.CreateFromOptions(options, GpuManager.GpuResources);
        
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
        var waitForEndOfFrame = new WaitForEndOfFrame();
        var result = GestureRecognizerResult.Alloc(options.numHands);
        
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
            HandWorldLandmarkVisualizer.instance.DrawLater(result);
            Debug.Log($"HandGestureRecognizerOutput: {result.gestures[0].categories[0].categoryName}");
        }
    }
}
using System.Collections;
using Mediapipe.Tasks;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity;
using Mediapipe.Unity.Experimental;
using Mediapipe.Unity.Sample;
using UnityEngine;
using UnityEngine.Rendering;

namespace Mediapipe.Unity.HandWorldLandmarkDetection
{
    public class HandWorldLandmarkerRunner : HandWorldLandmarkVisionTaskApiRunner<HandLandmarker>
    {
        [SerializeField] private HandWorldLandmarkProcesser processer;
        
        private Experimental.TextureFramePool _textureFramePool;

        public readonly HandWorldLandmarkDetectionConfig config = new HandWorldLandmarkDetectionConfig();

        public override void Stop()
        {
            base.Stop();
            _textureFramePool?.Dispose();
            _textureFramePool = null;
        }

        protected override IEnumerator Run()
        {
            Debug.Log($"Delegate = {config.Delegate}");
            Debug.Log($"Image Read Mode = {config.ImageReadMode}");
            Debug.Log($"Running Mode = {config.RunningMode}");
            Debug.Log($"NumHands = {config.NumHands}");
            Debug.Log($"MinHandDetectionConfidence = {config.MinHandDetectionConfidence}");
            Debug.Log($"MinHandPresenceConfidence = {config.MinHandPresenceConfidence}");
            Debug.Log($"MinTrackingConfidence = {config.MinTrackingConfidence}");

            yield return AssetLoader.PrepareAssetAsync(config.ModelPath);

            var options = config.GetHandLandmarkerOptions(
                config.RunningMode == Mediapipe.Tasks.Vision.Core.RunningMode.LIVE_STREAM
                    ? OnHandWorldLandmarkDetectionOutput
                    : null);

            taskApi = HandLandmarker.CreateFromOptions(options, GpuManager.GpuResources);

            var imageSource = ImageSourceProvider.ImageSource;

            yield return imageSource.Play();

            if (!imageSource.isPrepared)
            {
                Debug.LogError("Failed to start ImageSource, exiting...");
                yield break;
            }

            _textureFramePool = new Experimental.TextureFramePool(imageSource.textureWidth,
                imageSource.textureHeight, TextureFormat.RGBA32, 10);

            var transformationOptions = imageSource.GetTransformationOptions();
            var flipHorizontally = transformationOptions.flipHorizontally;
            var flipVertically = transformationOptions.flipVertically;
            var imageProcessingOptions =
                new Tasks.Vision.Core.ImageProcessingOptions(
                    rotationDegrees: (int)transformationOptions.rotationAngle);

            AsyncGPUReadbackRequest req = default;
            var waitUntilReqDone = new WaitUntil(() => req.done);
            var waitForEndOfFrame = new WaitForEndOfFrame();
            var result = HandLandmarkerResult.Alloc(options.numHands);

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

                taskApi.DetectAsync(image, GetCurrentTimestampMillisec(), imageProcessingOptions);
            }
        }
        
        private void OnHandWorldLandmarkDetectionOutput(HandLandmarkerResult result, Image image, long timestamp)
        {
            processer.DrawLater(result);
            
            //Debug.Log($"OnHandWorldLandmarkDetectionOutput: {result}");
        }
    }
}
